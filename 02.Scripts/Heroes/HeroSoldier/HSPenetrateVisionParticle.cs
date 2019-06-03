using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HSPenetrateVisionParticle : MonoBehaviour {
    [SerializeField]
    float showTime;
    float showTimeDiv;
    [SerializeField]
    [Range(0, 1)]
    float radiusUpDonePercent;
    float radiusUpDonePercentDiv;
    float fadeAwayDiv;
    [SerializeField]
    float scaleUpPerSec;
    public float ScaleUpPerSec { get; set; }

    float time;

    const float maxModelRadius = 0.5f;
    public float MaxModelRadius { get { return maxModelRadius; } }

    Material mat;

    Camera toLookCam;

    bool show;
    public bool Show { get { return show; } }
    
    private void Awake()
    {
        mat = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = mat;
        mat.SetFloat("QuadModelRadius", maxModelRadius);

        showTimeDiv = 1 / showTime;
        radiusUpDonePercentDiv = 1/ radiusUpDonePercent;
        fadeAwayDiv = 1/(1 - radiusUpDonePercent);
        DeActivate();
    }
    public void CalculateScaleUpPerSec()
    {
        scaleUpPerSec = (transform.localScale.x * maxModelRadius) / (radiusUpDonePercent * showTime);
    }
    public void SetShowTime(float newShowTime)
    {
        showTime = newShowTime;
        showTimeDiv = 1 / showTime;
    }

    public void Activate(Camera lookCam,Vector2 scale,bool calcurateScaleUpPerSec)
    {
        show = true;
        if (!lookCam) toLookCam = Camera.main;
        else toLookCam = lookCam;

        transform.localScale = scale;
        gameObject.SetActive(true);
        time = 0f;
        if (calcurateScaleUpPerSec) CalculateScaleUpPerSec();
    }
    public void Activate(Vector3 lookAtPos, Vector2 scale, bool calcurateScaleUpPerSec)
    {
        show = true;
        toLookCam = null;
        transform.LookAt(lookAtPos);

        transform.localScale = scale;
        gameObject.SetActive(true);
        time = 0f;
        if (calcurateScaleUpPerSec) CalculateScaleUpPerSec();
    }
    public void DeActivate()
    {
        mat.SetFloat("QuadModelRadius", 0);
        mat.SetFloat("FadeAway", 1);

        show = false;
        gameObject.SetActive(false);
    }

	void Update () {
        if (!show)
        {
            DeActivate();
            return;
        }
        if (toLookCam)
            transform.LookAt(toLookCam.transform);
        
        time += Time.deltaTime;
        float progress = time * showTimeDiv;
        if (progress <= radiusUpDonePercent)
        {
            mat.SetFloat("QuadModelRadius", maxModelRadius * (progress * radiusUpDonePercentDiv));
        }
        else if (progress <= 1)
        {
            Vector2 scaleUp = transform.localScale;
            scaleUp += new Vector2(scaleUpPerSec * Time.deltaTime, scaleUpPerSec * Time.deltaTime);
            transform.localScale = scaleUp;
            float fadeAway = 1 - progress;
            mat.SetFloat("FadeAway", fadeAway * fadeAwayDiv);
        }
        else
        {
            DeActivate();
        }
    }
}
