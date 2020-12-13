using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AARHistory {

	private List<int> history = new List<int>();
	public List<int> History { get { return history; } }

	public int GoBack()
	{
		history.RemoveAt(history.Count - 1);
		return history[history.Count - 1];
	}

	public void GoForward(int nextTileIndex)
	{
		history.Add(nextTileIndex);
	}

	public void PrintHistory()
	{
		string historyPrint = "History: ";

		foreach(int i in history)
		{
			historyPrint += i.ToString();
		}

		Debug.Log (historyPrint);
	}

	public void ClearHistory()
	{
		history.Clear();
	}

//	public void SkipTo(int historyIndex)
//	{
//		int skipRangeStart = 0;
//
//		for (int i = 0; i < history.Count; i++) {
//			if(history[i] == historyIndex)
//				skipRangeStart = i;
//		}
//
//		if(skipRangeStart == 0) {
//			Debug.LogWarning("Range Not Found!");
//			return;
//		}
//
//		history.RemoveRange(skipRangeStart + 1, history.Count - skipRangeStart);
//	}

}
