//using UnityEngine;
//using System.Collections;
//
//public class TypewriterEffect : MonoBehaviour {
//
//	private const float DURATION_BUFFER = 3.0f;
//
//	public float animationLength = 5;
//
//	private bool skipEffect = false;
//	private string originalText;
//	private dfLabel label;
//
//	void Start()
//	{
//		label = gameObject.GetComponent<dfLabel>();
//		originalText = label.Text;
//
//		StartCoroutine( TypewriterAnimation() );
//	}
//
//	public void OnClick()
//	{
//		skipEffect = true;
//	}
//
//	private IEnumerator TypewriterAnimation()
//	{
//		label.Text = "";
//
//		float delay = animationLength / ((float)originalText.Length + DURATION_BUFFER);
//
//		for (int i = 0; i < originalText.Length; i++)
//		{
//			if( !label )
//				break;
//			
//			if( Debug.isDebugBuild && skipEffect )
//			{
//				label.Text = originalText;
//				break;
//			}
//			
//			label.Text += originalText[i];
//
//			/* Gods of opimization, have mercy. DaikonForge label doesn't update when changed unless I do this. */
//			//Note from Thom: It's because you're in a coroutine.  To optimize the code, this should be happening in an Update.
//			label.enabled = false;
//			label.enabled = true;
//
//			yield return new WaitForSeconds( delay );
//		}
//	}
//}
