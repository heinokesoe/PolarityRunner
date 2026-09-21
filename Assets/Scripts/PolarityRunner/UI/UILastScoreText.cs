using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PolarityRunner.Utilities;

namespace PolarityRunner.UI
{

	public class UILastScoreText : Text
	{

		protected override void Awake ()
		{
			GameManager.OnScoreChanged += GameManager_OnScoreChanged;
			base.Awake ();
		}

		protected override void OnDestroy ()
		{
			GameManager.OnScoreChanged -= GameManager_OnScoreChanged;
			base.OnDestroy ();
		}

		void GameManager_OnScoreChanged ( float newScore, float highScore, float lastScore )
		{
			text = lastScore.ToLength ();
		}

	}

}
