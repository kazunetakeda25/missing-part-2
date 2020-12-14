using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Examples {

	/// <summary>
	/// This example component implements a command-line Lua interpreter.
	/// It also registers a method named sqrt() for use in Lua.
	/// </summary>
	public class LuaExample : MonoBehaviour {
		
		/// <summary>
		/// Max lines of output to show.
		/// </summary>
		private const int MaxOutputLines = 20;
		
		/// <summary>
		/// The output buffer, containing the last MaxOutputLines of output lines.
		/// </summary>
		private List<string> output = new List<string>();
		
		/// <summary>
		/// The user's current input.
		/// </summary>
		private string input = string.Empty;
		
		/// <summary>
		/// Starts the component by registering the sqrt() function.
		/// </summary>
		void Start() {
			// Alternate way to register: Lua.RegisterFunction("sqrt", this, this.GetType().GetMethod("sqrt"));
			Lua.RegisterFunction("sqrt", null, SymbolExtensions.GetMethodInfo(() => sqrt(0)));
			AddOutputLine("Registered function sqrt(x)");
		}
	
		/// <summary>
		/// Draws the current input line and the output buffer using Unity GUI.
		/// </summary>
		void OnGUI() {
			GUI.Label(new Rect(5, 5, 1000, 30), "Enter Lua commands below: (for example, return 1+2)");
	        if ((Event.current.keyCode == KeyCode.Return) && !string.IsNullOrEmpty(input)) {
				RunLuaCommand();
			}
			GUI.SetNextControlName("Input");
			GUI.FocusControl("Input");
			input = GUI.TextField(new Rect(5, 35, 500, 30), input);
			for (int i = 0; i < output.Count; i++) {
				GUI.Label(new Rect(5, 70 + (20 * i), 1000, 20), output[i]);
			}
		}
	
		/// <summary>
		/// Adds a line of output.
		/// </summary>
		/// <param name='line'>
		/// The line to add.
		/// </param>
		private void AddOutputLine(string line) {
			if (output.Count >= MaxOutputLines) {
				output.RemoveAt(0);
			}
			output.Add(line);
		}
	
		/// <summary>
		/// Runs the current input line through Lua, adds the result to the output buffer, and resets the input line.
		/// </summary>
		void RunLuaCommand() {
			AddOutputLine(string.Format("Lua.Run({0}) returned:", input));
			try {
				AddOutputLine(Lua.Run(input).AsString);
			} catch (Exception e) {
				AddOutputLine(e.Message);
			}
			input = string.Empty;
		}
	
		/// <summary>
		/// This is an example function implemented in C# and made available to Lua using Lua.RegisterFunction().
		/// </summary>
		/// <returns>
		/// The square root of x.
		/// </returns>
		/// <param name='x'>
		/// A float value.
		/// </param>
		public static float sqrt(float x) {
			return Mathf.Sqrt(x);
		}
		
	}

}
