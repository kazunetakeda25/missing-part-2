/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Moveable.cs"
 * 
 *	This script is attached to any gameObject that is to be transformed
 *	during gameplay via the action ActionTransform.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class Moveable : MonoBehaviour
	{
		
		[HideInInspector] public bool isMoving;
		
		private bool doEulerRotation = false;
		
		private float moveChangeTime;
		private float moveStartTime;
		private AnimationCurve timeCurve;
		
		private MoveMethod moveMethod;
		private TransformType transformType;
		
		private	Vector3 startPosition;
		private Vector3 endPosition;
		
		private Vector3 startScale;
		private Vector3 endScale;
		
		private Vector3 startEulerRotation;
		private Vector3 endEulerRotation;
		
		private Quaternion startRotation;
		private Quaternion endRotation;
		
		
		public void StopMoving ()
		{
			isMoving = false;
			StopCoroutine ("_UpdateMovement");
		}


		public void EndMovement ()
		{
			StopMoving ();

			if (transformType == TransformType.Translate || transformType == TransformType.CopyMarker)
			{
				transform.localPosition = endPosition;
			}
			if (transformType == TransformType.Rotate || transformType == TransformType.CopyMarker)
			{
				transform.localEulerAngles = endEulerRotation;
			}
			if (transformType == TransformType.Scale || transformType == TransformType.CopyMarker)
			{
				transform.localScale = endScale;
			}
		}

		
		private IEnumerator _UpdateMovement ()
		{
			while (isMoving)
			{
				if (Time.time < moveStartTime + moveChangeTime)
				{
					if (transformType == TransformType.Translate || transformType == TransformType.CopyMarker)
					{
						if (moveMethod == MoveMethod.Curved)
						{
							transform.localPosition = Vector3.Slerp (startPosition, endPosition, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
						}
						else
						{
							transform.localPosition = AdvGame.Lerp (startPosition, endPosition, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
						}
					}
					
					if (transformType == TransformType.Rotate || transformType == TransformType.CopyMarker)
					{
						if (doEulerRotation)
						{
							if (moveMethod == MoveMethod.Curved)
							{
								transform.localEulerAngles = Vector3.Slerp (startEulerRotation, endEulerRotation, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
							}
							else
							{
								transform.localEulerAngles = AdvGame.Lerp (startEulerRotation, endEulerRotation, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
							}
						}
						else
						{
							if (moveMethod == MoveMethod.Curved)
							{
								transform.localRotation = Quaternion.Slerp (startRotation, endRotation, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
							}
							else
							{
								transform.localRotation = AdvGame.Lerp (startRotation, endRotation, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
							}
						}
					}
					
					if (transformType == TransformType.Scale || transformType == TransformType.CopyMarker)
					{
						if (moveMethod == MoveMethod.Curved)
						{
							transform.localScale = Vector3.Slerp (startScale, endScale, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
						}
						else
						{
							transform.localScale = AdvGame.Lerp (startScale, endScale, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
						}
					}
				}
				else
				{
					EndMovement ();
				}
				
				yield return new WaitForFixedUpdate ();
			}
			
			StopCoroutine ("_UpdateMovement");
		}
		
		
		public void Move (Vector3 _newVector, MoveMethod _moveMethod, float _transitionTime, TransformType _transformType, bool _doEulerRotation, AnimationCurve _timeCurve)
		{
			StopCoroutine ("_UpdateMovement");
			
			if (GetComponent <Rigidbody>() && !GetComponent <Rigidbody>().isKinematic)
			{
				GetComponent <Rigidbody>().velocity = GetComponent <Rigidbody>().angularVelocity = Vector3.zero;
			}
			
			if (_transitionTime == 0f)
			{
				isMoving = false;
				
				if (_transformType == TransformType.Translate)
				{
					transform.localPosition = _newVector;
				}
				else if (_transformType == TransformType.Rotate)
				{
					transform.localEulerAngles = _newVector;
				}
				else if (_transformType == TransformType.Scale)
				{
					transform.localScale = _newVector;
				}
			}
			else
			{
				isMoving = true;
				
				doEulerRotation = _doEulerRotation;
				moveMethod = _moveMethod;
				transformType = _transformType;
				
				startPosition = endPosition = transform.localPosition;
				startEulerRotation = endEulerRotation = transform.localEulerAngles;
				startRotation = endRotation = transform.localRotation;
				startScale = endScale = transform.localScale;
				
				if (_transformType == TransformType.Translate)
				{
					endPosition = _newVector;
				}
				else if (_transformType == TransformType.Rotate)
				{
					endRotation = Quaternion.Euler (_newVector);
					endEulerRotation = _newVector;
				}
				else if (_transformType == TransformType.Scale)
				{
					endScale = _newVector;
				}
				
				moveChangeTime = _transitionTime;
				moveStartTime = Time.time;
				
				if (moveMethod == MoveMethod.CustomCurve)
				{
					timeCurve = _timeCurve;
				}
				else
				{
					timeCurve = null;
				}
				
				StartCoroutine ("_UpdateMovement");
			}
		}
		
		
		public void Move (Marker _marker, MoveMethod _moveMethod, float _transitionTime, AnimationCurve _timeCurve)
		{
			if (GetComponent <Rigidbody>() && !GetComponent <Rigidbody>().isKinematic)
			{
				GetComponent <Rigidbody>().velocity = GetComponent <Rigidbody>().angularVelocity = Vector3.zero;
			}
			
			StopCoroutine ("_UpdateMovement");
			transformType = TransformType.CopyMarker;
			
			if (_transitionTime == 0f)
			{
				isMoving = false;
				
				transform.localPosition = _marker.transform.localPosition;
				transform.localEulerAngles = _marker.transform.localEulerAngles;
				transform.localScale = _marker.transform.localScale;
			}
			else
			{
				isMoving = true;
				
				doEulerRotation = false;
				moveMethod = _moveMethod;
				
				startPosition = transform.localPosition;
				startRotation = transform.localRotation;
				startScale = transform.localScale;
				
				endPosition = _marker.transform.localPosition;
				endRotation = _marker.transform.localRotation;
				endScale = _marker.transform.localScale;
				
				moveChangeTime = _transitionTime;
				moveStartTime = Time.time;
				
				if (moveMethod == MoveMethod.CustomCurve)
				{
					timeCurve = _timeCurve;
				}
				else
				{
					timeCurve = null;
				}
				
				StartCoroutine ("_UpdateMovement");
			}
		}
		
		
		public void Kill ()
		{
			isMoving = false;
		}
		
	}
	
}