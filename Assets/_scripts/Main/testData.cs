using UnityEngine;
using System.Collections;

public class testData : MonoBehaviour {

	AC.VariablesManager sadfMan;
	public ShowInputControls sic;
	public Vector3 chat1Pos = new Vector3 (0, 0, 0);
	public float height,width;
	public float testH, testY;
	public Vector3 testPos;
	// Use this for initialization
	void Start () {
		sadfMan = AC.KickStarter.variablesManager;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.A)) 
		{
			if(sadfMan.vars[0].val == 1)
			sadfMan.vars[0].val = 0;
			else
				sadfMan.vars[0].val = 1;
		
		}
	}
	public void testChat()
	{
		height = 690f;
		width = 690f;
		Debug.Log ("fucks yeah");
		sic.ShowDialogBox (testPos,testH,testY);
		sic.AddDialog ("One million hits...ONE MILLION HITS! We are officially hot. Ladies and gentlemen, I give you MANHATTAN AZIMUTH!", 8.0f);
	}
}
