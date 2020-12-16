using UnityEngine;

/// <summary>
/// 
/// </summary>
class ImageScreenFader : DefaultScreenFader
{
    public Texture image = null;
    protected override void Init()
    {
        colorTexture = image;
        last_fadeColor = fadeColor;
    }
}