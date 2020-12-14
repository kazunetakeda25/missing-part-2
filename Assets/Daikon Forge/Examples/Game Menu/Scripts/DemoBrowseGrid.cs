using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Browse Games Grid" )]
[Serializable]
public class DemoBrowseGrid : MonoBehaviour
{

	#region Public events 

	public delegate void SelectionChangedHandler( DemoHostedGameInfo item );
	public event SelectionChangedHandler SelectionChanged;

	#endregion

	#region Public serialized fields

	public List<DemoHostedGameInfo> Items = new List<DemoHostedGameInfo>();

	#endregion

	#region Private variables 

	private bool isGridPopulated = false;
	private List<dfControl> rows = new List<dfControl>();

	private dfDataObjectProxy selectedItemProxy;

	#endregion

	#region Public properties

	public DemoHostedGameInfo SelectedItem { get; private set; }

	#endregion

	#region Unity events 

	void Awake() { }
	void OnEnable() { }

	void Start()
	{

		selectedItemProxy = GetComponent<dfDataObjectProxy>();

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
				dfPoolManager.Pool[ "Browse" ].Despawn( rows[ i ].gameObject );
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

		var container = GetComponent<dfControl>();
		if( container == null )
			yield break;

		for( int i = 0; i < Items.Count; i++ )
		{

			yield return null;

			var item = Items[ i ];

			var rowGO = dfPoolManager.Pool[ "Browse" ].Spawn( false );
			rowGO.hideFlags = HideFlags.DontSave;
			rowGO.transform.parent = container.transform;

			rowGO.gameObject.SetActive( true );

			var listItem = rowGO.GetComponent<DemoBrowseGridListItem>();
			if( listItem != null )
			{
				listItem.Bind( item );
			}

			var row = rowGO.GetComponent<dfControl>();
			row.ZOrder = rows.Count;
			row.Show();

			rows.Add( row );

			var itemIndex = i;

			initializeRowEvents( item, row, itemIndex );

			if( i == 0 )
			{
				row.Focus();
			}

		}

	}

	private void initializeRowEvents( DemoHostedGameInfo item, dfControl row, int itemIndex )
	{

		row.MouseEnter += ( sender, args ) => { row.Focus(); };

		row.EnterFocus += ( sender, args ) =>
		{
			this.SelectedItem = item;
			if( SelectionChanged != null ) SelectionChanged( item );
			if( selectedItemProxy != null ) selectedItemProxy.Data = item;
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
