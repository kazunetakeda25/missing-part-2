using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConversationDaikon : MonoBehaviour {
	public bool isTimed = false;
	public Quiz quizType = Quiz.NONE;
	public float timer = 5f;
	public AC.ButtonDialog defaultOption;
	
	private float startTime;
	private bool isRunning;
	public List<AC.ButtonDialog> options = new List<AC.ButtonDialog>();
	public List<Vector3> position = new List<Vector3> ();
	public List<float> height = new List<float>();
	string[] labels;
	Vector3[] positions;
	float[] heights;
	public Vector2 pos;
	public float hei;
	
	//private PlayerInput playerInput;
	
	
	void Awake ()
	{
		/*if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
		{
			playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		}*/

	}
	
	
	public void Interact ()
	{
		// End the conversation if no options are present

		bool onePresent = false;
		labels = new string[options.Count];
		heights = new float[options.Count];
		positions = new Vector3[options.Count];
		int count = 0;
		foreach (AC.ButtonDialog _option in options)
		{
			if (_option.isOn)
			{
				onePresent = true;
				positions[count] = position[count];
				heights[count] = height[count];
				labels[count++] = _option.label;
				//_option.dialogueOption.Interact();
			}
		}
		if (onePresent) {
			Vector3 newposition = new Vector3( Screen.width * pos.x/100f, -Screen.height * pos.y /100f);
			float height = Screen.width * hei /100.0f;
			GameObject.Find("AC Communicator").GetComponent<ACCommunicator>().CreateList (labels,newposition,height,this, quizType);
				}
		
		/*if (playerInput)
		{
			if (onePresent)
			{
				playerInput.activeConversation = this;
			}
			else
			{
				playerInput.activeConversation = null;
			}
		}*/
		
		if (isTimed)
		{
			startTime = Time.time;
			StartCoroutine (RunDefault ());
		}
	}
	
	
	public void TurnOn ()
	{
		Interact ();
	}
	
	
	public void TurnOff ()
	{
		/*if (playerInput)
		{
			playerInput.activeConversation = null;
		}*/
	}
	
	
	private IEnumerator RunDefault ()
	{
		yield return new WaitForSeconds (timer);
		
		/*if (playerInput && playerInput.activeConversation != null && defaultOption != null)
		{
			playerInput.activeConversation = null;
			
			if (defaultOption.returnToConversation)
			{
				defaultOption.dialogueOption.conversation = this;
			}
			else
			{
				defaultOption.dialogueOption.conversation = null;
			}
			
			defaultOption.dialogueOption.Interact ();
		}
		*/
	}
	
	
	private IEnumerator RunOptionCo (int i)
	{
		yield return new WaitForSeconds (0.3f);
		
		/*if (options[i].returnToConversation)
		{
			options[i].dialogueOption.conversation = this;
		}
		else
		{
			options[i].dialogueOption.conversation = null;
		}
		*/
		if (options [i].dialogueOption != null)
			options[i].dialogueOption.Interact ();
	}
	
	
	public void RunOption (int slot)
	{
		int i = ConvertSlotToOption (slot);
		
		/*if (playerInput)
		{
			playerInput.activeConversation = null;
		}
		*/
		StartCoroutine (RunOptionCo (i));
	}
	
	
	public float GetTimeRemaining ()
	{
		return ((startTime + timer - Time.time) / timer);
	}
	
	
	private int ConvertSlotToOption (int slot)
	{
		int numberOff = 0;
		for (int j=0; j<=slot; j++)
		{
			if (!options[j].isOn)
			{
				numberOff ++;
			}
		}
		
		int i = slot + numberOff;
		
		while (!options[i].isOn && i < options.Count)
		{
			numberOff++;
			i = slot + numberOff;
		}
		
		return i;
	}
	
	public string GetOptionName (int slot)
	{
		int i = ConvertSlotToOption (slot);
		return options[i].label;
	}
	
	
	public void SetOption (int i, bool flag, bool isLocked)
	{
		if (!options[i].isLocked)
		{
			options[i].isLocked = isLocked;
			options[i].isOn = flag;
		}
	}
	
	
	public int GetCount ()
	{
		int numberOn = 0;
		foreach (AC.ButtonDialog _option in options)
		{
			if (_option.isOn)
			{
				numberOn ++;
			}
		}
		return numberOn;
	}
	
	
	public List<bool> GetOptionStates ()
	{
		List<bool> states = new List<bool>();
		foreach (AC.ButtonDialog _option in options)
		{
			states.Add (_option.isOn);
		}
		
		return states;
	}
	
	
	public List<bool> GetOptionLocks ()
	{
		List<bool> locks = new List<bool>();
		foreach (AC.ButtonDialog _option in options)
		{
			locks.Add (_option.isLocked);
		}
		
		return locks;
	}
	
	
	public void SetOptionStates (List<bool> states)
	{
		int i=0;
		foreach (AC.ButtonDialog _option in options)
		{
			_option.isOn = states[i];
			i++;
		}
	}
	
	
	public void SetOptionLocks (List<bool> locks)
	{
		int i=0;
		foreach (AC.ButtonDialog _option in options)
		{
			_option.isLocked = locks[i];
			i++;
		}
	}
	
	
	private void OnDestroy ()
	{
		//playerInput = null;
	}
}
