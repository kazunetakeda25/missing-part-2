/* Copyright 2013 Daikon Forge */

/****************************************************************************
 * PLEASE NOTE: The code in this file is under extremely active development
 * and is likely to change quite frequently. It is not recommended to modify
 * the code in this file, as your changes are likely to be overwritten by
 * the next product update when it is published.
 * **************************************************************************/

using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityObject = UnityEngine.Object;
using UnityFont = UnityEngine.Font;
using UnityMaterial = UnityEngine.Material;

[CanEditMultipleObjects()]
[CustomEditor( typeof( dfDynamicFont ) )]
public class dfDynamicFontInspector : Editor
{

	private static Texture2D lineTex;

	#region Unity menu integration 

	//[MenuItem( "Assets/Daikon Forge/Inspect Font" )]
	//public static void InspectDynamicFont()
	//{


	//    var selection = Selection.objects;
	//    for( int i = 0; i < selection.Length; i++ )
	//    {
	//        Debug.Log( "Selected: " + selection[ i ] );
	//    }

	//    UnityFont baseFont = Selection.objects.Where( x => x is UnityFont ).FirstOrDefault() as UnityFont;
	//    if( baseFont == null )
	//    {
	//        EditorUtility.DisplayDialog( "Create Dynamic Font", "No .TTF or .OTF file was selected", "OK" );
	//        return;
	//    }

	//    var buffer = new StringBuilder();

	//    var serialized = new SerializedObject( baseFont );
	//    var iter = serialized.GetIterator();
	//    iter.Next( true );
	//    do
	//    {
	//        buffer.Append( iter.propertyPath );
	//        buffer.Append( "\r\n" );
	//    } while( iter.Next( false ) );

	//    Debug.Log( "m_FontSize: " + getSerializedProperty( serialized, "m_FontSize" ) );
	//    Debug.Log( "m_LineSpacing: " + getSerializedProperty( serialized, "m_LineSpacing" ) );
	//    Debug.Log( "m_Ascent: " + getSerializedProperty( serialized, "m_Ascent" ) );
	//    Debug.Log( "m_CharacterSpacing: " + getSerializedProperty( serialized, "m_CharacterSpacing" ) );
	//    Debug.Log( "m_PixelScale: " + getSerializedProperty( serialized, "m_PixelScale" ) );

	//}

	[MenuItem( "Assets/Daikon Forge/Create Dynamic Font" )]
	//[MenuItem( "GameObject/Daikon Forge/Create Dynamic Font" )]
	public static void CreateDynamicFont()
	{

		UnityFont baseFont = Selection.objects.Where( x => x is UnityFont ).FirstOrDefault() as UnityFont;
		if( baseFont == null )
		{
			EditorUtility.DisplayDialog( "Create Dynamic Font", "No .TTF or .OTF file was selected", "OK" );
			return;
		}

		var go = new GameObject();
		var font = go.AddComponent<dfDynamicFont>();
		if( font == null )
			return;

		font.BaseFont = baseFont;
		font.Material = baseFont.material;

		var serialized = new SerializedObject( baseFont );

		var sizeProperty = getSerializedProperty( serialized, "m_FontSize" );
		font.FontSize = (int)Mathf.Max( sizeProperty, 16 );

		var lineheight = getSerializedProperty( serialized, "m_LineSpacing" );
		font.LineHeight = (int)Mathf.Max( lineheight, font.FontSize );

		var ascent = getSerializedProperty( serialized, "m_Ascent" );
		font.Baseline = (int)Mathf.Max( ascent, font.FontSize );

		go.name = (baseFont.fontNames.FirstOrDefault() ?? baseFont.name) + " (Dynamic)";

		var basePath = ( baseFont != EditorStyles.standardFont ) ? AssetDatabase.GetAssetPath( baseFont ) : Application.dataPath + "/Assets";
		var saveFolder = Path.GetDirectoryName( basePath );
		var prefabPath = EditorUtility.SaveFilePanel( "Create Dynamic Font", saveFolder, go.name, "prefab" );
		if( string.IsNullOrEmpty( prefabPath ) )
			return;

		prefabPath = prefabPath.MakeRelativePath();

		var prefab = PrefabUtility.CreateEmptyPrefab( prefabPath );
		prefab.name = go.name;
		PrefabUtility.ReplacePrefab( go, prefab );

		DestroyImmediate( go );
		AssetDatabase.Refresh();

		#region Delay execution of object selection to work around a Unity issue

		// Declared with null value to eliminate "uninitialized variable" 
		// compiler error in lambda below.
		EditorApplication.CallbackFunction callback = null;

		callback = () =>
		{
			EditorUtility.FocusProjectWindow();
			go = AssetDatabase.LoadMainAssetAtPath( prefabPath ) as GameObject;
			Selection.objects = new UnityObject[] { go };
			EditorGUIUtility.PingObject( go );
			Debug.Log( "Dynamic Font created at " + prefabPath, prefab );
			EditorApplication.delayCall -= callback;
		};

		EditorApplication.delayCall += callback;

		#endregion

	}

	#endregion

	public override void OnInspectorGUI()
	{

		var font = this.target as dfDynamicFont;

		var resetDefaults = false;

		dfEditorUtil.LabelWidth = 120f;

		using( dfEditorUtil.BeginGroup( "Base Font" ) )
		{

			var baseFont = EditorGUILayout.ObjectField( "Base Font", font.BaseFont, typeof( UnityFont ), false ) as UnityFont;
			if( baseFont != font.BaseFont )
			{
				resetDefaults = true;
				dfEditorUtil.MarkUndo( font, "Assign Base Font" );
				font.BaseFont = baseFont;
			}

			var material = EditorGUILayout.ObjectField( "Material", font.Material, typeof( UnityMaterial ), false ) as UnityMaterial;
			if( material != font.Material )
			{
				dfEditorUtil.MarkUndo( font, "Assign Font Material" );
				font.Material = material;
			}

		}

		using( dfEditorUtil.BeginGroup( "Defaults" ) )
		{

			var warningMessage = "Please note: The following values must match the corresponding values defined in the base font in order to render correctly, and should not be changed unless absolutely necessary.";
			EditorGUILayout.HelpBox( warningMessage, MessageType.Warning );

			var baseSize = EditorGUILayout.IntField( "Font Size", font.FontSize );
			if( baseSize != font.FontSize )
			{
				dfEditorUtil.MarkUndo( font, "Assign Base Font Size" );
				font.FontSize = baseSize;
			}

			var lineheight = EditorGUILayout.IntField( "Line Height", font.LineHeight );
			if( lineheight != font.LineHeight )
			{
				dfEditorUtil.MarkUndo( font, "Assign Font LineHeight Value" );
				font.LineHeight = lineheight;
			}

			var baseline = EditorGUILayout.IntField( "Baseline", font.Baseline );
			if( baseline != font.Baseline )
			{
				dfEditorUtil.MarkUndo( font, "Assign Font Baseline Value" );
				font.Baseline = baseline;
			}

			EditorGUILayout.BeginHorizontal();
			{

				GUILayout.Space( dfEditorUtil.LabelWidth + 10 );

				if( resetDefaults || GUILayout.Button( "Reset Defaults" ) )
				{

					var serialized = new SerializedObject( font.BaseFont );

					var sizeProperty = getSerializedProperty( serialized, "m_FontSize" );
					font.FontSize = (int)sizeProperty;

					var lineheightProperty = getSerializedProperty( serialized, "m_LineSpacing" );
					font.LineHeight = Mathf.CeilToInt( lineheightProperty );

					var ascentProperty = getSerializedProperty( serialized, "m_Ascent" );
					font.Baseline = Mathf.CeilToInt( ascentProperty );

					dfEditorUtil.MarkUndo( target, "Reset font values to defaults" );

				}

				//if( GUILayout.Button( "Reset Font" ) )
				//{

				//    font.BaseFont.characterInfo = null;
				//    EditorUtility.SetDirty( font.BaseFont );

				//    var path = AssetDatabase.GetAssetPath( font.BaseFont );
				//    if( !string.IsNullOrEmpty( path ) )
				//    {
				//        AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate );
				//    }

				//    dfGUIManager.RefreshAll();

				//}

			}
			EditorGUILayout.EndHorizontal();

		}

	}

	public override bool HasPreviewGUI()
	{
		return true;
	}

	public override void OnPreviewGUI( Rect rect, GUIStyle background )
	{

		var font = this.target as dfDynamicFont;

		var previewString = "Grumpy wizards make toxic brew for the evil Queen and Jack. The quick brown fox jumps over the lazy dog. 0123456789 Aa Bb Cc Dd Ee Ff Gg Hh Ii Jj Kk Ll Mm Nn Oo Pp Qq Rr Ss Tt Uu Vv Ww Xx Yy Zz !@#$%^&*()[]{}\\/|";
		var style = new GUIStyle()
		{
			font = font.BaseFont,
			fontSize = font.FontSize,
			fontStyle = FontStyle.Normal,
			wordWrap = true,
			normal = new GUIStyleState() { textColor = Color.white }
		};

		rect.x += 5;
		rect.y += 5;
		rect.width -= 10;
		rect.height -= 10;

		DrawLine( rect.x, rect.y + font.Baseline, rect.width, false, Color.cyan );
		DrawLine( rect.x, rect.y + font.LineHeight, rect.width, false, Color.white );

		EditorGUI.DropShadowLabel( rect, previewString, style );

	}

	private void DrawLine( float left, float top, float size, bool vert, Color color )
	{

		if( !lineTex )
		{
			lineTex = new Texture2D( 1, 1 ) { hideFlags = HideFlags.HideAndDontSave };
		}

		var saveColor = GUI.color;
		GUI.color = color;

		if( !vert )
			GUI.DrawTexture( new Rect( left, top, size, 1 ), lineTex );
		else
			GUI.DrawTexture( new Rect( left, top, 1, size ), lineTex );

		GUI.color = saveColor;

	}

	private static float getSerializedProperty( SerializedObject target, string propertyName )
	{

		var property = target.FindProperty( propertyName );
		if( property == null )
		{
			Debug.LogWarning( "Failed to find property: " + propertyName );
			return -1;
		}

		switch( property.propertyType )
		{
			case SerializedPropertyType.Float:
				return property.floatValue;
			case SerializedPropertyType.Integer:
				return property.intValue;
		}

		return -1;

	}

}
