Shader "Voxelize" 
{
	Properties 
	{
	_Emit ("Sound Emission", Vector) = (0,0,0,0)
	}

	SubShader 
	{
		Pass
		{
			zwrite off ztest always cull off
			
			Tags { "RenderType"="Opaque" }
			LOD 200
		
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 

				// **************************************************************
				// Data structures												*
				// **************************************************************
				struct GS_INPUT
				{
					float4	pos		: POSITION;

				};

				struct FS_INPUT
				{
					float4	pos		: POSITION;
				
					float4 worldPos : texcoord0;
					
				};


				// **************************************************************
				// Vars															*
				// **************************************************************

				float4x4 zMVP;
				RWTexture3D<float> Media : register(u1);
				float4 _Emit;
				
				// **************************************************************
				// Shader Programs												*
				// **************************************************************

				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main(appdata_base v)
				{
					GS_INPUT output = (GS_INPUT)0;

					output.pos =  mul(_Object2World,v.vertex);

					return output;
				}



				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(3)]
				void GS_Main(triangle GS_INPUT p[3], inout TriangleStream<FS_INPUT> triStream)
				{
				
					//calculate normal
					float3 normal = cross( (p[1].pos.xyz - p[0].pos.xyz ), (p[2].pos.xyz - p[0].pos.xyz) );
					
					float xDot = abs( dot(normal,float3(1,0,0)));
					float yDot = abs( dot(normal,float3(0,1,0)));
					float zDot = abs( dot(normal,float3(0,0,1)));

					FS_INPUT pIn;
					float4 p0world = p[0].pos;
					float4 p1world = p[1].pos;
					float4 p2world = p[2].pos;
					

					if(xDot > yDot && xDot > zDot){
					
						p[0].pos = p[0].pos.yzxw;
						p[1].pos = p[1].pos.yzxw;
						p[2].pos = p[2].pos.yzxw;
					}
					else if(yDot > xDot && yDot > zDot){
						p[0].pos = p[0].pos.zxyw;
						p[1].pos = p[1].pos.zxyw;
						p[2].pos = p[2].pos.zxyw;
					
					}
					
					pIn.pos = mul(zMVP, p[0].pos);
					pIn.worldPos = p0world;
					triStream.Append(pIn);

					pIn.pos =  mul(zMVP, p[1].pos);
					pIn.worldPos = p1world;
					triStream.Append(pIn);

					pIn.pos =  mul(zMVP, p[2].pos);
					pIn.worldPos = p2world;
					triStream.Append(pIn);

				}

				float4 FS_Main(FS_INPUT input) : COLOR
				{
					//128 128 128 -> 128 128 128
					// 144, 144, 144 -> 256, 256, 256
					// 128 + ( (world pos - 128 ) * 128/16 )
					// 112-> 0
					//float3 relativePos = 128 + ((input.worldPos.xyz - 128) * 128 / 16);

					Media[uint3(input.worldPos.xyz)] = 10;
					
					discard;
					return float4(0,0,0,0);
				}

			ENDCG
		}
	} 
}
