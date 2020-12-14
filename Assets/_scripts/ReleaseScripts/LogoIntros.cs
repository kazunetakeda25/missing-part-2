using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace MissingComplete 
{
	
	public class LogoIntros : MonoBehaviour 
	{
		[System.Serializable]
		public class OpeningImages
		{
			public Image img;
			public float timeForAnimation;
			public float delayPostAnimation;
			public float xOffset;
		}
			
		[SerializeField] OpeningImages[] openingImages;
		[SerializeField] float startDelay;
		[SerializeField] bool runIntro;
		[SerializeField] AudioSource introMusic;

		private float timer;
		private int imageIndex = 0;
		private bool showingLogos;

		private void Awake()
		{
			if(AC.KickStarter.stateHandler != null) {
				GameObject.Destroy(AC.KickStarter.stateHandler.gameObject);
			}

			if(GameObject.FindGameObjectWithTag("Player") != null) {
				GameObject.Destroy(GameObject.FindGameObjectWithTag("Player"));
			}
			Cursor.visible = true;
			Time.timeScale = 1.0f;
		}

		private void Start()
		{
			if(runIntro == false) {
				imageIndex = openingImages.Length;
				MainMenu.Instance.FadeInMenu();
				return;
			}

			imageIndex = -1;
			showingLogos = true;
			timer = startDelay;

			introMusic.Play();

			foreach(OpeningImages oi in openingImages) {
				oi.img.color = ColorTools.GetAlphaColorChange(oi.img.color, 0f);
			}
				
		}

		private void Update()
		{
			if(showingLogos == false)
				return;

			if(Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1)) {
				DOTween.KillAll();
				foreach(OpeningImages oi in openingImages) {
					oi.img.DOFade(0.0f, 0.25f);
				}
				imageIndex = openingImages.Length;
				timer = 0;
			}

			timer -= Time.deltaTime;
			StateUpdate();
		}

		private void StateUpdate()
		{
			if (timer <= 0) 
			{
				imageIndex++;
				if(imageIndex >= openingImages.Length) {
					showingLogos = false;
					MainMenu.Instance.FadeInMenu();
					return;
				}

				SetupLogo(openingImages[imageIndex]);

				timer = openingImages[imageIndex].timeForAnimation + openingImages[imageIndex].delayPostAnimation;
			}
		}

		private void SetupLogo(OpeningImages oi)
		{
			oi.img.transform.position = new Vector3(-oi.xOffset, oi.img.transform.position.y, oi.img.transform.position.z);
			oi.img.transform.DOMoveX(oi.xOffset, oi.timeForAnimation).SetEase(Ease.Linear);
			oi.img.DOFade(1f, oi.timeForAnimation / 2).OnComplete(() => FadeOut(oi.img, oi.timeForAnimation)).SetEase(Ease.Linear);
		}

		private void FadeOut(Image img, float animationTime)
		{
			img.DOFade(0f, animationTime / 2).SetEase(Ease.Linear);
		}
	}
}
