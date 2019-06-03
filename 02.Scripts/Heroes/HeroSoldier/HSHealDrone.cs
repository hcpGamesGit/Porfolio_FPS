using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
namespace hcp
{
    public class HSHealDrone : MonoBehaviour
    {
        [Tooltip("hero this drone attached")]
        [SerializeField]
        HeroSoldier attachingHero;

        [SerializeField]
        float healAmount;
        [SerializeField]
        float healCoolTime ;
        [SerializeField]
        float healRange ;
        [SerializeField]
        float moveUpAmount;
        [SerializeField]
        float moveDownAmount;
        [SerializeField]
        Vector3 originLocalPos;

        [Tooltip("heal drone activating Time")]
        [SerializeField]
        float activeMaxTime;

        [SerializeField]
        Animator anim;

        [SerializeField]
        Material droneDissolveMat;

        float SqrHealRange;
        WaitForSeconds ws;

        float activateTime;

        Transform[] initPoses;
        Vector3[] localInitPoses;
        Quaternion[] localInitRotes;

        private void Awake()
        {
            originLocalPos = transform.localPosition;
            SqrHealRange = healRange * healRange;
            ws = new WaitForSeconds(healCoolTime);

            gameObject.SetActive(false);

            initPoses = gameObject.GetComponentsInChildren<Transform>();
            localInitPoses = new Vector3[initPoses.Length];
            localInitRotes = new Quaternion[initPoses.Length];
            for (int i = 0; i < initPoses.Length; i++)
            {
                localInitPoses[i] = initPoses[i].localPosition;
                localInitRotes[i] = initPoses[i].localRotation;
            }
            /*
             메터리얼을 동적으로 생성해서 이 인스턴스 하나만의 메터리얼로 사용.
             */
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            droneDissolveMat = new Material(renderers[0].material);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = droneDissolveMat;
            }


        }
        void SetPosToInit()
        {
            for (int i = 0; i < initPoses.Length; i++)
            {
                initPoses[i].localPosition = localInitPoses[i];
                initPoses[i].localRotation = localInitRotes[i];
            }
        }

        #region 드론 나타나고 사라지는 효과
        public void Appear()
        {
            gameObject.SetActive(true);
            anim.SetTrigger("show");
            StartCoroutine(MoveHealDrone());
            StartCoroutine(AppearEffect());
        }
        IEnumerator AppearEffect()
        {
            float startTime = 0f;
            while (startTime < 1f)
            {
                startTime += Time.deltaTime;
                droneDissolveMat.SetFloat("_Level", 1 - startTime);
                yield return null;
            }
            droneDissolveMat.SetFloat("_Level", 0);
        }
        IEnumerator MoveHealDrone()
        {
            float time = 0f;
            Vector3 localed = transform.localPosition;
            while (true)
            {
                time += Time.deltaTime;
                transform.localPosition = localed + new Vector3(0, Mathf.Sin(time * 2f) * 0.13f, 0);

                yield return null;
            }
        }

        public void DisAppear()
        {
            StartCoroutine(DisAppearEffect());
        }

        IEnumerator DisAppearEffect()
        {
            float startTime = 0f;
            while (startTime < 1f)
            {
                startTime += Time.deltaTime;
                droneDissolveMat.SetFloat("_Level", startTime);
                yield return null;
            }
            droneDissolveMat.SetFloat("_Level", 1);
            StopCoroutine(MoveHealDrone());
            transform.localPosition = originLocalPos;
            SetPosToInit();

            gameObject.SetActive(false);
        }
        #endregion

        #region 힐드론 힐링 로직
        public void Activate()
        {
            if (attachingHero.photonView.IsMine)
                StartCoroutine(StartingHealingProtocol());
        }

        IEnumerator StartingHealingProtocol()
        {
            activateTime = Time.time;
            MyDebug.Log("HSHealDrone :: StartingHealingProtocol- 힐드론 액티베이트 시간 =" + activateTime);
            while (true)
            {
                List<Hero> sameSideHeroes = TeamInfo.GetInstance().MyTeamHeroes;

                for (int i = 0; i < sameSideHeroes.Count; i++)
                {
                    if (SqrHealRange >= (sameSideHeroes[i].transform.position - transform.position).sqrMagnitude)   //힐 범위에 아군이 있으면
                    {
                        attachingHero.DroneHeal( sameSideHeroes[i], healAmount);
                    }
                }
                
                if (activateTime + activeMaxTime < Time.time)
                {
                    break;
                }
                yield return ws;
            }
            Debug.Log("HSHealDrone :: StartingHealingProtocol- 힐드론 종료 시간 =" + Time.time + "총 가동시간=" + (Time.time - activateTime));

            attachingHero.photonView.RPC("DroneDisAppear", RpcTarget.All);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, healRange);
        }
    }
}