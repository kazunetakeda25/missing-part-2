using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Credits : MonoBehaviour 
{
	[SerializeField] TextAsset creditsFile;
	[SerializeField] Text creditsText;
	[SerializeField] ScrollRect scroller;
	[SerializeField] float scrollSpeed;

	private bool scrolling;

	private void Start()
	{
		Initialize();
	}

	private void Initialize()
	{
		creditsText.text = creditsFile.text;
	}

	public void OnStartScroll()
	{
		scroller.normalizedPosition = Vector2.one;
		scrolling = true;
	}

	public void OnStopScroll()
	{
		scrolling = false;
	}

	private void Update()
	{
		if(scrolling == false) {
			return;
		}

		float yPos = scroller.normalizedPosition.y;
		yPos -= (Time.deltaTime * scrollSpeed);

		if(yPos <= 0.0f) {
			yPos = 1.0f;
		}

		scroller.normalizedPosition = new Vector2(0, yPos);
	}

}
