using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class HSPenetrateVisionEffect : MonoBehaviour
    {
        [SerializeField]
        HSPenetrateVisionParticle[] particles;
        [SerializeField]
        HSPenetrateVisionIntersectSphere intersectSphere;
        Coroutine trail;
        private void Start()
        {
            StopEffect();
        }
        public void ActivateIntersectSphere(Vector3 position, float scaleUpVelocity, float showTime)
        {
            intersectSphere.Activate(position, scaleUpVelocity, showTime);
        }

        public void ParticleActivate(int number,float term, Vector3 position, Camera lookCam, Vector2 scale, float showTime, bool calcurateScaleUpPerSec)
        {
            StopTrail();
            StartCoroutine(ParticleActivateContinue(number, term, position, lookCam, scale, showTime, calcurateScaleUpPerSec));
        }
        IEnumerator ParticleActivateContinue(int number, float term, Vector3 position, Camera lookCam, Vector2 scale, float showTime, bool calcurateScaleUpPerSec)
        {
            WaitForSeconds ws = new WaitForSeconds(term);
            for (int i = 0; i < number; i++)
            {
                HSPenetrateVisionParticle particle = GetNoShowingParticle();
                if (particle != null)
                {
                    particle.transform.position = position;
                    particle.SetShowTime(showTime);
                    particle.Activate(lookCam, scale, calcurateScaleUpPerSec);
                }
                yield return ws;
            }
        }

        public void MakeParticleTrail(Transform trailTarget, float behindZValue, Vector2 scale, float showTime,float term,float maxTrailTime)
        {
            if (trail!=null) StopCoroutine(trail);
            trail = StartCoroutine(MakeTrail(trailTarget, behindZValue, scale, showTime, term, maxTrailTime ));
        }
        IEnumerator MakeTrail(Transform trailTarget, float behindZValue, Vector2 scale, float showTime, float term,float maxTrailTime)
        {
            WaitForSeconds ws = new WaitForSeconds(term);
            float time = 0f;
            while(time<= maxTrailTime)
            {
                HSPenetrateVisionParticle particle = GetNoShowingParticle();
                if (particle != null)
                {
                    particle.transform.position = trailTarget.position + (trailTarget.forward*-behindZValue);
                    particle.SetShowTime(showTime);
                    particle.Activate(trailTarget.position, scale, true);
                }
                time += term;
                yield return ws;
            }
        }
        public void StopTrail()
        {
            if (trail != null) StopCoroutine(trail);
        }

        public void StopEffect()
        {
            StopAllCoroutines();
            DeActivateAllParticles();
            intersectSphere.DeActivate();
        }

        public void DeActivateAllParticles()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].DeActivate();
            }
        }

        HSPenetrateVisionParticle GetNoShowingParticle()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (!particles[i].Show) return particles[i];
            }
            return null;
        }
    }
}