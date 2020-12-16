using UnityEngine;
using System.Collections;
using System;

public class DefaultScreenFader : Fader
{
    public Color fadeColor = Color.black;
    protected Color last_fadeColor = Color.black;
    [Range(0, 1)]
    public float maxDensity = 1;
    protected Texture colorTexture;

    protected override void Init()
    {
        base.Init();

        colorTexture = base.GetTextureFromColor(fadeColor);
        last_fadeColor = fadeColor;
    }

    protected override void DrawOnGUI()
    {
        GUI.color = fadeColor;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), colorTexture, ScaleMode.StretchToFill, true);
    }
    protected override void Update()
    {
        if (fadeColor != last_fadeColor)
            Init();

        fadeColor.a = GetLinearBalance();
        base.Update();
    }

    protected virtual float GetLinearBalance()
    {
        return fadeBalance < maxDensity ? fadeBalance : maxDensity;
    }
    
}