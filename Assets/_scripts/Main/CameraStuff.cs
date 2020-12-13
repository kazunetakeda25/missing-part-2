using UnityEngine;
using System.Collections;

public class CameraStuff : MonoBehaviour {

	bool spreadOut = false;
	bool closeIn = false;
	float x;

	void Update()
	{
		if (spreadOut) 
		{
			x+= Time.deltaTime;
			if(x < 0.5f)
			{
				GetComponent<Camera>().rect = new Rect(0,0,x,GetComponent<Camera>().rect.height);
			}
			else
			{
				spreadOut = false;
				GetComponent<Camera>().rect = new Rect(0,0,x,GetComponent<Camera>().rect.height);
			}
		}
		if (closeIn) 
		{
			x-= Time.deltaTime;
			if(x > 0)
			{
				GetComponent<Camera>().rect = new Rect(0,0,x,GetComponent<Camera>().rect.height);
			}
			else
			{
				spreadOut = false;
				GetComponent<Camera>().rect = new Rect(0,0,0,GetComponent<Camera>().rect.height);
				GetComponent<Camera>().enabled = false;
			}
		}
	}
	void TurnOn()
	{
		GetComponent<Camera>().enabled = true;
		x = 0.0f;
		spreadOut = true;
	}
	void TurnOff()
	{
		x = 0.5f;
		closeIn = true;
	}
}
