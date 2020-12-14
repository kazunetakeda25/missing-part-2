using UnityEngine;
using System.Collections;

public static class ColorTools
{

	public static Color GetAlphaColorChange(Color color, float alpha) { return new Color(color.r, color.g, color.b, alpha); }

}
