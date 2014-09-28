using UnityEngine;
using System.Collections;



/// <summary>
/// Health is the primary class used to handle all health based tasks such as increasing health, capping health, indicating death, etc.
/// Health can be used for the player(s) and enemies as control based on the current health value should be managed by a controller class.
/// </summary>
public sealed class Health : MonoBehaviour 
{
	[SerializeField] private AudioClip m_damageAudio;							// The audio to play when damaged
	[SerializeField, Range (0f, 1)] private float m_damageVolume = 1f;			// The volume of the audio

	[SerializeField, Range (1f, 9999f)] private float m_healthCap = 100f;		// The maximum amount of health for the character
	[SerializeField, Range (0f, 999f)] private float m_temporaryCap = 50f;		// The maximum amount of temporary health for the character
	[SerializeField, Range (0f, 100f)] private float m_fadeSpeed = 5f;			// The amount of temporary health lost a second
	[SerializeField, Range (0f, 9999f)] private float m_startingHealth = 50f;	// The starting value

	private float m_health = 0f;		// Core health
	private float m_tempHealth = 0f;	// Temporary health
	private float m_totalHealth = 0f;	// The combination of the two
	private bool m_dead = true;			// Use a bool to avoid constant float comparisons
	private bool m_fade = false;		// Used to signify that temporary health needs fading

	public bool isDead { get { return m_dead; }	}
	public float totalHealth { get { return m_totalHealth; } }


	// Assign all default values
	private void Awake()
	{
		m_startingHealth = m_startingHealth > m_healthCap ? m_healthCap : m_startingHealth;

		m_health = m_startingHealth;
		m_totalHealth = m_health;

		if (m_totalHealth != 0f)
		{
			m_dead = false;
		}
	}

	
	// Update temporary health
	private void Update() 
	{
		if (m_fade && !m_dead)
		{
			m_tempHealth -= m_fadeSpeed * Time.deltaTime;

			if (m_tempHealth <= 1f)
			{
				// Ensure death doesn't occur when using temporary health
				m_tempHealth = 0f;
				m_health = m_health == 0f ? 1f : m_health;
				m_fade = false;
			}

			UpdateHealth();
		}
	}


	private void UpdateHealth()
	{
		m_totalHealth = m_health + m_tempHealth;

		// Don't allow any value below 10
		if (m_totalHealth < 1f)
		{
			m_health = 0f;
			m_tempHealth = 0f;
			m_totalHealth = 0f;
		}
		m_dead = m_totalHealth == 0f ? true : false;
	}


	public void AddHealth (float add)
	{
		// Ensure positive numbers
		add = Mathf.Abs (add);
		m_health += add;

		if (m_health > m_healthCap)
		{
			m_health = m_healthCap;
		}

		UpdateHealth();
	}


	public void AddTempHealth (float add)
	{
		// Ensure positive numbers, fadeSpeed could've been manipulated externally
		m_fadeSpeed = Mathf.Abs (m_fadeSpeed);
		add = Mathf.Abs (add);
		m_tempHealth += add;

		if (m_tempHealth > m_temporaryCap)
		{
			m_tempHealth = m_temporaryCap;
		}
		
		UpdateHealth();
		m_fade = m_fadeSpeed == 0f || m_tempHealth == 0f ? false : true;
	}


	public void Damage (float damage)
	{
		if (m_damageAudio)
		{
			AudioSource.PlayClipAtPoint (m_damageAudio, transform.position, m_damageVolume);
		}

		// Ensure positive numbers
		damage = Mathf.Abs (damage);
		if (damage >= m_tempHealth)
		{
			float difference = Mathf.Abs (m_tempHealth - damage);
			m_tempHealth = 0f;
			m_health -= difference;

			if (m_health < 1f)
			{
				m_health = 0f;
			}
			m_fade = false;
		}
		else
		{
			m_tempHealth -= damage;
		}
		
		UpdateHealth();
	}
}
