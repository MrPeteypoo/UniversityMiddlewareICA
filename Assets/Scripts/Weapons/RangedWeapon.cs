using UnityEngine;
using System.Collections;


/// <summary>
/// The script used for all ranged weaponry in the game, it includes the ability to manage ammo, the projectile, the impulsive power,
/// whether the weapon requires to be aimed and the light/particle systems
/// </summary>
[RequireComponent (typeof (Light))]
[RequireComponent (typeof (ParticleSystem))]
[RequireComponent (typeof (Torch))]
public class RangedWeapon : MonoBehaviour 
{
	// Unity modifiable values
	[SerializeField] private Projectile m_projectile;	// Require a rigidbody object for the projectile
	[SerializeField] private AudioClip m_audioShoot;	// The audio to play on fire
	[SerializeField] private AudioClip m_audioEmpty;	// The audio to play when empty
	
	[SerializeField, Range (0f, 1f)] private float m_shootVolume = 1f;			// How much damage each projectile can deal
	[SerializeField, Range (0f, 1f)] private float m_emptyVolume = 0.25f;		// How much damage each projectile can deal
	[SerializeField, Range (0f, 1000f)] private float m_damage = 25f;			// How much damage each projectile can deal
	[SerializeField, Range (0f, 1000f)] private float m_force = 5f;				// How powerful to shoot the object
	[SerializeField, Range (0f, 10f)] private float m_cooldownTime = 0.1f;		// The cooldown time between shots
	[SerializeField, Range (0f, 10f)] private float m_reloadTime = 0.1f;		// How long it takes to reload
	[SerializeField, Range (0f, 1f)] private float m_flashTime = 0.1f;			// How long the shot will cause the light to flash
	[SerializeField, Range (0f, 10f)] private float m_particleEmitTimer = 1f;	// How long the particle emitter will emit after shooting
	[SerializeField, Range (1, 100)] private int m_clipSize = 5;				// The maximum clip size
	[SerializeField, Range (0, 500)] private int m_ammo = 5;					// How much ammo to start with
	[SerializeField, Range (0, 500)] private int m_maxAmmo = 25;				// The maximum possible ammo
	[SerializeField] private bool m_needsAimToShoot = true;						// Whether aiming is required to shoot the gun
	[SerializeField] private FireMode[] m_fireModes = { FireMode.Single };		// The available FireModes


	// Member variables
	private FireMode m_fireMode = FireMode.Single;	// The default FireMode
	private int m_modeIndex = 0;					// The last used mode index
	private int m_currentClip = 0;					// The amount of ammo currently in the clip
	private float m_cooling = 0f;					// Indicates whether the weapon is cooling down or not
	private bool m_reloading = false;				// Indicates whether the weapon is reloading or not
	private Torch m_torch;							// A reference to the Torch component


	// Properties
	public FireMode fireMode
	{
		get { return m_fireMode; }
	}
	
	public bool needsAimToShoot
	{
		get { return m_needsAimToShoot; }
	}

	public float currentClip
	{
		get { return m_currentClip; }
	}

	public float ammo
	{
		get { return m_ammo; }
	}



	// Functions
	private void Awake()
	{
		if (!m_projectile)
		{
			Debug.LogError ("Unable to determine RangedWeapon: .m_projectile.");
		}

		if (m_fireModes.Length == 0)
		{
			Debug.LogError ("Empty list of fire modes in RangedWeapon.");
		}

		else
		{
			m_fireMode = m_fireModes[0];
		}
		
		m_maxAmmo = Mathf.Max (m_ammo, m_maxAmmo);
		m_torch = GetComponent<Torch>();
		m_torch.TurnOff (false);
	}
	
	
	// Make sure the light gets turned off
	private void OnDisable()
	{
		if (m_torch.enabled)
		{
			m_torch.TurnOff (false);
			m_torch.enabled = false;
		}

		particleSystem.enableEmission = false;
	}


	// Force reload or cooldown timer to continue lowering
	private void OnEnable()
	{
		if (m_reloading)
		{
			StartCoroutine (ReloadWeapon());
		}

		if (m_cooling != 0f)
		{
			StartCoroutine (CooldownWeapon (false));
		}
	}
	

	// Reduce the cooldown time and reset it if necessary
	private IEnumerator CooldownWeapon (bool reset)
	{
		if (reset) { m_cooling = m_cooldownTime; }

		while (m_cooling != 0f)
		{
			m_cooling = Mathf.Max (0f, m_cooling - Time.deltaTime);
			yield return null;
		}
	}


	private IEnumerator ReloadWeapon()
	{
		// Set m_currentClip to 0 so that if the player swaps weapon they have to reload
		if (m_ammo != 0 && !m_reloading)
		{
			m_ammo = Mathf.Min (m_ammo + m_currentClip, m_maxAmmo);
			m_currentClip = 0;
			m_reloading = true;
			
			yield return new WaitForSeconds (m_reloadTime);
			
			m_reloading = false;
			m_currentClip = Mathf.Min (m_clipSize, m_ammo);
			m_ammo -= m_currentClip;
		}
	}


	private IEnumerator ShootOverTime()
	{
		int m_shotsLeft = CalculateTotalShots();

		while (m_shotsLeft > 0)
		{
			if (m_cooling == 0f)
			{
				if (m_currentClip != 0)
				{
					if (m_audioShoot)
					{
						AudioSource.PlayClipAtPoint (m_audioShoot, transform.position, m_shootVolume);
					}

					ShootProjectile();
					StartCoroutine (ShootingFlash());
					StartCoroutine (EmitParticles());
					--m_currentClip;
				}

				else
				{
					if (m_audioEmpty)
					{
						AudioSource.PlayClipAtPoint (m_audioEmpty, transform.position, m_emptyVolume);
					}
				}
				
				--m_shotsLeft;
			}

			yield return StartCoroutine (CooldownWeapon (true));
		}

		// Reload if necessary
		if (m_currentClip == 0)
		{
			StartCoroutine (ReloadWeapon());
		}
	}


	// Manage the light component
	private IEnumerator ShootingFlash()
	{
		m_torch.enabled = true;
		m_torch.TurnOn (false);

		yield return new WaitForSeconds (m_flashTime);

		m_torch.TurnOff (false);
		m_torch.enabled = false;
	}


	private IEnumerator EmitParticles()
	{
		particleSystem.enableEmission = true;

		yield return new WaitForSeconds (m_particleEmitTimer);

		particleSystem.enableEmission = false;
	}


	private int CalculateTotalShots()
	{
		switch (m_fireMode)
		{
			default:
				return 0;

			case FireMode.Single:
			case FireMode.Automatic:
				return 1;

			case FireMode.Double:
				return 2;

			case FireMode.Triple:
				return 3;
		}
	}


	private void ShootProjectile()
	{
		// Prepare the projectile
		Vector3 rotation = transform.rotation.eulerAngles;
		rotation.x += 90;

		Projectile projectile = (Projectile) Instantiate (m_projectile, transform.position, Quaternion.Euler (rotation));
		projectile.projectileDamage = m_damage;
		projectile.rigidbody.AddForce (transform.forward * m_force, ForceMode.Impulse);
	}

	
	private bool CanShoot()
	{
		return !m_reloading && m_cooling == 0f;
	}


	public void Shoot()
	{
		if (CanShoot())
		{
			StartCoroutine (ShootOverTime());
		}
	}


	public void Reload()
	{
		if (m_cooling == 0f && m_currentClip != m_clipSize)
		{
			StartCoroutine (ReloadWeapon());
		}
	}
	
	
	public void ToggleFireMode()
	{
		if (m_fireModes.Length > 0)
		{
			m_modeIndex = m_modeIndex == m_fireModes.Length - 1 ? 0 : ++m_modeIndex;
			m_fireMode = m_fireModes[m_modeIndex];
		}
	}


	public bool AddAmmo (int amount)
	{
		if (amount < 0)
		{
			Debug.LogWarning ("Attempt to add a negative amount of ammo.");
		}

		if (m_ammo == m_maxAmmo - m_currentClip)
		{
			return false;
		}

		else
		{
			m_ammo = Mathf.Clamp (m_ammo + amount, 0, m_maxAmmo - m_currentClip);
			return true;
		}
	}
}
