// Based on: http://wiki.unity3d.com/index.php/Click_To_Move_C
// By: Vinicius Rezendrix
using UnityEngine;

namespace PixelCrushers.DialogueSystem.Examples {
	
	/// <summary>
	/// Navigates to the place where the player mouse clicks.
	/// </summary>
	[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
	public class NavigateOnMouseClick : MonoBehaviour {
		
		public AnimationClip idle;
		public AnimationClip run;
		
		public enum MouseButtonType { Left, Right, Middle };
		public MouseButtonType mouseButton = MouseButtonType.Left;
		
		private Transform myTransform;
		private UnityEngine.AI.NavMeshAgent navMeshAgent;
		
		void Awake() {
			myTransform = transform;
			navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
			if (navMeshAgent == null) {
				Debug.LogWarning(string.Format("{0}: No NavMeshAgent found on {1}. Disabling {2}.", DialogueDebug.Prefix, name, this.GetType().Name));
				enabled = false;
			}
		}
	 
		void Update() {
			if (navMeshAgent.remainingDistance < 0.5f) {
				if (idle != null) GetComponent<Animation>().CrossFade(idle.name);
			} else {
				if (run != null) GetComponent<Animation>().CrossFade(run.name);
			}
	 
			// Moves the Player if the Left Mouse Button was clicked:
			if (Input.GetMouseButtonDown((int) mouseButton) && GUIUtility.hotControl == 0) {
				Plane playerPlane = new Plane(Vector3.up, myTransform.position);
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				float hitdist = 0.0f;
	 
				if (playerPlane.Raycast(ray, out hitdist)) {
					navMeshAgent.SetDestination(ray.GetPoint(hitdist));
				}
			}
	 
			// Moves the player if the mouse button is held down:
			else if (Input.GetMouseButton((int) mouseButton) && GUIUtility.hotControl == 0) {
	 
				Plane playerPlane = new Plane(Vector3.up, myTransform.position);
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				float hitdist = 0.0f;
	 
				if (playerPlane.Raycast(ray, out hitdist)) {
					navMeshAgent.SetDestination(ray.GetPoint(hitdist));
				}
			}
		}
	}
}