/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"FollowSortingMap.cs"
 * 
 *	This script causes any attached Sprite Renderer
 *	to change according to the scene's Sorting Map.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	[ExecuteInEditMode]
	public class FollowSortingMap : MonoBehaviour
	{
		
		public bool lockSorting = false;
		public bool affectChildren = true;
		public bool followSortingMap = false;
		public bool offsetOriginal = false;
		public bool livePreview = false;
		
		private float originalDepth = 0f;
		private enum DepthAxis { Y, Z };
		private DepthAxis depthAxis = DepthAxis.Y;
		
		private Renderer _renderer;
		
		private List<int> offsets = new List<int>();
		private int sortingOrder = 0;
		private string sortingLayer = "";
		private SortingMap sortingMap;
		
		
		private void Awake ()
		{
			if (KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}
			if (GetComponent <Renderer>())
			{
				_renderer = GetComponent <Renderer>();
			}
			else
			{
				Debug.LogWarning ("FollowSortingMap on " + gameObject.name + " must be attached alongside a Renderer component.");
			}
			SetOriginalDepth ();
		}
		
		
		private void Start ()
		{
			if (KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}
			
			UpdateSortingMap ();
			SetOriginalOffsets ();
		}
		
		
		private void LateUpdate ()
		{
			UpdateRenderers ();
		}
		
		
		private void SetOriginalOffsets ()
		{
			if (offsets.Count > 0)
			{
				return;
			}
			
			offsets = new List<int>();
			
			if (offsetOriginal && _renderer)
			{
				if (affectChildren)
				{
					Renderer[] renderers = GetComponentsInChildren <Renderer>();
					foreach (Renderer childRenderer in renderers)
					{
						offsets.Add (childRenderer.sortingOrder);
					}
				}
				else
				{
					offsets.Add (_renderer.sortingOrder);
				}
			}
		}
		
		
		public string GetOrder ()
		{
			if (sortingMap == null)
			{
				return "";
			}
			
			if (sortingMap.mapType == SortingMapType.OrderInLayer)
			{
				return sortingOrder.ToString ();
			}
			else
			{
				return sortingLayer;
			}
		}
		
		
		private void OnLevelWasLoaded ()
		{
			if (KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}
			
			UpdateSortingMap ();
			SetOriginalOffsets ();
		}
		
		
		private void SetOriginalDepth ()
		{
			if (KickStarter.settingsManager == null)
			{
				return;
			}
			
			if (KickStarter.settingsManager.IsTopDown ())
			{
				depthAxis = DepthAxis.Y;
				originalDepth = transform.localPosition.y;
			}
			else
			{
				depthAxis = DepthAxis.Z;
				originalDepth = transform.localPosition.z;
			}
		}
		
		
		public void SetDepth (float depth)
		{
			if (depthAxis == DepthAxis.Y)
			{
				transform.localPosition = new Vector3 (transform.localPosition.x, originalDepth + depth, transform.localPosition.z);
			}
			else
			{
				transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, originalDepth + depth);
			}
		}
		
		
		public void UpdateSortingMap ()
		{
			if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.sortingMap != null)
			{
				sortingMap = KickStarter.sceneSettings.sortingMap;
				SetOriginalDepth ();
				sortingMap.UpdateSimilarFollowers (this);
			}
			else
			{
				Debug.Log (this.gameObject.name + " cannot find sorting map to follow!");
			}
		}
		
		
		private void UpdateRenderers ()
		{
			if (lockSorting || !followSortingMap || sortingMap == null || _renderer == null)
			{
				return;
			}
			
			#if UNITY_EDITOR
			if (!Application.isPlaying && !livePreview)
			{
				if (GetComponentInParent <Char>())
				{
					GetComponentInParent <Char>().transform.localScale = Vector3.one;
					return;
				}
			}
			#endif
			
			sortingMap.UpdateSimilarFollowers (this);
			
			if (sortingMap.sortingAreas.Count > 0)
			{
				// Set initial value as below the last line
				if (sortingMap.mapType == SortingMapType.OrderInLayer)
				{
					sortingOrder = sortingMap.sortingAreas [sortingMap.sortingAreas.Count-1].order;
				}
				else if (sortingMap.mapType == SortingMapType.SortingLayer)
				{
					sortingLayer = sortingMap.sortingAreas [sortingMap.sortingAreas.Count-1].layer;
				}
				
				for (int i=0; i<sortingMap.sortingAreas.Count; i++)
				{
					// Determine angle between SortingMap's normal and relative position - if <90, must be "behind" the plane
					if (Vector3.Angle (sortingMap.transform.forward, sortingMap.GetAreaPosition (i) - transform.position) < 90f)
					{
						if (sortingMap.mapType == SortingMapType.OrderInLayer)
						{
							sortingOrder = sortingMap.sortingAreas [i].order;
						}
						else if (sortingMap.mapType == SortingMapType.SortingLayer)
						{
							sortingLayer = sortingMap.sortingAreas [i].layer;
						}
						break;
					}
				}
			}
			
			if (!affectChildren)
			{
				if (sortingMap.mapType == SortingMapType.OrderInLayer)
				{
					_renderer.sortingOrder = sortingOrder;
					
					if (offsetOriginal && offsets.Count > 0)
					{
						_renderer.sortingOrder += offsets[0];
					}
				}
				else if (sortingMap.mapType == SortingMapType.SortingLayer)
				{
					_renderer.sortingLayerName = sortingLayer;
					
					if (offsetOriginal && offsets.Count > 0)
					{
						_renderer.sortingOrder = offsets[0];
					}
					else
					{
						_renderer.sortingOrder = 0;
					}
				}
				
				return;
			}
			
			Renderer[] renderers = GetComponentsInChildren <Renderer>();
			for (int i=0; i<renderers.Length; i++)
			{
				if (sortingMap.mapType == SortingMapType.OrderInLayer)
				{
					renderers[i].sortingOrder = sortingOrder;
					
					if (offsetOriginal && offsets.Count > i)
					{
						renderers[i].sortingOrder += offsets[i];
					}
				}
				else if (sortingMap.mapType == SortingMapType.SortingLayer)
				{
					renderers[i].sortingLayerName = sortingLayer;
					
					if (offsetOriginal && offsets.Count > i)
					{
						renderers[i].sortingOrder = offsets[i];
					}
					else
					{
						renderers[i].sortingOrder = 0;
					}
				}
			}
			
			#if UNITY_EDITOR
			if (!Application.isPlaying && livePreview && GetComponentInParent <Char>())
			{
				GetComponentInParent <Char>().transform.localScale = Vector3.one * GetLocalScale ();
			}
			#endif
		}
		
		
		public void LockSortingOrder (int order)
		{
			if (_renderer == null) return;
			
			lockSorting = true;
			
			if (!affectChildren)
			{
				_renderer.sortingOrder = order;
				return;
			}
			
			Renderer[] renderers = GetComponentsInChildren <Renderer>();
			foreach (Renderer childRenderer in renderers)
			{
				childRenderer.sortingOrder = order;
			}
		}
		
		
		public void LockSortingLayer (string layer)
		{
			if (_renderer == null) return;
			
			lockSorting = true;
			
			if (!affectChildren)
			{
				_renderer.sortingLayerName = layer;
				return;
			}
			
			Renderer[] renderers = GetComponentsInChildren <Renderer>();
			foreach (Renderer childRenderer in renderers)
			{
				childRenderer.sortingLayerName = layer;
			}
		}
		
		
		public float GetLocalScale ()
		{
			if (followSortingMap && sortingMap != null && sortingMap.affectScale)
			{
				return (sortingMap.GetScale (transform.position) / 100f);
			}
			
			return 0f;
		}
		
		
		public float GetLocalSpeed ()
		{
			if (followSortingMap && sortingMap != null && sortingMap.affectScale && sortingMap.affectSpeed)
			{
				return (sortingMap.GetScale (transform.position) / 100f);
			}
			
			return 1f;
		}
		
	}
	
}