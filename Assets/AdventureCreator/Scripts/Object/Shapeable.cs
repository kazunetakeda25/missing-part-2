/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Shapeable.cs"
 * 
 *	Attaching this script to an object with BlendShapes will allow
 *	them to be animated via the Actions Object: Animate and Character: Animate
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class Shapeable : MonoBehaviour
	{
		
		public List<ShapeGroup> shapeGroups = new List<ShapeGroup>();
		
		private SkinnedMeshRenderer skinnedMeshRenderer;
		
		// OLD
		private bool isChanging = false;
		private float targetShape;
		private float actualShape;
		private float originalShape;
		private int shapeKey;
		private float startTime;
		private float deltaTime;
		
		
		private void Awake ()
		{
			AssignSkinnedMeshRenderer ();
			
			if (skinnedMeshRenderer != null)
			{
				// Set all values to zero
				foreach (ShapeGroup shapeGroup in shapeGroups)
				{
					shapeGroup.SetSMR (skinnedMeshRenderer);
					
					foreach (ShapeKey shapeKey in shapeGroup.shapeKeys)
					{
						shapeKey.SetValue (0f, skinnedMeshRenderer);
					}
				}
			}
		}
		
		
		public void DisableAllKeys (int _groupID, float _deltaTime, MoveMethod _moveMethod, AnimationCurve _timeCurve)
		{
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				if (shapeGroup.ID == _groupID)
				{
					shapeGroup.SetActive (-1, 0f, _deltaTime, _moveMethod, _timeCurve);
				}
			}
		}
		
		
		public void SetActiveKey (int _groupID, int _keyID, float _value, float _deltaTime, MoveMethod _moveMethod, AnimationCurve _timeCurve)
		{
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				if (shapeGroup.ID == _groupID)
				{
					shapeGroup.SetActive (_keyID, _value, _deltaTime, _moveMethod, _timeCurve);
				}
			}
		}
		
		
		private void AssignSkinnedMeshRenderer ()
		{
			skinnedMeshRenderer = GetComponent <SkinnedMeshRenderer> ();
			
			if (skinnedMeshRenderer == null)
			{
				skinnedMeshRenderer = GetComponentInChildren <SkinnedMeshRenderer>();
			}
		}
		
		
		public ShapeGroup GetGroup (int ID)
		{
			AssignSkinnedMeshRenderer ();
			
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				if (shapeGroup.ID == ID)
				{
					return shapeGroup;
				}
			}
			return null;
		}
		
		
		private void LateUpdate ()
		{
			foreach (ShapeGroup shapeGroup in shapeGroups)
			{
				shapeGroup.UpdateKeys ();
			}
			
			// OLD
			if (isChanging)
			{
				actualShape = Mathf.Lerp (originalShape, targetShape, AdvGame.Interpolate (startTime, deltaTime, AC.MoveMethod.Linear, null));
				
				if (Time.time > startTime + deltaTime)
				{
					isChanging = false;
					actualShape = targetShape;
				}
				
				if (skinnedMeshRenderer)
				{
					skinnedMeshRenderer.SetBlendShapeWeight (shapeKey, actualShape);
				}
			}
		}
		
		
		public void Change (int _shapeKey, float _targetShape, float _deltaTime)
		{
			if (targetShape < 0f)
			{
				targetShape = 0f;
			}
			else if (targetShape > 100f)
			{
				targetShape = 100f;
			}
			
			isChanging = true;
			targetShape = _targetShape;
			deltaTime = _deltaTime;
			startTime = Time.time;
			shapeKey = _shapeKey;
			
			if (skinnedMeshRenderer)
			{
				originalShape = skinnedMeshRenderer.GetBlendShapeWeight (shapeKey);
			}
		}
		
	}
	
	
	[System.Serializable]
	public class ShapeGroup
	{
		
		public string label = "";
		public int ID = 0;
		public List<ShapeKey> shapeKeys = new List<ShapeKey>();
		
		private ShapeKey activeKey = null;
		private SkinnedMeshRenderer smr;
		private float startTime;
		private float changeTime;
		private AnimationCurve timeCurve;
		private MoveMethod moveMethod;
		
		
		public ShapeGroup (int[] idArray)
		{
			// Update id based on array
			ID = 0;
			foreach (int _id in idArray)
			{
				if (ID == _id)
				{
					ID ++;
				}
			}
		}
		
		
		public void SetSMR (SkinnedMeshRenderer _smr)
		{
			smr = _smr;
		}
		
		
		public int GetActiveKeyID ()
		{
			if (activeKey != null && shapeKeys.Contains (activeKey))
			{
				return activeKey.ID;
			}
			return -1;
		}
		
		
		public float GetActiveKeyValue ()
		{
			if (activeKey != null && shapeKeys.Contains (activeKey))
			{
				return activeKey.targetValue;
			}
			return 0f;
		}
		
		
		public void SetActive (int _ID, float _value, float _changeTime, MoveMethod _moveMethod, AnimationCurve _timeCurve)
		{
			if (_changeTime < 0f)
			{
				return;
			}
			
			activeKey = null;
			foreach (ShapeKey shapeKey in shapeKeys)
			{
				if (shapeKey.ID == _ID)
				{
					activeKey = shapeKey;
					shapeKey.targetValue = _value;
				}
				else
				{
					shapeKey.targetValue = 0f;
				}
			}
			
			moveMethod = _moveMethod;
			timeCurve = _timeCurve;
			changeTime = _changeTime;
			startTime = Time.time;
		}
		
		
		public void UpdateKeys ()
		{
			if (smr == null)
			{
				return;
			}
			
			foreach (ShapeKey shapeKey in shapeKeys)
			{
				if (changeTime > 0f)
				{
					float newValue = Mathf.Lerp (shapeKey.value, shapeKey.targetValue, AdvGame.Interpolate (startTime, changeTime, moveMethod, timeCurve));
					shapeKey.SetValue (newValue, smr);
					
					if ((startTime + changeTime) < Time.time)
					{
						changeTime = 0f;
					}
				}
				else
				{
					shapeKey.SetValue (shapeKey.targetValue, smr);
				}
			}
		}
		
	}
	
	
	[System.Serializable]
	public class ShapeKey
	{
		
		public int index = 0;
		public string label = "";
		public int ID = 0;
		public float value = 0;
		public float targetValue = 0;
		
		
		public ShapeKey (int[] idArray)
		{
			// Update id based on array
			ID = 0;
			foreach (int _id in idArray)
			{
				if (ID == _id)
				{
					ID ++;
				}
			}
		}
		
		
		public void SetValue (float _value, SkinnedMeshRenderer smr)
		{
			value = _value;
			smr.SetBlendShapeWeight (index, value);
		}
		
	}
	
}