using UnityEngine;
using System.Collections;



/// <summary>
/// This objective is the end of the level, once talking to the Chesire Cat they will be able to unlock the door and proceed to the next
/// level.
/// </summary>
[RequireComponent (typeof (OpenDoors))]
[RequireComponent (typeof (Hint))]
public class ObjectiveForestNightRabbitHole : MonoBehaviour 
{
	private enum RabbitHolePhase
	{
		None, Beginning, Dialogue, Ending, Finished
	}


	// Unity modifiable values
	[SerializeField] private MovingCamera m_cinematic;	// The cinematic camera to use
	[SerializeField] private Vector3 m_cameraEndingRotation;	// Where the camera should look when ending
	[SerializeField] private string m_dialogue = "";			// The dialogue to display


	// Member variables
	public bool locked = true;
	private bool m_playerPresent = false;
	private RabbitHolePhase m_phase = RabbitHolePhase.None;
	private bool m_doorsSet = false;


	// Component references
	private Hint m_hint;			// The hint to display if the player hasn't spoke to the Chesire Cat
	private OpenDoors m_doors;		// Used to open the doors upon completion


	// External references
	private CameraManager m_cameraManager;
	private InGameGUI m_gui;
	private InputManager m_input;
	private PlayerController m_player;


	
	// Functions
	private void Awake()
	{
		m_hint = GetComponent<Hint>();
		m_doors = GetComponent<OpenDoors>();

		GameObject gameController = GameObject.FindGameObjectWithTag (Tags.gameController);
		m_cameraManager = gameController.GetComponent<CameraManager>();
		m_gui = gameController.GetComponent<InGameGUI>();
		m_input = gameController.GetComponent<InputManager>();

		m_player = GameObject.FindGameObjectWithTag (Tags.player).GetComponent<PlayerController>();
	}


	private void Update () 
	{
		if (m_playerPresent)
		{
			switch (m_phase)
			{
				case RabbitHolePhase.None:
					PhaseNone();
					break;
					
				case RabbitHolePhase.Beginning:
					PhaseBeginning();
					break;
					
				case RabbitHolePhase.Dialogue:
					PhaseDialogue();
					break;
					
				case RabbitHolePhase.Ending:
					PhaseEnding();
					break;
			}
		}
	}

	
	private void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag (Tags.player) && m_phase != RabbitHolePhase.Finished)
		{
			if (locked)
			{
				m_hint.ShowHint (m_gui);
			}

			m_playerPresent = true;
		}
	}
	
	
	private void OnTriggerExit (Collider other)
	{
		if (other.CompareTag (Tags.player))
		{
			m_playerPresent = false;
			m_hint.HideHint (m_gui);
		}
	}
	
	
	private void PhaseNone()
	{
		// Check if the player has interacted with the objective
		if (!locked && m_player.grounded)
		{
			m_player.canMove = false;
			m_player.StopMovement();
			
			m_hint.HideHint (m_gui);
			m_gui.displayPlayer = false;
			m_phase = RabbitHolePhase.Beginning;
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
			m_cameraManager.StartCinematic (m_cinematic.camera);
			m_cameraManager.FadeOut();
			m_phase = RabbitHolePhase.Dialogue;
		}
	}
	
	
	private void PhaseDialogue()
	{
		// Faded out
		if (m_cameraManager.fade == Color.clear)
		{
			m_gui.displayDialogue = true;
			m_gui.dialogueText = m_dialogue;

			if (!m_doorsSet)
			{
				m_doors.SetDoors (true);
				m_doorsSet = true;
			}
			
			if (m_input.jump && !m_input.prevJump)
			{
				m_gui.displayDialogue = false;
				m_cinematic.targetRotation = Quaternion.Euler (m_cameraEndingRotation);
				m_phase = RabbitHolePhase.Ending;
			}
		}
	}
	
	
	private void PhaseEnding()
	{
		if (m_cinematic.transform.rotation == m_cinematic.targetRotation)
		{
			m_cameraManager.FadeIn();		
			m_gui.displayDialogue = true;
			m_gui.dialogueText = "To be continued.....";
			m_phase = RabbitHolePhase.Finished;
		}
	}
}
