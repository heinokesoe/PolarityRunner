using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolarityRunner.Utilities;

namespace PolarityRunner.Characters
{

	[RequireComponent ( typeof ( Rigidbody2D ) )]
	[RequireComponent ( typeof ( Collider2D ) )]
	[RequireComponent ( typeof ( Animator ) )]
	[RequireComponent ( typeof ( Skeleton ) )]
	public abstract class Character : MonoBehaviour
	{
		public delegate void PolarityChangedHandler ( Character character );

		public static event PolarityChangedHandler OnAnyPolarityChanged;

		[SerializeField]
		private GameplayPolarity m_Polarity = GameplayPolarity.Red;

		private float m_InvulnerableUntil;
		private Coroutine m_InvulnerabilityCoroutine;

		public delegate void DeadHandler ();

		#pragma warning disable 0067
		public virtual event DeadHandler OnDead;
		#pragma warning restore 0067

		public abstract float MaxRunSpeed { get; }

		public abstract float RunSmoothTime { get; }

		public abstract float RunSpeed { get; }

		public abstract float WalkSpeed { get; }

		public abstract float JumpStrength { get; }

		public abstract Vector2 Speed { get; }

		public abstract string[] Actions { get; }

		public abstract string CurrentAction { get; }

		public abstract int CurrentActionIndex { get; }

		public abstract GroundCheck GroundCheck { get; }

		public abstract Rigidbody2D Rigidbody2D { get; }

		public abstract Collider2D Collider2D { get; }

		public abstract Animator Animator { get; }

		public abstract ParticleSystem RunParticleSystem { get; }

		public abstract ParticleSystem JumpParticleSystem { get; }

		public abstract ParticleSystem WaterParticleSystem { get; }

		public abstract ParticleSystem BloodParticleSystem { get; }

		public abstract Skeleton Skeleton { get; }

		public virtual Property<bool> IsDead { get; set; }

		public GameplayPolarity Polarity
		{
			get { return m_Polarity; }
		}

		public bool IsInvulnerable
		{
			get { return Time.unscaledTime < m_InvulnerableUntil; }
		}

		public abstract bool ClosingEye { get; }

		public abstract bool Guard { get; }

		public abstract bool Block { get; }

		public abstract AudioSource Audio { get; }

		public abstract void Move ( float horizontalAxis );

		public abstract void Jump ();

		public abstract void Die ();

		public abstract void Die ( bool blood );

		public abstract void EmitRunParticle ();

		public virtual void SetPolarity ( GameplayPolarity polarity, bool notify = true )
		{
			m_Polarity = polarity;
			PolarityVisuals.Apply ( gameObject, m_Polarity );

			if ( notify && OnAnyPolarityChanged != null )
			{
				OnAnyPolarityChanged ( this );
			}
		}

		public virtual void TogglePolarity ()
		{
			SetPolarity ( PolarityRules.Opposite ( m_Polarity ) );
			if ( AudioManager.Singleton != null )
			{
				AudioManager.Singleton.PlayPolaritySwitchSound ();
			}
		}

		public virtual void GrantInvulnerability ( float duration )
		{
			m_InvulnerableUntil = Time.unscaledTime + Mathf.Max ( 0f, duration );
			if ( m_InvulnerabilityCoroutine != null )
			{
				StopCoroutine ( m_InvulnerabilityCoroutine );
			}
			m_InvulnerabilityCoroutine = StartCoroutine ( InvulnerabilityCrt () );
			NotifyCollisionRulesChanged ();
		}

		private IEnumerator InvulnerabilityCrt ()
		{
			while ( IsInvulnerable )
			{
				yield return null;
			}

			m_InvulnerabilityCoroutine = null;
			NotifyCollisionRulesChanged ();
		}

		private void NotifyCollisionRulesChanged ()
		{
			if ( OnAnyPolarityChanged != null )
			{
				OnAnyPolarityChanged ( this );
			}
		}

		public abstract void Reset ();

	}

}
