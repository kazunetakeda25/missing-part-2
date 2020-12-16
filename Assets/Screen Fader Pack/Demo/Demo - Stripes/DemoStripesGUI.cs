using System.Collections;
using System;
using UnityEngine;
using ScreenFaderComponents;
using ScreenFaderComponents.Actions;
using ScreenFaderComponents.Enumerators;
using ScreenFaderComponents.Events;


public class DemoStripesGUI : MonoBehaviour
{
    public StripeScreenFader component;
    public Texture2D logo;

    private bool showLogo = false;
    private float _r = 0;
    private float _g = 0;
    private float _b = 0;
    private float fadeSpeed = 1;

    void Start()
    {        
        /// I setup default state of Fader as "In" in Inspector panel, 
        /// so we have a black screen now.
        /// Let's Fade-out it in 2 seconds:
        Fader.Instance.FadeOut(2);

        /// Subscribe on events of Start and Finish fading
        Fader.Instance.FadeFinish += new EventHandler<FadeEventArgs>(Instance_FadeFinish);
        Fader.Instance.FadeStart += new EventHandler<FadeEventArgs>(Instance_FadeStart);

        /// Here we save a color of fading, we will use it in GUI 
        /// to change fading color at the runtime.
        _r = component.color.r;
        _g = component.color.g;
        _b = component.color.b;
    }

    void OnGUI()
    {
        GUI.depth = -3;
        GUI.Window(1, new Rect(0, 150, 220, 390), DoWindow, "Settings");

        if (showLogo)
        {
            GUI.DrawTexture(new Rect(500, 100, logo.width, logo.height), logo);
            GUI.Label(new Rect(500, 100 + logo.height, logo.width, 500), "Screen Fader it's the esiest way to fade-in or fade-out screen. \r\n\r\nScreen Fader is very simple, but on the other hand, it provide big possibilities. You can setup colors, transparency, speed of effect and delays before it starts in the Inspector panel.\r\nYou can subscribe on events and get notifications when effects will start or complete.");
        }
    }

    /// Draw Settings window.
    void DoWindow(int id)
    {
        DrawControls();

        /// "Fade IN" button
        if (GUI.Button(new Rect(10, 350, 95, 30), "Fade IN"))
            Fader.Instance.FadeIn(fadeSpeed);

        /// "Fade OUT" button
        if (GUI.Button(new Rect(115, 350, 95, 30), "Fade OUT"))
            Fader.Instance.FadeOut(fadeSpeed);
    }

    /// <summary>
    /// Draw GUI controls
    /// </summary>
    private void DrawControls()
    {
        GUI.Label(new Rect(10, 20, 200, 20), "Directions");
        Direction(20, 40, StripeScreenFader.Direction.HORIZONTAL_LEFT);
        Direction(20, 60, StripeScreenFader.Direction.HORIZONTAL_RIGHT);
        Direction(20, 80, StripeScreenFader.Direction.HORIZONTAL_OUT);
        Direction(20, 100, StripeScreenFader.Direction.HORIZONTAL_IN);

        GUI.Label(new Rect(10, 130, 200, 20), "Color");

        GUI.Label(new Rect(20, 150, 200, 20), "Red: ");
        GUI.Label(new Rect(20, 170, 200, 20), "Green: ");
        GUI.Label(new Rect(20, 190, 200, 20), "Blue: ");

        float r = GUI.HorizontalSlider(new Rect(100, 155, 100, 20), _r, 0.0f, 1.0f);
        float g = GUI.HorizontalSlider(new Rect(100, 175, 100, 20), _g, 0.0f, 1.0f);
        float b = GUI.HorizontalSlider(new Rect(100, 195, 100, 20), _b, 0.0f, 1.0f);

        if (r != _r | g != _g | b != _b)
        {
            _r = r;
            _g = g;
            _b = b;
            component.color = new Color(_r, _g, _b);
        }

        component.numberOfStripes = (int)GUI.HorizontalSlider(new Rect(100, 215, 100, 20), component.numberOfStripes, 3, 50);
        GUI.Label(new Rect(10, 210, 200, 20), string.Format("Stripes: {0}", component.numberOfStripes));

        fadeSpeed = GUI.HorizontalSlider(new Rect(100, 235, 100, 20), fadeSpeed, 0.5f, 10);
        GUI.Label(new Rect(10, 230, 100, 20), string.Format("Speed: {0}", fadeSpeed.ToString("#.0")));
    }
    
    /// Draw toggle button of fading directions.
    void Direction(int x, int y, StripeScreenFader.Direction direction)
    {
        if (GUI.Toggle(new Rect(x, y, 240, 20), component.direction == direction, Enum.GetName(typeof(StripeScreenFader.Direction), direction)))
            component.direction = direction;
    }

    /// Catch start fading and hide logotype in case of fading out.
    void Instance_FadeStart(object sender, FadeEventArgs e)
    {
        // Hide logo when fadings starts
        showLogo = false;
    }

    /// Catch finish fading and show logotype in case of fading in.
    void Instance_FadeFinish(object sender, FadeEventArgs e)
    {
        // Show lofo when fade-in finished 
        if (e.Direction == FadeDirection.In)
            showLogo = true;
    }
}