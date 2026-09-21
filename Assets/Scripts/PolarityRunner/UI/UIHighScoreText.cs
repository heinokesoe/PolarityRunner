using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PolarityRunner.UI
{

	public class UIHighScoreText : Text
	{

		protected override void Start ()
		{
			base.Start ();
			GameManager manager = GameManager.Singleton ?? FindAnyObjectByType<GameManager> ();
			if ( manager != null )
			{
				manager.m_Coin.AddEventAndFire ( UpdateCoinText, this, true );
			}
		}

		private void UpdateCoinText ( int coinAmount )
		{
			text = coinAmount.ToString ();
		}

	}

}
