using UnityEngine;
using System;
using System.Collections;
using Holoville.HOTween;

public class WheelSpinner : MonoBehaviour {

	public Transform wheelSpinPos;
	public float wheelBringInSpeed;
	public float wheelRotationSpeed;

	public GameObject wheelContainer;
	public Collider wheelCollider;
	public Camera guiCam;

	public Renderer[] wheelRenderers;

	private Vector3 wheelStartPos;

	private bool wheelReady = false;

	private Action spinCallback;

	public void SpinWheel()
	{
		this.GetComponent<AudioSource>().Play();

		wheelReady = false;

		TweenParms parms = new TweenParms();

		parms.Prop("localEulerAngles", new Vector3(0.0f, 0.0f, 736.0f), true);
		parms.Ease(EaseType.EaseOutExpo);

		parms.OnComplete(WheelDoneSpinning);

		HOTween.To(wheelContainer.transform, wheelRotationSpeed, parms);
	}

	private void WheelDoneSpinning()
	{
		StartCoroutine(WheelDismissDelay());
	}

	private IEnumerator WheelDismissDelay()
	{
		yield return new WaitForSeconds(3.0f);

		if(spinCallback != null)
			spinCallback();
		DismissWheel();
	}

	public void BringInWheel(Action spinCallback)
	{
		ToggleRenderer(true);
		this.spinCallback = spinCallback;

		TweenParms parms = new TweenParms();

		parms.Prop("position", wheelSpinPos.position);
		parms.Ease(EaseType.EaseInOutQuad);
		parms.Delay(1);
		parms.OnComplete(ReadyWheel);

		HOTween.To(this.gameObject.transform, wheelBringInSpeed, parms);
	}

	private void ReadyWheel()
	{
		wheelReady = true;
	}

	public void DismissWheel()
	{
		ToggleRenderer(false);
		this.gameObject.transform.position = wheelStartPos;
	}

	private void ToggleRenderer(bool on)
	{
		foreach(Renderer render in wheelRenderers)
			render.enabled = on;
	}

	private void Awake()
	{
		wheelStartPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
	}

	private void Start()
	{
		//BringInWheel();
	}

	private void Update()
	{
		if(wheelReady == false)
			return;

		if(Input.GetMouseButtonUp(0))
			CheckHit();
	}

	private void CheckHit()
	{
		RaycastHit hit;
		Ray ray = guiCam.ScreenPointToRay(Input.mousePosition);
		if(Physics.Raycast(ray, out hit)) {
			if(hit.collider == wheelCollider)
				SpinWheel();
		}
	}

}
