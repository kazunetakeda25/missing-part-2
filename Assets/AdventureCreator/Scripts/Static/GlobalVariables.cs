/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"GlobalVariables.cs"
 * 
 *	This script contains static functions to access Global Variables at runtime.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class GlobalVariables : MonoBehaviour
	{

		public static List<GVar> GetAllVars ()
		{
			if (KickStarter.runtimeVariables)
			{
				return KickStarter.runtimeVariables.globalVars;
			}
			return null;
		}


		public static void BackupAll ()
		{
			if (KickStarter.localVariables)
			{
				foreach (GVar _var in KickStarter.runtimeVariables.globalVars)
				{
					_var.BackupValue ();
				}
			}
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
		
		
		public static string GetPopupValue (int _id)
		{
			return RuntimeVariables.GetVariable (_id).GetValue ();
		}


		public static void SetPopupValue (int _id, int _value)
		{
			RuntimeVariables.GetVariable (_id).val = _value;
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

	}

}