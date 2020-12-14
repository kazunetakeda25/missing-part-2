/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberConversation.cs"
 * 
 *	This script is attached to conversation objects in the scene
 *	with DialogOption states we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class RememberConversation : Remember
	{

		public override string SaveData ()
		{
			ConversationData conversationData = new ConversationData();
			conversationData.objectID = constantID;

			if (GetComponent <Conversation>())
			{
				bool[] optionStates = GetComponent <Conversation>().GetOptionStates ();
				conversationData._optionStates = ArrayToString <bool> (optionStates);

				bool[] optionLocks = GetComponent <Conversation>().GetOptionLocks ();
				conversationData._optionLocks = ArrayToString <bool> (optionLocks);
			}

			return Serializer.SaveScriptData <ConversationData> (conversationData);
		}


		public override void LoadData (string stringData)
		{
			ConversationData data = Serializer.LoadScriptData <ConversationData> (stringData);
			if (data == null) return;

			if (GetComponent <Conversation>())
			{
				bool[] optionStates = StringToBoolArray (data._optionStates);
				GetComponent <Conversation>().SetOptionStates (optionStates);

				bool[] optionLocks = StringToBoolArray (data._optionLocks);
				GetComponent <Conversation>().SetOptionLocks (optionLocks);
			}
		}

	}


	[System.Serializable]
	public class ConversationData : RememberData
	{
		public string _optionStates;
		public string _optionLocks;
		
		public ConversationData () { }
	}

}