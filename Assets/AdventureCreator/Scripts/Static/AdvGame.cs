/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"AdvGame.cs"
 * 
 *	This script provides a number of static functions used by various game scripts.
 * 
 * 	The "DrawOutline" function is based on Bérenger's code: http://wiki.unity3d.com/index.php/ShadowAndOutline
 * 
 */

using UnityEngine;
#if UNITY_5
using UnityEngine.Audio;
#endif
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class AdvGame : ScriptableObject
	{
		
		public static List<AC.Action> copiedActions = new List<AC.Action>();
		private static References references = null;

		#if UNITY_EDITOR
		private static Texture2D _aaLineTex = null;
		#endif
		
		
		public static string GetSceneName (ChooseSceneBy chooseSceneBy, string sceneName)
		{
			if (chooseSceneBy == ChooseSceneBy.Number)
			{
				return "";
			}
			return sceneName;
		}


		#if UNITY_5
		public static void SetMixerVolume (AudioMixerGroup audioMixerGroup, string parameter, float volume)
		{
			if (audioMixerGroup != null && KickStarter.settingsManager.volumeControl == VolumeControl.AudioMixerGroups)
			{
				float attenuation = ((2f * volume) - (volume * volume) - 1f) * 80f;

				audioMixerGroup.audioMixer.SetFloat (parameter, attenuation);
			}
		}
		#endif

		public static void AssignMixerGroup (AudioSource audioSource, SoundType soundType, bool isSpeech = false)
		{
			#if UNITY_5
			if (audioSource != null && KickStarter.settingsManager.volumeControl == VolumeControl.AudioMixerGroups)
			{
				if (audioSource.outputAudioMixerGroup != null)
				{
					return;
				}

				if (soundType == SoundType.Music)
				{
					if (KickStarter.settingsManager.musicMixerGroup)
					{
						audioSource.outputAudioMixerGroup = KickStarter.settingsManager.musicMixerGroup;
					}
					else
					{
						Debug.LogWarning ("Cannot assign " + audioSource.gameObject.name + " a music AudioMixerGroup!");
					}
				}
				else if (soundType == SoundType.SFX)
				{
					if (KickStarter.settingsManager.sfxMixerGroup)
					{
						audioSource.outputAudioMixerGroup = KickStarter.settingsManager.sfxMixerGroup;
					}
					else
					{
						Debug.LogWarning ("Cannot assign " + audioSource.gameObject.name + " a sfx AudioMixerGroup!");
					}
				}
				else if (isSpeech)
				{
					if (KickStarter.settingsManager.speechMixerGroup)
					{
						audioSource.outputAudioMixerGroup = KickStarter.settingsManager.speechMixerGroup;
					}
					else
					{
						Debug.LogWarning ("Cannot assign " + audioSource.gameObject.name + " a speech AudioMixerGroup!");
					}
				}
			}
			#endif
		}

		
		public static int GetAnimLayerInt (AnimLayer animLayer)
		{
			int layerInt = (int) animLayer;
			
			// Hack, because we actually use two neck layers
			if (layerInt > 4)
			{
				layerInt ++;
			}
			
			return layerInt;
		}
		
		
		public static References GetReferences ()
		{
			if (references == null)
			{
				references = (References) Resources.Load (Resource.references);
			}
			return references;
		}
		
		
		public static RuntimeActionList RunActionListAsset (ActionListAsset actionListAsset)
		{
			return RunActionListAsset (actionListAsset, null, 0, false, true);
		}
		
		
		public static RuntimeActionList SkipActionListAsset (ActionListAsset actionListAsset)
		{
			return RunActionListAsset (actionListAsset, null, 0, true, false);
		}
		
		
		public static RuntimeActionList RunActionListAsset (ActionListAsset actionListAsset, int i, bool addToSkipQueue)
		{
			return RunActionListAsset (actionListAsset, null, i, false, addToSkipQueue);
		}
		
		
		public static RuntimeActionList SkipActionListAsset (ActionListAsset actionListAsset, int i)
		{
			return RunActionListAsset (actionListAsset, null, i, true, false);
		}
		
		
		public static RuntimeActionList RunActionListAsset (ActionListAsset actionListAsset, Conversation endConversation)
		{
			return RunActionListAsset (actionListAsset, endConversation, 0, false, true);
		}
		
		
		public static RuntimeActionList RunActionListAsset (ActionListAsset actionListAsset, Conversation endConversation, int i, bool doSkip, bool addToSkipQueue)
		{
			if (actionListAsset != null && actionListAsset.actions.Count > 0)
			{
				GameObject runtimeActionListObject = (GameObject) Instantiate (Resources.Load (Resource.runtimeActionList));
				RuntimeActionList runtimeActionList = runtimeActionListObject.GetComponent <RuntimeActionList>();
				runtimeActionList.DownloadActions (actionListAsset, endConversation, i, doSkip, addToSkipQueue);
				return runtimeActionList;
			}
			
			return null;
		}
		
		
		public static void DrawCubeCollider (Transform transform, Color color)
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawCube (Vector3.zero, Vector3.one);
		}
		
		
		public static void DrawBoxCollider (Transform transform, Color color)
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawLine (new Vector3 (-0.5f, -0.5f), new Vector3 (-0.5f, 0.5f));
			Gizmos.DrawLine (new Vector3 (-0.5f, 0.5f), new Vector3 (0.5f, 0.5f));
			Gizmos.DrawLine (new Vector3 (0.5f, 0.5f), new Vector3 (0.5f, -0.5f));
			Gizmos.DrawLine (new Vector3 (0.5f, -0.5f), new Vector3 (-0.5f, -0.5f));
		}
		
		
		public static void DrawPolygonCollider (Transform transform, PolygonCollider2D poly, Color color)
		{
			Gizmos.color = color;
			Gizmos.DrawLine (transform.TransformPoint (poly.points [0]), transform.TransformPoint (poly.points [poly.points.Length-1]));
			for (int i=0; i<poly.points.Length-1; i++)
			{
				Gizmos.DrawLine (transform.TransformPoint (poly.points [i]), transform.TransformPoint (poly.points [i+1]));
			}
		}
		
		
		public static double CalculateFormula (string formula)
		{
			#if UNITY_WP8 || UNITY_WINRT
			return 0;
			#else
			return ((double) new System.Xml.XPath.XPathDocument
			        (new System.IO.StringReader("<r/>")).CreateNavigator().Evaluate
			        (string.Format("number({0})", new System.Text.RegularExpressions.Regex (@"([\+\-\*])").Replace (formula, " ${1} ").Replace ("/", " div ").Replace ("%", " mod "))));
			#endif
		}
		
		
		public static string ConvertTokens (string _text)
		{
			if (!Application.isPlaying)
			{
				return _text;
			}
			
			if (_text != null && _text.Contains ("var:"))
			{
				if (_text.Contains ("[var:"))
				{
					foreach (GVar _var in KickStarter.runtimeVariables.globalVars)
					{
						string tokenText = "[var:" + _var.id + "]";
						if (_text.Contains (tokenText))
						{
							_var.Download ();
							_text = _text.Replace (tokenText, _var.GetValue ());
						}
					}
				}
				if (_text.Contains ("[localvar:"))
				{
					foreach (GVar _var in KickStarter.localVariables.localVars)
					{
						string tokenText = "[localvar:" + _var.id + "]";
						if (_text.Contains (tokenText))
						{
							_text = _text.Replace (tokenText, _var.GetValue ());
						}
					}
				}
			}
			
			return _text;
		}
		
		
		#if UNITY_EDITOR
		
		public static void FindObjectWithConstantID (int _constantID)
		{
			string originalScene = EditorApplication.currentScene;
			
			if (EditorApplication.SaveCurrentSceneIfUserWantsTo ())
			{
				// Search scene files for ID
				string[] sceneFiles = GetSceneFiles ();
				foreach (string sceneFile in sceneFiles)
				{
					if (EditorApplication.currentScene != sceneFile)
					{
						EditorApplication.OpenScene (sceneFile);
					}
					
					ConstantID[] idObjects = FindObjectsOfType (typeof (ConstantID)) as ConstantID[];
					if (idObjects != null && idObjects.Length > 0)
					{
						foreach (ConstantID idObject in idObjects)
						{
							if (idObject.constantID == _constantID)
							{
								Debug.Log ("Found Constant ID: " + _constantID + " on '" + idObject.gameObject.name + "' in scene: " + sceneFile);
								EditorGUIUtility.PingObject (idObject.gameObject);
								EditorGUIUtility.ExitGUI ();
								return;
							}
						}
					}
				}
				
				Debug.LogWarning ("Cannot find object with Constant ID: " + _constantID);
				if (EditorApplication.currentScene != originalScene)
				{
					EditorApplication.OpenScene (originalScene);
				}
			}
		}
		
		
		public static string[] GetSceneFiles ()
		{
			List<string> temp = new List<string>();
			
			foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
			{
				if (S.enabled)
				{
					temp.Add(S.path);
				}
			}
			
			return temp.ToArray();
		}


		public static int GlobalVariableGUI (string label, int variableID)
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().variablesManager)
			{
				VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;
				// Create a string List of the field's names (for the PopUp box)
				List<string> labelList = new List<string>();
				
				int i = 0;
				int variableNumber = -1;

				if (variablesManager.vars.Count > 0)
				{
					foreach (GVar _var in variablesManager.vars)
					{
						labelList.Add (_var.label);
						
						// If a GlobalVar variable has been removed, make sure selected variable is still valid
						if (_var.id == variableID)
						{
							variableNumber = i;
						}
						
						i++;
					}
					
					if (variableNumber == -1)
					{
						// Wasn't found (variable was deleted?), so revert to zero
						Debug.LogWarning ("Previously chosen variable no longer exists!");
						variableNumber = 0;
						variableID = 0;
					}
					
					variableNumber = EditorGUILayout.Popup (label, variableNumber, labelList.ToArray());
					variableID = variablesManager.vars [variableNumber].id;
				}
				else
				{
					EditorGUILayout.HelpBox ("No global variables exist!", MessageType.Info);
					variableID = -1;
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No Variables Manager exists!", MessageType.Info);
				variableID = -1;
			}

			return variableID;
		}
		
		
		public static void DrawNodeCurve (Rect start, Rect end, Color color, int offset, bool onSide, bool isDisplayed)
		{
			float endOffset = 0f;
			if (onSide)
			{
				endOffset = ((float) offset)/4f;
			}

			bool arrangeVertically = true;
			if (AdvGame.GetReferences ().actionsManager && AdvGame.GetReferences ().actionsManager.displayActionsInEditor == DisplayActionsInEditor.ArrangedHorizontally)
			{
				arrangeVertically = false;
			}

			Color originalColor = GUI.color;
			GUI.color = color;

			if (arrangeVertically)
			{
				Vector2 endPos = new Vector2 (end.x + end.width / 2f + endOffset, end.y - 8);
				DrawNodeCurve (start, endPos, color, offset, onSide, !arrangeVertically, isDisplayed);
				Texture2D arrow = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/node-arrow.png", typeof (Texture2D));
				GUI.Label (new Rect (endPos.x-5, endPos.y-4, 12, 16), arrow, "Label");
			}
			else
			{
				Vector2 endPos = new Vector2 (end.x - 8f, end.y + 10 + endOffset);
				DrawNodeCurve (start, endPos, color, offset, onSide, !arrangeVertically, isDisplayed);
				Texture2D arrow = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/node-arrow-side.png", typeof (Texture2D));
				GUI.Label (new Rect (endPos.x-4, endPos.y-7, 16, 12), arrow, "Label");
			}

			GUI.color = originalColor;
		}
		
		
		public static void DrawNodeCurve (Rect start, Vector2 end, Color color, int offset, bool fromSide, bool toSide, bool isDisplayed)
		{
			Vector3 endPos = new Vector3(end.x, end.y - 1, 0);

			if (fromSide)
			{
				if (!isDisplayed)
				{
					offset = 0;
				}
				Vector3 startPos = new Vector3(start.x + start.width + 10, start.y + start.height - offset - 4, 0);
				if (!isDisplayed)
				{
					startPos.x -= 10;
				}
				float dist = Mathf.Abs (startPos.y - endPos.y);

				Vector3 startTan = startPos + Vector3.right * Mathf.Min (Mathf.Abs (startPos.x - endPos.x), 200f) / 2f;

				if (toSide)
				{
					Vector3 endTan = endPos + Vector3.left * Mathf.Min (dist, 200) / 2f;
					Handles.DrawBezier (startPos, endPos, startTan, endTan, color, adLineTex, 3);
				}
				else
				{
					Vector3 endTan = endPos + Vector3.down * Mathf.Min (dist, 200) / 2f;
					Handles.DrawBezier (startPos, endPos, startTan, endTan, color, adLineTex, 3);
				}
			}
			else
			{
				Vector3 startPos = new Vector3(start.x + start.width / 2f, start.y + start.height + offset + 2, 0);
				float dist = Mathf.Abs (startPos.y - endPos.y);
				Vector3 startTan = startPos + Vector3.up * Mathf.Min (dist, 200f) / 2f;
				if (endPos.y < startPos.y && endPos.x <= startPos.x && !toSide)
				{
					startTan.x -= Mathf.Min (dist, 200f) / 2f;
				}

				if (toSide)
				{
					Vector3 endTan = endPos + Vector3.left * Mathf.Min (dist, 200f) / 2f;
					Handles.DrawBezier (startPos, endPos, startTan, endTan, color, adLineTex, 3);
				}
				else
				{
					Vector3 endTan = endPos + Vector3.down * Mathf.Min (dist, 200f) / 2f;
					Handles.DrawBezier (startPos, endPos, startTan, endTan, color, adLineTex, 3);
				}
			}
		}


		private static Texture2D adLineTex
		{
			get
			{
				if (!_aaLineTex)
				{
					_aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, true);
					_aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
					_aaLineTex.SetPixel(0, 1, Color.white);
					_aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
					_aaLineTex.Apply();
				}
				return _aaLineTex;
			}
		}
		
		
		public static void DuplicateActionsBuffer ()
		{
			List<AC.Action> tempList = new List<AC.Action>();
			foreach (Action action in copiedActions)
			{
				Action copyAction = Object.Instantiate (action) as Action;
				copyAction.skipActionActual = null;
				tempList.Add (copyAction);
			}
			
			foreach (Action action in tempList)
			{
				action.AfterCopy (tempList);
			}
			
			copiedActions.Clear ();
			copiedActions = tempList;
		}
		
		#endif
		
		public static Vector3 GetScreenDirection (Vector3 originWorldPosition, Vector3 targetWorldPosition)
		{
			Vector3 originScreenPosition = Camera.main.WorldToScreenPoint (originWorldPosition);
			Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint (targetWorldPosition);
			
			Vector3 lookVector = targetScreenPosition - originScreenPosition;
			lookVector.z = lookVector.y;
			lookVector.y = 0;
			
			return (lookVector);
		}
		
		
		public static Vector3 GetScreenNavMesh (Vector3 targetWorldPosition)
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			
			Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint (targetWorldPosition);
			Ray ray = Camera.main.ScreenPointToRay (targetScreenPosition);
			RaycastHit hit = new RaycastHit();
			
			if (settingsManager && Physics.Raycast (ray, out hit, settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.navMeshLayer)))
			{
				return hit.point;
			}
			
			return targetWorldPosition;
		}
		
		
		public static Vector2 GetMainGameViewSize ()
		{
			if (Application.isPlaying)
			{
				return new Vector2 (Screen.width, Screen.height);
			}

			#if UNITY_EDITOR
			System.Type T = System.Type.GetType("UnityEditor.GameView, UnityEditor");
			System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod ("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			System.Object Res = GetSizeOfMainGameView.Invoke (null,null);
			return (Vector2) Res;
			#else
			return new Vector2 (Screen.width, Screen.height);
			#endif
		}
		
		
		public static Matrix4x4 SetVanishingPoint (Camera _camera, Vector2 perspectiveOffset)
		{
			Matrix4x4 m = _camera.projectionMatrix;
			float w = 2f * _camera.nearClipPlane / m.m00;
			float h = 2f * _camera.nearClipPlane / m.m11;
			
			float left = -(w / 2) + perspectiveOffset.x;
			float right = left + w;
			float bottom = -(h / 2) + perspectiveOffset.y;
			float top = bottom + h;
			
			return (PerspectiveOffCenter (left, right, bottom, top, _camera.nearClipPlane, _camera.farClipPlane));
		}
		
		
		private static Matrix4x4 PerspectiveOffCenter (float left, float right, float bottom, float top, float near, float far)
		{
			float x =  (2f * near) / (right - left);
			float y =  (2f * near) / (top - bottom);
			float a =  (right + left) / (right - left);
			float b =  (top + bottom) / (top - bottom);
			float c = -(far + near) / (far - near);
			float d = -(2f * far * near) / (far - near);
			float e = -1f;
			
			Matrix4x4 m = new Matrix4x4();
			m[0,0] = x;		m[0,1] = 0f;	m[0,2] = a;		m[0,3] = 0f;
			m[1,0] = 0f;	m[1,1] = y;		m[1,2] = b;		m[1,3] = 0f;
			m[2,0] = 0f;	m[2,1] = 0f;	m[2,2] = c;		m[2,3] =   d;
			m[3,0] = 0f;	m[3,1] = 0f;	m[3,2] = e;		m[3,3] = 0f;
			return m;
		}
		
		
		public static string UniqueName (string name)
		{
			if (GameObject.Find (name))
			{
				string newName = name;
				
				for (int i=2; i<20; i++)
				{
					newName = name + i.ToString ();
					
					if (!GameObject.Find (newName))
					{
						break;
					}
				}
				
				return newName;
			}
			else
			{
				return name;
			}
		}
		
		
		public static string GetName (string resourceName)
		{
			int slash = resourceName.IndexOf ("/");
			string newName;
			
			if (slash > 0)
			{
				newName = resourceName.Remove (0, slash+1);
			}
			else
			{
				newName = resourceName;
			}
			
			return newName;
		}
		
		
		public static Rect GUIBox (float centre_x, float centre_y, float size)
		{
			Rect newRect;
			newRect = GUIRect (centre_x, centre_y, size, size);
			return (newRect);
		}
		
		
		public static Rect GUIRect (float centre_x, float centre_y, float width, float height)
		{
			Rect newRect;
			newRect = new Rect (Screen.width * centre_x - (Screen.width * width)/2, Screen.height * centre_y - (Screen.width * height)/2, Screen.width * width, Screen.width * height);
			return (newRect);
		}
		
		
		public static Rect GUIBox (Vector2 posVector, float size)
		{
			Rect newRect;
			newRect = GUIRect (posVector.x / Screen.width, (Screen.height - posVector.y) / Screen.height, size, size);
			return (newRect);
		}
		
		
		public static void AddAnimClip (Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, Transform mixingBone)
		{
			if (clip != null)
			{
				// Initialises a clip
				_animation.AddClip (clip, clip.name);
				
				if (mixingBone != null)
				{
					_animation [clip.name].AddMixingTransform (mixingBone);
				}
				
				// Set up the state
				_animation [clip.name].layer = layer;
				_animation [clip.name].normalizedTime = 0f;
				_animation [clip.name].blendMode = blendMode;
				_animation [clip.name].wrapMode = wrapMode;
				_animation [clip.name].enabled = true;
			}
		}
		
		
		public static void PlayAnimClipFrame (Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, float fadeTime, Transform mixingBone, float normalisedFrame)
		{
			// Initialises and plays the last frame of a clip
			
			if (clip != null)
			{
				AddAnimClip (_animation, layer, clip, blendMode, wrapMode, mixingBone);
				_animation [clip.name].normalizedTime = normalisedFrame;
				_animation [clip.name].speed *= 1f;
				_animation.Play (clip.name);
				CleanUnusedClips (_animation);
			}
		}
		
		
		public static void PlayAnimClip (Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, float fadeTime, Transform mixingBone, bool reverse)
		{
			// Initialises and crossfades a clip
			
			if (clip != null)
			{
				AddAnimClip (_animation, layer, clip, blendMode, wrapMode, mixingBone);
				if (reverse)
				{
					_animation[clip.name].speed *= -1f;
				}
				_animation.CrossFade (clip.name, fadeTime);
				CleanUnusedClips (_animation);
			}
		}
		
		
		public static void CleanUnusedClips (Animation _animation)
		{
			// Remove any non-playing animations
			List <string> removeClips = new List <string>();
			
			foreach (AnimationState state in _animation)
			{
				if (!_animation [state.name].enabled)
				{
					// Queued animations get " - Queued Clone" appended to it, so remove
					
					int queueIndex = state.name.IndexOf (" - Queued Clone");
					
					if (queueIndex > 0)
					{
						removeClips.Add (state.name.Substring (0, queueIndex));
					}
					else
					{
						removeClips.Add (state.name);
					}
				}
			}
			
			foreach (string _clip in removeClips)
			{
				_animation.RemoveClip (_clip);
			}
		}
		
		
		public static float Lerp (float from, float to, float t)
		{
			// Like regular Lerp, but t>1 gives an overshoot
			if (t <= 1)
			{
				return Mathf.Lerp (from, to, t);
			}
			
			return from + (to-from)*t;
		}
		
		
		public static Vector3 Lerp (Vector3 from, Vector3 to, float t)
		{
			// Like regular Lerp, but t>1 gives an overshoot
			if (t <= 1)
			{
				return Vector3.Lerp (from, to, t);
			}
			
			return from + (to-from)*t;
		}
		
		
		public static Quaternion Lerp (Quaternion from, Quaternion to, float t)
		{
			// Like regular Lerp, but t>1 gives an overshoot
			if (t <= 1)
			{
				return Quaternion.Lerp (from, to, t);
			}
			
			Vector3 fromVec = from.eulerAngles;
			Vector3 toVec = to.eulerAngles;
			
			if (fromVec.x - toVec.x > 180f)
			{
				toVec.x -= 360f;
			}
			else if (fromVec.x - toVec.x > 180f)
			{
				toVec.x += 360;
			}
			if (fromVec.y - toVec.y < -180f)
			{
				toVec.y -= 360f;
			}
			else if (fromVec.y - toVec.y > 180f)
			{
				toVec.y += 360;
			}
			if (fromVec.z - toVec.z > 180f)
			{
				toVec.z -= 360f;
			}
			else if (fromVec.z - toVec.z > 180f)
			{
				toVec.z += 360;
			}
			
			return Quaternion.Euler (Lerp (fromVec, toVec, t));
		}
		
		
		public static float Interpolate (float startT, float deltaT, MoveMethod moveMethod, AnimationCurve timeCurve)
		{
			if (moveMethod == MoveMethod.Curved)
			{
				moveMethod = MoveMethod.Smooth;
			}
			
			else if (moveMethod == MoveMethod.Smooth)
			{
				return -0.5f * (Mathf.Cos (Mathf.PI * (Time.time - startT) / deltaT) - 1f);
			}
			else if (moveMethod == MoveMethod.EaseIn)
			{
				return 1f - Mathf.Cos ((Time.time - startT) / deltaT * (Mathf.PI / 2));
			}
			else if (moveMethod == MoveMethod.EaseOut)
			{
				return Mathf.Sin ((Time.time - startT) / deltaT * (Mathf.PI / 2));
			}
			else if (moveMethod == MoveMethod.CustomCurve)
			{
				if (timeCurve == null || timeCurve.length == 0)
				{
					return 1f;
				}
				float startTime = timeCurve [0].time;
				float endTime = timeCurve [timeCurve.length - 1].time;
				
				return timeCurve.Evaluate ((endTime - startTime) * (Time.time - startT) / deltaT + startTime);
			}
			
			return ((Time.time - startT) / deltaT);
		}
		
		
		public static Rect Rescale (Rect _rect)
		{
			float ScaleFactor;
			ScaleFactor = Screen.width / 884.0f;
			int ScaleFactorInt = Mathf.RoundToInt(ScaleFactor);
			Rect newRect = new Rect (_rect.x * ScaleFactorInt, _rect.y * ScaleFactorInt, _rect.width * ScaleFactorInt, _rect.height * ScaleFactorInt);
			
			return (newRect);
		}
		
		
		public static int Rescale (int _int)
		{
			float ScaleFactor;
			ScaleFactor = Screen.width / 884.0f;
			int ScaleFactorInt = Mathf.RoundToInt(ScaleFactor);
			int returnValue;
			returnValue = _int * ScaleFactorInt;
			
			return (returnValue);
		}
		
		
		public static void DrawTextEffect (Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size, TextEffects textEffects)
		{
			if (textEffects == TextEffects.Outline || textEffects == TextEffects.OutlineAndShadow)
			{
				AdvGame.DrawTextOutline (rect, text, style, outColor, inColor, size);
			}
			if (textEffects == TextEffects.Shadow || textEffects == TextEffects.OutlineAndShadow)
			{
				AdvGame.DrawTextShadow (rect, text, style, outColor, inColor, size);
			}
		}
		
		
		private static void DrawTextShadow (Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size)
		{
			GUIStyle backupStyle = new GUIStyle(style);
			Color backupColor = GUI.color;
			
			outColor.a = GUI.color.a;
			style.normal.textColor = outColor;
			GUI.color = outColor;
			
			rect.x += size;
			GUI.Label(rect, text, style);
			
			rect.y += size;
			GUI.Label(rect, text, style);
			
			rect.x -= size;
			rect.y -= size;
			style.normal.textColor = inColor;
			GUI.color = backupColor;
			GUI.Label(rect, text, style);
			
			style = backupStyle;
		}
		
		
		private static void DrawTextOutline (Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size)
		{
			float halfSize = size * 0.5F;
			GUIStyle backupStyle = new GUIStyle(style);
			Color backupColor = GUI.color;
			
			outColor.a = GUI.color.a;
			style.normal.textColor = outColor;
			GUI.color = outColor;
			
			rect.x -= halfSize;
			GUI.Label(rect, text, style);
			
			rect.x += size;
			GUI.Label(rect, text, style);
			
			rect.x -= halfSize;
			rect.y -= halfSize;
			GUI.Label(rect, text, style);
			
			rect.y += size;
			GUI.Label(rect, text, style);
			
			rect.y -= halfSize;
			style.normal.textColor = inColor;
			GUI.color = backupColor;
			GUI.Label(rect, text, style);
			
			style = backupStyle;
		}
		
	}
	
}	