using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace hcp
{
    public class InGameUIManager : MonoBehaviour
    {
        [SerializeField]
        Vector3 charactorMoveV = Vector3.zero;
        [SerializeField]
        Vector3 charactorRotateV = Vector3.zero;
        
        [Space(10)]
        [SerializeField]
        Image crossHair;

        [SerializeField]
        Hero targetHero;

        [SerializeField]
        Image hpBarUI;
        float maxHPDiv;
        [SerializeField]
        Text heroName;

        [SerializeField]
        GameObject[] controlPanelPerHero;

        [SerializeField]
        Transform killLogPanel;
        [SerializeField]
        GameObject killLog;

        [SerializeField]
        Image[] heroControlBtnsScreen;

        [SerializeField]
        GameObject ESCCanvasGOPrefab;

        [SerializeField]
        GameObject stunNotice;
        
        static InGameUIManager instance;
        public static InGameUIManager Instance
        {
            get { return instance; }
        }

        private void Awake()
        {
            instance = this;
        }
        
        void Update()
        {
            if (targetHero == null) return;
            
            for (int i = 0; i < (int)E_ControlParam.MAX; i++)
            {
                heroControlBtnsScreen[
                    ((int)E_ControlParam.MAX* (int)targetHero.HeroType)
                    +i].fillAmount = targetHero.GetReUseRemainTimeByZeroToOne ((E_ControlParam)i);
            }

            hpBarUI.fillAmount = targetHero.CurrHP * maxHPDiv;
            
            stunNotice.SetActive(targetHero.IsSetBadState(E_BadState.Stun));

#if UNITY_STANDALONE_WIN
            #region 윈도우즈 컨트롤  
            if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Instantiate(ESCCanvasGOPrefab);
                    SetCursorLock(false);
                    return;
                }

            if (Input.GetMouseButtonDown(1))    //오른쪽 클릭으로 마우스 브레이크.
            {
                SetCursorLock(!IsSetCursorLock());
            }

            if (IsSetCursorLock())
            {
                float xMove = Input.GetAxis("Horizontal");
                float zMove = Input.GetAxis("Vertical");

                float xRotate = Input.GetAxis("Mouse X");
                float yRotate = Input.GetAxis("Mouse Y");
                charactorMoveV = new Vector3(xMove, 0, zMove);
                charactorRotateV = new Vector3(xRotate,yRotate*-1, 0);

                targetHero.MoveHero(charactorMoveV);
                targetHero.RotateHero(charactorRotateV);

                if (Input.GetMouseButton(0))
                {
                    OnClick_NormalAttack();
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    OnClick_Reload();
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    OnClick_FirstSkill();
                }
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    OnClick_SecondSkill();
                }
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    OnClick_Ultimate();
                }
            }
            else
            {
                targetHero.MoveHero(Vector3.zero);
                return;
            }
            #endregion
#endif
        }
        
        Vector3 ChangexyVector3ToxzVector3(Vector3 xyVector3)
        {
            return new Vector3(xyVector3.x, 0, xyVector3.y);
        }
        

        public void OnClick_NormalAttack()
        {
            targetHero.ControlHero(E_ControlParam.NormalAttack);
        }
        public void OnClick_Reload()
        {
            targetHero.ControlHero(E_ControlParam.Reload);
        }
        public void OnClick_FirstSkill()
        {
            targetHero.ControlHero(E_ControlParam.FirstSkill);
        }
        public void OnClick_SecondSkill()
        {
            targetHero.ControlHero(E_ControlParam.SecondSkill);
        }
        public void OnClick_Ultimate()
        {
            targetHero.ControlHero(E_ControlParam.Ultimate);
        }
        public void CrossHairChange(Sprite crossHair)
        {
            this.crossHair.sprite = crossHair;
        }
        public void SetTargetHero(Hero hero)
        {
            targetHero = hero;
            CrossHairChange(targetHero.crossHairs[0]);
            maxHPDiv = 1 / targetHero.MaxHP;
            ShowControlPanel(targetHero.HeroType);
            heroName.text = targetHero.PlayerName;
#if UNITY_STANDALONE_WIN
            SetCursorLock(true);
#endif
        }
        void ShowControlPanel(E_HeroType heroType)
        {
            controlPanelPerHero[(int)heroType].SetActive(true);
        }

        public void ShowKillLog(string killerName, E_HeroType killerHeroType, string victimName, E_HeroType victimHeroType)
        {
            GameObject temp =  GameObject.Instantiate(killLog, killLogPanel);
            temp.GetComponent<KillLog>().SetKillLog(killerName, killerHeroType, victimName, victimHeroType);
        }

        public void SetCursorLock(bool lockCursor)
        {
            Cursor.lockState = lockCursor ?  CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = lockCursor?false:true;
            
        }
        public bool IsSetCursorLock()
        {
            return Cursor.lockState == CursorLockMode.Locked ? true : false;
        }
        
    }
}