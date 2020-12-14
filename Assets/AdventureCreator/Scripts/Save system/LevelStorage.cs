/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"LevelStorage.cs"
 * 
 *	This script handles the loading and unloading of per-scene data.
 *	Below the main class is a series of data classes for the different object types.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	public class LevelStorage : MonoBehaviour
	{
		
		[HideInInspector] public List<SingleLevelData> allLevelData = new List<SingleLevelData>();
		
		
		private void Awake ()
		{
			allLevelData = new List<SingleLevelData>();
		}


		public void ClearAllLevelData ()
		{
			allLevelData.Clear ();
			allLevelData = new List<SingleLevelData>();
		}


		public void ClearCurrentLevelData ()
		{
			foreach (SingleLevelData levelData in allLevelData)
			{
				if (levelData.sceneNumber == Application.loadedLevel)
				{
					allLevelData.Remove (levelData);
					return;
				}
			}
		}
		
		
		public void ReturnCurrentLevelData (bool restoringSaveFile)
		{
			foreach (SingleLevelData levelData in allLevelData)
			{
				if (levelData.sceneNumber == Application.loadedLevel)
				{
					UnloadTransformData (levelData.allTransformData);

					foreach (ScriptData _scriptData in levelData.allScriptData)
					{
						Remember saveObject = Serializer.returnComponent <Remember> (_scriptData.objectID);				
						if (saveObject != null && _scriptData.data != null && _scriptData.data.Length > 0)
						{
							// May have more than one Remember script on the same object, so check all
							Remember[] saveScripts = saveObject.gameObject.GetComponents <Remember>();
							foreach (Remember saveScript in saveScripts)
							{
								saveScript.LoadData (_scriptData.data, restoringSaveFile);
							}
						}
					}

					UnloadVariablesData (levelData.localVariablesData);

					break;
				}
			}

			AssetLoader.UnloadAssets ();
		}
		
		
		public void StoreCurrentLevelData ()
		{
			List<TransformData> thisLevelTransforms = PopulateTransformData ();
			List<ScriptData> thisLevelScripts = PopulateScriptData ();

			SingleLevelData thisLevelData = new SingleLevelData ();
			thisLevelData.sceneNumber = Application.loadedLevel;
			
			if (KickStarter.sceneSettings)
			{
				if (KickStarter.sceneSettings.navMesh && KickStarter.sceneSettings.navMesh.GetComponent <ConstantID>())
				{
					thisLevelData.navMesh = Serializer.GetConstantID (KickStarter.sceneSettings.navMesh.gameObject);
				}
				if (KickStarter.sceneSettings.defaultPlayerStart && KickStarter.sceneSettings.defaultPlayerStart.GetComponent <ConstantID>())
				{
					thisLevelData.playerStart = Serializer.GetConstantID (KickStarter.sceneSettings.defaultPlayerStart.gameObject);
				}
				if (KickStarter.sceneSettings.sortingMap && KickStarter.sceneSettings.sortingMap.GetComponent <ConstantID>())
				{
					thisLevelData.sortingMap = Serializer.GetConstantID (KickStarter.sceneSettings.sortingMap.gameObject);
				}
				if (KickStarter.sceneSettings.cutsceneOnLoad && KickStarter.sceneSettings.cutsceneOnLoad.GetComponent <ConstantID>())
				{
					thisLevelData.onLoadCutscene = Serializer.GetConstantID (KickStarter.sceneSettings.cutsceneOnLoad.gameObject);
				}
				if (KickStarter.sceneSettings.cutsceneOnStart && KickStarter.sceneSettings.cutsceneOnStart.GetComponent <ConstantID>())
				{
					thisLevelData.onStartCutscene = Serializer.GetConstantID (KickStarter.sceneSettings.cutsceneOnStart.gameObject);
				}
			}

			thisLevelData.localVariablesData = SaveSystem.CreateVariablesData (KickStarter.localVariables.localVars, false, VariableLocation.Local);
			thisLevelData.allTransformData = thisLevelTransforms;
			thisLevelData.allScriptData = thisLevelScripts;

			bool found = false;
			for (int i=0; i<allLevelData.Count; i++)
			{
				if (allLevelData[i].sceneNumber == Application.loadedLevel)
				{
					allLevelData[i] = thisLevelData;
					found = true;
					break;
				}
			}
			
			if (!found)
			{
				allLevelData.Add (thisLevelData);
			}
		}

		
		private void UnloadNavMesh (int navMeshInt)
		{
			NavigationMesh navMesh = Serializer.returnComponent <NavigationMesh> (navMeshInt);

			if (navMesh && KickStarter.sceneSettings && KickStarter.sceneSettings.navigationMethod != AC_NavigationMethod.UnityNavigation)
			{
				if (KickStarter.sceneSettings.navMesh)
				{
					NavigationMesh oldNavMesh = KickStarter.sceneSettings.navMesh;
					oldNavMesh.TurnOff ();
				}

				//navMesh.collider.GetComponent <NavigationMesh>().TurnOn ();
				navMesh.TurnOn ();
				KickStarter.sceneSettings.navMesh = navMesh;
			}
		}


		private void UnloadPlayerStart (int playerStartInt)
		{
			PlayerStart playerStart = Serializer.returnComponent <PlayerStart> (playerStartInt);

			if (playerStart && KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.defaultPlayerStart = playerStart;
			}
		}


		private void UnloadSortingMap (int sortingMapInt)
		{
			SortingMap sortingMap = Serializer.returnComponent <SortingMap> (sortingMapInt);

			if (sortingMap && KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.sortingMap = sortingMap;

				// Reset all FollowSortingMap components
				FollowSortingMap[] followSortingMaps = FindObjectsOfType (typeof (FollowSortingMap)) as FollowSortingMap[];
				foreach (FollowSortingMap followSortingMap in followSortingMaps)
				{
					followSortingMap.UpdateSortingMap ();
				}
			}
		}


		private void UnloadCutsceneOnLoad (int cutsceneInt)
		{
			Cutscene cutscene = Serializer.returnComponent <Cutscene> (cutsceneInt);

			if (cutscene && KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.cutsceneOnLoad = cutscene;
			}
		}


		private void UnloadCutsceneOnStart (int cutsceneInt)
		{
			Cutscene cutscene = Serializer.returnComponent <Cutscene> (cutsceneInt);

			if (cutscene && KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.cutsceneOnStart = cutscene;
			}
		}


		private List<TransformData> PopulateTransformData ()
		{
			List<TransformData> allTransformData = new List<TransformData>();
			RememberTransform[] transforms = FindObjectsOfType (typeof (RememberTransform)) as RememberTransform[];
			
			foreach (RememberTransform _transform in transforms)
			{
				if (_transform.constantID != 0)
				{
					allTransformData.Add (_transform.SaveTransformData ());
				}
				else
				{
					Debug.LogWarning ("GameObject " + _transform.name + " was not saved because it's ConstantID has not been set!");
				}
			}
			
			return allTransformData;
		}


		private void UnloadTransformData (List<TransformData> _transforms)
		{
			// Delete any objects (if told to)
			RememberTransform[] currentTransforms = FindObjectsOfType (typeof (RememberTransform)) as RememberTransform[];
			foreach (RememberTransform transformOb in currentTransforms)
			{
				if (transformOb.saveScenePresence)
				{
					// Was object not saved?
					bool found = false;
					foreach (TransformData _transform in _transforms)
					{
						if (_transform.objectID == transformOb.constantID)
						{
							found = true;
						}
					}

					if (!found)
					{
						// Can't find: delete
						Destroy (transformOb.gameObject);
					}
				}
			}

			foreach (TransformData _transform in _transforms)
			{
				RememberTransform saveObject = Serializer.returnComponent <RememberTransform> (_transform.objectID);

				// Restore any deleted objects (if told to)
				if (saveObject == null && _transform.bringBack)
				{
					Object[] assets = Resources.LoadAll ("", typeof (GameObject));
					foreach (Object asset in assets)
					{
						if (asset is GameObject)
						{
							GameObject assetObject = (GameObject) asset;
							if (assetObject.GetComponent <RememberTransform>() && assetObject.GetComponent <RememberTransform>().constantID == _transform.objectID)
							{
								GameObject newObject = (GameObject) Instantiate (assetObject.gameObject);
								newObject.name = assetObject.name;
								saveObject = newObject.GetComponent <RememberTransform>();
							}
						}
					}
					Resources.UnloadUnusedAssets ();
				}

				if (saveObject != null)
				{
					saveObject.LoadTransformData (_transform);
				}
			}
		}


		private List<ScriptData> PopulateScriptData ()
		{
			List<ScriptData> allScriptData = new List<ScriptData>();
			Remember[] scripts = FindObjectsOfType (typeof (Remember)) as Remember[];
			
			foreach (Remember _script in scripts)
			{
				if (_script.constantID != 0)
				{
					allScriptData.Add (new ScriptData (_script.constantID, _script.SaveData ()));
				}
				else
				{
					Debug.LogWarning ("GameObject " + _script.name + " was not saved because it's ConstantID has not been set!");
				}
			}
			
			return allScriptData;
		}


		private void AssignMenuLocks (List<Menu> menus, string menuLockData)
		{
			if (menuLockData.Length == 0)
			{
				return;
			}

			string[] lockArray = menuLockData.Split ("|"[0]);
			
			foreach (string chunk in lockArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				int _id = 0;
				int.TryParse (chunkData[0], out _id);
				
				bool _lock = false;
				bool.TryParse (chunkData[1], out _lock);
				
				foreach (AC.Menu _menu in menus)
				{
					if (_menu.id == _id)
					{
						_menu.isLocked = _lock;
						break;
					}
				}
			}
		}


		private void UnloadVariablesData (string data)
		{
			if (data == null)
			{
				return;
			}
			
			if (data.Length > 0)
			{
				string[] varsArray = data.Split ("|"[0]);
				
				foreach (string chunk in varsArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);

					GVar var = LocalVariables.GetVariable (_id);
					if (var.type == VariableType.String)
					{
						string _text = chunkData[1];
						var.SetValue (_text);
					}
					else if (var.type == VariableType.Float)
					{
						float _value = 0f;
						float.TryParse (chunkData[1], out _value);
						var.SetValue (_value, SetVarMethod.SetValue);
					}
					else
					{
						int _value = 0;
						int.TryParse (chunkData[1], out _value);
						var.SetValue (_value, SetVarMethod.SetValue);
					}
				}
			}
		}

	}
		

	[System.Serializable]
	public class SingleLevelData
	{
		
		public List<ScriptData> allScriptData;
		public List<TransformData> allTransformData;
		public int sceneNumber;

		public int navMesh;
		public int playerStart;
		public int sortingMap;
		public int onLoadCutscene;
		public int onStartCutscene;

		public string localVariablesData;


		public SingleLevelData ()
		{
			allScriptData = new List<ScriptData>();
			allTransformData = new List<TransformData>();
		}

	}


	[System.Serializable]
	public struct ScriptData
	{
		public int objectID;
		public string data;

		public ScriptData (int _objectID, string _data)
		{
			objectID = _objectID;
			data = _data;
		}
	}

}