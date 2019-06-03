using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class HHUltWolves : Projectile
    {
        enum E_HHUltState
        {
            Idle,
            Activate,
            DeActivate,
            MAX
        }
        [SerializeField]
        Animator[] wolvesAnimator;
        [SerializeField]
        GameObject[] wolvesGO;
        [SerializeField]
        Renderer[] wolvesRenders;
        [SerializeField]
        GameObject showEffects;
        [SerializeField]
        Animator SakuraPetalAnim;
        [SerializeField]
        ParticleSystem potalEffect;
        [SerializeField]
        float showEffectSeconds;

        [SerializeField]
        float StartVelocity;
        [SerializeField]
        float MaxVelocity;

        [SerializeField]
        E_HHUltState state;
        [SerializeField]
        float distance;
        float distanceSqr;
        [SerializeField]
        float damageTick;
        int animParamRunHash = Animator.StringToHash("run");

        protected override void Awake()
        {
            base.Awake();
            distanceSqr = distance * distance;
            for (int i = 0; i < wolvesRenders.Length; i++)
            {
                wolvesRenders[i].material = new Material(wolvesRenders[i].material);
            }
        }

        private void Start()
        {
            showEffects.transform.SetParent(null);
            showEffects.SetActive(false);
            StartVelocity = distance * 2 / showEffectSeconds;
            DeActivate();
        }
        public void Activate(Vector3 activatePos, Quaternion activeRot)
        {
            StopAllCoroutines();
            transform.SetPositionAndRotation(activatePos, activeRot);
            Vector3 planeNormalV =transform.localToWorldMatrix.MultiplyVector( 
                (transform.localPosition + Vector3.forward) - transform.localPosition
                
                );
            planeNormalV.Normalize();
            showEffects.SetActive(true);
            SetShowEffectPotal(activatePos, activeRot , planeNormalV);
            
            state = E_HHUltState.Activate;
            velocity = StartVelocity;
            transform.Translate(Vector3.back * (distance), Space.Self);
            gameObject.SetActive(true);
            wolvesRun(true);

            StartCoroutine(ActivateMove(activatePos, planeNormalV));
        }
        
        public void DeActivate()
        {
            wolvesRun(false);
            state = E_HHUltState.DeActivate;
            transform.SetPositionAndRotation(transform.parent.position, transform.parent.rotation);
            showEffects.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            showEffects.SetActive(false);
            gameObject.SetActive(false);
        }

        IEnumerator ActivateMove(Vector3 activatePos, Vector3  cutPlaneNormal)
        {
            float time = 0f;
            while (state == E_HHUltState.Activate)
            {
                time += Time.deltaTime;
                if (velocity < MaxVelocity)
                {
                    velocity += Time.deltaTime;
                }
                else if (velocity > MaxVelocity)
                {
                    velocity = MaxVelocity;
                }

                transform.Translate(Vector3.forward * velocity*Time.deltaTime, Space.Self);

                if (time > damageTick)
                {
                    time = 0f;
                    HitEnemy(activatePos, cutPlaneNormal);
                }
                yield return null;
            }
        }

        void HitEnemy(Vector3 activatePos, Vector3 cutPlaneNormal)
        {
            if (!attachingHero.photonView.IsMine) return;
            
            List<Hero> enemyHeroes = TeamInfo.GetInstance().EnemyHeroes;
            for (int i = 0; i < enemyHeroes.Count; i++)
            {
                if (Vector3.Dot((enemyHeroes[i].CenterPos - activatePos), cutPlaneNormal) < Mathf.Epsilon)
                {
                    continue;
                }

                Vector3 enemyPosition = enemyHeroes[i].CenterPos - transform.position;
                if ( enemyPosition.sqrMagnitude< distanceSqr)
                {
                    enemyHeroes[i].photonView.RPC("GetDamaged", Photon.Pun.RpcTarget.All, amount,attachingHero.photonView.ViewID);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!attachingHero.photonView.IsMine) return;
            if (other.gameObject.CompareTag(Constants.outLineTag))
            {
                state = E_HHUltState.DeActivate;
                attachingHero.photonView.RPC("HHHUltDeActivate", Photon.Pun.RpcTarget.All);
            }
        }

        void wolvesRun(bool run)
        {
            for (int i = 0; i < wolvesAnimator.Length; i++)
            {
                wolvesAnimator[i].SetBool(animParamRunHash, run);
            }
        }

        void SetShowEffectPotal(Vector3 potalStartPos, Quaternion potalForward , Vector3 cutPlaneNormalV)
        {
            showEffects.SetActive(true);
            showEffects.transform.SetPositionAndRotation(potalStartPos, potalForward);
            SakuraPetalAnim.SetTrigger("makeBig");
            SakuraPetalAnim.speed = 1/showEffectSeconds;
            
            potalEffect.Clear();
            ParticleSystem.MainModule seMainModule = potalEffect.main;
            seMainModule.startLifetime = showEffectSeconds;
            
            potalEffect.Play();

            for (int i = 0; i < wolvesRenders.Length; i++)
            {
                wolvesRenders[i].material.SetVector("cutPlaneCenterPoint", potalStartPos);
                wolvesRenders[i].material.SetVector("cutPlaneNormalVector", cutPlaneNormalV);
            }
        }
    }
}