// Marmoset Skyshop
// Copyright 2013 Marmoset LLC
// http://marmoset.co
using UnityEngine;
using System.Collections;

namespace mset {
	[System.Serializable]
	public class Sky : MonoBehaviour {
		public static mset.Sky activeSky = null;
		public Cubemap diffuseCube = null;
		public Cubemap specularCube = null;
		public Cubemap skyboxCube = null;
		public float masterIntensity = 1f;
		public float skyIntensity = 1f;
		public float specIntensity = 1f;
		public float diffIntensity = 1f;
		public float camExposure = 1f;
		public float specIntensityLM = 1f;
		public float diffIntensityLM = 1f;
		public bool hdrSky = false;
		public bool hdrSpec = false;
		public bool hdrDiff = false;
		public bool showSkybox = true;
		public bool linearSpace = true;
		public bool autoDetectColorSpace = true; //for inspector use
		public bool hasDimensions = false;
		
		public mset.SHEncoding SH = null;
		private Matrix4x4 skyMatrix = Matrix4x4.identity;
		private Vector4 exposures = Vector4.one;
		private Vector2 exposuresLM = Vector2.one;
		private float hdrScale = 1f;
				
		//Skybox material, allocated only if requested
		private Material _skyboxMaterial;
		private Material skyboxMaterial {
			get {
				if(_skyboxMaterial == null) {
					Shader shader = Shader.Find("Hidden/Marmoset/Skybox IBL");
					if(shader) {
						_skyboxMaterial = new Material(shader);
						_skyboxMaterial.name = "Internal IBL Skybox";
					} else {
						Debug.LogError("Failed to create IBL Skybox material. Missing shader?");
					}
				}
				return _skyboxMaterial;
			}
		}

		//A black cubemap texture only allocated if requested
		private Cubemap _blackCube;
		private Cubemap blackCube {
			get {
				if(_blackCube == null) {
					_blackCube = new Cubemap(16, TextureFormat.ARGB32, true);
					for(int f = 0; f < 6; ++f)
						for(int x = 0; x < 16; ++x)
							for(int y = 0; y < 16; ++y) {
								_blackCube.SetPixel((CubemapFace)f, x, y, Color.black);
							}
					_blackCube.Apply(true);
				}
				return _blackCube;
			}
		}
		
		// Public Interface //
		
		public void Apply() {			
			Apply(null); 
		}
		public void Apply(Renderer target) {
			// Binds IBL data, exposure, and a skybox texture globally or to a specific game object
			
			if(this.enabled && this.gameObject.activeInHierarchy) {
				//certain global parameters are only bound on a global basis
				if(target == null) {
					//turn off previously bound sky
					if(mset.Sky.activeSky != null) mset.Sky.activeSky.UnApply();
					mset.Sky.activeSky = this;
					ToggleChildLights(true);
					//don't update exposures during targeted apply, only once a frame at most
					UpdateExposures();					
					ApplySkybox();
					
					//toggle between linear-space (gamma-corrected) and gamma-space (uncorrected) shader permutations
					Shader.DisableKeyword("MARMO_GAMMA");
					Shader.DisableKeyword("MARMO_LINEAR");
					if(linearSpace) Shader.EnableKeyword("MARMO_LINEAR");
					else 			Shader.EnableKeyword("MARMO_GAMMA");
					
					Shader.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
					Shader.DisableKeyword("MARMO_BOX_PROJECTION");
					if(hasDimensions)	Shader.EnableKeyword("MARMO_BOX_PROJECTION");
					else 				Shader.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
				} else {
				#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2)
					Material mat = target.material;					
					mat.DisableKeyword("MARMO_BOX_PROJECTION_OFF");
					mat.DisableKeyword("MARMO_BOX_PROJECTION");
					if(hasDimensions)	mat.EnableKeyword("MARMO_BOX_PROJECTION");
					else 				mat.EnableKeyword("MARMO_BOX_PROJECTION_OFF");
				#endif
				}
				
				ApplyExposures(target);
				ApplyIBL(target);

				//upload the sky transform to the shader
				ApplySkyTransform(target);
				
				
			}
		}
		
		public void ApplySkyTransform()				   { ApplySkyTransform(null); }
		public void ApplySkyTransform(Renderer target) {
			// Binds only the sky transform and bounds globally or to a specific game object.
			// Faster than full Apply in that exposures are not recomputed, lights not toggled, etc.

			if(this.enabled && this.gameObject.activeInHierarchy) {				
				if(target == null) {
					//don't update skyMatrix during targeted apply, only once a frame at most
					UpdateSkyTransform();
					Shader.SetGlobalMatrix("SkyMatrix", skyMatrix);
					Shader.SetGlobalMatrix("InvSkyMatrix", skyMatrix.inverse);
					Shader.SetGlobalVector("SkyPosition", transform.position);
					Shader.SetGlobalVector("_SkySize", 0.5f * transform.localScale);
					Shader.SetGlobalFloat("_UseBoxProjection", hasDimensions ? 1f : 1f);
				} else {
					target.material.SetMatrix("SkyMatrix", skyMatrix);
					target.material.SetMatrix("InvSkyMatrix", skyMatrix.inverse);
					target.material.SetVector("SkyPosition", transform.position);
					target.material.SetVector("_SkySize", 0.5f * transform.localScale);
					target.material.SetFloat("_UseBoxProjection", hasDimensions ? 1f : 1f);
				}
			}
		}
		
		public static void SetUniformOcclusion(Renderer target, float diffuse, float specular) {
			//Sets a custom multiplier on the diffuse and specular intensities from the active Sky.			
			Vector4 occlusion = Vector4.one;
			occlusion.x = diffuse;
			occlusion.y = specular;
			target.material.SetVector("UniformOcclusion", occlusion);
		}
		
		public void ToggleChildLights(bool enable) {
			//Enable/disable all lights that are child objects of this Sky
			//NOTE: this causes scene changes on sky selection, may not be desireable in the editor!
			Light[] lights = this.GetComponentsInChildren<Light>();
			for(int i = 0; i < lights.Length; ++i) {
				lights[i].enabled = enable;
			}
		}
		
		//Private Functions //
		
		private void UnApply() {
			ToggleChildLights(false);
		}
		
		private void UpdateSkyTransform() {
			skyMatrix.SetTRS(transform.position, transform.rotation, Vector3.one);
		}
		
		private void UpdateExposures() {
			//build exposure values for shader, HDR skies need the RGBM expansion constant 6.0 in there
			exposures.x = masterIntensity * diffIntensity;
			exposures.y = masterIntensity * specIntensity;
			exposures.z = masterIntensity * skyIntensity * camExposure; //exposure baked right into skybox exposure
			exposures.w = camExposure;

			//prepare exposure values for gamma correction
			float toLinear = 2.2f;
			float toSRGB = 1f / toLinear;

			hdrScale = 6f;
			if(linearSpace) {
				//HDR scale needs to be applied in linear space
				hdrScale = Mathf.Pow(6f, toLinear);
			} else {
				//Exposure values are treated as being in linear space, but the shader is working in sRGB space.
				//Move exposure into sRGB as well before applying.
				exposures.x = Mathf.Pow(exposures.x, toSRGB);
				exposures.y = Mathf.Pow(exposures.y, toSRGB);
				exposures.z = Mathf.Pow(exposures.z, toSRGB);
				exposures.w = Mathf.Pow(exposures.w, toSRGB);
			}
			//RGBM cubemaps need a scalar added to their exposure
			if(hdrDiff) exposures.x *= hdrScale;
			if(hdrSpec) exposures.y *= hdrScale;
			if(hdrSky)	exposures.z *= hdrScale;

			exposuresLM.x = diffIntensityLM;
			exposuresLM.y = specIntensityLM;
		}

		private void ApplyExposures(Renderer target) {
			if(target == null) {
				Shader.SetGlobalVector("ExposureIBL", exposures);
				Shader.SetGlobalVector("ExposureLM", exposuresLM);
				//this is a hint for the Beast Lightmapper, rendering is unaffected
				Shader.SetGlobalFloat("_EmissionLM", 1f);
				Shader.SetGlobalVector("UniformOcclusion", Vector4.one);
			} else {
				target.material.SetVector("ExposureIBL", exposures);
				target.material.SetVector("ExposureLM", exposuresLM);
			}
		}

		private void ApplyIBL(Renderer target) {
			float SHScale = 1f / hdrScale / Mathf.PI;
			if(target == null) {
				//bind cubemaps
				if(diffuseCube) Shader.SetGlobalTexture("_DiffCubeIBL", diffuseCube);
				else 			Shader.SetGlobalTexture("_DiffCubeIBL", blackCube);
				if(specularCube)Shader.SetGlobalTexture("_SpecCubeIBL", specularCube);
				else 			Shader.SetGlobalTexture("_SpecCubeIBL", blackCube);
				if(skyboxCube) 	Shader.SetGlobalTexture("_SkyCubeIBL", skyboxCube);
				else 			Shader.SetGlobalTexture("_SkyCubeIBL", blackCube);

				//bind spherical harmonics
				if(this.SH != null) {
					for(uint i = 0; i < 9; ++i) {
						Shader.SetGlobalVector("_SH" + i, this.SH.cBuffer[i]);
					}
					Shader.SetGlobalFloat("_SHScale", SHScale);
				}
			} else {
				//bind cubemaps
				if(diffuseCube) target.material.SetTexture("_DiffCubeIBL", diffuseCube);
				else 			target.material.SetTexture("_DiffCubeIBL", blackCube);
				if(specularCube)target.material.SetTexture("_SpecCubeIBL", specularCube);
				else 			target.material.SetTexture("_SpecCubeIBL", blackCube);
				if(skyboxCube)	target.material.SetTexture("_SkyCubeIBL", skyboxCube);
				else 			target.material.SetTexture("_SkyCubeIBL", blackCube);

				//bind spherical harmonics
				if(this.SH != null) {
					for(int i = 0; i < 9; ++i) {
						target.material.SetVector("_SH" + i, this.SH.cBuffer[i]);
					}
					target.material.SetFloat("_SHScale", SHScale);
				}
			}
		}
		
		private void ApplySkybox() {
			Shader.DisableKeyword("MARMO_RGBM");
			Shader.EnableKeyword("MARMO_RGBA");
			//NOTE: this causes scene changes on sky selection, may not be desireable in the editor!
			if(showSkybox) {
				if(RenderSettings.skybox != skyboxMaterial) {
					RenderSettings.skybox = skyboxMaterial;
				}
			} else {
				if(RenderSettings.skybox && RenderSettings.skybox.name == "Internal IBL Skybox") { 
					RenderSettings.skybox = null;
				}
			}
		}

		// Run-Time //		
		private void Reset() {
			skyMatrix = Matrix4x4.identity;
			exposures = Vector4.one;
			exposuresLM = Vector2.one;
			hdrScale = 1f;
					
			diffuseCube = specularCube = skyboxCube = null;
			masterIntensity = skyIntensity = specIntensity = diffIntensity = 1f;
			hdrSky = hdrSpec = hdrDiff = false;
		}
		
		//on enable or activate
		private void OnEnable() {
			//finalize or allocate serialized properties here
			if(SH == null) SH = new mset.SHEncoding();
			SH.copyToBuffer();
		}
		
		//on frame the script is activated, before update		
		private void Start() {
			Apply();
		#if UNITY_ANDROID
			// on mobile devices all gloss levels are discarded
			if( this.specularCube ) {
				this.specularCube.Apply(true);
			}
		#endif
		}
		
		private void Update() {
			if(mset.Sky.activeSky == this) {
				if(transform.hasChanged) {
					ApplySkyTransform();
				}
			}
		}
		
		//script instance is destroyed
		private void OnDestroy() {
			UnityEngine.Object.DestroyImmediate(_skyboxMaterial, false);
			SH = null;
			_skyboxMaterial = null;
			_blackCube = null;
			diffuseCube = null;
			specularCube = null;
			skyboxCube = null;
		}
				
		// Editor Functions //
		
		private void OnDrawGizmos() {
			//The most reliable place to bind shader variables in the editor
			if(mset.Sky.activeSky == null) {
				Apply();
			}
			
			Gizmos.DrawIcon(transform.position, "cubelight.tga", true);
			if(this.hasDimensions) {
				Color c = new Color(0.4f, 0.7f, 1f, 0.333f);
				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.color = c;				
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
		}
		
		private void OnDrawGizmosSelected() {
			//Selected skies can be changed without Inspector input, apply here also
			Apply();
			if(this.hasDimensions) {
				Color c = new Color(0.4f, 0.7f, 1f, 1f);
				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.color = c;				
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
		}
	}
}