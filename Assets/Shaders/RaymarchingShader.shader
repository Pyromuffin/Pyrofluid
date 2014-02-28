Shader "Raymarching" {
	Properties {
		
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 200
		
		Pass {
			cull front ZTest always zwrite off
			 Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

				#pragma target 5.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
			
				struct v2f {
					float4 pos : SV_POSITION;
					float3 worldPos : texcoord0;
					float4 screenPos : texcoord1;
				};
			
			
				v2f vert(appdata_base v) {
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.worldPos = mul(_Object2World, v.vertex);
					o.screenPos = ComputeScreenPos(o.pos);
					return o;
				}

				

			
				float4 screenCorner;
				float4 cameraUp;
				float4 cameraRight;
				float StepSize;
				float4 cameraWorldSize;
				float worldSize;
				float3 simulationTextureSize;
				float4 cameraWorldPosition;

				sampler3D Current;
				sampler2D _CameraDepthTexture;

			
				float4 frag(v2f IN) : COLOR {
				    
					float2 uv = IN.screenPos.xy/IN.screenPos.w;
				    float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, uv));
					
				    float3 front = screenCorner.xyz + (uv.x * cameraWorldSize.x * cameraRight.xyz) + (uv.y * cameraWorldSize.y * cameraUp.xyz);
				    float3 back = IN.worldPos.xyz;
				 
				    float3 dir = normalize(front - cameraWorldPosition);
				    float4 pos = float4(front, 0);
				 
				    float4 dst = float4(0, 0, 0, 0);
				    float4 src = 0;
				 
				    float3 value = float3(0,0,0);
				 
				    float3 Step = dir * StepSize; 
					
					
				    for(int i = 0; i < 100; i++)
				    {
				        
						
						
				        value = abs(tex3Dlod(Current, float4(pos.xyz/128,0) ).rgb);
					
						src = float4(value,length(value) ) * 10;
					
						//src.a *= .1f; //reduce the alpha to have a more transparent result 
				         
				        //Front to back blending
				        // dst.rgb = dst.rgb + (1 - dst.a) * src.a * src.rgb
				        // dst.a   = dst.a   + (1 - dst.a) * src.a     
				        src.rgb *= src.a;
				        dst = (1.0f - dst.a)*src + dst;     
				     
				        //break from the loop when alpha gets high enough
				        if(dst.a >= .95)
				            break; 
					
				     
				        //advance the current position
				        pos.xyz += Step;
						
						if(length(pos - front) > depth)
							break;
				     
				        //break if the position is greater than <1, 1, 1>
				    
				    }
					
					return dst;
				}

			
			ENDCG
		}
	}
}