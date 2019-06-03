Shader "MyCustom/HSPenetrateVisionWaveParticle"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		CenterPoint("CenterPoint",Vector) = (1,1,1,1)
		WaveColor("WaveColor",Color) = (1,1,1,1)
		EndLineRange("EndLineRange",Range(0,0.1))=0	//모델 공간에서.
		FadeAway("FadeAway",Range(0,1))=0			//전체적으로 Fade Out.
		QuadModelRadius("QuadModelRadius",Range(0,0.5))=0.5
		PowDegree("PowDegree",Float)=0
		DepthDifferMax("DepthDifferMax" , Range(0,1)) = 0.3
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque"  "Queue" = "Transparent"}
			Blend One One
			ZWrite Off
			Cull Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct vi
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct vo
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 oPos : TEXCOORD1;
				float2 screenUV : TEXCOORD2;
				float depth : NORMAL;
			};

			sampler2D _CameraDepthTexture;
			const float QuadModelRadius = 0.5;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float3 CenterPoint;
			float EndLineRange;
			float FadeAway;
			float PowDegree;
			float DepthDifferMax;
			fixed4 WaveColor;
			
			vo vert (vi v)
			{
				vo o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.oPos = v.vertex;

				float frag01z = UnityObjectToViewPos(v.vertex).z*_ProjectionParams.w;
#if UNITY_REVERSED_Z	//하드웨어 적 이슈 대응(뎁스값 플립)
				frag01z = -1 * frag01z;
#endif
				o.depth = frag01z;
				float2 screenUV = ((o.vertex.xy / o.vertex.w) + 1) / 2;//투영 공간인 -1~ +1 공간을 0~1 인 UV 공간으로 스위치
#if UNITY_UV_STARTS_AT_TOP	//uv 플립 대응 https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
				screenUV.y = 1 - screenUV.y;
#endif
				o.screenUV = screenUV;

				return o;
			}
			
			fixed4 frag (vo i) : SV_Target
			{
				float4 clipPos = UnityObjectToClipPos(i.oPos);
				float2 screenUV = ((clipPos.xy / clipPos.w) + 1) / 2;//투영 공간인 -1~ +1 공간을 0~1 인 UV 공간으로 스위치

#if UNITY_UV_STARTS_AT_TOP	//uv 플립 대응 https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
				screenUV.y = 1 - screenUV.y;
#endif
				float screen01Depth = Linear01Depth(tex2D(_CameraDepthTexture, screenUV));
				float depthDiffer = screen01Depth - i.depth;

				float distanceFromCenter = distance(float2(0,0), i.oPos.xy);	//모델 공간에서 중앙에서 부터의 거리를 구해서 사용
				float factor = 1;
				if (distanceFromCenter > QuadModelRadius) return fixed4(0, 0, 0, 0);
				if ((QuadModelRadius - distanceFromCenter) <= EndLineRange) 
				factor = 1 + smoothstep( EndLineRange,0, QuadModelRadius - distanceFromCenter);

				fixed4 col =  lerp(fixed4(0, 0, 0, 0), WaveColor, smoothstep( 0,QuadModelRadius, pow(distanceFromCenter, PowDegree)))
					*PowDegree
					*(1 + distanceFromCenter)
					*factor
					*FadeAway;
					;

				col = lerp(fixed4(0, 0, 0, 0), col, smoothstep(0, _ProjectionParams.w*DepthDifferMax, depthDiffer));

				return col;
			}
			ENDCG
		}
	}
			FallBack "Mobile/Diffuse"
}
