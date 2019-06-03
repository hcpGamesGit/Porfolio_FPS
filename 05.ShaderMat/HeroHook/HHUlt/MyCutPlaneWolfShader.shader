Shader "Custom/MyCutPlaneWolfShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		cutPlaneCenterPoint("cutPlaneCenterPoint",Vector)=(1,1,1)
		cutPlaneNormalVector("cutPlaneNormalVector",Vector) = (1,1,1)
		cutPlaneNearColor("cutPlaneNearColor",Color) = (1,1,1,1)
		cutPlaneColorWidth("cutPlaneWidth",Range(0,1))=1

		rimLightingWidth("rimLightingWidth",Range(0,1)) = 1
		rimLightingColor("rimLightingColor",Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
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
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct vo
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float4 oVertex : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 cutPlaneCenterPoint;
			float4 cutPlaneNormalVector;
			float4 cutPlaneNearColor;
			float cutPlaneColorWidth;
			float rimLightingWidth;
			half4 rimLightingColor;
			
			vo vert (vi i)
			{
				vo o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv = i.uv;
				o.normal = i. normal;
				o.oVertex = i.vertex;
				return o;
			}
			
			fixed4 frag (vo vertOut) : COLOR
			{
				half4 col = tex2D(_MainTex, vertOut.uv);
				float4 wPos = mul(unity_ObjectToWorld, vertOut.oVertex);

				float3 nVFromCutCenter = normalize(wPos - cutPlaneCenterPoint);
				float3 cutPlaneNormalV = normalize(cutPlaneNormalVector);

				float dotPosCPlaneNormal = dot(nVFromCutCenter, cutPlaneNormalV);
				if (dotPosCPlaneNormal < 0) discard;

				float3 nVCamV = normalize(_WorldSpaceCameraPos - wPos);

				float dotCam = dot(nVCamV, 
					normalize( mul(unity_ObjectToWorld, vertOut.normal))
				);
				if (max(0, dotCam) < rimLightingWidth)
				{
					col = lerp(rimLightingColor, col, max(0, dotCam) / rimLightingWidth);
				}

				if (dotPosCPlaneNormal > cutPlaneColorWidth)
				{
					return col;
				}
				return lerp(cutPlaneNearColor, col, dotPosCPlaneNormal / cutPlaneColorWidth);
			}
			ENDCG
		}
	}
			FallBack "Mobile/Diffuse"
}
