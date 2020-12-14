/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"ActionMixerSnapshot.cs"
 * 
 *	Transitions to a single or multiple Audio Mixer snapshots. (Unity 5 only)
 * 
 */

using UnityEngine;
#if UNITY_5
using UnityEngine.Audio;
#endif
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionMixerSnapshot : Action
	{

		#if UNITY_5
		public int numSnapshots = 1;
		public AudioMixer audioMixer = null;
		public AudioMixerSnapshot snapshot = null;
		public List<SnapshotMix> snapshotMixes = new List<SnapshotMix>();
		public float changeTime = 0.1f;
		#endif
		
		public ActionMixerSnapshot ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Set Mixer snapshot";
			description = "Transitions to a single or multiple Audio Mixer snapshots.";
		}
		
		
		override public float Run ()
		{
			#if UNITY_5
			if (!isRunning)
			{
				isRunning = true;

				if (numSnapshots == 1)
				{
					if (snapshot != null)
					{
						snapshot.TransitionTo (changeTime);
						if (changeTime > 0f && willWait)
						{
							return changeTime;
						}
					}
					else
					{
						Debug.LogWarning ("No Audio Mixer Snapshot assigned.");
					}
				}
				else if (audioMixer)
				{
					List<AudioMixerSnapshot> snapshots = new List<AudioMixerSnapshot>();
					List<float> weights = new List<float>();

					foreach (SnapshotMix snapshotMix in snapshotMixes)
					{
						snapshots.Add (snapshotMix.snapshot);
						weights.Add (snapshotMix.weight);
					}

					audioMixer.TransitionToSnapshots (snapshots.ToArray (), weights.ToArray (), changeTime);
					if (changeTime > 0f && willWait)
					{
						return changeTime;
					}
				}
				return 0f;
			}
			else
			{
				isRunning = false;
				return 0f;
			}
			#else
			return 0f;
			#endif
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			#if UNITY_5
			numSnapshots = EditorGUILayout.IntSlider ("Number of snapshots:", numSnapshots, 1, 10);
			if (numSnapshots == 1)
			{
				snapshot = (AudioMixerSnapshot) EditorGUILayout.ObjectField ("Snapshot:", snapshot, typeof (AudioMixerSnapshot), false);
			}
			else
			{
				audioMixer = (AudioMixer) EditorGUILayout.ObjectField ("Audio mixer:", audioMixer, typeof (AudioMixer), false);

				if (numSnapshots < snapshotMixes.Count)
				{
					snapshotMixes.RemoveRange (numSnapshots, snapshotMixes.Count - numSnapshots);
				}
				else if (numSnapshots > snapshotMixes.Count)
				{
					if (numSnapshots > snapshotMixes.Capacity)
					{
						snapshotMixes.Capacity = numSnapshots;
					}
					for (int i=snapshotMixes.Count; i<numSnapshots; i++)
					{
						snapshotMixes.Add (new SnapshotMix ());
					}
				}

				for (int i=0; i<snapshotMixes.Count; i++)
				{
					snapshotMixes[i].snapshot = (AudioMixerSnapshot) EditorGUILayout.ObjectField ("Snapshot " + (i+1).ToString () + ":", snapshotMixes[i].snapshot, typeof (AudioMixerSnapshot), false);
					snapshotMixes[i].weight = EditorGUILayout.FloatField ("Weight " + (i+1).ToString () + ":", snapshotMixes[i].weight);
				}
			}

			changeTime = EditorGUILayout.Slider ("Transition time (s):", changeTime, 0f, 10f);
			if (changeTime > 0f)
			{
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
			#else
			EditorGUILayout.HelpBox ("This Action is only available for Unity 5", MessageType.Info);
			#endif
			
			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			#if UNITY_5
			if (numSnapshots == 1 && snapshot != null)
			{
				return " (" + snapshot.name + ")";
			}
			if (numSnapshots > 1 && audioMixer != null)
			{
				return " (" + audioMixer.name + ")";
			}
			#endif
			return "";
		}
		
		#endif
		
	}


	#if UNITY_5
	[System.Serializable]
	public class SnapshotMix
	{
		public AudioMixerSnapshot snapshot;
		public float weight;
	}
	#endif
	
}