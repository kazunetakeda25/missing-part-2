/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Paths.cs"
 * 
 *	This script stores a series of "nodes", which act
 *	as waypoints for character movement.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class Paths : MonoBehaviour
	{
		
		public List<Vector3> nodes = new List<Vector3>();
		public List<NodeCommand> nodeCommands = new List<NodeCommand>();
		public ActionListSource commandSource;

		public AC_PathType pathType = AC_PathType.ForwardOnly;
		public PathSpeed pathSpeed;
		public bool teleportToStart;
		public bool affectY;
		public float nodePause;
		
		
		private void Awake ()
		{
			if (nodePause < 0f)
			{
				nodePause = 0f;
			}

			if (nodes == null || nodes.Count == 0)
			{
				nodes.Add (transform.position);
			}
			else
			{
				nodes[0] = transform.position;
			}
		}
		
		
		public bool WillStopAtNextNode (int currentNode)
		{
			if (GetNextNode (currentNode, currentNode-1, false) == -1)
			{
				return true;
			}
			
			return false;
		}
		
		
		public void BuildNavPath (Vector3[] pointData)
		{
			if (pointData.Length > 0)
			{
				pathType = AC_PathType.ForwardOnly;
				affectY = false;
				nodePause = 0;
				
				nodes.Clear ();
				nodes.Add (this.transform.position);

				nodeCommands.Clear ();
				
				foreach (Vector3 point in pointData)
				{
					nodes.Add (point);
				}
			}
		}
		
		
		public int GetNextNode (int currentNode, int prevNode, bool playerControlled)
		{
			int numNodes = nodes.Count;
			
			if (numNodes == 1)
			{
				return -1;
			}
			
			else if (playerControlled)
			{
				if (currentNode == 0)
				{
					return 1;
				}
				else if (currentNode == numNodes - 1)
				{
					return -1;
				}

				return (currentNode + 1);
			}
			
			else
			{
				if (pathType == AC_PathType.ForwardOnly)
				{
					if (currentNode == numNodes - 1)
					{
						return -1;
					}

					return (currentNode + 1);
				}
				
				else if (pathType == AC_PathType.Loop)
				{
					if (currentNode == numNodes-1)
					{
						return 0;
					}

					return (currentNode + 1);
				}
				
				else if (pathType == AC_PathType.PingPong)
				{
					if (prevNode > currentNode)
					{
						// Going backwards
						if (currentNode == 0)
						{
							return 1;
						}
						else
						{
							return (currentNode - 1);
						}
					}
					else
					{
						// Going forwards
						if (currentNode == numNodes-1)
						{
							return (currentNode - 1);
						}
						
						return (currentNode + 1);
					}
				}
				
				else if (pathType == AC_PathType.IsRandom)
				{
					if (numNodes > 0)
					{
						int randomNode = Random.Range (0, numNodes);
						
						while (randomNode == currentNode)
						{
							randomNode = Random.Range (0, numNodes);
						}
						
						return (randomNode);
					}
					
					return 0;
				}
				
				return -1;
			}
		}
		
		
		private void OnDrawGizmos ()
		{
			// Draws a blue line from this transform to the target
			Gizmos.color = Color.blue;
			int i;
			int numNodes = nodes.Count;

			if (nodes.Count > 0)
			{
				nodes[0] = transform.position;
			}

			if (pathType == AC_PathType.IsRandom && numNodes > 1)
			{
				for (i=1; i<numNodes; i++)
				{
					for (int j=0; j<numNodes; j++)
					{
						if (i != j)
						{
							ConnectNodes (i,j);
						}
					}
				}
			}
			else
			{
				if (numNodes > 1)
				{
					for (i=1; i<numNodes; i++)
					{
						Gizmos.DrawIcon (nodes[i], "", true);
						
						ConnectNodes (i, i - 1);
					}
				}
				
				if (pathType == AC_PathType.Loop && !teleportToStart)
				{
					if (numNodes > 2)
					{
						ConnectNodes (numNodes-1, 0);
					}
				}
			}
		}


		public float GetLengthToNode (int n)
		{
			if (n > 0 && nodes.Count > n)
			{
				float length = 0f;
				
				for (int i=1; i<=n; i++)
				{
					length += Vector3.Distance (nodes[i-1], nodes[i]);
				}
				
				return length;
			}
			
			return 0f;
		}


		public float GetLengthBetweenNodes (int a, int b)
		{
			if (a == b)
			{
				return 0f;
			}

			if (b < a)
			{
				int c = a;
				a = b;
				b = c;
			}

			float length = 0f;
			
			for (int i=a+1; i<=b; i++)
			{
				length += Vector3.Distance (nodes[i-1], nodes[i]);
			}
			
			return length;
		}


		public float GetTotalLength ()
		{
			if (nodes.Count > 1)
			{
				return GetLengthToNode (nodes.Count-1);
			}

			return 0f;
		}
		
		
		private void ConnectNodes (int a, int b)
		{
			Vector3 PosA  = nodes[a];
			Vector3 PosB = nodes[b];
			Gizmos.DrawLine (PosA, PosB);
		}
		
	}


	[System.Serializable]
	public class NodeCommand
	{

		public Cutscene cutscene;
		public ActionListAsset actionListAsset;
		public int parameterID;


		public NodeCommand ()
		{
			cutscene = null;
			actionListAsset = null;
			parameterID = -1;
		}

	}

}