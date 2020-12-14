/*
Dialogue System Bark Example - Three NPCs Bark

This folder contains a scene that demonstrates barks between three NPCs.

It uses the SendMessage(OnUse,,<npc>) sequencer command to prompt the next speaker, 
and a variable named Line to keep track of which line the speaker should bark.

Each NPC has a Bark Trigger set to OnUse, which responds to the previous bark's
SendMessage() command. Adam also has an additional Bark Trigger set to OnStart
to get the barks started.
*/