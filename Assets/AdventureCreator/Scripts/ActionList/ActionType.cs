/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionType.cs"
 * 
 *	This defines the variables needed by the ActionsManager Editor Window.
 * 
 */

namespace AC
{

	[System.Serializable]
	public class ActionType
	{
		
		public string fileName;
		public ActionCategory category;
		public string title;
		public string description;
		public bool isEnabled;
		
		
		public ActionType (string _fileName, Action _action)
		{
			fileName = _fileName;
			category = _action.category;
			title = _action.title;
			description = _action.description;
			isEnabled = true;
		}


		public string GetFullTitle (bool forSorting = false)
		{
			if (forSorting)
			{
				if (category == ActionCategory.Custom)
				{
					return ("ZZ" + title);
				}
			}
			return (category.ToString () + ": " + title);
		}
		
	}

}