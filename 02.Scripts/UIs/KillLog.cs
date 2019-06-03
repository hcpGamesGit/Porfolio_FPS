using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace hcp {
    public class KillLog : MonoBehaviour {
        [SerializeField]
        Sprite[] heroImages;
        [SerializeField]
        Image killerImage;
        [SerializeField]
        Image victimImage;
        [SerializeField]
        Image killWayImage;
        [SerializeField]
        Text killerName;
        [SerializeField]
        Text victimName;
        [SerializeField]
        float destroyTime = 5f;

        public void SetKillLog(string killerName, E_HeroType killerType, string victimName, E_HeroType victimType)
        {
            if (killerName == null)
            {
                this.killerName.text = "";
                this.killerImage.sprite = heroImages[(int)victimType];
                this.killerImage.color = Color.clear;
            }
            else
            {
                this.killerName.text = killerName;

                this.killerImage.sprite = heroImages[(int)killerType];
            }
            this.victimName.text = victimName;
            this.victimImage.sprite = heroImages[(int)victimType];
            GameObject.Destroy(this.gameObject, destroyTime);
        }
    }
}