using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ScreenFaderComponents.Actions;

class SwitchScenesGUI : MonoBehaviour
{
    void OnGUI()
    {
        GUI.depth = -3;
        GUI.Window(0, new Rect(0, 0, 220, 140), DoWindow, "Screen Fader Types");
    }

    void DoWindow(int id)
    {
        if (GUI.Button(new Rect(10, 30, 200, 30), "Default Fading"))
            Fader.Instance.FadeIn().StartAction(new LoadSceneAction(), 1);
        if (GUI.Button(new Rect(10, 65, 200, 30), "Squares Effect"))
            Fader.Instance.FadeIn().StartAction(new LoadSceneAction(), 2);
        if (GUI.Button(new Rect(10, 100, 200, 30), "Stripes Effect"))
            Fader.Instance.FadeIn().StartAction(new LoadSceneAction(), 3);
    }
}