using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.Examples {

	/// <summary>
	/// Increments an element of the Lua Variable[] table when the GameObject is destroyed,
	/// and then updates the quest tracker if it's attached to the Dialogue Manager object or
	/// its children. This script is useful for kill quests or gathering quests.
	/// 
	/// </summary>
	public class IncrementOnDestroy : MonoBehaviour {

		/// <summary>
		/// The variable to increment.
		/// </summary>
		public string variable;

		/// <summary>
		/// The increment amount. To decrement, use a negative number.
		/// </summary>
		public int increment = 1;

		/// <summary>
		/// The minimum value.
		/// </summary>
		public int min = 0;

		/// <summary>
		/// The maximum value.
		/// </summary>
		public int max = 100;

		void OnDestroy() {
			if (!string.IsNullOrEmpty(variable)) {
				int oldValue = DialogueLua.GetVariable(variable).AsInt;
				int newValue = Mathf.Clamp(oldValue + increment, min, max);
				DialogueLua.SetVariable(variable, newValue);
				if (DialogueManager.Instance != null) {
					DialogueManager.Instance.BroadcastMessage("UpdateTracker", SendMessageOptions.DontRequireReceiver);
				}
			}
		}

	}

}