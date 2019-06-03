using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class HeroSoldierFPSCam : FPSCameraPerHero
    {
        [SerializeField]
        ParticleSystem fpsNormalMuzzleFlash;
        [SerializeField]
        GameObject rifleModel;
        [SerializeField]
        Animator rifleModelAnimator;

        public override void FPSCamAct(E_ControlParam param)
        {
            switch (param)
            {
                case E_ControlParam.NormalAttack:
                    HSFPSCamNormalAttack();
                    break;
                case E_ControlParam.Reload:
                    HSFPSCamReload();
                    break;
                case E_ControlParam.FirstSkill:

                    break;
                case E_ControlParam.SecondSkill:

                    break;
                case E_ControlParam.Ultimate:

                    break;
            }
        }

        void HSFPSCamNormalAttack()
        {
            rifleModelAnimator.SetTrigger("NormalShot");
            fpsNormalMuzzleFlash.Play();
        }
         void HSFPSCamReload()
        {
            rifleModelAnimator.SetTrigger("Reload");
        }
    }
}