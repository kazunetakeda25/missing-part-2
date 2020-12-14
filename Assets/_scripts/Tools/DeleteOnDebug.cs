using UnityEngine;
using System.Collections;

public class DeleteOnDebug : MonoBehaviour 
{
	private void Awake()
	{
		if(Debug.isDebugBuild == false) {
			GameObject.Destroy(this.gameObject);
		}
	}
}
