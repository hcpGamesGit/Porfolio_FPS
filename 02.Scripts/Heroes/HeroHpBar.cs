using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp {
    public class HeroHpBar : MonoBehaviour
    {
        [SerializeField]
        string playerName;
        [SerializeField]
        TMPro.TextMeshProUGUI playerNameTextMesh;
        [SerializeField]
        UnityEngine.UI.Image hpBar;
        [SerializeField]
        bool teamSettingDone=false;
        float attachingHeroMaxHPDiv;

        [SerializeField]
        Hero attachingHero;

        public float offeredScaleFactor = 0.2f;
        Transform camTr;
        Vector3 screenCenterPoint;
        float camForwardVDisF=5000f;
        float camForwardVDisFDiv;
        

        private void Start()
        {
            screenCenterPoint = new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane);
            camForwardVDisFDiv = 1 / camForwardVDisF;
            camTr = Camera.main.transform;
        }
        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        public void SetAsTeamSetting()
        {
            if (TeamInfo.GetInstance().IsThisLayerEnemy(attachingHero.gameObject.layer))
            {
                playerNameTextMesh.color = Color.red;
                hpBar.color = Color.red;
                Show(false);
            }
            else
            {
                playerNameTextMesh.color = Color.blue;
                hpBar.color = Color.blue;
            }
           
            if (attachingHero != null)
            {
                playerNameTextMesh.text = attachingHero.PlayerName;
                attachingHeroMaxHPDiv = 1 / attachingHero.MaxHP;
            }
            teamSettingDone = true;
        }
        [SerializeField]
        Vector3 barScrPos;
        [SerializeField]
        Vector3 barSizeUpScrPos;
        [SerializeField]
        Vector3 barWScrPos;
        [SerializeField]
        Vector3 barWUpScrPos;
        [SerializeField]
        float distance;

        private void LateUpdate()
        {
            if (!teamSettingDone||attachingHero==null)
                return;

            hpBar.fillAmount = attachingHero.CurrHP* attachingHeroMaxHPDiv;
            
            Vector3 camForwardDir = Camera.main.transform.forward;

            Vector3 fromCamV = transform.position - camTr.position ;
            Vector3 camForwardV = camForwardDir * camForwardVDisF;
            float camDot = Vector3.Dot(fromCamV, camForwardV);
            if (camDot < Mathf.Epsilon) return;
           
            camDot *= camForwardVDisFDiv *
                offeredScaleFactor;  
 
            transform.localScale = Vector3.one *Mathf.Max(1, camDot);
            transform.rotation = Quaternion.LookRotation(camForwardDir);
            
        }
    }
}