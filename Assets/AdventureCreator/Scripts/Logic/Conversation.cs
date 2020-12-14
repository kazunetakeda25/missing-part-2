/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013
 *	
 *	"Conversation.cs"
 * 
 *	This script is handles character conversations.
 *	It generates instances of DialogOption for each line
 *	that the player can choose to say.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class Conversation : MonoBehaviour
	{

		public InteractionSource interactionSource;
		public List<ButtonDialog> options = new List<ButtonDialog>();
		public ButtonDialog selectedOption;

		public bool isTimed = false;
		public bool autoPlay = false;
		public float timer = 5f;
		public int defaultOption = 0;

		private float startTime;

		
		private void Awake ()
		{
			Upgrade ();
		}


		public void Interact (ActionConversation actionConversation = null)
		{
			KickStarter.actionListManager.SetConversationPoint (actionConversation);

			CancelInvoke ("RunDefault");
			int numPresent = 0;
			foreach (ButtonDialog _option in options)
			{
				if (_option.isOn)
				{
					numPresent ++;
				}
			}
			
			if (KickStarter.playerInput)
			{
				if (numPresent == 1 && autoPlay)
				{
					foreach (ButtonDialog _option in options)
					{
						if (_option.isOn)
						{
							RunOption (_option);
							return;
						}
					}
				}
				else if (numPresent > 0)
				{
					KickStarter.playerInput.activeConversation = this;
					KickStarter.stateHandler.gameState = GameState.DialogOptions;
				}
				else
				{
					KickStarter.playerInput.activeConversation = null;
				}
			}
			
			if (isTimed)
			{
				startTime = Time.time;
				Invoke ("RunDefault", timer);
			}
		}


		private void RunOption (ButtonDialog _option)
		{
			if (options.Contains (_option))
			{
				if (KickStarter.actionListManager.OverrideConversation (options.IndexOf (_option)))
				{
					return;
				}
			}

			Conversation endConversation;
			if (_option.conversationAction == ConversationAction.ReturnToConversation)
			{
				endConversation = this;
			}
			else if (_option.conversationAction == ConversationAction.RunOtherConversation && _option.newConversation != null)
			{
				endConversation = _option.newConversation;
			}
			else
			{
				endConversation = null;
			}
			
			if (interactionSource == InteractionSource.AssetFile && _option.assetFile)
			{
				AdvGame.RunActionListAsset (_option.assetFile, endConversation);
			}
			else if (interactionSource == InteractionSource.CustomScript)
			{
				if (_option.customScriptObject != null && _option.customScriptFunction != "")
				{
					_option.customScriptObject.SendMessage (_option.customScriptFunction);
				}
			}
			else if (interactionSource == InteractionSource.InScene && _option.dialogueOption)
			{
				_option.dialogueOption.conversation = endConversation;
				_option.dialogueOption.Interact ();
			}
			else
			{
				Debug.Log ("No Interaction object found!");
				KickStarter.stateHandler.gameState = GameState.Normal;
			}
		}
		
		
		public void TurnOn ()
		{
			Interact ();
		}
		
		
		public void TurnOff ()
		{
			if (KickStarter.playerInput)
			{
				CancelInvoke ("RunDefault");
				KickStarter.playerInput.activeConversation = null;
			}
		}
		
		
		private void RunDefault ()
		{
			if (KickStarter.playerInput && KickStarter.playerInput.activeConversation != null && options.Count > defaultOption && defaultOption > -1)
			{
				KickStarter.playerInput.activeConversation = null;
				RunOption (options[defaultOption]);
			}
		}
		
		
		private IEnumerator RunOptionCo (int i)
		{
			yield return new WaitForSeconds (0.3f);
			RunOption (options[i]);
		}
		
		
		public void RunOption (int slot)
		{
			CancelInvoke ("RunDefault");
			int i = ConvertSlotToOption (slot);
			if (i == -1)
			{
				return;
			}

			if (KickStarter.playerInput)
			{
				KickStarter.playerInput.activeConversation = null;
			}
			
			StartCoroutine (RunOptionCo (i));
		}
		
		
		public float GetTimeRemaining ()
		{
			return ((startTime + timer - Time.time) / timer);
		}
		
		
		private int ConvertSlotToOption (int slot)
		{
			int foundSlots = 0;
			
			for (int j=0; j<options.Count; j++)
			{
				if (options[j].isOn)
				{
					foundSlots ++;
					if (foundSlots == (slot+1))
					{
						return j;
					}
				}
			}
			
			return -1;
		}
		
		
		public string GetOptionName (int slot)
		{
			int i = ConvertSlotToOption (slot);
			if (i == -1)
			{
				i = 0;
			}

			return (SpeechManager.GetTranslation (options[i].label, options[i].lineID, Options.GetLanguage ()));
		}
		
		
		public Texture2D GetOptionIcon (int slot)
		{
			int i = ConvertSlotToOption (slot);
			if (i == -1)
			{
				i = 0;
			}
			return options[i].icon;
		}
		
		
		public void SetOption (int id, bool flag, bool isLocked)
		{
			foreach (ButtonDialog option in options)
			{
				if (option.ID == id)
				{
					if (!option.isLocked)
					{
						option.isLocked = isLocked;
						option.isOn = flag;
					}
					break;
				}
			}
		}
		
		
		public int GetCount ()
		{
			int numberOn = 0;
			foreach (ButtonDialog _option in options)
			{
				if (_option.isOn)
				{
					numberOn ++;
				}
			}
			return numberOn;
		}
		
		
		public bool[] GetOptionStates ()
		{
			List<bool> states = new List<bool>();
			foreach (ButtonDialog _option in options)
			{
				states.Add (_option.isOn);
			}
			return states.ToArray ();
		}
		
		
		public bool[] GetOptionLocks ()
		{
			List<bool> locks = new List<bool>();
			foreach (ButtonDialog _option in options)
			{
				locks.Add (_option.isLocked);
			}
			return locks.ToArray ();
		}
		
		
		public void SetOptionStates (bool[] states)
		{
			for (int i=0; i<options.Count; i++)
			{
				if (states.Length > i)
				{
					options[i].isOn = states[i];
				}
			}
		}
		
		
		public void SetOptionLocks (bool[] locks)
		{
			for (int i=0; i<options.Count; i++)
			{
				if (locks.Length > i)
				{
					options[i].isLocked = locks[i];
				}
			}
		}


		public void Upgrade ()
		{
			// Set IDs as index + 1 (because default is 0 when not upgraded)
			if (options.Count > 0 && options[0].ID == 0)
			{
				for (int i=0; i<options.Count; i++)
				{
					options[i].ID = i+1;
				}
				#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					Debug.Log ("Conversation '" + gameObject.name + "' has been temporarily upgraded - please view it's Inspector when the game ends and save the scene.");
				}
				else
				{
					UnityEditor.EditorUtility.SetDirty (this);
					if (!this.gameObject.activeInHierarchy)
					{
						// Asset file
						UnityEditor.AssetDatabase.SaveAssets ();
					}
					Debug.Log ("Upgraded Conversation '" + gameObject.name + "', please save the scene.");
				}
				#endif
			}
		}


		public int[] GetIDArray ()
		{
			List<int> idArray = new List<int>();
			foreach (ButtonDialog option in options)
			{
				idArray.Add (option.ID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}

	}

}