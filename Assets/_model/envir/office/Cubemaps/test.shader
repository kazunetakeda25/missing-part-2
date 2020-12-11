Shader "Custom/Reflective BPCEM Diffuse" {
	
	Properties {
		
		//Box Projected Cubemap Environment Mapping
		
		_Color ("Main Color", Color) = (1,1,1,1)
			
			_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
				
			_MainTex ("Base (RGB) RefStrength (A)", 2D) = "white" {}
		
		_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
		
		_BumpMap ("Normalmap", 2D) = "bump" {}
		
		
		
		_EnvBoxStart ("Env Box Start", Vector) = (0, 0, 0)
			
			_EnvBoxSize ("Env Box Size", Vector) = (10, 10, 10)
				
	}
	
	
	
	SubShader {
		
		Tags { "RenderType"="Opaque" }
		
		LOD 300
			
			
			
			CGPROGRAM
				
				#pragma surface surf Lambert
				
				
				
				sampler2D _MainTex;
		
		sampler2D _BumpMap;
		
		samplerCUBE _Cube;
		
		
		
		fixed4 _Color;
		
		fixed4 _ReflectColor;
		
		fixed4 _EnvBoxStart;
		
		fixed4 _EnvBoxSize;
		
		
		
		struct Input {
			
			float2 uv_MainTex;
			
			float2 uv_BumpMap;
			
			float3 worldRefl;
			
			fixed3 worldPos;
			
			float3 worldNormal;
			
			INTERNAL_DATA
			
		};
		
		
		
		void surf (Input IN, inout SurfaceOutput o) {
			
			
			
			
			
			
			
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			
			fixed4 c = tex * _Color;
			
			o.Albedo = c.rgb;
			
			
			
			fixed3 n = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			
			
			
			float3 dir = IN.worldPos - _WorldSpaceCameraPos;                            // pos fragment relativo a pos camera 
			
			float3 worldNorm = IN.worldNormal;
			
			worldNorm.xy -= n;
			
			float3 rdir = reflect (dir, worldNorm);                             // vettore riflesso da normale
			
			
			
			//BPCEM
			
			float3 nrdir = normalize (rdir);                                            // vettore riflesso normalizzato
			
			float3 rbmax = (_EnvBoxStart + _EnvBoxSize - IN.worldPos)/nrdir;            // AABB max value +...
			
			float3 rbmin = (_EnvBoxStart - IN.worldPos)/nrdir;                          // AABB min value +...
			
			float3 rbminmax = (nrdir>0.0f)?rbmax:rbmin;                                 // ...?
			
			float fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);                    // ...?
			
			float3 posonbox = IN.worldPos + nrdir*fa;                                   // ...?
			
			rdir = posonbox - (_EnvBoxStart +_EnvBoxSize/2);                            // ...? - posizione (centro) del box
			
			//PBCEM end
			
			
			
			float3 env = texCUBE (_Cube, rdir);
			
			
			
			float3 luminosity = float3 (0.30, 0.59, 0.11);
			
			float reflectivity = dot(luminosity, env.rgb);
			
			o.Emission = env.rgb *reflectivity *_ReflectColor;
			
			
			
		}
		
		ENDCG
			
	}
	
	
	
	FallBack "Reflective/VertexLit"
		
}