using UnityEngine;
using System.Collections;

public class SimpleRotate : MonoBehaviour {

	public Vector3 rotationAngles;
	public float speed;
	public bool rotating;
	public GameObject rotateTarget;

	private void Awake()
	{
		if(rotateTarget == null)
			rotateTarget = this.gameObject;
	}

	private void Update () 
	{
		if(rotating)
			UpdateRotation();
	}

	private void UpdateRotation()
	{
		Vector3 rotateAmountThisFrame = new Vector3(rotationAngles.x, rotationAngles.y, rotationAngles.z);
		rotateAmountThisFrame *= Time.deltaTime * speed;
		rotateTarget.transform.Rotate(rotateAmountThisFrame);
	}
}
