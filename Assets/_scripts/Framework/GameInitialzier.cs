using UnityEngine;
using System.Collections;

public class GameInitialzier : MonoBehaviour {

	public float logoDelay;

	private void Init()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("SOUND_ADJUST");
		GameObject.Destroy(this.gameObject);
	}

	private void Update()
	{
		logoDelay -= Time.deltaTime;
		if(logoDelay <= 0)
		{
			Init();
		}
	}
}
