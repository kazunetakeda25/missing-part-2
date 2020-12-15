using UnityEngine;
using UnityEditor;
using PixelCrushers.DialogueSystem.Examples;

namespace PixelCrushers.DialogueSystem.Editors {

	/// <summary>
	/// This class defines menu items for the supplemental utility scripts in the Dialogue System menu.
	/// </summary>
	static public class ExampleMenuItems {
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Usable", false, 300)]
		public static void AddComponentUsable() {
			DialogueSystemMenuItems.AddComponentToSelection<Usable>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Selector", false, 301)]
		public static void AddComponentSelector() {
			DialogueSystemMenuItems.AddComponentToSelection<Selector>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Proximity Selector", false, 302)]
		public static void AddComponentProximitySelector() {
			DialogueSystemMenuItems.AddComponentToSelection<ProximitySelector>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Always Face Camera", false, 303)]
		public static void AddComponentAlwaysFaceCamera() {
			DialogueSystemMenuItems.AddComponentToSelection<AlwaysFaceCamera>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Simple Controller", false, 304)]
		public static void AddComponentSimpleController() {
			DialogueSystemMenuItems.AddComponentToSelection<SimpleController>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Navigate On Mouse Click", false, 305)]
		public static void AddComponentNavigateOnMouseClick() {
			DialogueSystemMenuItems.AddComponentToSelection<NavigateOnMouseClick>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Range Trigger", false, 304)]
		public static void AddComponentRangeTrigger() {
			DialogueSystemMenuItems.AddComponentToSelection<RangeTrigger>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Conversation Logger", false, 305)]
		public static void AddComponentConversationLogger() {
			DialogueSystemMenuItems.AddComponentToSelection<ConversationLogger>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Quest Tracker", false, 310)]
		public static void AddComponentQuestTracker() {
			DialogueSystemMenuItems.AddComponentToSelection<QuestTracker>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Increment On Destroy", false, 311)]
		public static void AddComponentIncrementOnDestroy() {
			DialogueSystemMenuItems.AddComponentToSelection<IncrementOnDestroy>();
		}
		
		[MenuItem("Window/Dialogue System/Component/Supplemental/Unity Community/Smooth Camera With Bumper", false, 353)]
		public static void AddComponentSmoothCameraWithBumper() {
			DialogueSystemMenuItems.AddComponentToSelection<SmoothCameraWithBumper>();
		}
		
	}
		
}
