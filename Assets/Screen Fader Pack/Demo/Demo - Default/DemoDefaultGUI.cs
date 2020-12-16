using UnityEngine;
using System.Collections;
using ScreenFaderComponents.Actions;

public class DemoDefaultGUI : MonoBehaviour
{
    [SerializeField]
    private DefaultScreenFader component = null;
    [SerializeField]
    private Texture2D logo = null;

    private float _r = 0;
    private float _g = 0;
    private float _b = 0;
    private float r;
    private float g;
    private float b;
    private float fadeSpeed = 1;
    private ShowLogoAction showLogoAction = new ShowLogoAction();

    void Start()
    {
        /// I setup default state of Fader as "In" in Inspector panel, 
        /// so we have a black screen now.
        /// Let's Fade-out it in 2 seconds:
        Fader.Instance.FadeOut(2);

        /// Here we save a color of fading, we will use it in GUI 
        /// to change fading color at the runtime.
        _r = component.fadeColor.r;
        _g = component.fadeColor.g;
        _b = component.fadeColor.b;
    }

    void OnGUI()
    {
        GUI.depth = -3;
        GUI.Window(1, new Rect(0, 150, 220, 390), DoWindow, "Settings");

        if (showLogoAction.IsLogoVisible)
        {
            GUI.DrawTexture(new Rect(500, 100, logo.width, logo.height), logo);
            GUI.Label(new Rect(500, 100 + logo.height, logo.width, 500), "Screen Fader it's the esiest way to fade-in or fade-out screen. \r\n\r\nScreen Fader is very simple, but on the other hand, it provide big possibilities. You can setup colors, transparency, speed of effect and delays before it starts in the Inspector panel.\r\nYou can subscribe on events and get notifications when effects will start or complete.");
        }
    }

    /// <summary>
    /// Draw Settings window.
    /// </summary>
    void DoWindow(int id)
    {
        DrawControls();

        /// "Fade IN" button 
        if (GUI.Button(new Rect(10, 270, 95, 30), "Fade IN " + fadeSpeed.ToString("#.0")))
            Fader.Instance.FadeIn(fadeSpeed);

        /// "Fade OUT" button
        if (GUI.Button(new Rect(115, 270, 95, 30), "Fade OUT " + fadeSpeed.ToString("#.0")))
            Fader.Instance.FadeOut(fadeSpeed);

        /// "FadeIN-PAUSE-FadeOUT" button
        if (GUI.Button(new Rect(10, 310, 200, 30), "FadeIn, Pause 3 sec, FadeOUT"))
            Fader.Instance.FadeIn().StartAction(showLogoAction).Pause(3).StartAction(showLogoAction).FadeOut();

        /// "Flash" button
        if (GUI.Button(new Rect(10, 350, 200, 30), "Flash"))
            Fader.Instance.Flash();
    }

    /// <summary>
    /// Draw GUI controls
    /// </summary>
    private void DrawControls()
    {
        GUI.Label(new Rect(10, 30, 200, 20), "Color");
        GUI.Label(new Rect(20, 50, 200, 20), "Red: ");
        GUI.Label(new Rect(20, 70, 200, 20), "Green: ");
        GUI.Label(new Rect(20, 90, 200, 20), "Blue: ");
        
        r = GUI.HorizontalSlider(new Rect(100, 55, 100, 20), _r, 0.0f, 1.0f);
        g = GUI.HorizontalSlider(new Rect(100, 75, 100, 20), _g, 0.0f, 1.0f);
        b = GUI.HorizontalSlider(new Rect(100, 95, 100, 20), _b, 0.0f, 1.0f);
        if (r != _r | g != _g | b != _b)
        {
            _r = r;
            _g = g;
            _b = b;
            component.fadeColor = new Color(_r, _g, _b);
        }
        
        GUI.Label(new Rect(10, 110, 200, 20), "Max Density: ");
        component.maxDensity = GUI.HorizontalSlider(new Rect(100, 115, 100, 20), component.maxDensity, 0.0f, 1.0f);
        
        fadeSpeed = GUI.HorizontalSlider(new Rect(100, 135, 100, 20), fadeSpeed, 0.1f, 5);
        GUI.Label(new Rect(10, 130, 100, 20), string.Format("Speed: {0}", fadeSpeed.ToString("#.0")));
    }
}