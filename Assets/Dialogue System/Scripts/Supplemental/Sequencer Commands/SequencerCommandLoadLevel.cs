using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands {

	/// <summary>
	/// This script implements the sequencer command LoadLevel(levelName).
	/// Before loading the level, it calls PersistentDataManager.Record() to
	/// tell objects in the current level to record their persistent data first.
	/// </summary>
	public class SequencerCommandLoadLevel : SequencerCommand {
		
		public void Start() {
			string levelName = GetParameter(0);
			if (string.IsNullOrEmpty(levelName)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: LoadLevel() level name is an empty string", DialogueDebug.Prefix));
			} else {
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: LoadLevel({1})", DialogueDebug.Prefix, levelName));
				PersistentDataManager.Record();
				UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
			}
			Stop();
		}
	}
}
