using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class HeroRenderer
    {
        public Renderer[] renderer;
        float[] initRimWidth;
        public HeroRenderer(Renderer[] renderer)
        {
            this.renderer = renderer;
            initRimWidth = new float[renderer.Length];
            for (int i = 0; i < renderer.Length; i++)
            {
                renderer[i].material = new Material(renderer[i].material);
                initRimWidth[i] = renderer[i].material.GetFloat("rimLightRange");
            }
        }
        public void Show(bool show)
        {
            for (int i = 0; i < renderer.Length; i++)
            {
                renderer[i].enabled = show ? true : false;
            }
        }
        public void SetOutLine(float outLineWidth,Color outLineColor)
        {
            for (int i = 0; i < renderer.Length; i++)
            {
                renderer[i].material.SetFloat("outLineWidth", outLineWidth);
                renderer[i].material.SetColor("outLineColor", outLineColor);
            }
        }
        public void SetRimColor(Color rimColor)
        {
            for (int i = 0; i < renderer.Length; i++)
            {
                renderer[i].material.SetColor("rimLightColor", rimColor);
            }
        }
        public void SetOcclude(bool occlude)
        {
            for (int i = 0; i < renderer.Length; i++)
            {
                renderer[i].material.SetFloat("setOccludeVision", occlude? 1f:0f);
            }
        }
        public void PlusRimWidth(float plusAmount)
        {
            for (int i = 0; i < renderer.Length; i++)
            {
                renderer[i].material.SetFloat("rimLightRange",initRimWidth[i]+plusAmount);
            }
        }
    }
}