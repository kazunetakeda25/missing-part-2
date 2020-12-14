/*
Dialogue System Bark Example

This folder contains a scene that demonstrates barks between two NPCs.

Sergeant Graves uses a Bark Trigger set to OnStart to start the barks.

Sergeant Graves and Private Hart are bark targets for each other. When
one finishes a bark, the other receives an OnBarkEnd message and starts
his line. 

Since OnBarkEnd is sent to both NPCs (barker and target), Private Hart
has a Set Enabled On Dialogue Event component that keeps the right NPC's
Bark On dialogue Event component active.

See Three NPCs Bark for an alternate method using SendMessage.

See Bark Conversation for an example of using BarkDialogueUI to run a
non-interactive conversation using Bark UIs.
*/