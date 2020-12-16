using UnityEngine;
using System.Collections;
using System;

public class StripeScreenFader : Fader// ScreenFaderBase
{
    public Color color = Color.black;
    [Range(2, 50)]
    public int numberOfStripes = 10;
    public Direction direction = Direction.HORIZONTAL_LEFT;
    public enum Direction { HORIZONTAL_LEFT, HORIZONTAL_RIGHT, HORIZONTAL_IN, HORIZONTAL_OUT }

    private Color last_color = Color.black;
    private int last_numberOfStripes = 10;

    Texture texture = null;
    AnimRect[] rcs = null;

    protected override void Init()
    {
        base.Init();

        texture = base.GetTextureFromColor(color);

        rcs = new AnimRect[numberOfStripes];
        int a = Screen.width / rcs.Length * 3;
        for (int i = 0; i < rcs.Length; i++)
        {
            rcs[i] = new AnimRect(
                new Rect((Screen.width + a) / rcs.Length * i - 5, -5, (Screen.width + a) / rcs.Length, Screen.height + 10), 
                0.1f, 
                1f);
        }

        last_color = color;
        last_numberOfStripes = numberOfStripes;
    }

    protected override void Update()
    {
        if (color != last_color | numberOfStripes != last_numberOfStripes)
            Init();

        base.Update();
    }

    protected override void DrawOnGUI()
    {
        for (int i = 0; i < rcs.Length; i++)
        {
            switch (direction)
            {
                case Direction.HORIZONTAL_LEFT:
                    GUI.DrawTexture(rcs[i].GetRect(GetLinearT(i, rcs.Length)), texture);
                    break;
                case Direction.HORIZONTAL_RIGHT:
                    GUI.DrawTexture(rcs[rcs.Length - i - 1].GetRect(GetLinearT(i, rcs.Length)), texture);
                    break;
                case Direction.HORIZONTAL_IN:
                        GUI.DrawTexture(rcs[rcs.Length - i - 1].GetRect(GetLinearT(i * 2, rcs.Length)), texture);
                        GUI.DrawTexture(rcs[i].GetRect(GetLinearT(i * 2, rcs.Length)), texture);
                    break;
                case Direction.HORIZONTAL_OUT:
                    if (i < rcs.Length / 2)
                    {
                        GUI.DrawTexture(rcs[rcs.Length / 2 - i - 1].GetRect(GetLinearT(i * 2, rcs.Length)), texture);
                        GUI.DrawTexture(rcs[rcs.Length / 2 + i].GetRect(GetLinearT(i * 2, rcs.Length)), texture);
                    }
                    break;
                default:
                    break;
            }

            if ((direction == Direction.HORIZONTAL_IN | direction == Direction.HORIZONTAL_OUT) && i > (rcs.Length / 2) + 1)
                break;
        }
    }

    struct AnimRect
    {
        private Rect rect;
        public float fromScale;
        public float toScale;

        public AnimRect(Rect rect, float fromScale, float toScale)
        {
            this.rect = rect;
            this.fromScale = fromScale;
            this.toScale = toScale;
        }

        public Rect GetRect(float time)
        {
            if (time >= 1)
                return rect;
            else if (time < 0)
                return new Rect(rect.xMin + rect.width * time / 2, rect.yMin + rect.height * time / 2, 0, 0);
            else
                return new Rect(rect.xMin + (rect.width - rect.width * time) / 2, rect.yMin + (rect.height - rect.height * time) / 2 * time, rect.width * time, rect.height * time);
        }
    }
}