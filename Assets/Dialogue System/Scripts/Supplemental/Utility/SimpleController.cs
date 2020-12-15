using UnityEngine;

namespace PixelCrushers.DialogueSystem.Examples {
	
	/// <summary>
	/// This component implements a very simple third person shooter-style controller.
	/// The mouse rotates the character, the axes (WASD/arrow keys) move, and the Fire1
	/// button (left mouse button) fires.
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public class SimpleController : MonoBehaviour {
		
		public AnimationClip idle;
		public AnimationClip runForward;
		public AnimationClip runBack;
		public AnimationClip aim;
		public AnimationClip fire;
		public Transform upperBodyMixingTransform;
		public float runSpeed = 5f;
		public float mouseSensitivityX = 15f;
		public float mouseSensitivityY = 10f;
		public float mouseMinimumY = -60f;
	    public float mouseMaximumY = 60f;
		public LayerMask fireLayerMask = 1;
		public AudioClip fireSound;
		public float weaponDamage = 100;
		
		private CharacterController controller = null;
		private float centralSpeed = 0;
		private float centralVelocity = 0;
		private float lateralSpeed = 0;
		private float lateralVelocity = 0;
	    private float cameraRotationY = 0;
	    private Quaternion originalCameraRotation;
		private bool firing = false;
		private bool fired = false;
		private float endFiringTime = 0;
		private AudioSource audioSource = null;
		
		void Awake() {
			controller = GetComponent<CharacterController>();
			audioSource = GetComponentInChildren<AudioSource>();
		}
		
		void Start() {
			originalCameraRotation = Camera.main.transform.localRotation;
			GetComponent<Animation>().AddClip((aim != null) ? aim : idle, "aim");
			GetComponent<Animation>()["aim"].layer = 1;
			GetComponent<Animation>()["aim"].AddMixingTransform(upperBodyMixingTransform);
			GetComponent<Animation>()[fire.name].layer = 1;
			GetComponent<Animation>()[fire.name].AddMixingTransform(upperBodyMixingTransform);
		}
	
		void Update() {
			if (Time.timeScale <= 0) return;
			
			// Mouse rotation:
			transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivityX, 0);
			Camera.main.transform.rotation = transform.rotation;
			cameraRotationY += Input.GetAxis("Mouse Y") * mouseSensitivityY;
			cameraRotationY = ClampAngle (cameraRotationY, mouseMinimumY, mouseMaximumY);
			Quaternion yQuaternion = Quaternion.AngleAxis (cameraRotationY, -Vector3.right);
			Camera.main.transform.localRotation = originalCameraRotation * yQuaternion;

			// Firing:
			if ((fire != null) && Input.GetButtonDown("Fire1") && !firing) {
				GetComponent<Animation>().CrossFade(fire.name);
				firing = true;
				fired = false;
				endFiringTime = Time.time + fire.length - 0.3f;
			}
			if (firing) {
				if (Time.time > endFiringTime) {
					GetComponent<Animation>().CrossFade("aim");
					firing = false;
					if (!fired) OnFired();
				}
			}
			
			// Movement:
			float centralTargetSpeed = Input.GetAxis("Vertical");
			float lateralTargetSpeed = Input.GetAxis ("Horizontal");
			centralSpeed = Mathf.SmoothDamp(centralSpeed, centralTargetSpeed, ref centralVelocity, 0.3f);
			lateralSpeed = Mathf.SmoothDamp(lateralSpeed, lateralTargetSpeed, ref lateralVelocity, 0.3f);
			if ((Mathf.Abs(centralTargetSpeed) > 0.1f) || (Mathf.Abs(lateralTargetSpeed) > 0.1f)) {
				if (centralTargetSpeed >= 0f) {
					GetComponent<Animation>()[runForward.name].speed = 1;
					GetComponent<Animation>().CrossFade(runForward.name);
				} else if (centralTargetSpeed < -0.1f) {
					if (runBack != null) {
						GetComponent<Animation>().CrossFade(runBack.name);
					} else {
						GetComponent<Animation>()[runForward.name].speed = -1;
						GetComponent<Animation>().CrossFade(runForward.name);
					}
				}
			} else {
				GetComponent<Animation>().CrossFade(idle.name);
			}
			
			// Move, including gravity:
			controller.Move(transform.rotation * ((Vector3.forward * centralSpeed * runSpeed * Time.deltaTime) + (Vector3.right * lateralSpeed * runSpeed * Time.deltaTime)) + Vector3.down * 20f * Time.deltaTime);
		}
		
		/// <summary>
		/// When the character is involved in a conversation, stop moving and play the idle animation.
		/// </summary>
		void OnConversationStart() {
			GetComponent<Animation>().CrossFade(idle.name);
			centralSpeed = 0;
			centralVelocity = 0;
			lateralSpeed = 0;
			lateralVelocity = 0;
			firing = false;
		}

		/// <summary>
		/// When the 
		/// </summary>
		void OnFired() {
			fired = true;
			if ((fireSound != null) && (audioSource != null)) audioSource.PlayOneShot(fireSound);
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, fireLayerMask)) {
				hit.collider.gameObject.BroadcastMessage("TakeDamage", weaponDamage, SendMessageOptions.DontRequireReceiver);
			}
		}
		
		public static float ClampAngle (float angle, float min, float max) {
			if (angle < -360f) angle += 360f;
			if (angle > 360f) angle -= 360f;
			return Mathf.Clamp(angle, min, max);
		}
			 
	}
	
}
