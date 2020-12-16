using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using wyDay;

public class TurboActivateChecker : MonoBehaviour 
{
	private static TurboActivateChecker instance;
	public static TurboActivateChecker Instance { get { return instance; } }

	private const string ERROR_DATE_TIME = "This system's Date/Time is not valid. Please be sure your computer is connected to the internet and the system Date/Time is set correctly.";
	private const string ERROR_INACTIVE = "This copy of the software has not been Activated. Please run the TurboActivate executable to Activate. You also must have a valid internet connection.";
	private const string ERROR_TRIAL_EXPIRED = "This license has expired, please contact CTI at: info@cretecinc.com for a new key.";

	[SerializeField] Text activated;
	[SerializeField] Text errorMessage;
	[SerializeField] Text trialActive;
	[SerializeField] Text daysRemainingInTrial;

	bool lmActivated = false;
	public bool Activated { get { return lmActivated; } }
	private bool dateValid;
	public bool DateValid { get { return dateValid; } }
	bool trialValid = true;
	public bool TrialValid { get { return trialValid; } }

	// Use this for initialization
	void Awake () 
	{
		instance = this;

		wyDay.TurboActivate.TurboActivate.VersionGUID = "49b9b11f567afe186ce228.81210550";

		try {
			lmActivated =  wyDay.TurboActivate.TurboActivate.IsGenuine() == wyDay.TurboActivate.IsGenuineResult.Genuine;
		} catch (Exception ex) {
			Debug.Log ("TurboActivate Exception: " + ex.Message);
			Debug.LogException(ex);
			lmActivated = false;
		}

		if(lmActivated) {
			activated.text = "Activated: Yes";
		} else {
			activated.text = "Activated: No";
			errorMessage.text = ERROR_INACTIVE;
			daysRemainingInTrial.text = "";
			trialActive.text = "";
			return;
		}

		string trialExpires = wyDay.TurboActivate.TurboActivate.GetFeatureValue("trial_expires");
		daysRemainingInTrial.text = "Expiration Date: " + wyDay.TurboActivate.TurboActivate.GetFeatureValue("trial_expires");

		string date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
		Debug.Log (date);
		bool dateOK = wyDay.TurboActivate.TurboActivate.IsDateValid(date, wyDay.TurboActivate.TurboActivate.TA_DateCheckFlags.TA_HAS_NOT_EXPIRED);
		Debug.Log ("Valid Date: " + dateOK);
		Debug.Log (wyDay.TurboActivate.TurboActivate.TrialDaysRemaining());

		if(dateOK == false) {
			trialValid = false;
			errorMessage.text = ERROR_DATE_TIME;
		} else {
			DateTime expirationDate = Convert.ToDateTime(trialExpires);

			Debug.Log (DateTime.Compare(expirationDate, DateTime.UtcNow));
			if(DateTime.Compare(expirationDate, DateTime.UtcNow) > 0) {
				trialValid = true;
			} else {
				trialValid = false;
				errorMessage.text = ERROR_TRIAL_EXPIRED;
			}
		}

		trialActive.text = trialValid ? "Trial Active: Yes" : "Trial Active: No";
	}
}
