using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BulletCount : MonoBehaviour 
{
	public int count = 5;
	public dfScrollPanel[] bulletContainers = new dfScrollPanel[2];
	public dfSprite bullet;
	public dfControl acceptButton;

	public AudioClip bulletLoad;
	public AudioClip bulletUnload;

	private int max = 30;
	private dfSprite[] bullets = new dfSprite[30];

	void Start () 
	{
		for( int i = 0; i < max; i++ )
		{
			dfSprite newBullet = (dfSprite)Instantiate( bullet );
			newBullet.IsVisible = false;
			newBullet.name = "Bullet Number " + i.ToString();

			// Add bullets to multiple containers in a staggered way that supports any amount of containers
			newBullet.transform.parent = bulletContainers[i % bulletContainers.Length].transform;

			bullets[i] = newBullet;
		}

		RefreshCounterVisibility();
	}

	public void Increment()
	{
		if( count >= max )
			return;

		count++;

		this.GetComponent<AudioSource>().clip = bulletLoad;
		this.GetComponent<AudioSource>().Play();

		RefreshCounterVisibility();
	}

	public void Decrement()
	{
		if( count <= 0 )
			return;

		count--;

		this.GetComponent<AudioSource>().clip = bulletUnload;
		this.GetComponent<AudioSource>().Play();

		RefreshCounterVisibility();
	}

	private void RefreshCounterVisibility()
	{
		for( int i = 0; i < bullets.Length; i++ )
		{
			bullets[i].IsVisible = i < count ? true : false;
		}
	}
}
