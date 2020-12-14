using UnityEngine;
using UnityEngine.EventSystems;

namespace AC
{

	public class UISlotClick : MonoBehaviour, IPointerClickHandler
	{

		private AC.Menu menu;
		private MenuElement menuElement;
		private int slot;


		public void Setup (AC.Menu _menu, MenuElement _element, int _slot)
		{
			if (_menu == null)
			{
				return;
			}

			menu = _menu;
			menuElement = _element;
			slot = _slot;
		}


		public void OnPointerClick (PointerEventData eventData)
		{
			if (menuElement)
			{
				if (eventData.button == PointerEventData.InputButton.Right)
				{
					menuElement.ProcessClick (menu, slot, MouseState.RightClick);
				}
			}
		}

	}

}