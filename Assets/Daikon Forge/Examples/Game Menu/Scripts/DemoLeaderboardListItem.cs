using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Leaderboard List Item" )]
public class DemoLeaderboardListItem : MonoBehaviour
{

	public void Bind( DemoLeaderboardDataItem data )
	{

		var control = GetComponent<dfControl>();
		if( !control.enabled )
			return;

		control.Find<dfLabel>( "Rank" ).Text = data.Rank.ToString();
		control.Find<dfLabel>( "Gamertag" ).Text = data.GamerTag;
		control.Find<dfLabel>( "Score" ).Text = string.Format( "{0:N0}", data.Score );
		control.Find<dfLabel>( "Kills" ).Text = string.Format( "{0:N0}", data.Kills );
		control.Find<dfLabel>( "Deaths" ).Text = string.Format( "{0:N0}", data.Deaths );
		control.Find<dfLabel>( "KDR" ).Text = formatKDR( data.Kills, data.Deaths );

	}

	public string formatKDR( int Kills, int Deaths )
	{

		if( Deaths == 0 )
		{
			if( Kills > 0 ) return Kills.ToString();
			else return "0";
		}

		return string.Format( "{0:F1}", ( (float)Kills / (float)Deaths ) );

	}


}
