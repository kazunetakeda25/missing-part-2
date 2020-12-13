using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class AARJSONTester : MonoBehaviour 
{
	private const string AAR_TEST_PATH = "Json/AAR1Data";

	private void Awake()
	{
		DeserializeTextFile();
	}

	private void DeserializeTextFile()
	{
		TextAsset json = Resources.Load<TextAsset> (AAR_TEST_PATH);
		Debug.Log (json.text);

		SlideContainer container = JsonConvert.DeserializeObject<SlideContainer>(json.text);

		Debug.Log ("Found basic slides: " + container.basicSlides.Length);
		Debug.Log ("Found movie slides: " + container.movieSlides.Length);
		Debug.Log ("Found Multiple Choice Quiz slides: " + container.multipleChoiceQuizzes.Length);
		Debug.Log ("Found Pathfinder slides: " + container.pathfinders.Length);
		Debug.Log ("Found Slider Quiz slides: " + container.sliderQuizzes.Length);
	}
}
