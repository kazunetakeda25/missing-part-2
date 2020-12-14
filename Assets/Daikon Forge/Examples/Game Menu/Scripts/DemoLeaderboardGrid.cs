using UnityEngine;

using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Leaderboard Grid" )]
[Serializable]
public class DemoLeaderboardGrid : MonoBehaviour
{

	#region Public events 

	public delegate void SelectionChangedHandler( DemoLeaderboardDataItem item );
	public event SelectionChangedHandler SelectionChanged;

	#endregion

	#region Public serialized fields 

	public List<DemoLeaderboardDataItem> Items = new List<DemoLeaderboardDataItem>();
	public dfDataObjectProxy SelectedItemProxy;

	#endregion

	#region Private variables 

	private bool isGridPopulated = false;
	private List<dfControl> rows = new List<dfControl>();

	#endregion

	#region Public properties 

	public DemoLeaderboardDataItem SelectedItem { get; private set; }
	public List<dfControl> Rows = new List<dfControl>();

	#endregion

	#region Unity events

	void Awake() { }
	void OnEnable() { }

	void Start()
	{

		var container = GetComponent<dfControl>();
		if( container == null )
			return;

		container = container.GetRootContainer();

		container.EnterFocus += ( sender, args ) =>
		{
			StartCoroutine( PopulateGrid() );
		};

		container.LeaveFocus += ( sender, args ) =>
		{

			StopAllCoroutines();

			isGridPopulated = false;

			for( int i = 0; i < rows.Count; i++ )
			{
				rows[ i ].RemoveAllEventHandlers();
				dfPoolManager.Pool[ "Leaderboard" ].Despawn( rows[ i ].gameObject );
			}

			rows.Clear();

		};

	}

	#endregion

	#region Private utility methods

	private IEnumerator PopulateGrid()
	{

		if( isGridPopulated )
			yield break;

		isGridPopulated = true;

		if( Items.Count == 0 )
			yield break;

		var container = GetComponent<dfScrollPanel>();
		if( container == null )
			yield break;

		// Sort players by Score
		Items.Sort();

		for( int i = 0; i < Items.Count; i++ )
		{

			var item = Items[ i ];
			item.Rank = i + 1;

			var rowGO = dfPoolManager.Pool[ "Leaderboard" ].Spawn( true );
			rowGO.hideFlags = HideFlags.DontSave;

			var row = rowGO.GetComponent<dfControl>();
			row.ZOrder = i;
			container.AddControl( row );
			row.Show();

			rows.Add( row );

			// Bind the row's data
			var binding = rowGO.GetComponent<DemoLeaderboardListItem>();
			binding.Bind( item );

			// Set background for alternate rows
			row.Find( "AltRow" ).IsVisible = ( i % 2 > 0 );

			initializeRowEvents( item, row, i );
			if( i == 0 )
			{
				row.Focus();
			}

			yield return null;

		}

	}

	private void initializeRowEvents( DemoLeaderboardDataItem item, dfControl row, int itemIndex )
	{

		row.MouseEnter += ( sender, args ) => { row.Focus(); };

		row.EnterFocus += ( sender, args ) =>
		{
			this.SelectedItem = item;
			if( SelectionChanged != null ) SelectionChanged( item );
			if( SelectedItemProxy != null ) SelectedItemProxy.Data = item;
		};

		row.KeyDown += ( sender, args ) =>
		{

			if( args.Used )
				return;

			if( args.KeyCode == KeyCode.DownArrow )
			{
				selectNext( itemIndex );
				args.Use();
			}
			else if( args.KeyCode == KeyCode.UpArrow )
			{
				selectPrevious( itemIndex );
				args.Use();
			}
			else if( args.KeyCode == KeyCode.Home )
			{
				selectFirst();
				args.Use();
			}
			else if( args.KeyCode == KeyCode.End )
			{
				selectLast();
				args.Use();
			}

		};

	}

	private void selectLast()
	{
		var row = rows.LastOrDefault( control => control.IsEnabled );
		if( row != null )
		{
			row.Focus();
		}
	}

	private void selectFirst()
	{
		var row = rows.FirstOrDefault( control => control.IsEnabled );
		if( row != null )
		{
			row.Focus();
		}
	}

	private void selectPrevious( int index )
	{
		while( --index >= 0 )
		{
			if( rows[ index ].IsEnabled )
			{
				rows[ index ].Focus();
				return;
			}
		}
	}

	private void selectNext( int index )
	{
		while( ++index < rows.Count )
		{
			if( rows[ index ].IsEnabled )
			{
				rows[ index ].Focus();
				return;
			}
		}
	}

	#endregion

}
