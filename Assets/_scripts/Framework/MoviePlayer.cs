using UnityEngine;
using System.Collections;
using UnityEngine.Video;

public class MoviePlayer : MonoBehaviour
{

	private const string MOVIE_PLAYER_PREFAB = "Movie Player";
	private const float OVERLAY_CAM_SIZE = 5.19f;

	private const string MOVIE_PATH = "";
	private const string INTRO_MOVIE_PATH = "Videos/Missing-01-20-cb";

	public enum Movie
	{
		NONE,
		INTRO
	}

	public enum Type
	{
		OVERLAY,
		FULLSCREEN
	}

	public float fadeInTime;
	public bool autoPlay;
	public Movie movie;
	public Type type;

	//private MovieTexture movieTexture;
	private VideoPlayer videoPlayer;
	//private MovieScreen movieScreen;
	private bool playWhenReady;
	private bool movieStarted;
	private AudioSource audioSource;

	private string[] movieFileNames;
	private int movieIndex = 0;

	[SerializeField] private float movieVolume = 1.0f;

	public void PauseMovie()
	{
		//movieTexture.Pause();
		videoPlayer.Pause();
		movieStarted = false;
	}

	public void UnpauseMovie()
	{
		//movieTexture.Play();
		videoPlayer.Play();
		movieStarted = true;
	}

	public void SetupMovie(Movie movie, Type type)
	{
		this.movie = movie;

		if (type == Type.OVERLAY)
			SetCameraForOverlay();

		SetMovieTexture();
	}

	public void SetupMovie(string movieToPlay, Type type, float volume)
	{
		movieVolume = volume;
		SetupMovie(movieToPlay, type);
		Debug.Log("Test1");
	}

	public void SetupMovie(string[] moviesToPlay, Type type, float volume)
	{
		movieVolume = volume;
		SetupMovie(moviesToPlay, type);
	}

	public void SetupMovie(string movieToPlay, Type type)
	{
		string[] movie = new string[] { movieToPlay };
		SetupMovie(movie, type);
	}

	public void SetupMovie(string[] moviesToPlay, Type type)
	{
		movieFileNames = moviesToPlay;

		if (type == Type.OVERLAY)
			SetCameraForOverlay();

		SetMovieTexture();
	}

	public void PlayMovie()
	{
		playWhenReady = true;
	}

	private void Awake()
	{
		if (FindObjectOfType<AudioListener>() == null)
			this.gameObject.AddComponent<AudioListener>();

		FindMoviePathFromEnum();
		CreateMovieScreen();
		SetMovieTexture();

		if (autoPlay)
			playWhenReady = true;
	}

	private void Update()
	{

		//if(playWhenReady && movieTexture.isReadyToPlay) {
		if (playWhenReady && videoPlayer.isPrepared == true)
		{
			Debug.Log("Playing " + movieVolume);
			playWhenReady = false;
			videoPlayer.Play();
			//movieTexture.Play();
			//movieScreen.speakers.clip = movieTexture.audioClip;
			//movieScreen.speakers.Play();
			//movieScreen.speakers.volume = movieVolume;
			movieStarted = true;
		}

		//if(movieStarted && movieTexture.isPlaying == false)
		if (movieStarted && videoPlayer.isPlaying == false)
		{
			EndMovie();
		}

		if (Debug.isDebugBuild && Input.GetMouseButtonUp(1))
		{
			EndMovie();
		}
	}

	private void EndMovie()
	{
		//movieTexture.Stop();
		videoPlayer.Stop();
		if (type == Type.FULLSCREEN)
		{
			//GameObject.Destroy(movieScreen.gameObject);
			GameObject.Destroy(videoPlayer.gameObject);
			GameObject.Destroy(this.gameObject);
			SessionManager.Instance.GotoNextLevel();
		}

		if (type == Type.OVERLAY)
		{

			if (movieIndex >= movieFileNames.Length)
			{
				//GameObject.Destroy(movieScreen.gameObject);
				GameObject.Destroy(videoPlayer.gameObject);
				GameObject.Destroy(this.gameObject);
			}
			else
			{
				SetMovieTexture();
				playWhenReady = true;
			}
		}
	}

	private void CreateMovieScreen()
	{
		GameObject movieScreenGO = (GameObject)GameObject.Instantiate(Resources.Load(MOVIE_PLAYER_PREFAB) as GameObject);
		//this.movieScreen = movieScreenGO.GetComponentInChildren<MovieScreen>();
		this.videoPlayer = movieScreenGO.GetComponentInChildren<VideoPlayer>();
	}

	private void SetMovieTexture()
	{
		if (movieFileNames == null)
			return;

		string path = MOVIE_PATH + movieFileNames[movieIndex];

		movieIndex++;

		//movieTexture = Resources.Load(path) as MovieTexture;
		//if(movieTexture == null) {
		//	Debug.LogError("Movie Texture: " + path + " is a bad path.");
		//}
		//movieScreen.screen.material.mainTexture = movieTexture;

		VideoClip clip = Resources.Load<VideoClip>(path) as VideoClip;
		if (clip == null)
		{
			Debug.LogError("VideoClip: " + path + " is a bad path.");
		}
		videoPlayer.clip = clip;
	}

	private void FindMoviePathFromEnum()
	{
		switch (movie)
		{
			case Movie.INTRO:
				movieFileNames = new string[] { INTRO_MOVIE_PATH };
				break;
		}
	}

	private void SetCameraForOverlay()
	{
		//movieScreen.GetComponent<Camera>().orthographicSize = OVERLAY_CAM_SIZE;
		videoPlayer.GetComponent<Camera>().orthographicSize = OVERLAY_CAM_SIZE;
	}

}
