using UnityEngine;
using System.Collections;



/// <summary>
/// The purpose of this script is entirely about controlling the chesire cat objective in the game. Upon reaching the chesire cat, the 
/// player will be able to interact with it which will start a dialogue explaining some of the story. The cat will then disappear leaving
/// behind a gun for the player to pick up and finally the platform will lower to the ground.
/// </summary>
[RequireComponent (typeof (Hint))]
public sealed class ObjectiveForestNightChesireCat : MonoBehaviour 
{
	private enum ChesireCatPhase
	{
		None, Beginning, Dialogue1, Dialogue2, PlatformLower, Finished
	}


	// Unity modifiable variables
	[SerializeField] private ObjectiveForestNightRabbitHole m_unlock;	// The objective to unlock
	[SerializeField] private GameObject m_chesireCat;					// The Chesire Cat
	[SerializeField] private GameObject m_gun;							// The gun to be obtained by the player

	[SerializeField] private MovingCamera m_camera1;	// The camera for the Dialogue1 phase
	[SerializeField] private MovingCamera m_camera2;	// The camera for the Dialogue2 phase
	[SerializeField] private Transform m_camera1Pan;	// Where camera1 should pan to
	[SerializeField] private Transform m_camera2Pan;	// Where camera2 should pan to
	
	[SerializeField] private string m_dialogue1 = "";
	[SerializeField] private string m_dialogue2 = "";	
	

	// Component references
	private Hint m_hint;					// What to display for the hint
	private MovingPlatform m_platform;		// Used to lower the platform to the ground upon the disappearance of the cat
	private LookAtTarget m_catLookAt;		// A reference to the Chesire Cats' LookAtTarget
	private ReturnToRotation m_catReturn;	// Used to return the cat to its' original position


	// External references
	private CameraManager m_cameraManager;	// Provides the ability to transition between the cameras
	private InGameGUI m_gui;				// Used for the entire cinematic sequence
	private InputManager m_input;			// Used for continuing the cinematic
	private PlayerController m_player;		// A refernce to the player to prevent movement during the scene


	// Variable cache
	private bool m_playerPresent = false;					// Indicates whether the player is present
	private ChesireCatPhase m_phase = ChesireCatPhase.None;	// Used in handling the cinematic scene



	// Functions
	private void Awake()
	{
		m_hint = GetComponent<Hint>();
		m_platform = GetComponent<MovingPlatform>();

		GameObject gameController = GameObject.FindGameObjectWithTag (Tags.gameController);
		m_cameraManager = gameController.GetComponent<CameraManager>();
		m_gui = gameController.GetComponent<InGameGUI>();
		m_input = gameController.GetComponent<InputManager>();

		m_player = GameObject.FindGameObjectWithTag (Tags.player).GetComponent<PlayerController>();

		if (!m_chesireCat || !m_gun || !m_platform || !m_gui || !m_player)
		{
			Debug.LogError ("Unable to determine ObjectiveForestNightChesireCat: .m_chesireCat || .m_gun || .m_platform || .m_gui || " +
				".m_player.");
		}

		else
		{
			m_catLookAt = m_chesireCat.GetComponentInChildren<LookAtTarget>();
			m_catReturn = m_chesireCat.GetComponentInChildren<ReturnToRotation>();

			m_catLookAt.enabled = false;
			m_catReturn.enabled = false;
		}
	}

	
	private void Update () 
	{
		if (m_playerPresent)
		{
			switch (m_phase)
			{
				case ChesireCatPhase.None:
					PhaseNone();
					break;
					
				case ChesireCatPhase.Beginning:
					PhaseBeginning();
					break;
					
				case ChesireCatPhase.Dialogue1:
					PhaseDialogue1();
					break;
				
				case ChesireCatPhase.Dialogue2:
					PhaseDialogue2();
					break;
					
				case ChesireCatPhase.PlatformLower:
					PhasePlatformLower();
					break;
			}
		}
	}
	

	private void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag (Tags.player) && m_phase != ChesireCatPhase.Finished)
		{
			m_catLookAt.enabled = true;
			m_catReturn.enabled = false;
			m_playerPresent = true;
			m_hint.ShowHint (m_gui);
		}
	}


	private void OnTriggerExit (Collider other)
	{
		if (other.CompareTag (Tags.player))
		{
			m_catLookAt.enabled = false;
			m_catReturn.enabled = true;
			m_playerPresent = false;
			m_hint.HideHint (m_gui);
		}
	}


	private void PhaseNone()
	{
		// Check if the player has interacted with the objective
		if (m_input.use && !m_input.prevUse && m_player.grounded)
		{
			m_player.canMove = false;
			m_player.StopMovement();

			m_hint.HideHint (m_gui);
			m_gui.displayPlayer = false;
			m_phase = ChesireCatPhase.Beginning;
		}
	}


	private void PhaseBeginning()
	{
		// Faded out
		if (m_cameraManager.fade == Color.clear)
		{
			m_cameraManager.FadeIn();
		}

		// Faded in
		else if (m_cameraManager.fade == Color.black)
		{
			m_cameraManager.StartCinematic (m_camera1.camera);
			m_cameraManager.FadeOut();
			m_phase = ChesireCatPhase.Dialogue1;
		}
	}


	private void PhaseDialogue1()
	{
		// Faded out
		if (m_cameraManager.fade == Color.clear)
		{
			m_camera1.targetPosition = m_camera1Pan.position;
			m_camera1.targetRotation = m_camera1Pan.rotation;

			m_gui.displayDialogue = true;
			m_gui.dialogueText = m_dialogue1;

			if (m_input.jump && !m_input.prevJump)
			{
				m_gui.displayDialogue = false;
				m_cameraManager.StartCinematic (m_camera2.camera);
				m_cameraManager.FadeOut();
				m_phase = ChesireCatPhase.Dialogue2;
			}
		}
	}
	
	
	private void PhaseDialogue2()
	{
		// Faded out
		if (m_cameraManager.fade == Color.clear)
		{
			m_camera2.targetPosition = m_camera2Pan.position;
			m_camera2.targetRotation = m_camera2Pan.rotation;
			
			m_gui.displayDialogue = true;
			m_gui.dialogueText = m_dialogue2;
			
			if (m_input.jump && !m_input.prevJump)
			{
				m_cameraManager.FadeIn();
			}
		}
		
		else if (m_cameraManager.fade == Color.black)
		{
			m_gui.displayDialogue = false;
			m_cameraManager.FadeOut();
			m_cameraManager.EndCinematic();
			m_phase = ChesireCatPhase.PlatformLower;
		}
	}
	
	
	private void PhasePlatformLower()
	{
		// Make the cat disappear
		m_unlock.locked = false;
		m_chesireCat.SetActive (false);
		m_gun.SetActive (true);
		m_platform.AlternateTarget();
		m_gui.displayPlayer = true;

		m_player.canMove = true;
		m_phase = ChesireCatPhase.Finished;
	}
}
