using UnityEngine;
using System.Collections;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Quit On Click" )]
public class DemoQuitOnClick : MonoBehaviour
{

	void OnClick( dfControl sender, dfMouseEventArgs args )
	{

#if UNITY_EDITOR
		if( Application.isEditor )
			UnityEditor.EditorApplication.isPlaying = false;
#endif

		Application.Quit();

	}

}
