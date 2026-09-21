using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolarityRunner.Characters;

namespace PolarityRunner.Enemies
{

	public class Water : Enemy
	{
		protected override bool UsesPolarity {
			get { return false; }
		}

		[SerializeField]
		private Collider2D m_Collider2D;

		public override Collider2D Collider2D {
			get {
				return m_Collider2D;
			}
		}

		void OnTriggerEnter2D (Collider2D other)
		{
			Character character = other.GetComponent<Character> ();
			if (CanAffect (character)) {
				Kill (character);
			}
		}

		public override void Kill (Character target)
		{
			if (!CanAffect (target)) {
				return;
			}
			target.Die ();
			Vector3 spawnPosition = target.transform.position;
			spawnPosition.y += -1f;
			ParticleSystem particle = Instantiate<ParticleSystem> (target.WaterParticleSystem, spawnPosition, Quaternion.identity);
			Destroy (particle.gameObject, particle.main.duration);
			AudioManager.Singleton.PlayWaterSplashSound (transform.position);
		}

	}

}
