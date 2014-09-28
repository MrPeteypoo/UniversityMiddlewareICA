using UnityEngine;



/// <summary>
/// A movement script used to move Kinematic objects between multiple points. A ping-pong effect can be applied or the movement can be
/// managed externally. Any GameObject with this script will become Kinematic.
/// </summary>
[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (Collider))]
public sealed class MovingPlatform : MonoBehaviour 
{
	// Unity modifiable variables
	[SerializeField, Range (0.1f, 25f)] private float m_speed = 1f;				// The speed at which to travel
	[SerializeField, Range (0.001f, 5f)] private float m_margin = 0.005f;		// The margin of tolerance for destination detection
	[SerializeField, Range (0f, 5f)] private float m_playerCheck = 0.1f;		// How far below the player can be from the platform

	[SerializeField] private TriggerType m_trigger = TriggerType.AlwaysOn;	// Determines when to start moving*/
	[SerializeField] private Transform[] m_waypoints;						// Used to move between waypoints


	// Implementation data
	private bool m_moving = false;		// Stops FixedUpdate code when movement is unnecessary
	private bool m_forwards = false;	// Indicates whether moving forwards or backwards
	private int m_target = 0;			// Used to access the correct element
	private int m_colliders = 0;		// How many colliders are currently colliding


	// Functions
	private void Awake() 
	{
		if (m_waypoints.Length == 0)
		{
			Debug.LogError ("Invalid waypoints size in MovingPlatform.");
		}

		else
		{
			for (int i = 0; i < m_waypoints.Length; ++i)
			{
				if (!m_waypoints[i])
				{
					Debug.LogError ("Null pointer found in MovingPlatform.m_waypoints[].");
				}
			}
			rigidbody.position = m_waypoints[0].position;
			rigidbody.rotation = m_waypoints[0].rotation;
		}

		rigidbody.isKinematic = true;
		m_moving = m_trigger == TriggerType.AlwaysOn ? true : false;
	}
	

	private void FixedUpdate() 
	{
		// Ensure the object needs to move
		if (m_moving)
		{
			// Determine whether the object will continue moving
			if (ReachedEnd())
			{
				if (m_trigger == TriggerType.AlwaysOn) 
				{
					AlternateTarget();
				}

				else
				{
					m_moving = false;
				}
			}

			else
			{
				// Check if the target has been reached
				if (ReachedTarget())
				{
					IncrementTarget();
				}
				
				// Move position and rotation closer to the target
				MoveCloserToTarget();
				RotateCloserToTarget();
			}
		}
	}
	
	
	// Check if above the player
	private void OnCollisionEnter (Collision other)
	{
		if (other.transform.CompareTag (Tags.player))
		{
			if (other.transform.position.y < rigidbody.position.y - m_playerCheck)
			{
				PlayerController player = other.gameObject.GetComponent<PlayerController>();

				if (player.grounded)
				{
					player.MoveToSafety (true);
				}
			}
		}
	}


	// Incrememnt collision count and set the platform to move forward
	private void OnTriggerEnter (Collider other)
	{
		switch (m_trigger)
		{
			case TriggerType.PlayerEnemy:
			case TriggerType.PlayerOnly:
				if (other.CompareTag (Tags.player))
				{
					if (!m_forwards)
					{
						AlternateTarget();
					}
					++m_colliders;
				}

				break;
		}

		switch (m_trigger)
		{
			case TriggerType.PlayerEnemy:
			case TriggerType.EnemyOnly:
				if (other.CompareTag (Tags.enemy))
				{
					if (!m_forwards)
					{
						AlternateTarget();
					}
					++m_colliders;
				}
				break;
		}
	}


	// Decrememnt collision count and set the platform to move backwards when m_trigger == 0
	private void OnTriggerExit (Collider other)
	{
		switch (m_trigger)
		{
		case TriggerType.PlayerEnemy:
		case TriggerType.PlayerOnly:
			if (other.CompareTag (Tags.player))
			{
				if (--m_colliders == 0)
				{
					AlternateTarget();
				}
			}
			break;
		}
		
		switch (m_trigger)
		{
		case TriggerType.PlayerEnemy:
		case TriggerType.EnemyOnly:
			if (other.CompareTag (Tags.enemy))
			{
				if (--m_colliders == 0)
				{
					AlternateTarget();
				}
			}
			break;
		}
	}


	// Check if the object haqs reached the current target
	private bool ReachedTarget()
	{
		return Reached (m_target);
	}


	// Check if the end has been reached
	private bool ReachedEnd()
	{

		int index = m_forwards ? m_waypoints.Length - 1 : 0;
		return Reached (index);
	}


	// Assumes m_waypoints.Length > 0
	private bool Reached (int index)
	{
		index = Mathf.Clamp (index, 0, m_waypoints.Length - 1);

		float distance = Mathf.Abs (Vector3.Distance (transform.position, m_waypoints[index].position));
		float angle = Mathf.Abs (Quaternion.Angle (transform.rotation, m_waypoints[index].rotation));

		return distance <= m_margin && angle <= m_margin;
	}


	// Determine the next target
	private void IncrementTarget()
	{
		// Skip null pointers
		do
		{

			if (m_forwards)
			{
				AddToTarget (1);
			}
			
			else
			{
				AddToTarget (-1);
			}

			if (m_target == 0 || m_target == m_waypoints.Length - 1)
			{
				break;
			}

		} while (!m_waypoints[m_target]);
	}


	// Use kinematic functions to move the position closer to the target
	private void MoveCloserToTarget()
	{
		Vector3 direction = (m_waypoints[m_target].position - rigidbody.position).normalized;

		rigidbody.MovePosition (rigidbody.position + direction * m_speed * Time.deltaTime);
	}


	// Use kinematic functions to move the rotate closer to the target
	private void RotateCloserToTarget()
	{
		rigidbody.MoveRotation (Quaternion.Slerp (rigidbody.rotation, m_waypoints[m_target].rotation, m_speed * Time.deltaTime));
	}


	// Makes sure that m_target never exceeds the length of m_waypoints, still need to check for 0.
	private void AddToTarget (int value)
	{
		m_target = Mathf.Clamp (m_target + value, 0, m_waypoints.Length - 1);
	}


	// Externally reverse the target route
	public void AlternateTarget()
	{
		m_moving = true;
		m_forwards = !m_forwards;

		IncrementTarget();
	}
}
