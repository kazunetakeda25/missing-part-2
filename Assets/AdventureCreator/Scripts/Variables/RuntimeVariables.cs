/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"RuntimeVariables.cs"
 * 
 *	This script creates a local copy of the VariableManager's Global vars.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class RuntimeVariables : MonoBehaviour
	{
		
		public List<GVar> globalVars = new List<GVar>();
		private List<SpeechLog> speechLines = new List<SpeechLog>();

		
		public void Start ()
		{
			// Transfer the vars set in VariablesManager to self on runtime
			UpdateSelf ();
			LinkAllValues ();
		}


		public void BackupAllValues ()
		{
			foreach (GVar _var in globalVars)
			{
				_var.BackupValue ();
			}
		}


		public SpeechLog[] GetSpeechLog ()
		{
			return speechLines.ToArray ();
		}


		public void ClearSpeechLog ()
		{
			speechLines.Clear ();
		}


		public void AddToSpeechLog (SpeechLog _line)
		{
			int ID = _line.lineID;
			if (ID >= 0)
			{
				foreach (SpeechLog speechLine in speechLines)
				{
					if (speechLine.lineID == ID)
					{
						speechLines.Remove (speechLine);
						break;
					}
				}
			}

			speechLines.Add (_line);
		}

		
		private void UpdateSelf ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().variablesManager)
			{
				VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;

				globalVars.Clear ();
				foreach (GVar assetVar in variablesManager.vars)
				{
					globalVars.Add (new GVar (assetVar));
				}

				// Options Variables
				if (GetComponent <Options>() && GetComponent <Options>().optionsData != null && GetComponent <Options>().optionsData.linkedVariables != "")
				{
					SaveSystem.AssignVariables (GetComponent <Options>().optionsData.linkedVariables, true);
				}
			}

		}


		private void LinkAllValues ()
		{
			foreach (GVar var in globalVars)
			{
				if (var.link == VarLink.PlaymakerGlobalVariable)
				{
					if (var.updateLinkOnStart)
					{
						var.Download ();
					}
					else
					{
						var.Upload ();
					}
				}
			}
		}


		public static List<GVar> GetAllVars ()
		{
			if (KickStarter.runtimeVariables)
			{
				return KickStarter.runtimeVariables.globalVars;
			}
			return null;
		}


		public static void UploadAll ()
		{
			if (KickStarter.runtimeVariables)
			{
				foreach (GVar var in KickStarter.runtimeVariables.globalVars)
				{
					var.Upload ();
				}
			}
		}


		public static void DownloadAll ()
		{
			if (KickStarter.runtimeVariables)
			{
				foreach (GVar var in KickStarter.runtimeVariables.globalVars)
				{
					var.Download ();
				}
			}
		}


		public static GVar GetVariable (int _id)
		{
			if (KickStarter.runtimeVariables)
			{
				foreach (GVar _var in KickStarter.runtimeVariables.globalVars)
				{
					if (_var.id == _id)
					{
						return _var;
					}
				}
			}

			return null;
		}


		public static int GetIntegerValue (int _id)
		{
			return RuntimeVariables.GetVariable (_id).val;
		}


		public static bool GetBooleanValue (int _id)
		{
			if (RuntimeVariables.GetVariable (_id).val == 1)
			{
				return true;
			}
			return false;
		}


		public static string GetStringValue (int _id)
		{
			return RuntimeVariables.GetVariable (_id).textVal;
		}


		public static float GetFloatValue (int _id)
		{
			return RuntimeVariables.GetVariable (_id).floatVal;
		}


		public static void SetIntegerValue (int _id, int _value)
		{
			RuntimeVariables.GetVariable (_id).val = _value;
		}
		
		
		public static void SetBooleanValue (int _id, bool _value)
		{
			if (_value)
			{
				RuntimeVariables.GetVariable (_id).val = 1;
			}
			else
			{
				RuntimeVariables.GetVariable (_id).val = 0;
			}
		}
		
		
		public static void SetStringValue (int _id, string _value)
		{
			RuntimeVariables.GetVariable (_id).textVal = _value;
		}
		
		
		public static void SetFloatValue (int _id, float _value)
		{
			RuntimeVariables.GetVariable (_id).floatVal = _value;
		}


		public static string GetPopupValue (int _id)
		{
			return RuntimeVariables.GetVariable (_id).GetValue ();
		}
		
		
		public static void SetPopupValue (int _id, int _value)
		{
			RuntimeVariables.GetVariable (_id).val = _value;
		}

	}

}