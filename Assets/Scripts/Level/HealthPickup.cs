using UnityEngine;


/// <summary>
/// The class used to manage all health pickups for the player, both positive and negative.
/// </summary>
public sealed class HealthPickup : PlayerPickup
{
	// Unity modifiable values
	[SerializeField] private HealthType m_healthType = HealthType.Permanent;	// Indicates the way the health should be applied
	[SerializeField, Range (-10000f, 10000f)] private float m_value = 10f;		// How much of an effect to apply to the player

	// Member variables
	Health m_health;	// A reference to the players Health component



	// Functions
	protected override void Awake()
	{
		collider.isTrigger = true;
		m_health = GameObject.FindGameObjectWithTag (Tags.player).GetComponent<Health>();

		if (!m_health)
		{
			Debug.LogError ("Unable to determine HealthPickup: .m_health.");
		}
	}


	protected override void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag (Tags.player))
		{
			switch (m_healthType)
			{
				case HealthType.Permanent:
					AddPermanentHealth();
					break;

				case HealthType.Temporary:
					AddTemporaryHealth();
					break;
			}
		}

		DestroySelf();
	}


	private void AddPermanentHealth()
	{
		if (m_value > 0f)
		{
			m_health.AddHealth (m_value);
		}

		else
		{
			m_health.Damage (m_value);
		}
	}


	private void AddTemporaryHealth()
	{
		if (m_value < 0f)
		{
			Debug.LogError ("Attempt to add" + (double) m_value + " temporary health to Health.");
		}

		else
		{
			m_health.AddTempHealth (m_value);
		}
	}
}
