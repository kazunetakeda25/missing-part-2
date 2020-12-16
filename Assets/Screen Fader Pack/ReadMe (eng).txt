Screen Fader
Easy screen fading for Unity3D 
Version 1.2 Release date 07-okt-2013  


ABOUT SCREEN FADER

This easy-to-use script will allow you to fade in or out your screen with only one line of code.
On the one hand all you need is drop prefab on scene and write:
ScreenFaderBase.Fade(FadeDirection.In);

But on the other hand, you will get powerful possibilities. You can setup colors, transparency, speed of effect and delays before it starts in the Inspector panel. 
You can subscribe on events and get notifications when effects will start or complete.

This works well on Free and Pro Unity, suitable for Web, Standalone, Android and iOS platform.

And all this takes less then 10kb on your drive and costs less then your morning starbucks coffee.

And one more thing: you'll get 2 extra scripts that will allow you to fade your screen with squares or stripes effect, and of course you can also setup their additional parameters, such as number of stripes or squares and direction of effect.

HOW TO USE SCREEN FADER ASSET

This asset includes 4 pre-configured prefabs with different settings. You can found them in ScreenFader\Prefabs folder. Detailed information about these configurations see bellow.
How to use Screen Fader in 2 steps:

1. Drag-and-drop prefab into the scene from 'Screen Fader Pack\Prefabs' folder
2. Open you script and type JUST ONE LINE OF MAGIC CODE where you want to fade-in your screen:
       Fader.Instance.FadeIn();

That is all. Now you can try to make different combinations of it.
For example - fade-in, than pause for 10 seconds, and fade-out after this:
      Fader.Instance.FadeIn().Pause(10).FadeOut(); 

or - fade-in, call coroutine method, and fade-out when conoutine method will be breaked:
      Fader.Instance.FadeIn().StartCoroutine(this, MoveCamera()).FadeOut(); 

or somthing more complicacy:
      Fader.Instance
                .FadeIn(0)
                .StartCoroutine(this, LoadPlayerInfoFromWebServer())
                .StartCoroutine(this, SetupPlayerCharacter())
                .StartCoroutine(this, ShowGUI())
                .StartAction(new PlayBackgroungMusic(), "ambience.mp3", defaultVolume)
                .FadeOut(2); 


WHAT'S NEW

What is new in Screen Fader 1.2
 
 - Chain calls
   You can make a chains of calls like this:
		Fader.Instance.FadeIn().Pause().FadeOut();
 
 - Actions and Coroutines
   You can call coroutines and custom actions 
   from chains, for example:
		Fader.Instance.FadeIn().StartCoroutine(this, ChangeLocation()).FadeOut();

So, you can combile these methods as you wish:
        FadeIn();
        FadeOut();
        Flash();
        Pause();
        StartAction(IAction);
        StartAction(IAction, params object[] args);
        StartCoroutine(MonoBehaviour, IEnumerator);
        StartCoroutine(MonoBehaviour, methodName, methodParameter);
        StopAllFadings();



INFORMATION ABOUT PREFABS

ScreenFaderPrefab_default.prefab – default fade-in and fade-out configuration.
Properties of this prefab:
Fade Speed 		- Speed of effect
Fade In Delay 		- Delay before fade-in starts   
Fade Out Delay 	- Delay before fade-out starts
GUI Depth 		- GUI.depth value
Fade Color 		- Color of the fading
Max Density 		- Maximum density (0 – means full transparency, 1 - full density)   

ScreenFaderPrefab_default_semi-transtarent.prefab – this fade-in configuration with 90% density can be useful when you want to show a popup window.

ScreenFaderPrefab_squares.prefab – this prefab will allow you to fade screen by filling its with squares. 
It has the same properties as ScreenFaderPrefab_default.prefab and some additional:
Color 	- color of the squares
Columns 	- number of squares in a row, number of rows will be calculated        automatically
Direction 	- direction of fading (horizontal – from left to right, vertical – from top to 	   bottom, diagonal – from left-top corner to the right-bottom)
Texture 	- texture of square. You can set it to none if you want to use only Color as texture of squares.

ScreenFaderPrefab_stripes.prefab – It works like ScreenFaderPrefab_squares.prefab, but fills the screen with vertical stripes instead of squares. This prefab also has it's own additional parameter:
	Number Of Stripes – it's just a number of stripes which fill the screen.
