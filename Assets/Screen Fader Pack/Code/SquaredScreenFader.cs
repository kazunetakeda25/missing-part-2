using UnityEngine;
using System.Collections;
using System;

public class SquaredScreenFader : Fader// ScreenFaderBase
{
    public Color color = Color.black;

    [Range(2, 50)]
    public int columns = 10;
    private int last_columns = 10;
    public Direction direction = Direction.DIAGONAL_LEFT_DOWN;
        
    public Texture texture = null;
    int rows;
    AnimRect[,] squares = null;

    protected override void Init()
    {
        base.Init();

        if(texture == null)
        texture = base.GetTextureFromColor(color);

        int w = Screen.width + columns;
        int h = Screen.height + columns;

        rows = h / (w / columns) + 2;

        squares = new AnimRect[columns, rows];
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                squares[c, r] = new AnimRect(
                    new Rect(
                    w / columns * c,
                    h / rows * r,
                    w / columns,
                    h / rows)
                    , 0.1f, 1f);
            }
        }

        last_columns = columns;
    }

    protected override void DrawOnGUI()
    {
        if (columns != last_columns)
            Init();

        for (int i = 0; i < columns; i++)
        {
            for (int y = 0; y < rows; y++)
            {
                switch (direction)
                {
                    case Direction.DIAGONAL_LEFT_DOWN:
                        GUI.DrawTexture(squares[i, y].GetRect(fadeBalance / ((float)(i + y) / (float)(columns + rows))), texture);
                        break;
                    case Direction.DIAGONAL_LEFT_UP:
                        GUI.DrawTexture(squares[columns - i-1, rows - y - 1].GetRect(fadeBalance / ((float)(i + y) / (float)(columns + rows))), texture);
                        break;
                    case Direction.DIAGONAL_RIGHT_DOWN:
                        GUI.DrawTexture(squares[columns - i - 1, y].GetRect(fadeBalance / ((float)(i + y) / (float)(columns + rows))), texture);
                        break;
                    case Direction.DIAGONAL_RIGHT_UP:
                        GUI.DrawTexture(squares[i, rows - y - 1].GetRect(fadeBalance / ((float)(i + y) / (float)(columns + rows))), texture);
                        break;

                    case Direction.VERTICAL_DOWN:
                        GUI.DrawTexture(squares[i, y].GetRect(GetLinearT(y, rows)), texture);
                        break;
                    case Direction.VERTICAL_UP:
                        GUI.DrawTexture(squares[i, rows - y - 1].GetRect(GetLinearT(y, rows)), texture);
                        break;

                    case Direction.HORIZONTAL_RIGHT:
                        GUI.DrawTexture(squares[i, y].GetRect(GetLinearT(i, columns)), texture);
                        break;
                    case Direction.HORIZONTAL_LEFT:
                        GUI.DrawTexture(squares[columns - i-1, rows-y-1].GetRect(GetLinearT(i, columns)), texture);
                        break;

                    case Direction.NONE:
                        GUI.DrawTexture(squares[i, y].GetRect(fadeBalance), texture);
                        break;
                }
            }
        }
    }

    public enum Direction { NONE, HORIZONTAL_LEFT, HORIZONTAL_RIGHT, VERTICAL_UP, VERTICAL_DOWN, DIAGONAL_LEFT_DOWN, DIAGONAL_LEFT_UP, DIAGONAL_RIGHT_UP, DIAGONAL_RIGHT_DOWN }

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
                return new Rect(
                    rect.x + rect.width / 2 * (0.5F - time / 2),
                    rect.y + rect.height / 2 * (0.5F - time / 2),
                    rect.width * time,
                    rect.height * time);
        }
    }
}