using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/General/Switch Runtime Shader" )]
public class SwitchRuntimeShader : MonoBehaviour 
{

	public Material material;
	public Shader runtimeShader;

	private Shader defaultShader;

	public void OnEnable()
	{
		if( Application.isPlaying )
		{
			defaultShader = material.shader;
			material.shader = runtimeShader;
		}
	}

	public void OnDisable()
	{
		if( defaultShader != null )
		{
			material.shader = defaultShader;
			defaultShader = null;
		}
	}

}
