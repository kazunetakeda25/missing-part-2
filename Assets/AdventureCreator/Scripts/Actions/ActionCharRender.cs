/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionCharRender.cs"
 * 
 *	This Action overrides Character
 *	render settings.
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
	public class ActionCharRender : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public bool isPlayer;
		public Char _char;

		public RenderLock renderLock_sorting;
		public SortingMapType mapType;
		public int sortingOrder;
		public string sortingLayer;

		public RenderLock renderLock_scale;
		public int scale;

		public RenderLock renderLock_direction;
		public CharDirection direction;
		
		
		public ActionCharRender ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Change rendering";
			description = "Overrides a Character's scale, sorting order or sprite direction. This is intended mainly for 2D games.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			_char = AssignFile <Char> (parameters, parameterID, constantID, _char);

			if (isPlayer)
			{
				_char = KickStarter.player;
			}
		}
		
		
		override public float Run ()
		{
			if (_char)
			{
				if (renderLock_sorting == RenderLock.Set)
				{
					if (mapType == SortingMapType.OrderInLayer)
					{
						_char.SetSorting (sortingOrder);
					}
					else if (mapType == SortingMapType.SortingLayer)
					{
						_char.SetSorting (sortingLayer);
					}
				}
				else if (renderLock_sorting == RenderLock.Release)
				{
					_char.ReleaseSorting ();
				}

				if (_char.animEngine == null)
				{
					_char.ResetAnimationEngine ();
				}
				
				if (_char.animEngine != null)
				{
					_char.animEngine.ActionCharRenderRun (this);
				}
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					_char = KickStarter.player;
				}
				else
				{
					_char = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
			}
			else
			{
				parameterID = Action.ChooseParameterGUI ("Character:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					_char = null;
				}
				else
				{
					_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
					
					constantID = FieldToID <Char> (_char, constantID);
					_char = IDToField <Char> (_char, constantID, false);
				}
			}

			if (_char)
			{
				EditorGUILayout.Space ();
				renderLock_sorting = (RenderLock) EditorGUILayout.EnumPopup ("Sorting:", renderLock_sorting);
				if (renderLock_sorting == RenderLock.Set)
				{
					mapType = (SortingMapType) EditorGUILayout.EnumPopup ("Sorting type:", mapType);
					if (mapType == SortingMapType.OrderInLayer)
					{
						sortingOrder = EditorGUILayout.IntField ("New order:", sortingOrder);
					}
					else if (mapType == SortingMapType.SortingLayer)
					{
						sortingLayer = EditorGUILayout.TextField ("New layer:", sortingLayer);
					}
				}

				if (_char.animEngine == null)
				{
					_char.ResetAnimationEngine ();
				}
				if (_char.animEngine)
				{
					_char.animEngine.ActionCharRenderGUI (this);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
			}

			EditorGUILayout.Space ();
			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = "";
			
			if (isPlayer)
			{
				labelAdd = " (Player)";
			}
			else if (_char)
			{
				labelAdd = " (" + _char.name + ")";
			}

			return labelAdd;
		}
		
		#endif
		
	}

}