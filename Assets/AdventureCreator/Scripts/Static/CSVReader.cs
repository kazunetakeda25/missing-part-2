/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"CSVReader.cs"
 * 
 *	This script imports CSV files for use by the Speech Manager.
 *	It is based on original code by Dock at http://wiki.unity3d.com/index.php?title=CSVReader
 * 
 */

using UnityEngine;
using System.Collections;

public class CSVReader
{
	
	public const string csvDelimiter = "|";
	public const string csvComma = ",";
	public const string csvTemp = "{{{$$}}}";
	
	
	static public string[,] SplitCsvGrid (string csvText)
	{
		csvText = csvText.Replace (csvComma, csvTemp);
		csvText = csvText.Replace (csvDelimiter, csvComma);
		
		string[] lines = csvText.Split ("\n"[0]); 
		
		int width = 0; 
		for (int i = 0; i < lines.Length; i++)
		{
			string[] row = lines[i].Split (csvComma[0]);
			width = Mathf.Max (width, row.Length);
		}
		
		string[,] outputGrid = new string [width + 1, lines.Length + 1]; 
		for (int y = 0; y < lines.Length; y++)
		{
			string[] row = lines[y].Split (csvComma[0]);
			for (int x = 0; x < row.Length; x++) 
			{
				outputGrid[x,y] = row[x]; 
			}
		}
		
		return outputGrid; 
	}
	
}