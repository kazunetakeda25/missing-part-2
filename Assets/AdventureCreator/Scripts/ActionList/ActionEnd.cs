/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionEnd.cs"
 * 
 *	This is a container for "end" Action data.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	[System.Serializable]
	public class ActionEnd
	{

		public ResultAction resultAction;
		public int skipAction;
		public Action skipActionActual;
		public Cutscene linkedCutscene;
		public ActionListAsset linkedAsset;


		public ActionEnd ()
		{
			resultAction = ResultAction.Continue;
			skipAction = -1;
			skipActionActual = null;
			linkedCutscene = null;
			linkedAsset = null;
		}


		public ActionEnd (ActionEnd _actionEnd)
		{
			resultAction = _actionEnd.resultAction;
			skipAction = _actionEnd.skipAction;
			skipActionActual = _actionEnd.skipActionActual;
			linkedCutscene = _actionEnd.linkedCutscene;
			linkedAsset = _actionEnd.linkedAsset;
		}


		public ActionEnd (int _skipAction)
		{
			resultAction = ResultAction.Continue;
			skipAction = _skipAction;
			skipActionActual = null;
			linkedCutscene = null;
			linkedAsset = null;
		}

	}

}