using UnityEngine; 
using UnityEngine.UI; 
using UnityEngine.EventSystems; 
using System.Collections;

[RequireComponent(typeof(InputField))]
public class CaretFix : MonoBehaviour {

	public float caretOffset = 20f;

	private bool runOnce = false;

	public void OnCaretFix() 
	{ 
		if(runOnce == true)
			return;

		runOnce = true;
		InputField ipFld = this.gameObject.GetComponent<InputField>(); 
		RectTransform caretTransform = (RectTransform)transform.Find(gameObject.name+" Input Caret"); 
		caretTransform.Translate(Vector3.up * caretOffset); 
	} 
}