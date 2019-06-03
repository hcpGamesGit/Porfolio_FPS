using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp {
    public class HHHook : Projectile  {

        enum HookState
        {
            Activate,
            Retrieve,
            HookSuccess,
            DeActivate,
            MAX
        }
        [SerializeField]
        HookState state = HookState.DeActivate;
        [SerializeField]
        float maxLength;
        [SerializeField]
        float hookVelocity;

        [Tooltip("same with HeroHook's hookOriginPos property")]
        [SerializeField]
        Transform originPosFromheroHook;
        [Tooltip("최대 거리만큼 뻗어나갔을 때 회수되는데 걸리는 시간")]
        [SerializeField]
        float retrieveMaxTime;
        [Tooltip("회수 속력")]
        [SerializeField]
        float retrieveVelocity;
        float retrieveVelocityDiv;

        [SerializeField]
        Transform rope;
        [SerializeField]
        Renderer ropeRenderer;

        [SerializeField]
        Material ropeMat;
        // 로프 머테리얼의 텍스쳐 타일링과 실제 로프 길이 사이 스케일 팩터. 1f 도출값
        float ropeToMaterialTileScaleFactor=1f;

        //갈고리가 처음 뻗어나오는 위치에서 갈고리 까지의 z 차에 따른 로프의 스케일 조정값.. 5f 도출값.")]
        float disToRopeScaleFactor = 5f;

        [Tooltip("적이 갈고리에 걸렸을 때 슈터와 끌려진 적 사이 거리")]
        [SerializeField]
        float hookedDestDis ;

        Hero hookedEnemy;
        float hookSuccessTime;
        float hookSuccessReturnTime;
        
        protected override void Awake()
        {
            base.Awake();
            retrieveVelocity = maxLength / retrieveMaxTime; //회수 속력.
            retrieveVelocityDiv = 1 / retrieveVelocity;

            ropeMat = new Material(ropeRenderer.material);
            ropeRenderer.material = ropeMat;
            DeActivate();
        }
        
        public void Activate()
        {
            gameObject.SetActive(true);
            velocity = hookVelocity;
            state = HookState.Activate;
        }
        public void DeActivate()
        {
            state = HookState.DeActivate;
            hookedEnemy = null;
            hookSuccessTime = 0f;
            hookSuccessReturnTime = 0f;
            transform.SetPositionAndRotation(originPosFromheroHook.position, originPosFromheroHook.rotation);
            gameObject.SetActive(false);
        }
        public void Retrieve()
        {
            velocity = retrieveVelocity;
            state = HookState.Retrieve;
        }
        public void HookSuccess(int hookedEnemyPhotonViewID,float hookReturnTime)
        {
            velocity = retrieveVelocity;
            state = HookState.HookSuccess;
            if (false == TeamInfo.GetInstance().HeroPhotonIDDic.ContainsKey(hookedEnemyPhotonViewID))
            {
                Retrieve();
                return;
            }
            hookedEnemy = TeamInfo.GetInstance().HeroPhotonIDDic[hookedEnemyPhotonViewID];
            if (hookedEnemy == null)
            {
                Retrieve();
                return;
            }
            hookSuccessTime = 0f;
            hookSuccessReturnTime = hookReturnTime;
            MyDebug.Log("HHHook - HookSuccess : 훅 성공. 대상 photonID= " + hookedEnemyPhotonViewID + ", Hook Return Time = " + hookReturnTime);
        }

        private void Update()
        {
            switch (state)
            {
                case HookState.Activate:
                    transform.Translate(Vector3.forward * velocity * Time.deltaTime , Space.Self);
                    MakeRope();
                    if (attachingHero.photonView.IsMine)
                    {
                        if (transform.localPosition.z > maxLength)
                        {
                            state = HookState.Retrieve;
                            velocity = 0f;
                            attachingHero.photonView.RPC("HookRetrieve", Photon.Pun.RpcTarget.All);
                        }
                    }
                  
                    break;
                case HookState.Retrieve:
                    transform.Translate(Vector3.back * velocity * Time.deltaTime , Space.Self);
                    MakeRope();

                    if (attachingHero.photonView.IsMine)
                    {
                        if (transform.localPosition.z < Mathf.Epsilon)
                        {
                            state = HookState.DeActivate;
                            attachingHero.photonView.RPC("HookIsDone", Photon.Pun.RpcTarget.All);
                        }
                    }
                    break;
                case HookState.HookSuccess:
                    originPosFromheroHook.LookAt(hookedEnemy.CenterPos);
                    transform.localPosition =new Vector3(0,0, Vector3.Distance(originPosFromheroHook.position, hookedEnemy.CenterPos) - 0.4f);
                    MakeRope();
                    if (attachingHero.photonView.IsMine)
                    {
                        hookSuccessTime += Time.deltaTime;
                        if (hookSuccessTime >= hookSuccessReturnTime)
                        {
                            MyDebug.Log("HHHook - Update :: case HookState.HookSuccess :  hookSuccessReturnTime 을 회수 시간이 초과. HookIsDone RPC 실행");
                            attachingHero.photonView.RPC("HookIsDone", Photon.Pun.RpcTarget.All);
                        }
                    }
                    break;
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!attachingHero.photonView.IsMine) return;
            if (state != HookState.Activate) return;

            int layer = other.gameObject.layer;

            //장애물에 부딪힌 경우
            if (layer == Constants.mapLayerMask && ! other.gameObject.CompareTag(LayerMask.LayerToName( attachingHero.gameObject.layer)))
            {
                attachingHero.photonView.RPC("HookRetrieve", Photon.Pun.RpcTarget.All);
                return;
            }

            if (TeamInfo.GetInstance().IsThisLayerEnemy(layer))
            {
                Hero enemy = other.gameObject.GetComponent<Hero>();
                if (enemy == null)
                {
                    MyDebug.Log("HHHook - OnTriggerEnter : 적으로 판정한 훅 대상에 Hero 컴포넌트가 없음");
                  
                    attachingHero.photonView.RPC("HookRetrieve", Photon.Pun.RpcTarget.All);
                    return;
                }

                //적 Hook 성공

                Vector3 enemyPos = enemy.transform.position;
                Vector3 destPos = attachingHero.transform.position + attachingHero. transform.TransformDirection(Vector3.forward)* hookedDestDis;
                float hookReturnTime = (enemyPos - destPos).magnitude * retrieveVelocityDiv;

                enemy.photonView.RPC("GetBadState", Photon.Pun.RpcTarget.All, E_BadState.Stun, transform.localPosition.z * retrieveVelocityDiv);//대상 스턴
                enemy.photonView.RPC("Hooked", Photon.Pun.RpcTarget.All, enemyPos, destPos, hookReturnTime);
                attachingHero.photonView.RPC("HookSuccess", Photon.Pun.RpcTarget.All, enemy.photonView.ViewID, hookReturnTime);
            }
        }
        void MakeRope() //rope 만들어줌
        {
            float dis = transform.localPosition.z;
            Vector3 ropeLocalScale = rope.localScale;
            if (dis < Mathf.Epsilon)
            {
                ropeLocalScale.z = 0f;
                rope.localScale = ropeLocalScale;
                return;
            }
            ropeLocalScale.z = dis * disToRopeScaleFactor;
            rope.localScale = ropeLocalScale;
            ropeMat.mainTextureScale = new Vector2(1, ropeLocalScale.z * ropeToMaterialTileScaleFactor);
        }
    }
}