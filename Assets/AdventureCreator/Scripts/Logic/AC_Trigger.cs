/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Trigger.cs"
 * 
 *	This ActionList runs when the Player enters it.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	[System.Serializable]
	public class AC_Trigger : ActionList
	{
		
		public TriggerDetects detects = TriggerDetects.Player;
		public string detectComponent = "";
		public GameObject obToDetect;
		
		public int triggerType;
		public bool showInEditor = false;
		public bool cancelInteractions = false;
		

		private void Interact (GameObject collisionOb)
		{
			if (cancelInteractions)
			{
				KickStarter.playerInteraction.StopInteraction ();
			}
			
			if (actionListType == ActionListType.PauseGameplay)
			{
				KickStarter.playerInteraction.DisableHotspot (false);
			}
			
			// Set correct parameter
			if (useParameters && parameters.Count == 1)
			{
				parameters[0].gameObject = collisionOb;
			}
			
			Interact ();
		}
		
		
		private void OnTriggerEnter (Collider other)
		{
			if (triggerType == 0 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerEnter2D (Collider2D other)
		{
			if (triggerType == 0 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerStay (Collider other)
		{
			if (triggerType == 1 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerStay2D (Collider2D other)
		{
			if (triggerType == 1 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerExit (Collider other)
		{
			if (triggerType == 2 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerExit2D (Collider2D other)
		{
			if (triggerType == 2 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		public void TurnOn ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = true;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = true;
			}
			else
			{
				Debug.LogWarning ("Cannot turn " + this.name + " on because it has no Collider component.");
			}
		}
		
		
		public void TurnOff ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = false;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = false;
			}
			else
			{
				Debug.LogWarning ("Cannot turn " + this.name + " off because it has no Collider component.");
			}
		}
		
		
		private bool IsObjectCorrect (GameObject obToCheck)
		{
			if (KickStarter.stateHandler.gameState != GameState.Normal || obToCheck == null)
			{
				return false;
			}
			
			if (KickStarter.stateHandler.triggerIsOff)
			{
				return false;
			}
			
			if (detects == TriggerDetects.Player)
			{
				if (obToCheck.CompareTag (Tags.player))
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.SetObject)
			{
				if (obToDetect != null && obToCheck == obToDetect)
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.AnyObjectWithComponent)
			{
				if (detectComponent != "" && obToCheck.GetComponent (detectComponent))
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.AnyObject)
			{
				return true;
			}
			
			return false;
		}
		
		
		private void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		private void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}
		
		
		private void DrawGizmos ()
		{
			AdvGame.DrawCubeCollider (transform, new Color (1f, 0.3f, 0f, 0.8f));
		}

	}
	
}
