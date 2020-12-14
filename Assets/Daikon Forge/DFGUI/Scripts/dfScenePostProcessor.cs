using UnityEngine;

#if UNITY_EDITOR

using UnityEditor.Callbacks;

#endif

using System;
using System.Collections;



public class BakeFunction
{

#if UNITY_EDITOR

	[PostProcessScene( -1 )]
	public static void OnPostprocessScene()
	{

		var controls = UnityEngine.Object.FindObjectsOfType( typeof( dfControl ) ) as dfControl[];
		Array.Sort( controls, ( lhs, rhs ) => lhs.RenderOrder.CompareTo( rhs.RenderOrder ) );

		for( int i = 0; i < controls.Length; i++ )
		{
			var control = controls[ i ];
			control.PerformLayout();
			control.ResetLayout( false, true );
		}

	}

#endif

}
