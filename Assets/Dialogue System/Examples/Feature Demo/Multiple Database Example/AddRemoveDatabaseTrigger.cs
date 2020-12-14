using UnityEngine;

namespace PixelCrushers.DialogueSystem.Examples {
	
	/// <summary>
	/// Adds a dialogue database to the master database on trigger enter, and removes the database
	/// on trigger exit.
	/// </summary>
	public class AddRemoveDatabaseTrigger : MonoBehaviour {
		
		/// <summary>
		/// The database to add/remove.
		/// </summary>
		public DialogueDatabase database;
		
		/// <summary>
		/// The condition that must be true in order to add/remove the database. You could, for 
		/// example, limit the trigger to only work if the Player GameObject enters or exits it.
		/// </summary>
		public Condition condition;
		
		/// <summary>
		/// If <c>true</c>, temporarily sets the debug level to Info while adding and removing
		/// the database.
		/// </summary>
		public bool debug = false;
	
		/// <summary>
		/// When something enters the trigger, this method adds the database if the condition is true.
		/// </summary>
		/// <param name='other'>
		/// Other.
		/// </param>
		void OnTriggerEnter(Collider other) {
			if (condition.IsTrue(other.transform)) {
				DialogueDebug.DebugLevel originalLevel = DialogueManager.DebugLevel;
				if (debug) {
					DialogueManager.DebugLevel = DialogueDebug.DebugLevel.Info;
					Debug.Log(string.Format("{0}: Adding database '{1}'", DialogueDebug.Prefix, database.name));
				}
				DialogueManager.AddDatabase(database);
				DialogueManager.DebugLevel = originalLevel;
			}
		}
		
		/// <summary>
		/// When something exits the trigger, this method removes the database if the condition is true.
		/// </summary>
		/// <param name='other'>
		/// Other.
		/// </param>
		void OnTriggerExit(Collider other) {
			if (condition.IsTrue(other.transform)) {
				DialogueDebug.DebugLevel originalLevel = DialogueManager.DebugLevel;
				if (debug) {
					DialogueManager.DebugLevel = DialogueDebug.DebugLevel.Info;
					Debug.Log(string.Format("{0}: Removing database '{1}'", DialogueDebug.Prefix, database.name));
				}
				DialogueManager.RemoveDatabase(database);
				DialogueManager.DebugLevel = originalLevel;
			}
		}
		
	}

}