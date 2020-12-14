using UnityEngine;
using System.Collections;

public class SoundDampening : MonoBehaviour 
{
	public float volume;

	private void Start()
	{
		if(Application.isEditor) {
			AudioListener.volume = this.volume;
		}
	}
}
