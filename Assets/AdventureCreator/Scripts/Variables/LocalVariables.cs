/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"LocalVariables.cs"
 * 
 *	This script stores Local variables per-scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	[System.Serializable]
	public class LocalVariables : MonoBehaviour
	{
		
		public List<GVar> localVars = new List<GVar>();


		public void BackupAllValues ()
		{
			foreach (GVar _var in localVars)
			{
				_var.BackupValue ();
			}
		}


		public static GVar GetVariable (int _id)
		{
			if (KickStarter.localVariables)
			{
				foreach (GVar _var in KickStarter.localVariables.localVars)
				{
					if (_var.id == _id)
					{
						return _var;
					}
				}
			}
			
			return null;
		}


		public static void BackupAll ()
		{
			if (KickStarter.localVariables)
			{
				foreach (GVar _var in KickStarter.localVariables.localVars)
				{
					_var.BackupValue ();
				}
			}
		}


		public static List<GVar> GetAllVars ()
		{
			if (KickStarter.localVariables)
			{
				return KickStarter.localVariables.localVars;
			}
			return null;
		}
		

		public static int GetIntegerValue (int _id)
		{
			return LocalVariables.GetVariable (_id).val;
		}
		
		
		public static bool GetBooleanValue (int _id)
		{
			if (LocalVariables.GetVariable (_id).val == 1)
			{
				return true;
			}
			return false;
		}
		
		
		public static string GetStringValue (int _id)
		{
			return LocalVariables.GetVariable (_id).textVal;
		}
		
		
		public static float GetFloatValue (int _id)
		{
			return LocalVariables.GetVariable (_id).floatVal;
		}
		
		
		public static void SetIntegerValue (int _id, int _value)
		{
			LocalVariables.GetVariable (_id).val = _value;
		}
		
		
		public static void SetBooleanValue (int _id, bool _value)
		{
			if (_value)
			{
				LocalVariables.GetVariable (_id).val = 1;
			}
			else
			{
				LocalVariables.GetVariable (_id).val = 0;
			}
		}
		
		
		public static void SetStringValue (int _id, string _value)
		{
			LocalVariables.GetVariable (_id).textVal = _value;
		}
		
		
		public static void SetFloatValue (int _id, float _value)
		{
			LocalVariables.GetVariable (_id).floatVal = _value;
		}


		public static string GetPopupValue (int _id)
		{
			return LocalVariables.GetVariable (_id).GetValue ();
		}
		
		
		public static void SetPopupValue (int _id, int _value)
		{
			LocalVariables.GetVariable (_id).val = _value;
		}
		
	}

}