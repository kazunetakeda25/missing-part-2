using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.Examples {

	/// <summary>
	/// This component indicates that the game object is usable. This component works in
	/// conjunction with the Selector component. If you leave overrideName blank but there
	/// is an OverrideActorName component on the same object, this component will use
	/// the name specified in OverrideActorName.
	/// </summary>
	public class Usable : MonoBehaviour {
		
		/// <summary>
		/// (Optional) Overrides the name shown by the Selector.
		/// </summary>
		public string overrideName;
		
		/// <summary>
		/// (Optional) Overrides the use message shown by the Selector.
		/// </summary>
		public string overrideUseMessage;
		
		/// <summary>
		/// The max distance at which the object can be used.
		/// </summary>
		public float maxUseDistance = 5f;

		public void Start() {
			if (string.IsNullOrEmpty(overrideName)) {
				OverrideActorName overrideActorName = GetComponentInChildren<OverrideActorName>();
				if (overrideActorName != null) overrideName = overrideActorName.overrideName;
			}
		}

	}

}
