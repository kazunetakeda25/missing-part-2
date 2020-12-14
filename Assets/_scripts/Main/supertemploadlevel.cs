using UnityEngine;
using System.Collections;

public class supertemploadlevel : MonoBehaviour {

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this);
	}
	
	// Update is called once per frame
	void Update () {
	if (Input.GetKeyDown (KeyCode.Alpha1)) 
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(1);
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) 
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(2);
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) 
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(3);
		}
	}
}
