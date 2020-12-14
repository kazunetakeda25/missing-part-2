using UnityEngine;
using System.Collections;

namespace MissingComplete
{
	public class CheckpointSaver : MonoBehaviour 
	{
		[SerializeField] int checkpointToSave;

		private void Start()
		{
			if(SessionManager.Instance.HasInit == false)
				return;

			if(SaveGameManager.Instance == null)
				return;

			SaveGameManager.Instance.GetCurrentSaveGame().checkPoint = checkpointToSave;
			SaveGameManager.Instance.SaveCurrentGame();

			Cursor.visible = true;
		}
	}
}
