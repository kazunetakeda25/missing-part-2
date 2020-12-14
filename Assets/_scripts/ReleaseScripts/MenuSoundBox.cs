using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MissingComplete
{

	public class MenuSoundBox : MonoBehaviour 
	{
		private static MenuSoundBox instance;
		public static MenuSoundBox Instance { get { return instance; } }

		[SerializeField] AudioClip click;
		[SerializeField] AudioClip highlight;
		[SerializeField] AudioClip warning;

		public void PlayClick(Button sender = null)
		{
			if(sender == null)
				return;

			if(sender.IsInteractable() == false)
				return;

			if(sender.isActiveAndEnabled == true)
				return;				

			this.GetComponent<AudioSource>().PlayOneShot(click);
		}

		public void PlayOnMouseOver(Button sender = null)
		{
			if(sender.IsInteractable() == false)
				return;

			if(sender.isActiveAndEnabled == false)
				return;

			this.GetComponent<AudioSource>().PlayOneShot(highlight);
		}

		public void PlayWarning()
		{
			this.GetComponent<AudioSource>().PlayOneShot(warning);
		}

		private void Awake()
		{
			instance = this;
		}

	}

}
