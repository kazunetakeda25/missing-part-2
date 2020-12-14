using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu( "Daikon Forge/Examples/Game Menu/Achievements Grid" )]
[Serializable]
public class DemoAchievementsGrid : MonoBehaviour
{

	#region Public events

	public delegate void SelectionChangedHandler( DemoAchievementInfo item );
	public event SelectionChangedHandler SelectionChanged;

	#endregion

	#region Public serialized fields

	public List<DemoAchievementInfo> Items = new List<DemoAchievementInfo>();
	public dfDataObjectProxy SelectedItemProxy;

	#endregion

	#region Private variables

	private bool isGridPopulated = false;
	private List<dfControl> rows = new List<dfControl>();

	private bool showAll = true;
	private bool showAsGrid = false;

	#endregion

	#region Public properties

	public DemoAchievementInfo SelectedItem { get; private set; }

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
				dfPoolManager.Pool[ "Achievement" ].Despawn( rows[ i ].gameObject );
			}

			rows.Clear();

		};

		ExpandAll();

	}

	#endregion

	#region Public methods 

	public void ToggleShowAll()
	{

		this.showAll = !this.showAll;

		for( int i = 0; i < rows.Count; i++ )
		{

			var item = Items[ i ];
			var row = rows[ i ];

			row.IsVisible = item.Unlocked || showAll;

		}

		rows.First( r => r.IsVisible ).Focus();

		var grid = GetComponent<dfScrollPanel>();
		grid.Reset();


	}

	public void ExpandAll()
	{

		showAsGrid = false;

		for( int i = 0; i < rows.Count; i++ )
		{
			var item = rows[ i ].GetComponent<DemoAchievementListItem>();
			item.Expand();
		}

		var grid = GetComponent<dfScrollPanel>();

		grid.Parent.Find<dfSprite>( "grid-style" ).Opacity = 0.5f;
		grid.Parent.Find<dfSprite>( "list-style" ).Opacity = 1f;

		if( rows.Count > 0 )
		{
			rows.First( r => r.IsVisible ).Focus();
			grid.Reset();
		}

	}

	public void CollapseAll()
	{

		showAsGrid = true;

		for( int i = 0; i < rows.Count; i++ )
		{
			var item = rows[ i ].GetComponent<DemoAchievementListItem>();
			item.Collapse();
		}

		var grid = GetComponent<dfScrollPanel>();

		grid.Parent.Find<dfSprite>( "grid-style" ).Opacity = 1f;
		grid.Parent.Find<dfSprite>( "list-style" ).Opacity = 0.5f;

		rows.First( r => r.IsVisible ).Focus();
		grid.Reset();

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

			var rowGO = dfPoolManager.Pool[ "Achievement" ].Spawn( false );
			rowGO.hideFlags = HideFlags.DontSave;
			rowGO.transform.parent = container.transform;
			rowGO.gameObject.SetActive( true );

			var row = rowGO.GetComponent<dfControl>();
			row.ZOrder = rows.Count;
			row.Show();

			rows.Add( row );

			var itemIndex = i;

			initializeRowEvents( item, row, itemIndex );

			if( !( showAll || item.Unlocked ) )
			{
				row.Hide();
			}

			var listItem = row.GetComponent<DemoAchievementListItem>();
			listItem.Bind( item );
			if( !showAsGrid )
			{
				listItem.Expand();
			}

			if( i == 0 )
			{
				row.Focus();
			}

		}

	}

	private void initializeRowEvents( DemoAchievementInfo item, dfControl row, int itemIndex )
	{

		//row.MouseEnter += ( sender, args ) => { row.Focus(); };

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

			if( args.KeyCode == KeyCode.RightArrow )
			{
				selectNext( itemIndex );
				args.Use();
			}
			else if( args.KeyCode == KeyCode.LeftArrow )
			{
				selectPrevious( itemIndex );
				args.Use();
			}
			else if( args.KeyCode == KeyCode.UpArrow )
			{
				selectUp( itemIndex );
				args.Use();
			}
			else if( args.KeyCode == KeyCode.DownArrow )
			{
				selectDown( itemIndex );
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

	private int getColumnCount()
	{

		var container = GetComponent<dfScrollPanel>();

		var horz = Mathf.CeilToInt( container.Width - container.FlowPadding.horizontal - container.ScrollPadding.horizontal );
		var columns = horz / Mathf.CeilToInt( rows[ 0 ].Width );

		return columns;

	}

	private void selectLast()
	{
		var row = rows.LastOrDefault( control => control.IsEnabled && control.IsVisible );
		if( row != null )
		{
			row.Focus();
		}
	}

	private void selectFirst()
	{
		var row = rows.FirstOrDefault( control => control.IsEnabled && control.IsVisible );
		if( row != null )
		{
			row.Focus();
		}
	}

	private void selectUp( int index )
	{

		var columns = getColumnCount();
		if( index >= columns )
		{
			rows[ index - columns ].Focus();
			return;
		}
	}

	private void selectDown( int index )
	{

		var columns = getColumnCount();
		if( index <= rows.Count - columns - 1 )
		{
			rows[ index + columns ].Focus();
			return;
		}

	}

	private void selectPrevious( int index )
	{
		while( --index >= 0 )
		{
			if( rows[ index ].IsEnabled && rows[ index ].IsVisible )
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
			if( rows[ index ].IsEnabled && rows[ index ].IsVisible )
			{
				rows[ index ].Focus();
				return;
			}
		}
	}

	#endregion

}
