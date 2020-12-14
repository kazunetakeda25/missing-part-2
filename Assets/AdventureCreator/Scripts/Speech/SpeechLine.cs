/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SpeechLine.cs"
 * 
 *	This script is a data container for speech lines found by Speech Manager.
 *	Such data is used to provide translation support, as well as auto-numbering
 *	of speech lines for sound files.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class SpeechLine
	{

		public bool isPlayer;
		public int lineID;
		public string scene;
		public string owner;
		public string text;
		public string description;
		public AC_TextType textType;
		
		public List<string> translationText = new List<string>();

		
		public SpeechLine ()
		{
			lineID = 0;
			scene = "";
			owner = "";
			text = "";
			description = "";
			isPlayer = false;
			translationText = new List<string> ();
			textType = AC_TextType.Speech;
		}


		public bool IsMatch (SpeechLine newLine)
		{
			if (lineID == newLine.lineID && text == newLine.text && textType == newLine.textType && owner == newLine.owner)
			{
				return true;
			}
			return false;
		}


		public SpeechLine (int _id, string _scene, string _text, int _languagues, AC_TextType _textType)
		{
			lineID = _id;
			scene = _scene;
			owner = "";
			text = _text;
			textType = _textType;
			description = "";
			isPlayer = false;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}
		
		
		public SpeechLine (int[] idArray, string _scene, string _text, int _languagues, AC_TextType _textType)
		{
			// Update id based on array
			lineID = 0;

			foreach (int _id in idArray)
			{
				if (lineID == _id)
					lineID ++;
			}

			scene = _scene;
			owner = "";
			text = _text;
			textType = _textType;
			description = "";
			isPlayer = false;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}
		
		
		public SpeechLine (int _id, string _scene, string _owner, string _text, int _languagues, AC_TextType _textType, bool _isPlayer = false)
		{
			lineID = _id;
			scene = _scene;
			owner = _owner;
			text = _text;
			textType = _textType;
			description = "";
			isPlayer = _isPlayer;
			
			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}
		
		
		public SpeechLine (int[] idArray, string _scene, string _owner, string _text, int _languagues, AC_TextType _textType,  bool _isPlayer = false)
		{
			// Update id based on array
			lineID = 0;
			foreach (int _id in idArray)
			{
				if (lineID == _id)
					lineID ++;
			}
			
			scene = _scene;
			owner = _owner;
			text = _text;
			description = "";
			textType = _textType;
			isPlayer = _isPlayer;

			translationText = new List<string>();
			for (int i=0; i<_languagues; i++)
			{
				translationText.Add (_text);
			}
		}


		public string GetFilename ()
		{
			string filename = "";
			if (owner != "")
			{
				filename = owner;

				if (isPlayer && (KickStarter.speechManager == null || !KickStarter.speechManager.usePlayerRealName))
				{
					filename = "Player";
				}
				
				string badChars = "/`'!@£$%^&*(){}:;.|<,>?#-=+-";
				for (int i=0; i<badChars.Length; i++)
				{
					filename = filename.Replace(badChars[i].ToString (), "_");
				}
				filename = filename.Replace ('"'.ToString (), "_");
			}
			else
			{
				filename = "Narrator";
			}
			return filename;
		}

		
		#if UNITY_EDITOR
		
		public void ShowGUI ()
		{
			SpeechManager speechManager = AdvGame.GetReferences ().speechManager;

			if (this == speechManager.activeLine)
			{
				EditorGUILayout.BeginVertical ("Button");
				ShowField ("ID #:", lineID.ToString (), false);
				ShowField ("Type:", textType.ToString (), false);

				string sceneName = scene.Replace ("Assets/", "");
				sceneName = sceneName.Replace (".unity", "");
				ShowField ("Scene:", sceneName, true);

				if (textType == AC_TextType.Speech)
				{
					if (isPlayer)
					{
						if (KickStarter.speechManager && KickStarter.speechManager.usePlayerRealName)
						{
							ShowField ("Speaker", owner, false);
						}
						else
						{
							ShowField ("Speaker:", "Player", false);
						}
					}
					else
					{
						ShowField ("Speaker:", owner, false);
					}
				}

				if (speechManager.languages != null && speechManager.languages.Count > 1)
				{
					for (int i=0; i<speechManager.languages.Count; i++)
					{
						if (i==0)
						{
							ShowField ("Original:", "'" + text + "'", true);
						}
						else if (translationText.Count > (i-1))
						{
							ShowField (speechManager.languages[i] + ":", "'" + translationText [i-1] + "'", true);
						}
						else
						{
							ShowField (speechManager.languages[i] + ":", "(Not defined)", false);
						}
						if (speechManager.translateAudio && textType == AC_TextType.Speech)
						{
							if (i==0)
							{
								if (speechManager.UseFileBasedLipSyncing ())
								{
									ShowField (" (Lipsync path):", GetFolderName ("", true), false);
								}
								ShowField (" (Audio path):", GetFolderName (""), false);
							}
							else
							{
								if (speechManager.UseFileBasedLipSyncing ())
								{
									ShowField (" (Lipsync path):", GetFolderName (speechManager.languages[i], true), false);
								}
								ShowField (" (Audio path):", GetFolderName (speechManager.languages[i]), false);
							}
							EditorGUILayout.Space ();
						}
					}

					if (!speechManager.translateAudio && textType == AC_TextType.Speech)
					{
						if (speechManager.UseFileBasedLipSyncing ())
						{
							ShowField ("Lipsync path:", GetFolderName ("", true), false);
						}
						ShowField ("Audio path:", GetFolderName (""), false);
					}
				}
				else if (textType == AC_TextType.Speech)
				{
					ShowField ("Text:", "'" + text + "'", true);
					if (speechManager.UseFileBasedLipSyncing ())
					{
						ShowField ("Lipsync path:", GetFolderName ("", true), false);
					}
					ShowField ("Audio Path:", GetFolderName (""), false);
				}

				if (textType == AC_TextType.Speech)
				{
					ShowField ("Filename:", GetFilename () + lineID.ToString (), false);
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Description:", GUILayout.Width (65f));
					description = EditorGUILayout.TextField (description);
					EditorGUILayout.EndHorizontal ();
				}

				EditorGUILayout.EndVertical ();
			}
			else
			{
				if (GUILayout.Button (lineID.ToString () + ": '" + text + "'", EditorStyles.label, GUILayout.MaxWidth (300)))
				{
					speechManager.activeLine = this;
				}
				GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height(1));
			}
		}


		public static void ShowField (string label, string field, bool multiLine)
		{
			if (field == "") return;

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (label, GUILayout.Width (85f));

			if (multiLine)
			{
				GUIStyle style = new GUIStyle ();
				#if UNITY_PRO_LICENSE
				style.normal.textColor = Color.white;
				#endif
				style.wordWrap = true;
				style.alignment = TextAnchor.MiddleLeft;
				EditorGUILayout.LabelField (field, style, GUILayout.MaxWidth (270f));
			}
			else
			{
				EditorGUILayout.LabelField (field);
			}
			EditorGUILayout.EndHorizontal ();
		}


		public string GetFolderName (string language, bool forLipsync = false)
		{
			string folderName = "Resources/";

			if (forLipsync)
			{
				folderName += "Lipsync/";
			}
			else
			{
				folderName += "Speech/";
			}

			if (language != "" && KickStarter.speechManager.translateAudio)
			{
				folderName += language + "/";
			}
			if (KickStarter.speechManager.placeAudioInSubfolders)
			{
				folderName += GetFilename ();
			}
			return folderName;
		}


		public bool Matches (string filter, FilterSpeechLine filterSpeechLine)
		{
			if (filter == null || filter == "")
			{
				return true;
			}
			filter = filter.ToLower ();
			if (filterSpeechLine == FilterSpeechLine.All)
			{
				if (description.ToLower ().Contains (filter)
				    || scene.ToLower ().Contains (filter)
				    || owner.ToLower ().Contains (filter)
				    || text.ToLower ().Contains (filter)
				    || textType.ToString ().ToLower ().Contains (filter))
				{
					return true;
				}
			}
			else if (filterSpeechLine == FilterSpeechLine.Description)
			{
				return description.ToLower ().Contains (filter);
			}
			else if (filterSpeechLine == FilterSpeechLine.Scene)
			{
				return scene.ToLower ().Contains (filter);
			}
			else if (filterSpeechLine == FilterSpeechLine.Speaker)
			{
				return owner.ToLower ().Contains (filter);
			}
			else if (filterSpeechLine == FilterSpeechLine.Text)
			{
				return text.ToLower ().Contains (filter);
			}
			else if (filterSpeechLine == FilterSpeechLine.Type)
			{
				return textType.ToString ().ToLower ().Contains (filter);
			}
			return false;
		}


		public string GetInfo ()
		{
			string info = textType.ToString ();

			if (owner != "")
			{
				info += " (" + owner + ")";
			}

			return info;
		}


		public string Print ()
		{
			string result = "Character: " + owner + "\nFilename: " + owner + lineID.ToString () + "\n";
			result += '"';
			result += text;
			result += '"';
			if (description != null && description != "")
			{
				result += "\nDescription: " + description;
			}
			return (result);
		}
		
		#endif
		
	}

}