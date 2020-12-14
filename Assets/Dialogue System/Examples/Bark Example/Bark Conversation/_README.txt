/*
Bark Conversation Example

This folder contains a scene that demonstrates how to run a conversation
using characters' bark UIs instead of the regular dialogue UI. In some
cases you may find this preferable to chaining barks using BarkOnDialogueEvent.

In this scene, Private Hart:
	- Now has a bark UI
	- Has an Override Display Settings component that:
		- Uses a dialogue UI named BarkDialogueUI (described below)
  		- Enables PC subtitles (so we can see the player's barks)
  		- Sets the default sequence to Delay({{end}}) so it doesn't touch the camera.

BarkDialogueUI is an empty GameObject that contains the BarkDialogueUI script. 
This implementation of IDialogueUI simply uses the participants' bark UIs to 
display lines in a conversation. If the player has multiple responses, it
automatically chooses the first response.

A new GameObject named Private Hart Conversation Trigger is the trigger for the
conversation. When the player enters the trigger area, the conversation starts.
Stop Conversation On Trigger Exit is ticked, so if the player leaves the trigger
area the conversation still stop.

The Player:
	- Now has a bark UI
	- Only disables gameplay control for the terminal and dead guard, not for
	  the conversation with Private Hart.

*/