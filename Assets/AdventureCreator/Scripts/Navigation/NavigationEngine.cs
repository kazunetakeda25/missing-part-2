/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavigationEngine.cs"
 * 
 *	This script is a base class for the Navigation method scripts.
 *  Create a subclass of name "NavigationEngine_NewMethodName" and
 * 	add "NewMethodName" to the AC_NavigationMethod enum to integrate
 * 	a new method into the engine.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

public class NavigationEngine : ScriptableObject
{

	public virtual void Awake ()
	{ }

	public virtual Vector3[] GetPointsArray (AC.Char _char, Vector3 startPosition, Vector3 targetPosition)
	{
		return GetPointsArray (startPosition, targetPosition);
	}

	public virtual Vector3[] GetPointsArray (Vector3 startPosition, Vector3 targetPosition)
	{
		List <Vector3> pointsList = new List<Vector3>();
		pointsList.Add (targetPosition);
		return pointsList.ToArray ();
	}


	public virtual string GetPrefabName ()
	{
		return "";
	}

	public virtual void SetVisibility (bool visibility)
	{ }


	public virtual void SceneSettingsGUI ()
	{ 
		#if UNITY_EDITOR
		#endif
	}

}
