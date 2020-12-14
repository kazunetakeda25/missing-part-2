using System;
using System.Collections;

using UnityEngine;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Browse Games List Item" )]
public class DemoBrowseGridListItem : MonoBehaviour
{

	#region Private variables 

	private dfLabel Host;
	private dfLabel Map;
	private dfLabel Mode;
	private dfLabel Players;
	private dfTiledSprite Connection;

	#endregion

	#region Public methods 

	public void Bind( DemoHostedGameInfo data )
	{

		var control = GetComponent<dfControl>();

		control.Find<dfLabel>( "Host" ).Text = data.Host;
		control.Find<dfLabel>( "Map" ).Text = data.Map;
		control.Find<dfLabel>( "Players" ).Text = string.Format( "{0}/{1}", data.Players, data.MaxPlayers );
		control.Find<dfLabel>( "Mode" ).Text = data.Mode;
		control.Find<dfTiledSprite>( "Connection" ).FillAmount = data.Health.Quantize( 0.25f );

	}

	#endregion

}
