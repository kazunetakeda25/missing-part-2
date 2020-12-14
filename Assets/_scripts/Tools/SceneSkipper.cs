using UnityEngine;
using System.Collections;

public class SceneSkipper : MonoBehaviour 
{
	private void Start()
	{
//		if(Debug.isDebugBuild) {
//			GameObject.DontDestroyOnLoad(this.gameObject);
//		} else {
//			GameObject.Destroy(this.gameObject);
//		}

		GameObject.DontDestroyOnLoad(this.gameObject);
	}

	private void Update()
	{
//
//		if(Input.GetKeyUp(KeyCode.PageUp)) {
//			SessionManager.Instance.GotoNextLevel();
//		}
	}
}
