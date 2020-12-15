/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy named SequencerCommand<YourCommand>, where
 * [REMOVE THIS LINE] "<YourCommand>" is the name of your sequencer command. Example: For a command
 * [REMOVE THIS LINE] named Foo, name the script SequencerCommandFoo.
 * [REMOVE THIS LINE]
 * [REMOVE THIS LINE] Then remove the lines that start with "[REMOVE THIS LINE]" and add your code
 * [REMOVE THIS LINE] where the comments indicate.
 * [REMOVE THIS LINE]



using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;

public class SequencerCommandTemplate : SequencerCommand {

	public void Start() {
		// Add your initialization code here. You can use the GetParameter***() and GetSubject()
		// functions to get information from the command's parameters. You can also use the
		// Sequencer property to access the SequencerCamera, CameraAngle, and other properties
		// on the sequencer.
	}
	
	public void Update() {
		// Add your update code here. When the command is done, call Stop().
	}
	
	public void OnDestroy() {
		// Add your finalization code here. This is critical. If the sequence is cancelled and this
		// command is marked as "required", then only Start() and OnDestroy() will be called.
	}
	
}
 
 

/**/