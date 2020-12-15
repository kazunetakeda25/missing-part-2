// Marmoset Skyshop
// Copyright 2013 Marmoset LLC
// http://marmoset.co

#ifndef MARMOSET_SKIN_CGINC
#define MARMOSET_SKIN_CGINC

float4 _SubdermisColor;
float  _NormalSmoothing;
#ifdef MARMO_DETAIL
	float  _DetailWeight;
	float4 _DetailTiling;
	sampler2D _DetailMap;
#endif
sampler2D _SubdermisTex;

float _Subdermis;
float _Translucency;
float _TranslucencySky;
float _Fuzz;	
float _Aniso;
float _AnisoDir;

float4 _TranslucencyColor;
sampler2D _TranslucencyMap;

float4	_FuzzColor;
float 	_FuzzScatter;
float 	_FuzzOcc;

inline float diffuseFresnel(float eyeDP, float scatter)
{
	eyeDP = 1.0 - eyeDP;
	float dp4 = eyeDP * eyeDP; dp4 *= dp4;
	return lerp(dp4, eyeDP*0.4, scatter);		//0.4 is energy conserving integral
}

inline float3 wrapLighting(float DP, float3 scatter) {
	scatter *= 0.5;
	float3 integral = float3(1.0,1.0,1.0)-scatter;
	float3 light = saturate(DP*(float3(1.0,1.0,1.0)-scatter) + scatter);
	float shadow = (DP*0.5+0.5);
	shadow *= shadow;
	return light * integral * shadow;
}

inline float3 wrapLightingSH(float3 band0, float3 band1, float3 band2, float3 scatter) {
	return SHConvolve(band0,band1,band2,scatter) * (4.0/(3*3.14159));
}

inline half4 LightingMarmosetSkinDirect( MarmosetSkinOutput s, half3 lightDir, half3 viewDir, half atten ) {
	half4 frag = half4(0.0,0.0,0.0,s.Alpha);
	#if defined(MARMO_DIFFUSE_DIRECT) || defined(MARMO_SPECULAR_DIRECT)
		half3 L = lightDir;
		half3 N = s.Normal;
		#ifdef MARMO_HQ
			L = normalize(L);
		#endif
	#endif
	
	float selfShadow = 1.0;
	#ifdef MARMO_DIFFUSE_DIRECT
		half dp = dot(N,L);
		half3 diff = wrapLighting(dp, s.Subdermis); //*2.0 to match Unity
		diff *= s.Albedo.rgb * 2.0;
		frag.rgb = diff * _LightColor0.rgb;		
		selfShadow = saturate(10.0*dp);
	#else
		selfShadow = saturate(10.0*dot(N,L));
	#endif
	
	float theta = _AnisoDir * 3.14159 / 180.0;
	float3 T = float3(cos(theta),sin(theta),0.0);
	
	#ifdef MARMO_SPECULAR_DIRECT
		half3 H = viewDir+L;
		H = H - T*dot(H,T)*_Aniso;
		H = normalize(H);
		float specRefl = saturate(dot(N,H));
		half3 spec = pow(specRefl, s.Specular*256.0);
		#ifdef MARMO_HQ
			spec *= selfShadow;			
		#endif
		spec *= _LightColor0.rgb;
		frag.rgb += (0.5 * spec) * s.SpecularRGB; //*0.5 to match Unity
	#endif

	diff *= s.Albedo.rgb * 2.0;
	
	#ifdef MARMO_SKIN_DIRECT
		//TRANSLUCENCY
		float transdp = dot(-N,L);
		half3 trans = wrapLighting(transdp, float3(1.0,1.0,1.0)); //*2.0 to match Unity
		frag.rgb += s.Translucency * trans;

		//PEACH-FUZZ
		float fuzzdp = saturate(dp*_FuzzOcc + (1.0-_FuzzOcc));
		frag.rgb += s.Fuzz * _FuzzColor * fuzzdp;
	#endif
	frag.rgb *= atten;
	return frag;
}

void MarmosetSkinVert (inout appdata_full v, out Input o) {
	UNITY_INITIALIZE_OUTPUT(Input,o);
	#ifdef MARMO_OCCLUSION
		o.texcoord1 = v.texcoord1.xy;
	#endif
	#ifdef MARMO_VERTEX_COLOR
		o.color = v.color;
	#endif
}

void MarmosetSkinSurf(Input IN, inout MarmosetSkinOutput OUT) {
	#define uv_diff IN.uv_MainTex
	#define uv_spec IN.uv_MainTex
	#define uv_bump IN.uv_MainTex
	#define uv_glow IN.uv_MainTex
	#define uv_occ  IN.texcoord1
	#define uv_subdermis IN.uv_MainTex
	
	#ifdef MARMO_DETAIL
		float4 uv_detail = float4(0.0,0.0,0.0,0.0);
		uv_detail.xy = uv_bump.xy * _DetailTiling.xy + _DetailTiling.zw;
	#endif
	
	ExposureIBL.xy *= UniformOcclusion.xy;
	#if LIGHTMAP_ON
		ExposureIBL.xy *= ExposureLM;
	#endif
	half4 baseColor = _Color;
	#ifdef MARMO_VERTEX_COLOR
		baseColor *= IN.color;
	#endif

	#ifdef MARMO_DIFFUSE_SPECULAR_COMBINED
		half4 diffspec = half4(1.0,1.0,1.0,1.0);
	#endif
		
	//DIFFUSE
	#if defined(MARMO_DIFFUSE_DIRECT) || defined(MARMO_DIFFUSE_IBL)
		half4 diff = tex2D( _MainTex, uv_diff );
		#ifdef MARMO_DIFFUSE_SPECULAR_COMBINED
			diffspec = diff.aaaa;
		#endif
		diff *= baseColor;
		//camera exposure is built into OUT.Albedo
		diff.rgb *= ExposureIBL.w;
		OUT.Albedo = diff.rgb;
		OUT.Alpha = diff.a;
		#ifdef MARMO_PREMULT_ALPHA
			OUT.Albedo *= diff.a;
		#endif
	#else
		OUT.Albedo = baseColor.rgb;
		OUT.Alpha = 1.0;
	#endif
	
	//AMBIENT OCC
	#if defined(MARMO_VERTEX_OCCLUSION) || defined(MARMO_OCCLUSION)
		half4 occ = half4(1.0,1.0,1.0,1.0);
		#ifdef MARMO_OCCLUSION
			occ = tex2D(_OccTex, uv_occ);
		#endif
		#ifdef MARMO_VERTEX_OCCLUSION
			occ.rg *= IN.color.rg;
		#endif
		occ = lerp(half4(1.0,1.0,1.0,1.0),occ, _OccStrength);
		//TODO: occlude lightprobe SH by diffuse AO
		ExposureIBL.xy *= occ.rg;
	#endif

	//SKIN
	#if defined(MARMO_SKIN_IBL) || defined(MARMO_SKIN_DIRECT)		
		//SUBDERMIS
		float4 subdermis = tex2D(_SubdermisTex, uv_subdermis);
		subdermis *= _SubdermisColor;
		float skinMask = subdermis.a;
		subdermis.rgb *= _Subdermis;
		OUT.Subdermis = subdermis.rgb * skinMask;
	#endif
	
	//NORMALS	
	#ifdef MARMO_NORMALMAP
		float3 localN = UnpackNormal(tex2D(_BumpMap, uv_bump));
		
		float bias = _NormalSmoothing * 4.0;
		float4 uv_blurred = float4(uv_bump.x, uv_bump.y, bias, bias);
		float3 smoothN = UnpackNormal(tex2Dbias(_BumpMap, uv_blurred));
		
		#ifdef MARMO_DETAIL
			float3 detailN = UnpackNormal(tex2D(_DetailMap, uv_detail.xy));
			float detailOcc = dot(detailN, localN);
			localN += detailN * _DetailWeight * skinMask;
			float3 smoothDetailN = UnpackNormal(tex2Dbias(_DetailMap, uv_detail));
			smoothN.xy += smoothDetailN.xy * _DetailWeight;
		#endif
	
		//localN and viewDir are in tangent-space
		localN = normalize(localN);
		smoothN = normalize(smoothN);
		OUT.Normal = localN;
		float3 worldN = WorldNormalVector(IN, localN);
		smoothN = WorldNormalVector(IN, smoothN);		
	#else
		float3 worldN =  IN.worldNormal;		
		#ifdef MARMO_HQ
			worldN = normalize(worldN);
		#endif
		#if !defined(MARMO_NORMALMAP) && defined(UNITY_PASS_PREPASSFINAL)
			float3 localN = float3(0.0,0.0,1.0);
			//localN and viewDir are in tangent-space
		#else
			float3 localN = worldN;
			//localN and viewDir are in world-space
		#endif
		float3 smoothN = localN;
	#endif
		
	float3 localE = IN.viewDir;
	#ifdef MARMO_HQ
		localE = normalize(localE);
	#endif

	//SPECULAR
	#if defined(MARMO_SPECULAR_DIRECT) || defined(MARMO_SPECULAR_IBL)
		#ifdef MARMO_DIFFUSE_SPECULAR_COMBINED
			half4 spec = diffspec;
		#else
			half4 spec = tex2D(_SpecTex, uv_spec);
		#endif

		#ifdef MARMO_DETAIL
			spec *= lerp(1.0, detailOcc*detailOcc, skinMask);
		#endif

		#ifdef MARMO_HQ
			half fresnel = splineFresnel(localN, localE, _SpecInt, _Fresnel);
		#else
			half fresnel = fastFresnel(localN, localE, _SpecInt, _Fresnel);		
		#endif
			
		//camera exposure is built into OUT.Specular
		spec.rgb *= _SpecColor.rgb;

		//filter the light that reaches diffuse reflection by specular intensity		
		#ifdef MARMO_SPECULAR_FILTER
			//Light reaching diffuse is filtered by 1-specColor*specIntensity
			half3 specFilter = half3(1.0,1.0,1.0) - spec.rgb * _SpecInt;
			
			//If the material exhibits strong fresnel, bias the filter some.
			specFilter += _Fresnel.xxx*0.5;
			
			//don't let it get t crazy, clamp 0-1 and apply
			OUT.Albedo *= saturate(specFilter);
		#endif
		
		spec.rgb *= fresnel * ExposureIBL.w;
		half glossLod = glossLOD(spec.a, _Shininess);		
		#ifdef MARMO_SPECULAR_DIRECT
			OUT.SpecularRGB = spec.rgb;
			OUT.Specular = glossExponent(glossLod);
			//conserve energy by dividing out specular integral		
			OUT.SpecularRGB *= specEnergyScalar(OUT.Specular);
			OUT.Specular *= 0.00390625; // 1/256
		#endif
	#endif

	#if defined(MARMO_SKIN_IBL) || defined(MARMO_SKIN_DIRECT)		
		//TRANSLUCENCY
		float4 trans = tex2D(_TranslucencyMap, uv_subdermis);
		OUT.Translucency = _Translucency * _TranslucencyColor.rgb * trans.rgb * skinMask;

		//PEACH-FUZZ
		float eyeDP = dot(localE, localN);
		OUT.Fuzz = _Fuzz * diffuseFresnel(eyeDP, _FuzzScatter) * skinMask;		
		#ifdef MARMO_SPECULAR_IBL
			OUT.Fuzz *= 1.0-saturate(OUT.Specular);
		#endif
	#endif
	
	//SPECULAR IBL
	#ifdef MARMO_SPECULAR_IBL
		float3 skyR = WorldReflectionVector(IN, localN);
		#ifdef MARMO_SKY_ROTATION
			skyR = mulVec3(SkyMatrix,skyR); //per-fragment matrix multiply, expensive
		#endif
		#ifdef MARMO_MIP_GLOSS
			half3 specIBL = glossCubeLookup(_SpecCubeIBL, skyR, glossLod);
		#else
			half3 specIBL =  specCubeLookup(_SpecCubeIBL, skyR)*spec.a;
		#endif
		OUT.Emission += specIBL.rgb * spec.rgb * ExposureIBL.y;
	#endif
	
	//DIFFUSE IBL
	#ifdef MARMO_DIFFUSE_IBL
		#ifdef MARMO_SKIN_IBL
			worldN = lerp(worldN, smoothN, skinMask);
		#endif
			float3 skyN = worldN;
		#ifdef MARMO_SKY_ROTATION
			skyN = mulVec3(SkyMatrix,skyN); //per-fragment matrix multiply, expensive
		#endif
			
		#ifdef MARMO_SKIN_IBL
			skyN = normalize(skyN);
			
			//SH DIFFUSE
			//half3 diffIBL = wrapLightingSH(skyN, subdermis.rgb * skinMask) * OUT.Albedo.rgb;
			float3 band0, band1, band2;			
			SHLookup(skyN,band0,band1,band2);
			
			float3 unity0, unity1, unity2;
			SHLookupUnity(worldN,unity0,unity1,unity2);
			band0 += unity0;
			band1 += unity1;
			band2 += unity2;
			
			band0 *= _SHScale;
			band1 *= _SHScale;
			band2 *= _SHScale;
			
			half3 diffIBL = wrapLightingSH(band0, band1, band2, subdermis.rgb*skinMask);
			diffIBL *= OUT.Albedo;
			
			//PEACH FUZZ
			//TODO: fuzz scattering?
			diffIBL += OUT.Fuzz * _FuzzColor.rgb * lightingSH(band0, band1, band2);			

			//TRANSLUCENCY
			SHLookup(-skyN,band0,band1,band2);
			SHLookupUnity(-worldN,unity0,unity1,unity2);			
			band0 += unity0 * _InvHDR;
			band1 += unity1 * _InvHDR;
			band2 += unity2 * _InvHDR;
			diffIBL += lightingSH(band0, band1, band2) * OUT.Translucency * _TranslucencySky;
		#endif
		OUT.Emission += diffIBL * ExposureIBL.x;
	#endif
	
	//GLOW
	#ifdef MARMO_GLOW
		half4 glow = tex2D(_Illum, uv_glow);
		glow.rgb *= _GlowColor.rgb;
		glow.rgb *= _GlowStrength;
		glow.rgb *= ExposureIBL.w;
		glow.a *= _EmissionLM;
		//NOTE: camera exposure is already in albedo from above
		glow.rgb += OUT.Albedo * glow.a;
		OUT.Emission += glow.rgb;
	#endif
	
	#ifndef MARMO_ALPHA
		OUT.Alpha = 1.0;
	#endif
}

#endif