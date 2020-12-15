using UnityEngine;

namespace PixelCrushers.DialogueSystem.Examples {

	/// <summary>
	/// This component activates game objects and enables components when it receives 
	/// OnTriggerEnter and the conditions are true, and deactivates/disables when it
	/// receives OnTriggerExit and the conditions are true.
	/// </summary>
	public class RangeTrigger : MonoBehaviour {
		
		/// <summary>
		/// The condition that must be true in order to activate/deactivate target
		/// game objects and components when the trigger is entered or exited.
		/// </summary>
		public Condition condition;
		
		/// <summary>
		/// The game objects to affect.
		/// </summary>
		public GameObject[] gameObjects;
		
		/// <summary>
		/// The components to affect.
		/// </summary>
		public Component[] components;
		
		/// <summary>
		/// Activates the target game objects and components if the condition is true.
		/// </summary>
		/// <param name='other'>
		/// The collider that entered the trigger.
		/// </param>
		public void OnTriggerEnter(Collider other) {
			if (condition.IsTrue(other.transform)) SetTargets(true);
		}
		
		/// <summary>
		/// Deactivates the target game objects and components if the condition is true.
		/// </summary>
		/// <param name='other'>
		/// The collider that exited the trigger.
		/// </param>
		public void OnTriggerExit(Collider other) {
			if (condition.IsTrue(other.transform)) SetTargets(false);
		}

		/// <summary>
		/// Activates the target game objects and components if the condition is true.
		/// </summary>
		/// <param name='other'>
		/// The 2D collider that entered the trigger.
		/// </param>
		public void OnTriggerEnter2D(Collider2D other) {
			if (condition.IsTrue(other.transform)) SetTargets(true);
		}
		
		/// <summary>
		/// Deactivates the target game objects and components if the condition is true.
		/// </summary>
		/// <param name='other'>
		/// The 2D collider that exited the trigger.
		/// </param>
		public void OnTriggerExit2D(Collider2D other) {
			if (condition.IsTrue(other.transform)) SetTargets(false);
		}
		
		/// <summary>
		/// Sets the targets active/inactive. Colliders and Renderers aren't MonoBehaviours, so we
		/// cast them separately to access their 'enabled' properties.
		/// </summary>
		/// <param name='value'>
		/// <c>true</c> for active, <c>false</c> for inactive.
		/// </param>
		private void SetTargets(bool value) {
			foreach (var gameObject in gameObjects) {
				gameObject.SetActive(value);
			}
			foreach (var component in components) {
				if (component is Collider) {
					(component as Collider).enabled = value;
				} else if (component is Renderer) {
					(component as Renderer).enabled = value;
				} else if (component is Animation) {
					(component as Animation).enabled = value;
				} else if (component is Animator) {
					(component as Animator).enabled = value;
				} else if (component is MonoBehaviour) {
					(component as MonoBehaviour).enabled = value;
				} else {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Internal error - Range Trigger doesn't know how to handle {1} of type {2}",	DialogueDebug.Prefix, component, component.GetType().Name));
				}
			}
		}
		
	}

}
