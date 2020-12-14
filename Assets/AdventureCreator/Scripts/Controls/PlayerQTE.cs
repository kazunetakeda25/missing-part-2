/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"PlayerQTE.cs"
 * 
 *	This script handles the processing of quick-time events
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class PlayerQTE : MonoBehaviour
	{

		private QTEState qteState = QTEState.None;
		private QTEType qteType = QTEType.SingleKeypress;
		
		private string inputName;
		private Animator animator;
		private bool wrongKeyFails;

		private float holdDuration;
		private float cooldownTime;
		private int targetPresses;
		private bool doCooldown;

		private float progress;
		private int numPresses;
		private float startTime;
		private float endTime;
		private float lastPressTime;


		private void Awake ()
		{
			SkipQTE ();
		}


		public QTEState GetState ()
		{
			return qteState;
		}


		public void SkipQTE ()
		{
			endTime = 0f;
			qteState = QTEState.Win;
		}


		public void StartQTE (string _inputName, float _duration, Animator _animator, bool _wrongKeyFails)
		{
			if (_inputName == "" || _duration <= 0f)
			{
				return;
			}

			Setup (QTEType.SingleKeypress, _inputName, _duration, _animator, _wrongKeyFails);
		}


		public void StartQTE (string _inputName, float _duration, float _holdDuration, Animator _animator, bool _wrongKeyFails)
		{
			if (_inputName == "" || _duration <= 0f)
			{
				return;
			}

			holdDuration = _holdDuration;
			Setup (QTEType.HoldKey, _inputName, _duration, _animator, _wrongKeyFails);
		}


		public void StartQTE (string _inputName, float _duration, int _targetPresses, bool _doCooldown, float _cooldownTime, Animator _animator, bool _wrongKeyFails)
		{
			if (_inputName == "" || _duration <= 0f)
			{
				return;
			}

			targetPresses = _targetPresses;
			doCooldown = _doCooldown;
			cooldownTime = _cooldownTime;

			Setup (QTEType.ButtonMash, _inputName, _duration, _animator, _wrongKeyFails);
		}


		private void Setup (QTEType _qteType, string _inputName, float _duration, Animator _animator, bool _wrongKeyFails)
		{
			qteType = _qteType;
			qteState = QTEState.None;

			progress = 0f;
			inputName = _inputName;
			animator = _animator;
			wrongKeyFails = _wrongKeyFails;
			numPresses = 0;
			startTime = Time.time;
			lastPressTime = 0f;
			endTime = Time.time + _duration;
		}


		public float GetRemainingTimeFactor ()
		{
			if (endTime == 0f || Time.time <= startTime)
			{
				return 1f;
			}

			if (Time.time >= endTime)
			{
				return 0f;
			}

			return (1f - (Time.time - startTime) / (endTime - startTime));
		}


		public float GetProgress ()
		{
			if (qteState == QTEState.Win)
			{
				progress = 1f;
			}
			else if (qteState == QTEState.Lose)
			{
				progress = 0f;
			}
			else if (endTime > 0f)
			{
				if (qteType == QTEType.HoldKey)
				{
					if (lastPressTime == 0f)
					{
						progress = 0f;
					}
					else
					{
						progress = ((Time.time - lastPressTime) / holdDuration);
					}
				}
				else if (qteType == QTEType.ButtonMash)
				{
					progress = (float) numPresses / (float) targetPresses;
				}
			}

			return progress;
		}


		public void UpdateQTE ()
		{
			if (endTime == 0f)
			{
				return;
			}

			if (Time.time > endTime)
			{
				Lose ();
				return;
			}
			
			if (qteType == QTEType.SingleKeypress)
			{
				if (KickStarter.playerInput.InputGetButton (inputName))
				{
					Win ();
					return;
				}
				else if (wrongKeyFails && Input.anyKey)
				{
					Lose ();
					return;
				}
			}
			else if (qteType == QTEType.ButtonMash)
			{
				if (KickStarter.playerInput.InputGetButtonDown (inputName))
				{
					numPresses ++;
					lastPressTime = Time.time;
					if (animator)
					{
						animator.Play ("Hit", 0, 0f);
					}
				}
				else if (doCooldown)
				{
					if (lastPressTime > 0f && Time.time > lastPressTime + cooldownTime)
					{
						numPresses --;
						lastPressTime = Time.time;
					}
				}

				if (KickStarter.playerInput.InputGetButtonDown (inputName)) {}
				else if (wrongKeyFails && Input.anyKeyDown)
				{
					Lose ();
					return;
				}

				
				if (numPresses < 0)
				{
					numPresses = 0;
				}
				
				if (numPresses >= targetPresses)
				{
					Win ();
					return;
				}
			}
			else if (qteType == QTEType.HoldKey)
			{
				if (KickStarter.playerInput.InputGetButton (inputName))
				{
					if (lastPressTime == 0f)
					{
						lastPressTime = Time.time;
					}
					else if (Time.time > lastPressTime + holdDuration)
					{
						Win ();
						return;
					}
				}
				else if (wrongKeyFails && Input.anyKey)
				{
					Lose ();
					return;
				}
				else
				{
					lastPressTime = 0f;
				}

				if (animator)
				{
					if (lastPressTime == 0f)
					{
						animator.SetBool ("Held", false);
					}
					else
					{
						animator.SetBool ("Held", true);
					}
				}
			}
		}


		private void Win ()
		{
			if (animator)
			{
				animator.Play ("Win");
			}
			qteState = QTEState.Win;
			endTime = 0f;
		}


		private void Lose ()
		{
			qteState = QTEState.Lose;
			endTime = 0f;
			if (animator)
			{
				animator.Play ("Lose");
			}
		}

	}

}