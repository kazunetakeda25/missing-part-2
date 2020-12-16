using UnityEngine;
using System.Collections;
using ScreenFaderComponents.Actions;

public class DemoCoroutineGUI : MonoBehaviour
{
    TextMesh text;
    TextMesh smalltext;
    bool showLabel = false;
    int showButton = 0;

    Transform cameraTransform;
    Vector3 originalCamPos = new Vector3(0, 1, -10);
    Vector3 whatsnewCamPos = new Vector3(0, -20, -10);
    Vector3 howtouseCamPos = new Vector3(0, 15, -10);

    void Start()
    {
        text = GameObject.Find("Text").GetComponent<TextMesh>();
        smalltext = GameObject.Find("SmallText").GetComponent<TextMesh>();
        cameraTransform = Camera.main.transform;

        /// Start fading sequence
        Fader.Instance.FadeIn(0).Pause()
            .StartCoroutine(this, ChangeTitleText1())
            .FadeOut().Pause(2).FadeIn()
            .StartCoroutine(this, ChangeTitleText2())
            .FadeOut().Pause(2).FadeIn()
            .StartCoroutine(this, "StartBlinkingLight", "light1")
            .StartCoroutine(this, "StartBlinkingLight", "light2")
            .StartCoroutine(this, ChangeTitleText3()).FadeOut()
            .StartCoroutine(this, ShowSmallTextAndGUI());
    }

    /// <summary>
    /// Blinking lights coroutine
    /// </summary>
    IEnumerator StartBlinkingLight(string name)
    {
        Light l = GameObject.Find(name).GetComponent<Light>();
        while (true)
        {
            l.enabled = true;
            yield return new WaitForSeconds(Random.Range(1.0f, 3f));
            l.enabled = false;
            yield return new WaitForSeconds(0.05f);
            if (Random.Range(0, 5) == 1)
            {
                l.intensity = 0.5f;
                l.enabled = true;
                yield return new WaitForSeconds(0.05f);
                l.enabled = false;
                l.intensity = 1f;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    /// <summary>
    /// Set text: welcome
    /// </summary>
    IEnumerator ChangeTitleText1()
    {
        text.text = "welcome";
        yield break; /// return immediately
    }
    /// <summary>
    /// Set text: to the magic
    /// </summary>
    IEnumerator ChangeTitleText2()
    {
        text.text = "to the magic";
        yield return new WaitForSeconds(1); /// return after 1 second
    }
    /// <summary>
    /// Set text: of Screen Fader
    /// </summary>
    IEnumerator ChangeTitleText3()
    {
        text.text = "of Screen Fader";
        GameObject.Find("light3").GetComponent<Light>().enabled = true;
        yield return new WaitForSeconds(1); /// return after 1 second
    }
    /// <summary>
    /// Set small text and activate GUI buttons
    /// </summary>
    IEnumerator ShowSmallTextAndGUI()
    {
        yield return new WaitForSeconds(2);
        smalltext.text = "the easiest";
        yield return new WaitForSeconds(1);
        smalltext.text = "the easiest   way";
        yield return new WaitForSeconds(1);
        smalltext.text = "the easiest   way   to make";
        yield return new WaitForSeconds(1);
        smalltext.text = "the easiest   way   to make   fadings";
        yield return new WaitForSeconds(2);

        while (showButton < 5)
        {
            showButton++;
            yield return new WaitForSeconds(0.15f);
        }

        showLabel = true;

        yield break;
    }

    IEnumerator MoveCameraToMenu()
    {
        while (Vector3.Distance(cameraTransform.position, originalCamPos) > 0.25f)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, originalCamPos, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        while (showButton < 5)
        {
            showButton++;
            yield return new WaitForSeconds(0.15f);
        }

        yield break;
    }
    IEnumerator MoveCameraToHowToUse()
    {
        while (Vector3.Distance(cameraTransform.position, howtouseCamPos) > 0.25f)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, howtouseCamPos, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        showButton = -1;
        yield break;
    }
    IEnumerator MoveCameraToWTN()
    {
        while (Vector3.Distance(cameraTransform.position, whatsnewCamPos) > 0.25f)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, whatsnewCamPos, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        showButton = -1;
        yield break;
    }

    void OnGUI()
    {
        float left = Screen.width / 2-100;
        float top = Screen.height / 2 + 50;
        if (showButton > 0 && GUI.Button(new Rect(left, top + 33 * 1, 200, 30), "Default Fading Demo")) { Fader.Instance.FadeIn(2).Pause().StartAction(new LoadSceneAction(), 1); }
        if (showButton > 1 && GUI.Button(new Rect(left, top + 33 * 2, 200, 30), "Squared effect Demo")) { Fader.Instance.FadeIn(2).Pause().StartAction(new LoadSceneAction(), 2); }
        if (showButton > 2 && GUI.Button(new Rect(left, top + 33 * 3, 200, 30), "Stripes effectDemo")) { Fader.Instance.FadeIn(2).Pause().StartAction(new LoadSceneAction(), 3); }
        if (showButton > 3 && GUI.Button(new Rect(left, top + 33 * 4, 200, 30), "How to use?")) { showButton = 0; Fader.Instance.StartCoroutine(this, "MoveCameraToHowToUse"); }
        if (showButton > 4 && GUI.Button(new Rect(left, top + 33 * 5, 200, 30), "What's new?")) { showButton = 0; Fader.Instance.StartCoroutine(this, "MoveCameraToWTN"); }

        if (showButton == -1 && GUI.Button(new Rect(left, Screen.height - 100, 200, 30), "Main Menu")) { showButton = 0; Fader.Instance.StartCoroutine(this, MoveCameraToMenu()); }

        if (showLabel)
            GUI.Label(new Rect(Screen.width / 2 - 175, Screen.height-50, 700, 30), "Note, this demo scene also created with Screen Fader v.1.2");

    }
}