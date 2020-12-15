using UnityEngine;

namespace PixelCrushers.DialogueSystem.Examples {

	/// <summary>
	/// Component that keeps its game object always facing the main camera.
	/// </summary>
	public class AlwaysFaceCamera : MonoBehaviour {
		
		public bool yAxisOnly = false;
		
		private Transform myTransform = null;
		
		void Awake() {
			myTransform = transform;
		}
	
		void Update() {
			if ((myTransform != null) && (Camera.main != null)) {
				if (yAxisOnly) {
					myTransform.LookAt(new Vector3(Camera.main.transform.position.x, myTransform.position.y, Camera.main.transform.position.z));
				} else {
					myTransform.LookAt(Camera.main.transform);
				}
			}
		}
		
	}

}
