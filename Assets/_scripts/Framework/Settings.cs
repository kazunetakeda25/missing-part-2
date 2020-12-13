using UnityEngine;
using System.Collections;

public class Settings {

	private const bool DEFAULT_PRIMING = false;
	private const bool DEFAULT_FIDELITY = false;
	private const bool DEFAULT_SCORING = true;
	private const bool DEFAULT_SEX = false; //Female

	private bool priming;
	public bool Priming { get {return priming;} }
	private bool fidelity;
	public bool Fidelity { get { return fidelity; } }
	private bool sex;
	public bool Sex { get { return sex; } }
	private bool scoring;
	public bool Scoring { get { return scoring; } }
	
	private static Settings instance;
	public static Settings Instance
	{
		get
		{
			if(instance == null) {
				instance = new Settings();
				instance.Init();
			}

			return instance;
		}
	}

	private void Init()
	{
		priming = DEFAULT_PRIMING;
		fidelity = DEFAULT_FIDELITY;
		sex = DEFAULT_SEX;
		scoring = DEFAULT_SCORING;
	}

	public void SetGameIVs(bool sex, bool priming, bool fidelity, bool scoring)
	{
		this.sex = sex;
		this.priming = priming;
		this.fidelity = fidelity;
		this.scoring = scoring;
	}

}
