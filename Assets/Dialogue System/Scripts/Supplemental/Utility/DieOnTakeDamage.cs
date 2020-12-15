using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.Examples {
	
	public class DieOnTakeDamage : MonoBehaviour {
		
		public GameObject deadPrefab;
		
		void TakeDamage(float damage) {
			if (deadPrefab != null) {
				GameObject dead = Instantiate(deadPrefab, transform.position, transform.rotation) as GameObject;
				if (dead != null) dead.transform.parent = transform.parent;
			}
			Destroy(gameObject);
		}
	
	}
	
}
