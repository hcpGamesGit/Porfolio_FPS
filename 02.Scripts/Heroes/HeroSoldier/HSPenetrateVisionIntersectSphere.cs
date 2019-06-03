using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class HSPenetrateVisionIntersectSphere : MonoBehaviour
    {
        bool activate=false;
        Material mat;
        float scaleUpVelocity;
        float showTime;
        float activateTime;
        
        private void Awake()
        {
            mat = new Material(GetComponent<Renderer>().material);
            GetComponent<Renderer>().material = mat;
        }
        public void Activate(Vector3 position,float scaleUpVelocity,float showTime)
        {
            transform.position = position;
            activate = true;
            this.showTime = showTime;
            this.scaleUpVelocity = scaleUpVelocity;

            activateTime = 0f;

            transform.localScale = Vector2.one;
            gameObject.SetActive(true);
        }
        private void Update()
        {
            if (activate)
            {
                activateTime += Time.deltaTime;
                if (activateTime > showTime)
                {
                    DeActivate();
                    return;
                }
                Vector3 scale = transform.localScale + new Vector3(Time.deltaTime*scaleUpVelocity, Time.deltaTime * scaleUpVelocity, Time.deltaTime * scaleUpVelocity);
                transform.localScale = scale;
            }
        }
        public void DeActivate()
        {
            activate = false;
            transform.localScale = Vector2.zero;
            activateTime = 0f;
            gameObject.SetActive(false);
        }
    }
}