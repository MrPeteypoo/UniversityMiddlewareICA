using UnityEngine;
using System.Collections;



/// <summary>
/// PlayerMovement is the main script used to perform all player movement including managing animations, jumping, crouching, falling, etc.
/// </summary>
[RequireComponent (typeof (Animator))]
[RequireComponent (typeof (AudioSource))]
[RequireComponent (typeof (CapsuleCollider))]
[RequireComponent (typeof (Health))]
[RequireComponent (typeof (Rigidbody))]
public sealed class PlayerMovement : MonoBehaviour 
{
	// Unity modifable variables
	[SerializeField, Range (1f, 10f)] private float m_uprightSpeed = 6f;				// Controls the movement speed when upright
	[SerializeField, Range (0.5f, 5f)] private float m_crouchSpeed = 2f;				// Controls the movement speed when crouching
	[SerializeField, Range (0f, 1f)] private float m_airMoveMulti = .25f;				// How much aerial movement is restricted
	[SerializeField, Range (1f, 15f)] private float m_jumpPower = 7f;					// How powerful the players jump is
	
	[SerializeField, Range (0f, 10f)] private float m_turnSpeed = 4f;					// Effects how smooth the player rotations arey
	[SerializeField, Range (0f, 1f)] private float m_groundMargin = 0.1f;				// Used in ground detection to improve accuracy
	[SerializeField, Range (0f, -50f)] private float m_fallVelocityBeforeDamage = -5f;	// How long to wait before applying fall damage
	[SerializeField, Range (0f, 1000f)] private float m_fallDamagePerVelocity = 5f;		// How long to wait before applying fall damage

	[SerializeField, Range (0.1f, 3f)] private float m_standHeight = 2f;	// The standing height of the collider
	[SerializeField, Range (0.1f, 3f)] private float m_crouchHeight = 1.1f;	// The crouching height of the collider
	[SerializeField, Range (0.1f, 3f)] private float m_standCentre = .97f;	// The standing centre of the collider
	[SerializeField, Range (0.1f, 3f)] private float m_crouchCentre = .52f;	// The crouching centre of the collider

	[SerializeField] private LayerMask m_safePositionLayers;				// Which layers to check for last safe position


	// Member variables
	private Animator m_animator;			// Allows control over the players Animator component
	private CapsuleCollider m_capsule;		// Avoid constant type casting by performing it on load
	private CameraManager m_cameraManager;	// Used in updating the last safe camera
	private HashIDs m_hashes;				// Contains the hashes of the players Animator properties
	private Health m_health;			// Used in applying fall damage

	private Vector3 m_forward = Vector3.forward;			// Stores the forward direction of the camera
	private Vector3 m_direction = Vector3.forward;			// Stores the direction of the players movement/rotation
	private Vector3 m_lastSafePosition;						// Stores the last safe position
	private RaycastHit m_groundInfo = new RaycastHit();		// Stores the latest information on the ground Raycast hit
	private RaycastHit m_ceilingInfo = new RaycastHit();	// Stores the latest information on the ceiling Raycast hit
	private float m_desiredSpeed = 1f;						// The speed to move the player
	private float m_currentSpeed = 0f;						// The average speed of the horizontal and vertical input
	private int m_currentHash = 0;							// The current animator state .nameHash
	private int m_nextHash = 0;								// The next animator state .nameHash
	private bool m_grounded = true;							// Restricts movement based on whether the player is grounded
	private bool m_jumping = false;							// Used to control animation transitions upon external jump calls
	private bool m_crouching = false;						// Used to stop the player standing up with limited headroom
	private bool m_falling = false;
	private bool m_aiming = false;
	private bool m_applyFallDamage = true;


	// Properties
	public bool aiming
	{
		get { return m_aiming; }
	}


	public bool grounded
	{
		get { return m_grounded; }
	}


	public bool applyFallDamage
	{
		get { return m_applyFallDamage; }
		set { m_applyFallDamage = value; }
	}



	// Functions
	private void Awake()
	{
		// Get required components
		m_animator = GetComponent<Animator>();
		m_capsule = collider as CapsuleCollider;
		m_health = GetComponent<Health>();

		GameObject gameController = GameObject.FindGameObjectWithTag (Tags.gameController);
		if (gameController)
		{
			m_cameraManager = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<CameraManager>();
			m_hashes = GameObject.FindGameObjectWithTag (Tags.gameController).GetComponent<HashIDs>();
		}

		m_lastSafePosition = rigidbody.position;
		m_desiredSpeed = m_uprightSpeed;

		if (!m_cameraManager || !m_hashes)
		{
			Debug.LogError ("Unable to determine PlayerMovement: .m_cameraManager || .m_hashes.");
		}
	}


	private void FixedUpdate()
	{
		// Update whether the player is in the air or not
		CheckGrounded();

		// Update the ceiling info
		CheckCeiling();

		// Check if the player is jumping
		CheckJumping();

		// Check if the player is falling
		CheckFalling();

		// Change the last safe position if possible
		UpdateSafePosition();
	}


	private void Update()
	{
		// Obtain animator name hashes
		UpdateNameHashes();

		// Modify the capsule collider 
		UpdateCollider();

		// Update all animator parameters for correct animations
		UpdateAnimator();

		// Update the footstep audio
		UpdateAudio();
	}
	
	
	private void CheckGrounded()
	{
		// Raycast below the player to see if it's touching ground
		if (Physics.Raycast (transform.position, Vector3.down, out m_groundInfo, m_groundMargin))
		{
			if (!m_groundInfo.collider.isTrigger)
			{
				m_grounded = true;
			}

			else
			{
				m_grounded = false;
			}
		}
		
		else
		{
			m_grounded = false;
		}
	}


	private void CheckCeiling()
	{
		Physics.Raycast (rigidbody.position, Vector3.up, out m_ceilingInfo, Mathf.Infinity, m_safePositionLayers);
	}
	
	
	private void CheckJumping()
	{
		if (rigidbody.velocity.y >= m_jumpPower * 0.75f)
		{
			m_jumping = true;
		}
		
		else
		{
			m_jumping = false;
		}
	}


	private void CheckFalling()
	{
		if (!m_grounded && rigidbody.velocity.y < 0f && !m_falling)
		{
			StartCoroutine (Fall());
		}
	}


	private IEnumerator Fall()
	{
		m_falling = true;
		float velocity = 0f;

		while (!m_grounded)
		{
			if (rigidbody.velocity.y < velocity)
			{
				velocity = rigidbody.velocity.y;
			}

			yield return null;
		}

		if (velocity < m_fallVelocityBeforeDamage && m_applyFallDamage)
		{
			m_health.Damage ((velocity - m_fallVelocityBeforeDamage) * m_fallDamagePerVelocity);
		}

		m_falling = false;
	}


	private void UpdateSafePosition()
	{
		if (m_grounded && !m_jumping && rigidbody.velocity.y < 0.1f && rigidbody.velocity.y > -0.1f &&
		    !m_groundInfo.transform.CompareTag (Tags.stickySurface) && 
		    (!m_ceilingInfo.transform || !m_ceilingInfo.transform.CompareTag (Tags.stickySurface)))
		{
			m_lastSafePosition = rigidbody.position;
			m_cameraManager.UpdateSafetyCamera();
		}
	}


	private void UpdateNameHashes()
	{
		m_currentHash = m_animator.GetCurrentAnimatorStateInfo (0).nameHash;
		m_nextHash = m_animator.GetNextAnimatorStateInfo (0).nameHash;
	}


	// Make sure the players collider is set the the correct height
	private void UpdateCollider()
	{
		Vector3 centre = m_capsule.center;

		if (m_currentHash == m_hashes.crouchMotionState || m_nextHash == m_hashes.crouchMotionState)
		{
			m_capsule.height = m_crouchHeight;
			centre.y = m_crouchCentre;
		}

		else
		{
			m_capsule.height = m_standHeight;
			centre.y = m_standCentre;
		}

		m_capsule.center = centre;
	}
	
	
	private void UpdateAnimator()
	{
		m_animator.SetFloat (m_hashes.speedFloat, m_currentSpeed);
		m_animator.SetBool (m_hashes.crouchingBool, m_crouching);
		m_animator.SetBool (m_hashes.jumpingBool, m_jumping);
		m_animator.SetBool (m_hashes.aimingBool, m_aiming);
		m_animator.SetBool (m_hashes.resetBool, false);
		m_animator.SetBool (m_hashes.groundedBool, m_grounded);
		m_animator.SetBool (m_hashes.fallingBool, m_falling);

		m_animator.SetLookAtPosition (rigidbody.position + transform.forward * 7.5f);
		m_animator.SetLookAtWeight (1f, 0f, 1f);
	}
	
	
	private void UpdateAudio()
	{
		// TODO: Refine audio
		if (m_grounded && m_currentSpeed != 0f && m_currentHash != m_hashes.deadState)
		{
			if (!audio.isPlaying)
			{
				audio.Play();
			}
		}

		else
		{
			audio.Stop();
		}
	}


	public void MovePlayer (float horizontal, float vertical, float rotate, bool walk, bool crouch, bool aim, Camera activeCamera)
	{
		// Manage the crouch functionality
		UpdateCrouch (crouch);

		// Don't allow aiming whilst crouching
		m_aiming = !m_crouching && aim ? true : false;

		// Determine the camera-relative direction
		DetermineDirection (horizontal, vertical, activeCamera);

		// Set correct speed value for movements
		DetermineSpeed (horizontal, vertical, walk);

		// Perform correct movement
		if (m_grounded) { GroundMove(); }

		else { AirMove(); }

		// Rotate the player in the correct manner
		RotatePlayer (rotate);
	}


	private void UpdateCrouch (bool crouch)
	{
		if (crouch && !m_jumping)
		{
			m_crouching = true;
		}

		else
		{
			if (CanStand())
			{
				m_crouching = false;
			}

			else
			{
				m_crouching = true;
			}
		}
	}


	private bool CanStand()
	{
		// Raycast above the player to check for ample headroom
		float radius = m_capsule.radius * 0.5f;
		Ray ray = new Ray (transform.position + Vector3.up * radius, Vector3.up);

		return !Physics.SphereCast (ray, radius, m_standHeight);
	}


	private void DetermineDirection (float horizontal, float vertical, Camera activeCamera)
	{
		m_forward = Vector3.Scale (activeCamera.transform.forward, new Vector3 (1, 0, 1)).normalized;
		m_direction = activeCamera.transform.right * horizontal + m_forward * vertical;
		
		if (m_direction.magnitude > 1) { m_direction.Normalize(); }
	}


	private void DetermineSpeed (float horizontal, float vertical, bool walk)
	{
		// Restrict movement speed during crouching and aiming
		if (m_grounded)
		{
			m_desiredSpeed = walk || m_crouching || m_aiming ? m_crouchSpeed : m_uprightSpeed;
		}

		// Apply a multiplier to any airborne movement
		else
		{
			m_desiredSpeed = m_uprightSpeed * m_airMoveMulti;
		}

		// Update the current speed for the correct animation
		m_currentSpeed = Mathf.Abs (horizontal) > Mathf.Abs (vertical) ? 
						 Mathf.Abs (horizontal) : Mathf.Abs (vertical);
		m_currentSpeed *= m_desiredSpeed;
	}


	private void GroundMove()
	{
		// When on the ground ensure movement stops when the player stops moving
		Vector3 velocity = m_direction * m_desiredSpeed - rigidbody.velocity;

		// Check if the ground is a sticky surface
		if (m_groundInfo.transform.CompareTag (Tags.stickySurface))
		{
			velocity += m_groundInfo.rigidbody.velocity;
		}

		velocity.y = 0;
		rigidbody.AddForce (velocity, ForceMode.Impulse);
	}


	private void AirMove()
	{
		// In the air allow the player to try and manipulate their trajectory
		Vector3 velocity = m_direction * m_desiredSpeed * m_airMoveMulti;
		velocity.y = 0;
		
		rigidbody.AddForce (velocity, ForceMode.Force);
	}


	private void RotatePlayer (float rotate)
	{
		// Rotate based on the desired directional movement
		if (!m_aiming && m_direction != Vector3.zero)
		{
			Quaternion desiredRotation = Quaternion.LookRotation (m_direction);
			
			// Interpolate the rotation for a smoothing effect
			rigidbody.rotation = Quaternion.Lerp (rigidbody.rotation, desiredRotation, m_turnSpeed * Time.deltaTime);
		}

		// Rotate based on input
		else if (m_aiming)
		{
			Vector3 rotation = rigidbody.rotation.eulerAngles;
			rotation.y += rotate * m_turnSpeed;

			rigidbody.rotation = Quaternion.Euler (rotation);
		}
	}
	

	// Allow for external Jump control for power-ups such as double jump
	public void Jump()
	{
		// Make sure the player is grounded and not jumping
		if (CanJump())
		{
			// Avoid jump power issues from double jumps by resetting velocity.y
			rigidbody.velocity.Set (rigidbody.velocity.x, 0f, rigidbody.velocity.z);

			// Add the impulsive jump force
			rigidbody.AddForce (0f, m_jumpPower, 0f, ForceMode.Impulse);
			m_jumping = true;
		}
	}
	
	
	private bool CanJump()
	{
		UpdateNameHashes();
		return (m_grounded && !m_jumping && !m_crouching && 
		        m_currentHash != m_hashes.crouchMotionState && m_nextHash != m_hashes.crouchMotionState);
	}


	// Position can be externally controlled using MoveToSafety()
	public void MoveToSafety()
	{
		// Reset position, velocity and animation
		m_cameraManager.RevertToSafetyCamera();
		rigidbody.position = m_lastSafePosition;
		rigidbody.velocity = Vector3.zero;
		m_animator.SetBool (m_hashes.resetBool, true);
	}
}
