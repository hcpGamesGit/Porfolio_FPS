Shader "Custom/MyOutlineShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[Toggle]outLineToggle("outLineToggle",Float)=0
		outLineWidth("outline width",Range(0,0.1)) = 1
		outLineColor("outLineColor",Color) = (1,1,1,1)
		occludeColor("occludeColor",Color) = (1,0,0,1)
		[Toggle] setOccludeVision("setOccludeVision",Float) = 0
		[Toggle] rimLightToggle("rimLight",Float) = 0
		rimLightColor("rimLightColor",Color) = (1,1,1,1)
		rimLightRange("rimLightWidth",Range(0,2)) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100
				Pass	//맵핵 패스(투시 비전)
				{
					Name "OccludePass"
					Tags { "Queue" = "Geometry" }
					ZTest Greater
					ZWrite Off

					CGPROGRAM
					#pragma vertex vert            
					#pragma fragment frag

					half4 occludeColor;
					float setOccludeVision;

					float4 vert(float4 pos : POSITION) : SV_POSITION
					{
						float4 viewPos = UnityObjectToClipPos(pos);
						return viewPos;
					}

						half4 frag(float4 pos : SV_POSITION) : COLOR
					{					
						if (!setOccludeVision) discard;
						return occludeColor;
					}

					ENDCG
				}
			
				Pass	//외곽선 패스
				{
				Tags { "Queue" = "Geometry" }
				Cull Front
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				sampler2D _MainTex;
				float outLineWidth;
				half4 outLineColor;
				float outLineToggle;

				struct vi 
				{
				float4 vertex  :POSITION;
				float3 normal  :NORMAL;
				};

				float4 vert(vi input) :SV_POSITION
				{
					float4 vertex=  UnityObjectToClipPos(
								input.vertex
								+ normalize(input.normal)*outLineWidth
								);

					return vertex;
				}

				fixed4 frag(float4 pos : POSITION) : COLOR
				{
					if (!outLineToggle) discard;
					return outLineColor;
				}
				ENDCG
		}
		
						
				Pass	//림라이팅 패스
				{
				Tags { "Queue" = "Geometry" }
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				sampler2D _MainTex;
				float outLineWidth;
				half4 outLineColor;
				float rimLightToggle;
				float4 rimLightColor;
				float rimLightRange;

				struct vi {
					float4 vertex  :POSITION;
					float3 normal  :NORMAL;
					float2 uv : TEXCOORD0;
				};
				struct vo {
					float4 vertex :POSITION;
					float3 normal : NORMAL;
					float2 uv : TEXCOORD0;
					float4 wVertex : TEXCOORD1;
					float4 oVertex :TEXCOORD2;
				};

				vo vert(vi input) 
				{
					vo o;
					o.vertex = UnityObjectToClipPos(input.vertex);
					o.normal = input.normal;
					o.uv = input.uv;
					o.wVertex =mul(unity_ObjectToWorld, input.vertex);
					o.oVertex = input.vertex;
					return o;
				}
				fixed4 frag(vo o) : COLOR
				{
					fixed4 texColor = tex2D(_MainTex, o.uv);
					if (!rimLightToggle) return texColor;

					float3 wNormal = normalize ( mul(unity_ObjectToWorld, o.normal) );
					float3 cameraV = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, o.oVertex));

					float dotForRim = dot(cameraV, wNormal);

					return lerp(rimLightColor, texColor, smoothstep(0, rimLightRange ,  dotForRim));
				}
					ENDCG
				}
		}
		FallBack "Mobile/Diffuse"
}
