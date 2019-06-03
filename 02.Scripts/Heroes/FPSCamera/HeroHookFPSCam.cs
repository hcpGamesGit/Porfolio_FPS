using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class HeroHookFPSCam : FPSCameraPerHero
    {
        [SerializeField]
        GameObject katanaModel;
        [SerializeField]
        Animator katanaAnim;

        public override void FPSCamAct(E_ControlParam param)
        {
            switch (param) {
                case E_ControlParam.NormalAttack:
                    HHFPSCamNormalAttack();
                    break;
                case E_ControlParam.Reload:
                    
                    break;
                case E_ControlParam.FirstSkill:

                    break;
                case E_ControlParam.SecondSkill:

                    break;
                case E_ControlParam.Ultimate:
                    HHFPSCamUltimate();
                    break;
            }
        }
        void HHFPSCamNormalAttack()
        {
            katanaAnim.SetTrigger("normalAttack");
        }
        void HHFPSCamUltimate()
        {
            katanaAnim.SetTrigger("ult");
        }
    }
}