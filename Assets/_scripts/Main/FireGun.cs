using UnityEngine;
using System.Collections;

public class FireGun : MonoBehaviour {
	public GameObject Flash;
	public GameObject smoke;
	public GameObject Gun;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void Fire()
	{
		Flash.SetActive (true);
		smoke.SetActive (true);
	}
	public void End()
	{
		Flash.SetActive (false);
		smoke.SetActive (false);
	}
	public void Animate()
	{
		Gun.GetComponent<Animation>().Play ();
	}
}
