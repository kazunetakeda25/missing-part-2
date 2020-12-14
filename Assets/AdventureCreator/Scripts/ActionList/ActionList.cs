/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionList.cs"
 * 
 *	This script stores, and handles the sequentual triggering of, actions.
 *	It is derived by Cutscene, Hotspot, Trigger, and DialogOption.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace AC
{
	
	[System.Serializable]
	public class ActionList : MonoBehaviour
	{
		[HideInInspector] public bool isSkippable = true;
		[HideInInspector] public float triggerTime = 0f;
		[HideInInspector] public bool autosaveAfter = false;
		[HideInInspector] public ActionListType actionListType = ActionListType.PauseGameplay;
		[HideInInspector] public List<AC.Action> actions = new List<AC.Action>();
		[HideInInspector] public Conversation conversation = null;
		[HideInInspector] public ActionListAsset assetFile;
		[HideInInspector] public ActionListSource source;
		[HideInInspector] public bool useParameters = false;
		[HideInInspector] public bool unfreezePauseMenus = true;
		[HideInInspector] public List<ActionParameter> parameters = new List<ActionParameter>();
		
		protected bool isSkipping = false;
		protected LayerMask LayerHotspot;
		protected LayerMask LayerOff;


		private void Awake ()
		{
			LayerHotspot = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
			LayerOff = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
			
			// If asset-based, download actions
			if (source == ActionListSource.AssetFile)
			{
				actions.Clear ();
				if (assetFile != null && assetFile.actions.Count > 0)
				{
					foreach (AC.Action action in assetFile.actions)
					{
						actions.Add (action);
					}
					useParameters = assetFile.useParameters;
					parameters = assetFile.parameters;
				}
			}
			
			if (useParameters)
			{
				// Reset all parameters
				foreach (ActionParameter _parameter in parameters)
				{
					_parameter.Reset ();
				}
			}
		}
		
		
		private void Start ()
		{
			/*if (!useParameters)
			{
				foreach (Action action in actions)
				{
					if (action != null)
					{
						action.AssignValues (null);
					}
				}
			} */
		}
		
		
		public virtual void Interact ()
		{
			Interact (0, true);
		}
		
		
		public void Interact (int i, bool addToSkipQueue)
		{
			if (actions.Count > 0 && actions.Count > i)
			{
				if (triggerTime > 0f && i == 0)
				{
					StartCoroutine ("PauseUntilStart", addToSkipQueue);
				}
				else
				{
					Reset ();
					ResetSkips ();
					BeginActionList (i, addToSkipQueue);
				}
			}
		}
		
		
		private IEnumerator PauseUntilStart (bool addToSkipQueue)
		{
			if (triggerTime > 0f)
			{
				yield return new WaitForSeconds (triggerTime);
			}

			//Kill ();
			Reset ();
			ResetSkips ();
			BeginActionList (0, addToSkipQueue);
		}
		
		
		private void ResetSkips ()
		{
			// "lastResult" is used to backup Check results when skipping
			foreach (Action action in actions)
			{
				if (action != null)
				{
					action.lastResult.skipAction = -10;
				}
			}
		}
		
		
		private void BeginActionList (int i, bool addToSkipQueue)
		{
			if (KickStarter.actionListManager)
			{
				KickStarter.actionListManager.AddToList (this, addToSkipQueue, i);
				ProcessAction (i);
			}
			else
			{
				Debug.LogWarning ("Cannot run " + this.name + " because no ActionListManager was found.");
			}
		}
		
		
		private void ProcessAction (int i)
		{
			if (i >= 0 && i < actions.Count && actions[i] != null && actions[i] is Action)
			{
				// Action exists
				if (!actions [i].isEnabled)
				{
					// Disabled, try next
					ProcessAction (i+1);
				}
				else
				{
					// Run it
					StartCoroutine ("RunAction", actions [i]);
				}
			}
			else
			{
				CheckEndCutscene ();
			}
		}
		
		
		private IEnumerator RunAction (Action action)
		{
			if (useParameters)
			{
				action.AssignValues (parameters);
			}
			else
			{
				action.AssignValues (null);
			}
			
			if (isSkipping)
			{
				action.Skip ();
			}
			else
			{
				if (action is ActionRunActionList)
				{
					ActionRunActionList actionRunActionList = (ActionRunActionList) action;
					actionRunActionList.isSkippable = IsSkippable ();
				}

				action.isRunning = false;
				float waitTime = action.Run ();	

				if (action is ActionParallel)
				{}
				else if (!(action is ActionQTE) && (action is ActionCheck || action is ActionCheckMultiple))
				{
					yield return new WaitForFixedUpdate ();
				}
				else if (waitTime > 0f)
				{
					while (action.isRunning)
					{
						if (this is RuntimeActionList && actionListType == ActionListType.PauseGameplay && !unfreezePauseMenus)
						{
							float endTime = Time.realtimeSinceStartup + waitTime;
							while (Time.realtimeSinceStartup < endTime)
							{
								yield return null;
							}
						}
						else
						{
							yield return new WaitForSeconds (waitTime);
						}

						waitTime = action.Run ();
					}
				}
			}

			if (action is ActionParallel)
			{
				EndActionParallel ((ActionParallel) action);
			}
			else
			{
				EndAction (action);
			}
		}


		private void EndAction (Action action)
		{
			action.isRunning = false;
			
			ActionEnd actionEnd = action.End (this.actions);
			if (isSkipping && action.lastResult.skipAction != -10 && (action is ActionCheck || action is ActionCheckMultiple))
			{
				// When skipping an ActionCheck that has already run, revert to previous result
				actionEnd = new ActionEnd (action.lastResult);
			}
			else
			{
				action.SetLastResult (new ActionEnd (actionEnd));
				ReturnLastResultToSource (actionEnd, actions.IndexOf (action));
			}

			ProcessActionEnd (actionEnd, actions.IndexOf (action));
		}


		private void ProcessActionEnd (ActionEnd actionEnd, int i)
		{
			if (actionEnd.resultAction == ResultAction.RunCutscene)
			{
				if (actionEnd.linkedAsset != null)
				{
					if (isSkipping)
					{
						AdvGame.SkipActionListAsset (actionEnd.linkedAsset);
					}
					else
					{
						AdvGame.RunActionListAsset (actionEnd.linkedAsset, 0, !IsSkippable ());
					}
					CheckEndCutscene ();
				}
				else if (actionEnd.linkedCutscene != null)
				{
					if (actionEnd.linkedCutscene != this)
					{
						if (isSkipping)
						{
							actionEnd.linkedCutscene.Skip ();
						}
						else
						{
							actionEnd.linkedCutscene.Interact (0, !IsSkippable ());
						}
						CheckEndCutscene ();
					}
					else
					{
						if (triggerTime > 0f)
						{
							Kill ();
							StartCoroutine ("PauseUntilStart", !IsSkippable ());
						}
						else
						{
							ProcessAction (0);
						}
					}
				}
			}
			else if (actionEnd.resultAction == ResultAction.Stop)
			{
				CheckEndCutscene ();
			}
			else if (actionEnd.resultAction == ResultAction.Skip)
			{
				ProcessAction (actionEnd.skipAction);
			}
			else if (actionEnd.resultAction == ResultAction.Continue)
			{
				ProcessAction (i+1);
			}
		}


		private void EndActionParallel (ActionParallel actionParallel)
		{
			actionParallel.isRunning = false;
			ActionEnd[] actionEnds = actionParallel.Ends (this.actions, isSkipping);

			foreach (ActionEnd actionEnd in actionEnds)
			{
				ProcessActionEnd (actionEnd, actions.IndexOf (actionParallel));
			}
		}


		private IEnumerator EndCutscene ()
		{
			yield return new WaitForEndOfFrame ();

			if (AreActionsRunning ())
			{
				yield break;
			}

			Kill ();
		}


		private void CheckEndCutscene ()
		{
			if (!AreActionsRunning ())
			{
				StartCoroutine ("EndCutscene");
			}
		}


		public bool AreActionsRunning ()
		{
			foreach (Action action in actions)
			{
				if (action != null && action.isRunning)
				{
					return true;
				}
			}
			return false;
		}

		
		private void TurnOn ()
		{
			gameObject.layer = LayerHotspot;
		}
		
		
		private void TurnOff ()
		{
			gameObject.layer = LayerOff;
		}
		
		
		public void Reset ()
		{
			isSkipping = false;
			StopCoroutine ("PauseUntilStart");
			StopCoroutine ("RunAction");
			StopCoroutine ("EndCutscene");

			foreach (Action action in actions)
			{
				if (action != null)
				{
					action.isRunning = false;
				}
			}
		}
		
		
		public void Kill ()
		{
			StopCoroutine ("PauseUntilStart");
			StopCoroutine ("RunAction");
			StopCoroutine ("EndCutscene");

			KickStarter.actionListManager.EndList (this);
		}
		
		
		public void Skip ()
		{
			Skip (0);
		}


		public void Skip (int i)
		{
			if (i < 0 || actions.Count <= i)
			{
				return;
			}

			if (actionListType == ActionListType.RunInBackground || !isSkippable)
			{
				// Can't skip, so just run normally
				Interact ();
				return;
			}

			// Already running
			if (!isSkipping)
			{
				isSkipping = true;
				StopCoroutine ("PauseUntilStart");
				StopCoroutine ("RunAction");
				StopCoroutine ("EndCutscene");
				
				BeginActionList (i, false);
			}
		}


		public void Initialise ()
		{
			actions.Clear ();
			if (actions == null || actions.Count < 1)
			{
				actions.Add (GetDefaultAction ());
			}
		}


		public static AC.Action GetDefaultAction ()
		{
			if (AdvGame.GetReferences ().actionsManager)
			{
				string defaultAction = AdvGame.GetReferences ().actionsManager.GetDefaultAction ();
				return ((AC.Action) ScriptableObject.CreateInstance (defaultAction));
			}
			else
			{
				Debug.LogError ("Cannot create Action - no Actions Manager found.");
				return null;
			}
		}


		protected void ReturnLastResultToSource (ActionEnd _lastResult, int i)
		{}


		public bool IsSkippable ()
		{
			if (isSkippable && actionListType == ActionListType.PauseGameplay)
			{
				return true;
			}
			return false;
		}


		public List<Action> GetActions ()
		{
			if (source == ActionListSource.AssetFile)
			{
				if (assetFile)
				{
					return assetFile.actions;
				}
			}
			else
			{
				return actions;
			}
			return null;
		}

	}
	
}
