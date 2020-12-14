using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace AC
{

	public class PhonemesWindow : EditorWindow
	{

		private SpeechManager speechManager;

		//[MenuItem ("Adventure Creator/Editors/Phonemes Editor")]
		public static void Init ()
		{
			PhonemesWindow window = (PhonemesWindow) EditorWindow.GetWindow (typeof (PhonemesWindow));
			window.title = "Phonemes Editor";
			window.position = new Rect (300, 200, 450, 400);
		}


		private void OnEnable ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager)
			{
				speechManager = AdvGame.GetReferences ().speechManager;
			}
		}


		private void OnGUI ()
		{
			if (speechManager == null)
			{
				return;
			}

			speechManager.phonemes = ShowPhonemesGUI (speechManager.phonemes, speechManager.lipSyncMode);

			if (GUI.changed)
			{
				EditorUtility.SetDirty (speechManager);
			}
		}


		private List<string> ShowPhonemesGUI (List<string> phonemes, LipSyncMode mode)
		{
			EditorGUILayout.HelpBox ("Sort letters or phoneme sounds into groups below, with each group representing a different animation frame.  Separate sounds with a forward slash (/).\nThe first frame will be considered the default.", MessageType.Info);

			int numOptions = phonemes.Count;
			numOptions = EditorGUILayout.IntField ("Number of frames:", phonemes.Count);
			if (phonemes.Count < 0)
			{
				phonemes = SetDefaults (mode);
				numOptions = phonemes.Count;
			}
			if (numOptions < 1)
			{
				numOptions = 1;
			}
			
			if (numOptions < phonemes.Count)
			{
				phonemes.RemoveRange (numOptions, phonemes.Count - numOptions);
			}
			else if (numOptions > phonemes.Count)
			{
				if(numOptions > phonemes.Capacity)
				{
					phonemes.Capacity = numOptions;
				}
				for (int i=phonemes.Count; i<numOptions; i++)
				{
					phonemes.Add ("");
				}
			}
			
			for (int i=0; i<phonemes.Count; i++)
			{
				phonemes [i] = EditorGUILayout.TextField ("Frame #" + i.ToString () + ":", phonemes [i]);
			}

			if (GUILayout.Button ("Revert to defaults"))
			{
				phonemes = SetDefaults (mode);
			}

			return phonemes;
		}
	

		private List<string> SetDefaults (LipSyncMode mode)
		{
			List<string> phonemes = new List<string>();

			if (mode == LipSyncMode.ReadPamelaFile)
			{
				phonemes.Add ("B/M/P/ ");
				phonemes.Add ("EH0/EH1/EH2/ER0/ER1/ER2/EY0/EY1/EY2/IY0/IY1/IY2");
				phonemes.Add ("CH/G/HH/IH0/IH1/IH2/JH/K/R/S/SH/Y/Z/ZH");
				phonemes.Add ("F/V");
				phonemes.Add ("D/DH/L/N/NG");
				phonemes.Add ("AA0/AA1/AA2/AE0/AE1/AE2/AH0/AH1/AH2/AY0/AY1/AY2");
				phonemes.Add ("AO0/AO1/AO2/AW0/AW1/AW2/OW0/OW1/OW2");
				phonemes.Add ("T/TH");
				phonemes.Add ("OY0/OY1/OY2/UH0/UH1/UH2/UW0/UW1/UW2/W");
			}
			else if (mode == LipSyncMode.FromSpeechText || mode == LipSyncMode.ReadSapiFile || mode == LipSyncMode.ReadPapagayoFile)
			{
				phonemes.Add ("B/M/P/MBP/ ");
				phonemes.Add ("AY/AH/IH/EY/ER");
				phonemes.Add ("G/O/OO/OH/W");
				phonemes.Add ("SH/R/Z/SF/D/L/F/TN/K/N/NG/H/X/FV");
				phonemes.Add ("UH/EH/DH/AE/IY");
			}

			return phonemes;
		}

	}

}

#endif
