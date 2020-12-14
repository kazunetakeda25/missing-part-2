using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionDaikonSpeech : AC.Action {
	public ACCommunicator displayCommunicator;
	public bool isPlayer;
	public AC.Char speaker;
	public string messageText;
	public int lineID;
	public bool isBackground = false;
	public AnimationClip headClip;
	public AnimationClip mouthClip;
	public float Height;
	public float Width;
	public Vector3 position;
	public bool partOfPriming = false;

	public float voDelay;
	public float animEndEarly;

	//private Dialog dialog;
	private AC.StateHandler stateHandler;
	private AC.SpeechManager speechManager;
	private AC.Options options;
	
	
	public ActionDaikonSpeech ()
	{
		this.isDisplayed = true;
		title = "Dialogue: Daikon speech";
		lineID = -1;
	}
	
	
	override public float Run ()
	{
		displayCommunicator = ACCommunicator.Instance;
		stateHandler = AC.KickStarter.stateHandler;
		options = AC.KickStarter.options;
		
		if (displayCommunicator && stateHandler && options)
		{
			if (!isRunning)
			{
				isRunning = true;
				
				string _text = messageText;

				if (_text != "")
				{
					//dialog.KillDialog ();
					//displayCommunicator.Stop();
					Vector3 newposition = new Vector3( Screen.width * position.x/100f, Screen.height * position.y /100f);
					float newHeight = Screen.height * Height/100f;
					float newWidth = Screen.width * Width/100f;
					if (isBackground)
					{
						stateHandler.gameState = AC.GameState.Normal;
					}
					else
					{
						stateHandler.gameState = AC.GameState.Cutscene;
					}
					
					if (isPlayer)
					{
						speaker = AC.KickStarter.player;
					}
					
					if (speaker)
					{
						displayCommunicator.ShowDialog(lineID, speaker, newposition, newHeight, newWidth,messageText, isBackground);
						
						if (headClip || mouthClip)
						{
							AC.AdvGame.CleanUnusedClips (speaker.GetComponent <Animation>());	
							
							if (headClip)
							{
								AC.AdvGame.PlayAnimClip 
								(
									speaker.GetComponent <Animation>(), 
									(int) AC.AnimLayer.Head, 
									headClip, 
									AnimationBlendMode.Additive, 
									WrapMode.Clamp, 
									0f, 
									speaker.neckBone,
									false
								);
							}
							
							if (mouthClip)
							{
								AC.AdvGame.PlayAnimClip (
									speaker.GetComponent <Animation>(), 
									(int) AC.AnimLayer.Mouth, 
									mouthClip, 
									AnimationBlendMode.Blend, 
									WrapMode.Clamp, 
									0f, 
									speaker.neckBone, 
									false);
							}
						}
					}
					else
					{
						if(partOfPriming)
							displayCommunicator.ShowDialog(newposition, newHeight, newWidth,messageText,partOfPriming);
						else
							displayCommunicator.ShowDialog(newposition, newHeight, newWidth,messageText);
					}
					
					if (!isBackground)
					{
						return defaultPauseTime;
					}
				}
				
				return 0f;
			}
			else
			{
				if (!displayCommunicator.isMessageAlive)
				{
					isRunning = false;
					stateHandler.gameState = AC.GameState.Cutscene;
					return 0f;
				}
				else
				{
					return defaultPauseTime;
				}
			}
		}
		
		return 0f;
	}
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		
		if (lineID > -1)
		{
			EditorGUILayout.LabelField ("Speech Manager ID:", lineID.ToString ());
		}
		
		isPlayer = EditorGUILayout.Toggle ("Player line?",isPlayer);
		if (!isPlayer)
		{
			speaker = (AC.Char) EditorGUILayout.ObjectField ("Speaker:", speaker, typeof(AC.Char), true);
		}

		position = EditorGUILayout.Vector3Field ("Display Position", position);
		Height = EditorGUILayout.FloatField ("Height", Height);
		Width = EditorGUILayout.FloatField("Width", Width);

		animEndEarly = EditorGUILayout.FloatField ("End Animation At: ", animEndEarly);
		voDelay = EditorGUILayout.FloatField("Delay VO", voDelay);

		messageText = EditorGUILayout.TextField ("Line text:", messageText);
		
		headClip = (AnimationClip) EditorGUILayout.ObjectField ("Head animation:", headClip, typeof (AnimationClip), true);
		mouthClip = (AnimationClip) EditorGUILayout.ObjectField ("Mouth animation:", mouthClip, typeof (AnimationClip), true);
		
		isBackground = EditorGUILayout.Toggle ("Play in background?", isBackground);
		partOfPriming = EditorGUILayout.Toggle ("Part of priming?", partOfPriming);
		
		AfterRunningOption ();
	}
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (messageText != "")
		{
			string shortMessage = messageText;
			if (shortMessage != null && shortMessage.Length > 30)
			{
				shortMessage = shortMessage.Substring (0, 28) + "..";
			}
			
			labelAdd = " (" + shortMessage + ")";
		}
		
		return labelAdd;
	}
	
	#endif
}
