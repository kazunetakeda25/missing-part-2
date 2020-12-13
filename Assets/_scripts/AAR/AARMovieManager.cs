using UnityEngine;
using System.Collections;

public class AARMovieManager : MonoBehaviour {

	private AARManager manager;
	private MoviePlayer moviePlayer;
	private bool startedMovie = false;

	public void Init(string[] movieTitles, AARManager manager)
	{
		Debug.Log ("Movie Intializing");
		this.manager = manager;

		GameObject moviePlayerGO = new GameObject("AAR Movie Player");
		moviePlayer = moviePlayerGO.AddComponent<MoviePlayer>();
		moviePlayer.SetupMovie(movieTitles, MoviePlayer.Type.OVERLAY, 0.75f);
		moviePlayer.PlayMovie();
		startedMovie = true;
	}

	private void Update()
	{
		if(startedMovie && moviePlayer == null)
		{
			OnComplete();
		}
	}

	private void OnComplete()
	{
		//Destroy Movie Player Prefab
		manager.NextSlide();

		GameObject.Destroy(this.gameObject);
	}

}
