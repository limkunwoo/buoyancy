// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/WaterShader"
{
    Properties
    {
		_Bumpmap("NormalMap",2D) = "bump" {}
		_Cube("Cube",Cube) = ""{}
		_Color("Color",color) = (1,1,1,1)
		_SPColor("Specular Color",color) = (1,1,1,1)
		_SPPower("Specular Power", Range(50,300)) = 150
		_SPMulti("Specular Multiply", Range(1,10)) = 3
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Transparent"}
		LOD 200


		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf WaterSpecular vertex:vert alpha:fade fullforwardshadows
		samplerCUBE _Cube;
		sampler2D _Bumpmap;

			struct Input
			{
				float2 uv_Bumpmap;
				float3 worldRefl;
				float3 viewDir;

				INTERNAL_DATA
			};


			float _WaterScale;
			float _WaterSpeed;
			float _WaterTime;
			float _WaterLength;
			float _Phase;
			
			float4 _Color;
			float4 _SPColor;
			float _SPPower;
			float _SPMulti;

			void vert(inout appdata_full IN)
			{
				float4 worldPos = mul(unity_ObjectToWorld, IN.vertex);
				float ypos = worldPos.y;
				ypos += _WaterScale * sin((_WaterTime / _WaterSpeed - worldPos.x / _WaterLength) + _Phase);
				
				ypos += _WaterScale * sin((_WaterTime / _WaterSpeed*2 - (worldPos.x + worldPos.z)/ _WaterLength*0.5) + 5.0f);
				ypos += _WaterScale * sin((_WaterTime / _WaterSpeed / 5 + (worldPos.x + 0.3f*worldPos.z) / _WaterLength * 0.2) + 15.0f);
				ypos += _WaterScale * sin((_WaterTime / _WaterSpeed - ( 2 * worldPos.x - worldPos.z) / _WaterLength) + 7.0f);
				ypos += 1.5f * _WaterScale * sin((_WaterTime / _WaterSpeed - (worldPos.x +  5.0f * worldPos.z) / _WaterLength) + 10.0f);
				ypos += _WaterScale * sin((_WaterTime / _WaterSpeed - (worldPos.x - worldPos.z) / _WaterLength/2) + 1.0f);
				

				worldPos.y = ypos;
				float4 localPos = mul(unity_WorldToObject, float4(worldPos));
				IN.vertex = localPos;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				float3 normal1 = UnpackNormal(tex2D(_Bumpmap, IN.uv_Bumpmap + _Time.x * 0.1f));
				float3 normal2 = UnpackNormal(tex2D(_Bumpmap, IN.uv_Bumpmap - _Time.x * 0.1f));
				o.Normal = (normal1 + normal2) / 2;

				float3 refcolor = texCUBE(_Cube, WorldReflectionVector(IN, o.Normal));

				float rim = saturate(dot(o.Normal, IN.viewDir));
				rim = pow(1 - rim, 1.5);

				o.Emission = refcolor * _Color.rgb * rim * 2;
				o.Alpha = saturate(rim + 0.5);
			}

			float4 LightingWaterSpecular(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten) {
				float3 H = normalize(lightDir + viewDir);
				float spec = saturate(dot(H,s.Normal));
				spec = pow(spec, _SPPower);

				float4 finalColor;
				finalColor.rgb = spec * _SPColor.rgb * _SPMulti;
				finalColor.a = s.Alpha + spec;

				return finalColor;
			}
			ENDCG
	}
    FallBack "Diffuse"
}
