using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DemoLeaderboardDataItem : IComparable<DemoLeaderboardDataItem>
{

	public int Rank;
	public string GamerTag;
	public int Score;
	public int Kills;
	public int Deaths;

	#region IComparable<DemoLeaderboardItem> Members

	public int CompareTo( DemoLeaderboardDataItem other )
	{
		return other.Score.CompareTo( this.Score );
	}

	#endregion

}
