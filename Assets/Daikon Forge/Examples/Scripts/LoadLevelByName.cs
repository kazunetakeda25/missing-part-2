using System;

using UnityEngine;

[AddComponentMenu( "Daikon Forge/Examples/General/Load Level On Click" )]
[Serializable]
public class LoadLevelByName : MonoBehaviour
{

	public string LevelName;

	void OnClick()
	{
		if( !string.IsNullOrEmpty( LevelName ) )
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene( LevelName );
		}
	}

}
