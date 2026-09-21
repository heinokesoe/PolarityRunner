using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PolarityRunner.Characters;

namespace PolarityRunner.Enemies
{

	public abstract class Enemy : MonoBehaviour
	{
		[SerializeField]
		private GameplayPolarity m_Polarity = GameplayPolarity.Red;

		private Character m_TargetCharacter;

		public GameplayPolarity Polarity
		{
			get { return m_Polarity; }
		}

		protected virtual bool UsesPolarity
		{
			get { return true; }
		}

		public abstract Collider2D Collider2D { get; }

		public abstract void Kill ( Character target );

		protected virtual void Awake ()
		{
			m_Polarity = PolarityRules.ForHazardPosition ( transform.position );
			if ( UsesPolarity )
			{
				PolarityVisuals.Apply ( gameObject, m_Polarity, true );
			}
		}

		protected virtual void Start ()
		{
			m_TargetCharacter = FindAnyObjectByType<Character> ();
			Character.OnAnyPolarityChanged += Character_OnAnyPolarityChanged;
			RefreshCollisionRule ();
		}

		protected virtual void OnDestroy ()
		{
			Character.OnAnyPolarityChanged -= Character_OnAnyPolarityChanged;
		}

		protected bool CanAffect ( Character target )
		{
			return target != null && !target.IsInvulnerable && ( !UsesPolarity || PolarityRules.Matches ( m_Polarity, target.Polarity ) );
		}

		private void Character_OnAnyPolarityChanged ( Character character )
		{
			m_TargetCharacter = character;
			RefreshCollisionRule ();
		}

		private void RefreshCollisionRule ()
		{
			if ( m_TargetCharacter == null )
			{
				return;
			}

			bool ignore = !CanAffect ( m_TargetCharacter );
			Collider2D[] hazardColliders = GetComponentsInChildren<Collider2D> ( true );
			Collider2D[] characterColliders = m_TargetCharacter.GetComponentsInChildren<Collider2D> ( true );

			foreach ( Collider2D hazardCollider in hazardColliders )
			{
				foreach ( Collider2D characterCollider in characterColliders )
				{
					Physics2D.IgnoreCollision ( hazardCollider, characterCollider, ignore );
				}
			}
		}

	}

}
