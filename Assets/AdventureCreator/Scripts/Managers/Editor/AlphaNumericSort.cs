/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"AlphaNumericSort.cs"
 * 
 *	This script adds an option to the Hierarchy window
 *	to sort GameObjects alphanumerically.
 * 
 */

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace AC
{

	#if !UNITY_ANDROID && !UNITY_5 &&!UNITY_2018
	public class AlphaNumericSort : BaseHierarchySort
	{
		public override int Compare(GameObject lhs, GameObject rhs)
		{
			if (lhs == rhs) return 0;
			if (lhs == null) return -1;
			if (rhs == null) return 1;
			return EditorUtility.NaturalCompare(lhs.name, rhs.name);
		}
	}
	#endif

}