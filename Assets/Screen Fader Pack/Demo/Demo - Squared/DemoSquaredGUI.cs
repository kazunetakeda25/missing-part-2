using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ScreenFaderComponents.Actions;

public class DemoSquaredGUI : MonoBehaviour {

    public SquaredScreenFader component;
    public Texture2D logo;
    public Texture2D[] patterns;

    private int selectedPattern = 0;
    private float fadeSpeed = 1;

    private ShowLogoAction showLogoAction = new ShowLogoAction();

    void Start()
    {
        /// I setup default state of Fader as "In" in Inspector panel, 
        /// so we have a black screen now.
        /// Let's Fade-out it in 2 seconds:
        Fader.Instance.FadeOut(2);
    }

    void OnGUI()
    {
        GUI.depth = -3;
        GUI.Window(1, new Rect(0, 150, 220, 390), DoWindow, "Settings");

        if (showLogoAction.IsLogoVisible && logo != null)
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
            Fader.Instance.FadeIn(fadeSpeed).StartAction(showLogoAction);

        /// "Fade OUT" button
        if (GUI.Button(new Rect(115, 350, 95, 30), "Fade OUT"))
            Fader.Instance.StartAction(showLogoAction).FadeOut(fadeSpeed);
    }

    /// <summary>
    /// Draw GUI controls
    /// </summary>
    private void DrawControls()
    {
        GUI.Label(new Rect(10, 20, 200, 20), "Directions");
        Direction(20, 40, SquaredScreenFader.Direction.NONE);
        Direction(20, 60, SquaredScreenFader.Direction.DIAGONAL_LEFT_DOWN);
        Direction(20, 80, SquaredScreenFader.Direction.DIAGONAL_LEFT_UP);
        Direction(20, 100, SquaredScreenFader.Direction.DIAGONAL_RIGHT_DOWN);
        Direction(20, 120, SquaredScreenFader.Direction.DIAGONAL_RIGHT_UP);
        Direction(20, 140, SquaredScreenFader.Direction.HORIZONTAL_LEFT);
        Direction(20, 160, SquaredScreenFader.Direction.HORIZONTAL_RIGHT);
        Direction(20, 180, SquaredScreenFader.Direction.VERTICAL_DOWN);
        Direction(20, 200, SquaredScreenFader.Direction.VERTICAL_UP);

        fadeSpeed = GUI.HorizontalSlider(new Rect(90, 235, 110, 20), fadeSpeed, 0.5f, 10);
        GUI.Label(new Rect(10, 230, 200, 20), string.Format("Speed {0:N1}", fadeSpeed));

        component.columns = (int)GUI.HorizontalSlider(new Rect(90, 260, 110, 20), component.columns, 5, 50);
        GUI.Label(new Rect(10, 255, 200, 20), string.Format("Columns {0}", component.columns));

        int selected = GUI.SelectionGrid(new Rect(10, 300, 200, 20), selectedPattern, patterns, patterns.Length);
        GUI.Label(new Rect(10, 280, 200, 20), "Patterns");
        if (selected != selectedPattern)
        {
            selectedPattern = selected;
            component.texture = patterns[selectedPattern];
        }
    }

    /// Draw toggle button of fading directions.
    void Direction(int x, int y, SquaredScreenFader.Direction direction)
    {
        if (GUI.Toggle(new Rect(x, y, 240, 20), component.direction == direction, Enum.GetName(typeof(SquaredScreenFader.Direction), direction)))
            component.direction = direction;
    }
}