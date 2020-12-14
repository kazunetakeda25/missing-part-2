using UnityEngine;
using System.Collections;

public class millionhitsflash : MonoBehaviour {

	public GameObject millionhits;
	public float timer = 0.0f;
	public float timeLapse = 0.5f;
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;
		if (timer >= timeLapse) 
		{
			millionhits.SetActive(!millionhits.activeSelf);
			timer = 0.0f;
		}
	}
}
