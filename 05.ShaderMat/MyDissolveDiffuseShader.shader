Shader "Custom/MyDissolveDiffuseShader" {
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex("Texture", 2D) = "white" {}
		_EdgeColour1("Edge colour 1", Color) = (1.0, 1.0, 1.0, 1.0)
		_EdgeColour2("Edge colour 2", Color) = (1.0, 1.0, 1.0, 1.0)
		_Level("Dissolution level", Range(0.0, 1.0)) = 0.1
		_Edges("Edge width", Range(0.0, 1.0)) = 0.1
		TempColor("tempCol",Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags {
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 
		}

		Pass
		{
			Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float4 localPos : TEXCOORD4;
			};

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			float4 _EdgeColour1;
			float4 _EdgeColour2;
			float _Level;
			float _Edges;
			float4 _MainTex_ST;

			half4 _LightColor0;//전역 조명 빛.
			half4 TempColor;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;
				o.localPos = v.vertex;

				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float cutout = tex2D(_NoiseTex, i.uv).r;

				if (cutout < _Level)
					discard;

				fixed4 originColor = tex2D(_MainTex, i.uv);
				float3 lightVNormalize = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, i.localPos));	//라이팅 벡터 (카메라 벡터로 갈음)
				float3 nvNormalize = normalize(mul(unity_ObjectToWorld, i.normal));	//월드공간 노말벡터.
				float dotLN = dot(nvNormalize, lightVNormalize);
				originColor = originColor * max(0, dotLN)* _LightColor0 * 2;

				if (cutout < _Level + _Edges)
				{
					return lerp(_EdgeColour1, _EdgeColour2, smoothstep(_Level, _Level + _Edges, cutout));
				}
				return originColor;
			}
		ENDCG
		}
	}
}