/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Recipe.cs"
 * 
 *	This script is a container class for recipes.
 * 
 */

using System.Collections;
using System.Collections.Generic;

namespace AC
{

	[System.Serializable]
	public class Recipe
	{

		public string label;
		public int id;
		public bool isEditing = false;
		public bool autoCreate = true;
		public List<Ingredient> ingredients = new List<Ingredient>();
		public ActionListAsset invActionList;
		public int resultID;
		public bool useSpecificSlots;
		public OnCreateRecipe onCreateRecipe = OnCreateRecipe.JustMoveToInventory;


		public Recipe (int[] idArray)
		{
			isEditing = false;
			ingredients = new List<Ingredient>();
			resultID = 0;
			useSpecificSlots = false;
			invActionList = null;
			autoCreate = true;
			onCreateRecipe = OnCreateRecipe.JustMoveToInventory;

			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
					id ++;
			}
			
			label = "Recipe " + (id + 1).ToString ();
		}

	}


	[System.Serializable]
	public class Ingredient
	{

		public int itemID;
		public int amount;
		public int slotNumber;


		public Ingredient ()
		{
			itemID = 0;
			amount = 1;
			slotNumber = 1;
		}

	}

}