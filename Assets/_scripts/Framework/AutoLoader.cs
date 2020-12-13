using UnityEngine;
using System.Collections;

public class AutoLoader : MonoBehaviour {

	public float delay;

	private void Update()
	{
		delay -= Time.deltaTime;
		if(delay <= 0)
		{
			SessionManager.Instance.GotoNextLevel();
			GameObject.Destroy(this.gameObject);
		}
	}

}
