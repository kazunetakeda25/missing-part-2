using UnityEngine;

namespace PixelCrushers.DialogueSystem.Examples {

	/// <summary>
	/// When you attach this script to an actor, conversations involving that actor will be
	/// logged to the console.
	/// </summary>
	public class ConversationLogger : MonoBehaviour {
		
		public void OnConversationStart(Transform actor) {
			Debug.Log(string.Format("{0}: Starting conversation with {1}", name, actor.name));
		}
		
		public void OnConversationLine(Subtitle subtitle) {
			if (string.IsNullOrEmpty(subtitle.formattedText.text)) return;
			Debug.Log(string.Format("<color={0}>{1}: {2}</color>", GetActorColor(subtitle), subtitle.speakerInfo.transform.name, subtitle.formattedText.text));
		}
		
		public void OnConversationEnd(Transform actor) {
			Debug.Log(string.Format("{0}: Ending conversation with {1}", name, actor.name));
		}
		
		private string GetActorColor(Subtitle subtitle) {
			return subtitle.speakerInfo.IsPlayer ? "blue" : "red";
		}
		
	}

}
