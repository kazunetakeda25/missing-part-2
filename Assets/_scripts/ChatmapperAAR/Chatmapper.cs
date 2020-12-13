using UnityEngine;
using System.Collections;

public class Chatmapper
{
	public Actors[] actors;
	public Items[] items;
	public Locations[] locations;
	public Conversations[] conversations;
	public UserVariables[] userVariables;

	public class Fields
	{
		public string Title;
		public string[] Pictures;
		public string Description;
		public string Actor;
		public string Conversant;
		public string MenuText;
		public string DialogueText;
		public string Parenthetical;
		public string[] AudioFiles;
		public string VideoFile;
		public string[] AnimationFiles;
	}

	public class Actors
	{
		public int ID;
		public Fields fields;
	}

	public class Items
	{
		public int ID;
		public Fields Fields;
	}

	public class Locations
	{
		public int ID;
		public Fields Fields;
	}

	public class Conversations
	{
		public int ID;
		public int NodeColor;
		public string Title;

		public Fields Fields;

		public DialogNodes[] DialogueNodes;

		public class DialogNodes
		{

			public int conversationID;
			public bool isRoot;
			public bool isGroup;
			public string conditionsString;
			public string userScript;
			public int numberColor;
			public bool delaySimStatus;
			public int falseConditionAction;
			public int conditionPriority;

			public OutgoingLinks[] outgoingLinks;

			public class OutgoingLinks
			{
				public int OriginConvoID;
				public int DestinationConvoID;
				public int DestinationDialogID;
			}
		}
	}

	public class UserVariables
	{
		public string name;
		public int value;
		public string description;
	}
}
