using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp {
    public class HSPenetrateVision : Projectile {
        [System.Serializable]
        enum E_PenetrateState
        {
            Idle,
            Shot,
            Settle,
            MAX
        }
        
        [SerializeField]
        E_PenetrateState state;
        [SerializeField]
        float searchRange;
        float searchRangeSqr;
        //amount가 총 탐색 시간.
        [SerializeField]
        float penetrateMaintainTime;    //한번 검출되면 투시가 지속되는 시간
        WaitForSeconds penetrateMaintainTimeWFS;
        [SerializeField]
        HSPenetrateVisionEffect effect;

        [SerializeField]
        float firstBigWaveScale;
        [SerializeField]
        float smallWaveScale;
        [SerializeField]
        float termPerEachWave;
        [SerializeField]
        float termPerWaveStage;
        [SerializeField]
        float waveShowTime;
       

        protected override void Awake()
        {
            base.Awake();
            state = E_PenetrateState.Idle;
            searchRangeSqr = searchRange * searchRange;
            penetrateMaintainTimeWFS = new WaitForSeconds(penetrateMaintainTime);
        }
        private void Start()
        {
            effect.transform.SetParent(null);
            if (!attachingHero.photonView.IsMine)
            {
                GetComponent<Collider>().enabled = false;
                Destroy(GetComponent<Rigidbody>());
            }
            DeActivate();
        }
        private void OnDestroy()
        {
            if (effect != null)
            {
                Destroy(effect.gameObject);
            }
        }

        public void Activate(Vector3 origin, Vector3 direction)
        {
            gameObject.SetActive(true);
            state = E_PenetrateState.Shot;
            transform.position = origin;
            transform.LookAt(origin + direction * 1f);

            effect.StopEffect();
            effect.MakeParticleTrail(transform, 0.1f, new Vector2(1f,1f), 1.5f, 0.2f, 20);
        }
        public void DeActivate()
        {
            effect.StopEffect();
            state = E_PenetrateState.Idle;
            transform.position = Vector3.zero;
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
        public void Settle(Vector3 position)
        {
            effect.StopTrail();
            state = E_PenetrateState.Settle;
            transform.position = position;
            StartCoroutine(PenetrateVision());//투시 효과
            StartCoroutine(PenetrateWaveEffect());
        }

        IEnumerator PenetrateWaveEffect()
        {
            if (state != E_PenetrateState.Settle) yield break;
            effect.ActivateIntersectSphere(transform.position + Vector3.up * smallWaveScale, firstBigWaveScale*0.7f , waveShowTime);
            yield return new WaitForSeconds(termPerEachWave);
            
            effect.ParticleActivate(5, termPerEachWave, transform.position + Vector3.up * smallWaveScale, Camera.main, new Vector2(firstBigWaveScale, firstBigWaveScale), waveShowTime, true);
            yield return new WaitForSeconds(termPerWaveStage);
            
            while (state == E_PenetrateState.Settle)
            {
                effect.ParticleActivate(4, termPerEachWave, transform.position + Vector3.up * smallWaveScale, Camera.main, new Vector2(smallWaveScale, smallWaveScale), waveShowTime, true);
                yield return new WaitForSeconds(termPerWaveStage);
            }
        }

        IEnumerator PenetrateVision()
        {
            if (state != E_PenetrateState.Settle) yield break;
            if (TeamInfo.GetInstance().IsThisLayerEnemy(attachingHero.gameObject.layer)) yield break;    //적의 것이면 투시 안되게 하기.

            float time = 0f;
            List<Hero> enemyHeroes = TeamInfo.GetInstance().EnemyHeroes;
            do
            {
                for (int i = 0; i < enemyHeroes.Count; i++)
                {
                    if (enemyHeroes[i] == null) continue;
                    if ((transform.position - enemyHeroes[i].CenterPos).sqrMagnitude < searchRangeSqr)  //범위 내에 들어옴.
                    {
                       enemyHeroes[i].SetPenetrateVision(penetrateMaintainTime);
                    }
                }
                time += penetrateMaintainTime;
                yield return penetrateMaintainTimeWFS;
            } while (time <= amount);

            if(attachingHero.photonView.IsMine)
            attachingHero.photonView.RPC("PenetrateVisionDeActivate", Photon.Pun.RpcTarget.All);
        }

        private void Update()
        {
            switch (state)
            {
                case E_PenetrateState.Shot:
                    transform.Translate(Vector3.forward * Time.deltaTime * velocity);
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!attachingHero.photonView.IsMine) return;
            if (state != E_PenetrateState.Shot) return;
           
            GameObject hit = other.gameObject;
            MyDebug.Log("HSPenetrateVision:: OnTriggerEnter - 충돌" + hit.name + " , 충돌 물체 레이어 = " + LayerMask.LayerToName(hit.layer) + ", 태그 = " + hit.tag);
            if ((hit.layer == Constants.mapLayerMask && !hit.CompareTag(LayerMask.LayerToName(attachingHero.gameObject.layer)))//벽에 부딪힌 경우.
                ||
                TeamInfo.GetInstance().IsThisLayerEnemy(hit.layer)    //적에 부딪힌 경우
                )  
            {
                attachingHero.photonView.RPC("PenetrateVisionSettle", Photon.Pun.RpcTarget.All, transform.position);
                return;
            }
        }
    }
}