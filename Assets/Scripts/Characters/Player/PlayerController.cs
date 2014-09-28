using UnityEngine;
using System.Collections;



/// <summary>
/// PlayerController provides means of controlling all aspects of the gameplay and providing a way for the character to react to input. 
/// PlayerController handles all player-related input using InputManager to avoid sluggish controls from the GetButtonDown methods of Input.
/// </summary>
[RequireComponent (typeof (Animator))]
[RequireComponent (typeof (Health))]
[RequireComponent (typeof (PlayerMovement))]
[RequireComponent (typeof (PlayerInventory))]
public sealed class PlayerController : MonoBehaviour 
{
	// Unity editable variables
	public bool canMove = true;													// Allows the players movement to be disabled externally.
	[SerializeField, Range (0f, 100f)] private float m_boundaryPenalty = 10f;	// How much to damage the player by upon invalid movements.


	// Member variables
	private bool m_hasDied = false;			// Stops the death animation happening multiple times.
	private Camera m_prevCamera;			// Stores the last camera so the players movement controls can feel smoother.

	private CameraManager m_cameras;		// Used for determining which camera to use for input.
	private InputManager m_input;			// Used to obtain all input.
	private HashIDs m_hashes;				// Contains the hashes of the players Animator properties.
	private GameObject[] m_torches;			// Stores a reference to each PlayerTorch.
	private Animator m_animator;			// Allows control over the players Animator component.
	private Health m_health;				// Contains all player health information.
	private PlayerInventory m_inventory;	// Used for all weaponry interactions
	private PlayerMovement m_movement;		// Used to move the player.
	private RangedWeapon m_weapon;			// A reference to the players current weapon


	// Properties
	public bool grounded
	{
		get { return m_movement.grounded; }
	}



	// Assign references on load
	private void Awake()
	{
		// Speed up loading by reducing FindGameObjectWithTag calls
		GameObject gameController = GameObject.FindGameObjectWithTag (Tags.gameController);
		if (gameController)
		{
			m_cameras = gameController.GetComponent<CameraManager>();
			m_input = gameController.GetComponent<InputManager>();
			m_hashes = gameController.GetComponent<HashIDs>();
			m_prevCamera = m_cameras.currentCamera;
		}

		else
		{
			Debug.LogError ("Unable to find gameController with tag: " + Tags.gameController + ".");
		}

		m_torches = GameObject.FindGameObjectsWithTag (Tags.playerTorch);
		if (m_torches.Length == 0)
		{
			Debug.LogError ("Unable to find any torches with tag: " + Tags.playerTorch + ".");
		}

		// Guaranteed by RequiredComponent
		m_animator = GetComponent<Animator>();
		m_health = GetComponent<Health>();
		m_inventory = GetComponent<PlayerInventory>();
		m_movement = GetComponent<PlayerMovement>();
		m_weapon = m_inventory.activeWeapon;
	}
	
	
	// Ensure correct physics by using FixedUpdate()
	private void FixedUpdate()
	{
		if (canMove && !m_health.isDead)
		{
			// Move/rotate the player based on current input
			MovePlayer();
		}
	}


	// Use Update for non-physics toggle input
	private void Update()
	{
		if (!m_health.isDead)
		{
			if (canMove)
			{
				// Check for aiming and movement change
				UpdateCamera();
				
				// Toggle torches upon correct input
				UpdateTorches();

				// Jump on toggle
				UpdateJump();

				// React to weapon-based input
				UpdateWeapon();
			}
		}
		
		else if (!m_hasDied)
		{
			m_hasDied = true;
			m_animator.SetBool (m_hashes.deadBool, m_hasDied);
			StartCoroutine (WaitForSafety (true));
		}
		
		else
		{
			// Prevent animation from constantly looping.
			m_animator.SetBool (m_hashes.deadBool, false);
		}
	}
	
	
	private void MovePlayer()
	{
		bool aim = m_weapon && m_input.aim ? true : false;

		m_movement.MovePlayer (m_input.moveX, m_input.moveY, m_input.rotateX, m_input.walk, m_input.crouch, aim, m_prevCamera);
	}


	// Trigger a camera change as well as updating m_prevCamera if necessary
	private void UpdateCamera()
	{
		// If movement input changes then obtain the current camera anyway, this way if input stays the same camera transitions won't 
		// have such a huge effect.
		if (m_input.moveX != m_input.prevMoveX || m_input.moveY != m_input.prevMoveY)
		{
			m_prevCamera = m_cameras.currentCamera;
		}
	}


	// Switch the players torches on/off
	private void UpdateTorches()
	{
		// The player must release the torch button to toggle it again.
		if (m_input.torch && !m_input.prevTorch)
		{
			foreach (GameObject playerTorch in m_torches)
			{
				Torch torch = playerTorch.GetComponent<Torch>();
				if (torch)
				{
					torch.AlternateIntensity (true);
				}
			}
		}
	}


	private void UpdateJump()
	{
		if (m_input.jump && !m_input.prevJump)
		{
			m_movement.Jump();
		}
	}


	private void UpdateWeapon()
	{
		// Check that the player is currently using a weapon
		if (m_weapon)
		{
			// Check fire mode toggles
			if (m_input.fireMode && !m_input.prevFireMode)
			{
				m_weapon.ToggleFireMode();
			}
			
			// Toggle based reloading
			if (m_input.reload && !m_input.prevReload)
			{
				m_weapon.Reload();
			}

			// Allow continuous fire when using automatic weaponry
			if (m_weapon.fireMode == FireMode.Automatic)
			{
				if (m_movement.aiming && m_input.shoot)
				{
					m_weapon.Shoot();
				}
			}

			// Use toggles for all other weaponry
			else if (m_movement.aiming && m_input.shoot && !m_input.prevShoot)
			{
				m_weapon.Shoot();
			}
		}

		if (m_input.changeWeapon != 0f)
		{
			bool increase = m_input.changeWeapon > 0f ? true : false;
			m_inventory.ToggleWeapon (increase);
			m_weapon = m_inventory.activeWeapon;
		}
	}


	public void MoveToSafety (bool applyPenalty)
	{
		// Apply damage penalty if necessary
		if (applyPenalty)
		{
			m_health.Damage (m_boundaryPenalty);
		}

		if (m_health.isDead)
		{
			StartCoroutine (WaitForSafety (true));
		}

		else
		{
			StartCoroutine (WaitForSafety (false));
		}
	}


	public void StopMovement()
	{
		m_movement.MovePlayer (0f, 0f, 0f, false, false, false, m_prevCamera);
	}


	private IEnumerator WaitForSafety (bool restartLevel)
	{
		m_cameras.FadeIn();
		canMove = false;
		m_movement.applyFallDamage = false;

		while (m_cameras.fade != Color.black) { yield return null; }

		if (restartLevel)
		{
			Application.LoadLevelAsync (Application.loadedLevel);
		}

		else
		{
			m_movement.MoveToSafety();
			m_cameras.FadeOut();
		}

		while (m_cameras.fade != Color.clear) { yield return null; }

		m_movement.applyFallDamage = true;
		canMove = true;
	}
}
