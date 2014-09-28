using UnityEngine;
using System.Collections;



/// <summary>
/// The primary projectile script used in the game. It controls particle emission and the collision of the projectile. Projectiles can
/// either be explosive or just damaging. They can also have their damage fade with distance.
/// </summary>
[RequireComponent (typeof (Collider))]
[RequireComponent (typeof (Rigidbody))]
public class Projectile : MonoBehaviour 
{
	// Unity modifiable variables
	[SerializeField] private ProjectileType m_type = ProjectileType.Standard;		// Used in determining what to do on collision
	[SerializeField, Range (0f, 1f)] private float m_triggerMargin = 0.25f;			// How long before detecting collisions
	[SerializeField, Range (0f, 60f)] private float m_killTimer = 10f;				// How long to wait before destroying the object

	[SerializeField] private bool m_applyImpactForce = true;						// Should the project apply impact force or not
	[SerializeField] private bool m_damageFadeOverTime = true;						// Whether the damage should reduce or not

	[SerializeField, Range (0f, 1000f)] private float m_impactForce = 1f;			// How much force to apply to the colliding object
	[SerializeField, Range (0f, 100f)] private float m_explosiveRadius = 2f;		// How large of an explosive radius to use
	[SerializeField, Range (0f, 1000f)] private float m_damageFadePerSecond = 10f;	// Allows the projectile damage to fade over time

	[SerializeField] private Explosion m_explosion;									// The explosion to instantiate upon collision

	[SerializeField] private AudioClip m_collisionAudio;							// The clip to play on collision
	[SerializeField, Range (0f, 1f)] private float m_audioVolume = 0.75f;			// How loud the audio should be


	// Member variables
	private float m_maxDamage = 10f;
	private float m_currentDamage = 10f;
	private bool m_trigger = false;


	// Properties
	public float projectileDamage
	{
		get { return m_maxDamage; }
		set 
		{
			float percentage = m_currentDamage / m_maxDamage;
			m_maxDamage = Mathf.Max (0f, value);
			m_currentDamage = m_maxDamage * percentage;
		}
	}



	private void Awake ()
	{
		StartCoroutine (WaitTriggerMargin());
		StartCoroutine (WaitKillTimer());
	}


	// Correct the damage value
	private void Update () 
	{
		if (m_damageFadeOverTime)
		{
			m_currentDamage = Mathf.Max (0f, m_currentDamage - m_damageFadePerSecond * Time.deltaTime);
		}
	}


	private void OnTriggerEnter (Collider other)
	{
		if (m_trigger)
		{
			// Cause the correct type of force
			switch (m_type)
			{
			case ProjectileType.Standard:
				StandardCollision (other);
				break;
				
			case ProjectileType.Explosive:
				ExplosiveCollision();
				break;
			}
			
			// Play audio
			if (m_collisionAudio)
			{
				AudioSource.PlayClipAtPoint (m_collisionAudio, rigidbody.position, m_audioVolume);
			}
			
			// Create explosion
			if (m_explosion)
			{
				Instantiate (m_explosion, rigidbody.position, rigidbody.rotation);
			}
			
			// Destroy the projectile
			enabled = false;
			Destroy (gameObject);
		}
	}


	private IEnumerator WaitTriggerMargin()
	{
		m_trigger = false;
		yield return new WaitForSeconds (m_triggerMargin);
		m_trigger = true;
	}


	private IEnumerator WaitKillTimer()
	{
		yield return new WaitForSeconds (m_killTimer);
		Destroy (gameObject);
	}


	private void StandardCollision (Collider other)
	{
		// Damage the colliding object
		Damage (other);

		// Apply the impact force
		if (m_applyImpactForce && other.rigidbody)
		{
			// Determine how to apply the force
			Vector3 force = (rigidbody.position - other.rigidbody.position).normalized * m_impactForce;
			other.rigidbody.AddForceAtPosition (force, rigidbody.position);
		}
	}


	private void ExplosiveCollision()
	{
		// Obtain all colliding objects
		Collider[] collisions = Physics.OverlapSphere (rigidbody.position, m_explosiveRadius);
		Health health;

		foreach (Collider other in collisions)
		{
			if (health = other.GetComponent<Health>())
			{
				health.Damage (m_currentDamage);
			}

			if (other.rigidbody)
			{
				other.rigidbody.AddExplosionForce (m_impactForce, rigidbody.position, m_explosiveRadius);
			}
		}


	}


	private void Damage (Collider other)
	{		
		Health health = other.GetComponent<Health>();
		if (health && m_currentDamage != 0f)
		{
			health.Damage (m_currentDamage);
		}
	}
}
