/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy and remove the lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]". Then add your code where the comments indicate.
 * [REMOVE THIS LINE]



using UnityEngine;
using PixelCrushers.DialogueSystem;

public class PersistentDataTemplate : MonoBehaviour {
	
	public void OnRecordPersistentData() {
		// Add your code here to record data into the Lua environment.
		// Typically, you'll record the data using a line similar to:
		// Lua.Run(string.Format("Actor[\"{0}\"].someField = \"{1}\"", DialogueLua.StringToTableIndex(name), someData));
	}
	
	public void OnApplyPersistentData() {
		// Add your code here to get data from Lua and apply it (usually to the game object).
		// Typically, you'll use a line similar to:
		// myProperty = Lua.Run(string.Format("return Actor[\"{0}\"].someField", DialogueLua.StringToTableIndex(name)));
	}
	
}



/**/