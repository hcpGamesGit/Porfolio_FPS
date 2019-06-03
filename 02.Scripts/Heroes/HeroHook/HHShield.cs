using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class HHShield : MonoBehaviour
    {
        [SerializeField]
        Color myTeamColor;
        [SerializeField]
        Color enemyTeamColor;
        [SerializeField]
        Collider coll;
        Material mat;

        [SerializeField]
        float activateTime;
        [SerializeField]
        float showEffectTime;

        private void Awake()
        {
            Renderer render = GetComponent<Renderer>();
            mat = new Material(render.material);
            render.material = mat;
        }
        public void SetTeam(string teamName, bool myTeamShield)
        {
            gameObject.tag = teamName;
            mat.SetColor("BarriorColor", myTeamShield? myTeamColor: enemyTeamColor);
        }
        public void Activate(Vector3 heroPosition)
        {
            coll.enabled = false;
            transform.position = heroPosition;
           
            StartCoroutine(ActivateEffect());
        }
        IEnumerator ActivateEffect()
        {
            DissolveToggle(true);
            WaveToggle(false);
            float startTime = 0f;
            float startPlanePointY = transform.position.y - (transform.localScale.y / 2);
            Vector3 planePoint = new Vector3(0, startPlanePointY, 0);
            SetPlanePoint(planePoint);
            
            float toLastPlanePointYDiv =(transform.localScale.y + mat.GetFloat("DissolveRange")) / showEffectTime  ;

            while (startTime < showEffectTime)
            {
                startTime += Time.deltaTime;
                planePoint.y = startPlanePointY + (toLastPlanePointYDiv * startTime);
                SetPlanePoint(planePoint);
                yield return null;
            }
            DissolveToggle(false); 
            mat.SetFloat("WaveValueForStartFromPole", Time.timeSinceLevelLoad);
            WaveToggle(true);
            coll.enabled = true;

            yield return new WaitForSeconds(activateTime);

            startTime = 0f;
            DissolveToggle(true);
            WaveToggle(false);
            coll.enabled = false;
            float topPlanePointY = startPlanePointY + toLastPlanePointYDiv * showEffectTime;

            while (startTime < showEffectTime)
            {
                startTime += Time.deltaTime;
                planePoint.y = topPlanePointY - (toLastPlanePointYDiv * startTime);
                SetPlanePoint(planePoint);
                yield return null;
            }
          Destroy(gameObject);
        }
        
        void SetPlanePoint(Vector3 point)
        {
            mat.SetVector("PlanePoint", point);
        }
        void DissolveToggle(bool dissolve)
        {
            if (dissolve)
                mat.SetFloat("DissolveToggle", 1f);
            else
                mat.SetFloat("DissolveToggle", 0f);
        }
        void WaveToggle(bool wave)
        {
            if (wave)
                mat.SetFloat("WaveToggle", 1f);
            else
                mat.SetFloat("WaveToggle", 0f);
        }
    }
}