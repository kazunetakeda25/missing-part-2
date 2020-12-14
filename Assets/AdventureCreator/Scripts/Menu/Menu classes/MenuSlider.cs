/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuSlider.cs"
 * 
 *	This MenuElement creates a slider for eg. volume control.
 * 
 */

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class MenuSlider : MenuElement
	{

		public Slider uiSlider;

		public string label;
		public TextEffects textEffects;
		public float amount;
		public TextAnchor anchor;
		public Texture2D sliderTexture;
		public SliderDisplayType sliderDisplayType = SliderDisplayType.FillBar;
		public AC_SliderType sliderType;
		public Vector2 blockSize = new Vector2 (0.05f, 1f);
		public bool useFullWidth = false;
		public int varID;
		public int numberOfSteps = 0;
		
		private Rect sliderRect;
		private string fullText;


		public override void Declare ()
		{
			uiSlider = null;

			label = "Slider";
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			amount = 1f;
			anchor = TextAnchor.MiddleLeft;
			sliderType = AC_SliderType.CustomScript;
			sliderDisplayType = SliderDisplayType.FillBar;
			blockSize = new Vector2 (0.05f, 1f);
			useFullWidth = false;
			varID = 0;
			textEffects = TextEffects.None;
			numberOfSteps = 0;
			
			base.Declare ();
		}

		
		public override MenuElement DuplicateSelf ()
		{
			MenuSlider newElement = CreateInstance <MenuSlider>();
			newElement.Declare ();
			newElement.CopySlider (this);
			return newElement;
		}
		
		
		public void CopySlider (MenuSlider _element)
		{
			uiSlider = _element.uiSlider;
			label = _element.label;
			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			amount = _element.amount;
			anchor = _element.anchor;
			sliderTexture = _element.sliderTexture;
			sliderType = _element.sliderType;
			sliderDisplayType = _element.sliderDisplayType;
			blockSize = _element.blockSize;
			useFullWidth = _element.useFullWidth;
			varID = _element.varID;
			numberOfSteps = _element.numberOfSteps;
			
			base.Copy (_element);
		}


		public override void LoadUnityUI (AC.Menu _menu)
		{
			uiSlider = LinkUIElement <Slider>();
			if (uiSlider)
			{
				uiSlider.interactable = isClickable;
				if (isClickable)
				{
					uiSlider.onValueChanged.AddListener ((amount) => {
						ProcessClick (_menu, 0, KickStarter.playerInput.mouseState);
					});
				}
			}
		}


		public override GameObject GetObjectToSelect ()
		{
			if (uiSlider)
			{
				return uiSlider.gameObject;
			}
			return null;
		}
		
		
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiSlider)
			{
				return uiSlider.GetComponent <RectTransform>();
			}
			return null;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");

			if (source == MenuSource.AdventureCreator)
			{
				label = EditorGUILayout.TextField ("Label text:", label);
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
				useFullWidth = EditorGUILayout.Toggle ("Use full width?", useFullWidth);
				sliderDisplayType = (SliderDisplayType) EditorGUILayout.EnumPopup ("Display type:", sliderDisplayType);
				
				EditorGUILayout.BeginHorizontal ();
				if (sliderDisplayType == SliderDisplayType.FillBar)
				{
					EditorGUILayout.LabelField ("Fill-bar texture:", GUILayout.Width (145f));
				}
				else if (sliderDisplayType == SliderDisplayType.MoveableBlock)
				{
					EditorGUILayout.LabelField ("Movable block texture:", GUILayout.Width (145f));
				}
				sliderTexture = (Texture2D) EditorGUILayout.ObjectField (sliderTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
				
				if (sliderDisplayType == SliderDisplayType.MoveableBlock)
				{
					blockSize = EditorGUILayout.Vector2Field ("Block size:", blockSize);
				}
			}
			else
			{
				uiSlider = LinkedUiGUI <Slider> (uiSlider, "Linked Slider:", source);
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}
			
			sliderType = (AC_SliderType) EditorGUILayout.EnumPopup ("Slider affects:", sliderType);
			if (sliderType == AC_SliderType.CustomScript)
			{
				amount = EditorGUILayout.Slider ("Default value:", amount, 0f, 1f);
				ShowClipHelp ();
			}
			else if (sliderType == AC_SliderType.FloatVariable)
			{
				varID = EditorGUILayout.IntField ("Global Variable ID:", varID);
			}

			if (source == MenuSource.AdventureCreator)
			{
				numberOfSteps = EditorGUILayout.IntField ("Number of steps:", numberOfSteps);
			}
			isClickable = EditorGUILayout.Toggle ("User can change value?", isClickable);
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}
		
		#endif


		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			CalculateValue ();

			fullText = TranslateLabel (label, languageNumber);

			if (uiSlider)
			{
				uiSlider.value = amount;
				UpdateUIElement (uiSlider);
			}
		}


		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			GUI.Label (ZoomRect (relativeRect, zoom), "", _style);
			
			if (sliderTexture)
			{
				DrawSlider (zoom);
			}
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			_style.normal.background = null;
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), fullText, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), fullText, _style);
			}
		}
		
		
		private void DrawSlider (float zoom)
		{
			sliderRect = relativeRect;
			if (sliderDisplayType == SliderDisplayType.FillBar)
			{
				if (useFullWidth)
				{
					sliderRect.x = relativeRect.x;
					sliderRect.width = slotSize.x * amount;
				}
				else
				{
					sliderRect.x = relativeRect.x + (relativeRect.width / 2);
					sliderRect.width = slotSize.x * amount * 0.5f;
				}
				
				if (sizeType != AC_SizeType.AbsolutePixels)
				{
					sliderRect.width *= AdvGame.GetMainGameViewSize ().x / 100f;
				}
			}
			else if (sliderDisplayType == SliderDisplayType.MoveableBlock)
			{
				sliderRect.width *= blockSize.x;
				sliderRect.height *= blockSize.y;
				sliderRect.y += (relativeRect.height - sliderRect.height) / 2f;
				
				if (useFullWidth)
				{
					sliderRect.x = slotSize.x * amount + relativeRect.x - (sliderRect.width / 2f);
				}
				else
				{
					sliderRect.x = slotSize.x * amount * 0.5f + relativeRect.x + (relativeRect.width / 2) - (sliderRect.width / 2f);
				}
				
				if (sizeType != AC_SizeType.AbsolutePixels)
				{
					sliderRect.x *= AdvGame.GetMainGameViewSize ().x / 100f;
				}
			}
			
			GUI.DrawTexture (ZoomRect (sliderRect, zoom), sliderTexture, ScaleMode.StretchToFill, true, 0f);
		}
		
		
		public override string GetLabel (int slot, int languageNumber)
		{
			return TranslateLabel (label, languageNumber);
		}
		
		
		private void Change ()
		{
			amount += 0.02f; 
			
			if (amount > 1f)
			{
				amount = 0f;
			}
			
			UpdateValue ();
		}
		
		
		private void Change (float mouseX)
		{
			if (useFullWidth)
			{
				mouseX = mouseX - relativeRect.x;
				amount = mouseX / relativeRect.width;
			}
			else
			{
				mouseX = mouseX - relativeRect.x - (relativeRect.width / 2f);
				amount = mouseX / (relativeRect.width / 2f);
			}
			
			UpdateValue ();
		}
		
		
		private void UpdateValue ()
		{
			if (uiSlider == null)
			{
				if (amount < 0f)
				{
					amount = 0;
				}
				else if (amount > 1f)
				{
					amount = 1f;
				}
			
				// Limit by steps
				if (numberOfSteps > 0)
				{
					float valueSeparation = 1f / (float) numberOfSteps;
					float nearestValue = 0f;
					while (nearestValue < amount)
					{
						nearestValue += valueSeparation;
					}
					
					// Now larger than amount, so which is closer?
					float lowerNearest = nearestValue - valueSeparation;
					if (amount - lowerNearest > nearestValue - amount)
					{
						amount = nearestValue;
					}
					else
					{
						amount = lowerNearest;
					}
				}
			}
				
			if (sliderType == AC_SliderType.Speech || sliderType == AC_SliderType.SFX || sliderType == AC_SliderType.Music)
			{
				if (KickStarter.options != null)
				{
					if (sliderType == AC_SliderType.Speech)
					{
						KickStarter.options.optionsData.speechVolume = amount;
					}
					else if (sliderType == AC_SliderType.Music)
					{
						KickStarter.options.optionsData.musicVolume = amount;
						KickStarter.options.SetVolume (SoundType.Music);
					}
					else if (sliderType == AC_SliderType.SFX)
					{
						KickStarter.options.optionsData.sfxVolume = amount;
						KickStarter.options.SetVolume (SoundType.SFX);
					}

					#if UNITY_5
					if (sliderType == AC_SliderType.Speech)
					{
						AdvGame.SetMixerVolume (KickStarter.settingsManager.speechMixerGroup, KickStarter.settingsManager.speechAttentuationParameter, amount);
					}
					else if (sliderType == AC_SliderType.Music)
					{
						AdvGame.SetMixerVolume (KickStarter.settingsManager.musicMixerGroup, KickStarter.settingsManager.musicAttentuationParameter, amount);
					}
					else if (sliderType == AC_SliderType.SFX)
					{
						AdvGame.SetMixerVolume (KickStarter.settingsManager.sfxMixerGroup, KickStarter.settingsManager.sfxAttentuationParameter, amount);
					}
					#endif

					KickStarter.options.SavePrefs ();
				}
				else
				{
					Debug.LogWarning ("Could not find Options data!");
				}
			}
			else if (sliderType == AC_SliderType.FloatVariable)
			{
				if (varID >= 0)
				{
					GVar var = RuntimeVariables.GetVariable (varID);
					if (var.type == VariableType.Float)
					{
						var.floatVal = amount;
						var.Upload ();
					}
				}
			}
		}


		private void CalculateValue ()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			if (sliderType == AC_SliderType.Speech || sliderType == AC_SliderType.SFX || sliderType == AC_SliderType.Music)
			{
				if (KickStarter.options != null && KickStarter.options.optionsData != null)
				{
					if (sliderType == AC_SliderType.Speech)
					{
						amount = KickStarter.options.optionsData.speechVolume;
					}
					else if (sliderType == AC_SliderType.Music)
					{
						amount = KickStarter.options.optionsData.musicVolume;
					}
					else if (sliderType == AC_SliderType.SFX)
					{
						amount = KickStarter.options.optionsData.sfxVolume;
					}
				}
			}
			else if (sliderType == AC_SliderType.FloatVariable)
			{
				if (varID >= 0)
				{
					GVar _variable = RuntimeVariables.GetVariable (varID);
					if (_variable != null)
					{
						if (_variable.type != VariableType.Float)
						{
							Debug.LogWarning ("Cannot link MenuSlider " + title + " to Variable " + varID + " as it is not a Float.");
						}
						else
						{
							amount = Mathf.Clamp (RuntimeVariables.GetFloatValue (varID), 0f, 1f);
						}
					}
					else
					{
						Debug.LogWarning ("Slider " + this.label + " is referencing Gloval Variable " + varID + ", which does not exist.");
					}
				}
			}
		}


		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			if (uiSlider != null)
			{
				amount = uiSlider.value;
				UpdateValue ();
			}
			else
			{
				if (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					Change ();
				}
				else
				{
					Change (KickStarter.playerInput.GetMousePosition ().x - _menu.GetRect ().x);
				}
			}

			if (sliderType == AC_SliderType.CustomScript)
			{
				MenuSystem.OnElementClick (_menu, this, _slot, (int) _mouseState);
			}
		}
		
		
		public override void ProcessContinuousClick (AC.Menu _menu, MouseState _mouseState)
		{
			if (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
			{
				Change ();
			}
			else
			{
				Change (KickStarter.playerInput.GetMousePosition ().x - _menu.GetRect ().x);
			}
			if (sliderType == AC_SliderType.CustomScript)
			{
				MenuSystem.OnElementClick (_menu, this, 0, (int) _mouseState);
			}
		}

		
		protected override void AutoSize ()
		{
			AutoSize (new GUIContent (TranslateLabel (label, Options.GetLanguage ())));
		}
		
	}
	
}