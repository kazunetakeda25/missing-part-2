using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

public class ToolbarLinks : EditorWindow
{

	[MenuItem ("Adventure Creator/Online resources/Website", false, 0)]
	static void Website ()
	{
		Application.OpenURL ("http://www.adventurecreator.org/");
	}


	[MenuItem ("Adventure Creator/Online resources/Tutorials", false, 1)]
	static void Tutorials ()
	{
		Application.OpenURL ("http://www.adventurecreator.org/tutorials/");
	}


	[MenuItem ("Adventure Creator/Online resources/Downloads", false, 2)]
	static void Downloads ()
	{
		Application.OpenURL ("http://www.adventurecreator.org/downloads/");
	}


	[MenuItem ("Adventure Creator/Online resources/Forum", false, 3)]
	static void Forum ()
	{
		Application.OpenURL ("http://www.adventurecreator.org/forum/");
	}


	[MenuItem ("Adventure Creator/Getting started/Load 2D Demo managers", false, 5)]
	static void Demo2D ()
	{
		ManagerPackage package = AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/2D Demo/ManagerPackage.asset", typeof (ManagerPackage)) as ManagerPackage;
		package.AssignManagers ();
		AdventureCreator.RefreshActions ();
	}


	[MenuItem ("Adventure Creator/Getting started/Load 3D Demo managers", false, 6)]
	static void Demo3D ()
	{
		ManagerPackage package = AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Demo/ManagerPackage.asset", typeof (ManagerPackage)) as ManagerPackage;
		package.AssignManagers ();
		AdventureCreator.RefreshActions ();
	}

}