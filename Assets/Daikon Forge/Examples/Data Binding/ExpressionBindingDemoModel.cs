using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[AddComponentMenu( "Daikon Forge/Examples/Data Binding/Expression Binding Model" )]
public class ExpressionBindingDemoModel : MonoBehaviour
{

	private dfListbox list;

	public List<string> SpellsLearned { get; set; }

	public SpellDefinition SelectedSpell
	{
		get { return SpellDefinition.FindByName( SpellsLearned[ list.SelectedIndex ] ); }
	}

	#region Unity callbacks

	void Awake()
	{

		list = GetComponentInChildren<dfListbox>();
		list.SelectedIndex = 0;
		
		SpellsLearned = SpellDefinition.AllSpells
			.OrderBy( x => x.Name )
			.Select( x => x.Name )
			.ToList();

	}

	#endregion

}
