using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class LipSyncTexture : MonoBehaviour
	{

		public SkinnedMeshRenderer skinnedMeshRenderer;
		public int materialIndex;
		public string propertyName = "_MainTex";
		public List<Texture2D> textures = new List<Texture2D>();


		private void Awake ()
		{
			LimitTextureArray ();
		}


		public void LimitTextureArray ()
		{
			if (AdvGame.GetReferences () == null || AdvGame.GetReferences ().speechManager == null)
			{
				return;
			}

			int arraySize = AdvGame.GetReferences ().speechManager.phonemes.Count;

			if (textures.Count != arraySize)
			{
				int numTextures = textures.Count;

				if (arraySize < numTextures)
				{
					textures.RemoveRange (arraySize, numTextures - arraySize);
				}
				else if (arraySize > numTextures)
				{
					for (int i=textures.Count; i<arraySize; i++)
					{
						textures.Add (null);
					}
				}
			}
		}


		public void SetFrame (int textureIndex)
		{
			if (skinnedMeshRenderer)
			{
				if (materialIndex >= 0 && skinnedMeshRenderer.materials.Length > materialIndex)
				{
					skinnedMeshRenderer.materials [materialIndex].SetTexture (propertyName, textures [textureIndex]);
				}
				else
				{
					Debug.LogWarning ("Cannot find material index " + materialIndex + " on SkinnedMeshRenderer " + skinnedMeshRenderer.gameObject.name);
				}
			}
		}

	}

}