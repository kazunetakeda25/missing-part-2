/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionBlendShape.cs"
 * 
 *	This action is used to animate blend shapes within
 *	groups, as set by the Shapeable script
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionBlendShape : Action
	{
		
		public int parameterID = -1;
		public int constantID = 0;
		
		public Shapeable shapeObject;
		public int shapeGroupID = 0;
		public int shapeKeyID = 0;
		public float shapeValue = 0f;
		public bool isPlayer = false;
		public bool disableAllKeys = false;
		public float fadeTime = 0f;
		public MoveMethod moveMethod = MoveMethod.Smooth;
		public AnimationCurve timeCurve = new AnimationCurve (new Keyframe(0, 0), new Keyframe(1, 1));
				

		public ActionBlendShape ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Blend shape";
			description = "Animates a Skinned Mesh Renderer's blend shape by a chosen amount. If the Shapeable script attached to the renderer has grouped multiple shapes into a group, all other shapes in that group will be deactivated.";
		}
		
		
		public override void AssignValues (List<ActionParameter> parameters)
		{
			shapeObject = AssignFile <Shapeable> (parameters, parameterID, constantID, shapeObject);
			
			if (isPlayer)
			{
				if (KickStarter.player && KickStarter.player.GetShapeable ())
				{
					shapeObject = KickStarter.player.GetShapeable ();
				}
				else
				{
					shapeObject = null;
					Debug.LogWarning ("Cannot BlendShape Player since cannot find Shapeable script on Player.");
				}
			}
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
			   
				if (shapeObject)
				{
					DoShape (fadeTime);
					
					if (willWait)
					{
						return (fadeTime);
					}
				}
			}
			else
			{
				isRunning = false;
				return 0f;
			}
			return 0f;
		}


		override public void Skip ()
		{
			DoShape (0f);
		}


		private void DoShape (float _time)
		{
			if (shapeObject)
			{
				if (disableAllKeys)
				{
					shapeObject.DisableAllKeys (shapeGroupID, _time, moveMethod, timeCurve);
				}
				else
				{
					shapeObject.SetActiveKey (shapeGroupID, shapeKeyID, shapeValue, _time, moveMethod, timeCurve);
				}
			}
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is player?", isPlayer);
			if (!isPlayer)
			{
				parameterID = ChooseParameterGUI ("Object:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					shapeObject = null;
				}
				else
				{
					shapeObject = (Shapeable) EditorGUILayout.ObjectField ("Object:", shapeObject, typeof (Shapeable), true);
					
					constantID = FieldToID <Shapeable> (shapeObject, constantID);
					shapeObject = IDToField <Shapeable> (shapeObject, constantID, false);
				}
			}
			else
			{
				Player _player = null;
				
				if (Application.isPlaying)
				{
					_player = KickStarter.player;
				}
				else
				{
					_player = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
				
				if (_player != null && _player.GetShapeable ())
				{
					shapeObject = _player.GetShapeable ();
				}
				else
				{
					shapeObject = null;
					EditorGUILayout.HelpBox ("Cannot find player with Shapeable script attached", MessageType.Warning);
				}
			}
			
			if (shapeObject != null && shapeObject.shapeGroups != null)
			{
				shapeGroupID = ActionBlendShape.ShapeableGroupGUI ("Shape group:", shapeObject.shapeGroups, shapeGroupID);
				disableAllKeys = EditorGUILayout.Toggle ("Disable all keys?", disableAllKeys);
				if (!disableAllKeys)
				{
					ShapeGroup _shapeGroup = shapeObject.GetGroup (shapeGroupID);
					if (_shapeGroup != null)
					{
						if (_shapeGroup.shapeKeys != null && _shapeGroup.shapeKeys.Count > 0)
						{
							shapeKeyID = ShapeableKeyGUI (_shapeGroup.shapeKeys, shapeKeyID);
						}
						else
						{
							EditorGUILayout.HelpBox ("No shape keys found.", MessageType.Info);
						}
					}
					shapeValue = EditorGUILayout.Slider ("New value:", shapeValue, 0f, 100f);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("An object must be assigned before more options can show.", MessageType.Info);
			}

			fadeTime = EditorGUILayout.FloatField ("Transition time:", fadeTime);
			if (fadeTime > 0f)
			{
				moveMethod = (MoveMethod) EditorGUILayout.EnumPopup ("Move method:", moveMethod);
				if (moveMethod == MoveMethod.CustomCurve)
				{
					timeCurve = EditorGUILayout.CurveField ("Time curve:", timeCurve);
				}
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
			
			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			if (shapeObject)
			{
				return (" (" + shapeObject.gameObject.name + ")");
			}
			return "";
		}


		public static int ShapeableGroupGUI (string label, List<ShapeGroup> shapeGroups, int groupID)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			int groupNumber = 0;
			
			if (shapeGroups.Count > 0)
			{
				foreach (ShapeGroup shapeGroup in shapeGroups)
				{
					if (shapeGroup.label != "")
					{
						labelList.Add (shapeGroup.label);
					}
					else
					{
						labelList.Add ("(Untitled)");
					}
					if (shapeGroup.ID == groupID)
					{
						groupNumber = i;
					}
					i++;
				}
				
				if (groupNumber == -1)
				{
					Debug.LogWarning ("Previously chosen shape group no longer exists!");
					groupID = 0;
				}
				
				groupNumber = EditorGUILayout.Popup (label, groupNumber, labelList.ToArray());
				groupID = shapeGroups[groupNumber].ID;
			}
			else
			{
				EditorGUILayout.HelpBox ("No shape groups exist!", MessageType.Info);
				groupID = -1;
			}
			
			return groupID;
		}
		
		
		private int ShapeableKeyGUI (List<ShapeKey> shapeKeys, int keyID)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			int keyNumber = 0;
			
			if (shapeKeys.Count > 0)
			{
				foreach (ShapeKey shapeKey in shapeKeys)
				{
					if (shapeKey.label != "")
					{
						labelList.Add (shapeKey.label);
					}
					else
					{
						labelList.Add ("(Untitled)");
					}
					if (shapeKey.ID == keyID)
					{
						keyNumber = i;
					}
					i++;
				}
				
				if (keyNumber == -1)
				{
					Debug.LogWarning ("Previously chosen shape key no longer exists!");
					keyID = 0;
				}
				
				keyNumber = EditorGUILayout.Popup ("Shape key:", keyNumber, labelList.ToArray());
				keyID = shapeKeys[keyNumber].ID;
			}
			else
			{
				EditorGUILayout.HelpBox ("No shape keys exist!", MessageType.Info);
				keyID = -1;
			}
			
			return keyID;
		}
		
		#endif

	}
	
}