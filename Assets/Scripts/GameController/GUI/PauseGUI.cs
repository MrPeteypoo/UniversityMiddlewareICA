using UnityEngine;



/// <summary>
/// Displays an interactive pause menu which can be used to quit, continue play, restart level or display the controls.
/// </summary>
[RequireComponent (typeof (GUIManager))]
public class PauseGUI : MonoBehaviour 
{
	private enum PausePhase
	{
		Main, Controls
	}


	// Unity modifiable variables
	// Main phase
	[SerializeField] private GUISector m_continueButtonPosition;	// Where the contiue button is placed
	[SerializeField] private GUISector m_restartButtonPosition;		// Where the restart button is placed
	[SerializeField] private GUISector m_controlsButtonPosition;	// Where the controls button is placed
	[SerializeField] private GUISector m_quitButtonPosition;		// Where the quit button is placed
	
	[SerializeField] private string m_continueButtonText = "Resume";
	[SerializeField] private string m_restartButtonText = "Restart";
	[SerializeField] private string m_controlsButtonText = "Controls";
	[SerializeField] private string m_quitButtonText = "Quit";

	// Controls phase
	[SerializeField] private GUISector m_returnButtonPosition;
	[SerializeField] private GUISector m_controlsHeadingTextPosition;
	[SerializeField] private GUISector m_controlsTextPosition;
	
	[SerializeField] private string m_returnButtonText = "Return";
	[SerializeField] private string m_controlsHeadingText = "Controls Menu";
	[SerializeField] private string m_movementText = "Movement: \t\tWASD / Left Analogue";
	[SerializeField] private string m_crouchText = "Crouch: \t\tCTRL / Left Analogue Button";
	[SerializeField] private string m_playerRotationText = "Rotate player: \tMouse / Right Analogue";
	[SerializeField] private string m_jumpText = "Jump/Accept: \tSpace / Button A";
	[SerializeField] private string m_interactText = "Interact: \t\tE / Button X";
	[SerializeField] private string m_torchText = "Torch: \t\tF / Button B";
	[SerializeField] private string m_changeWeaponText = "Change Weapon: \tMouse wheel / D-Pad";
	[SerializeField] private string m_aimText = "Aim: \t\tRight Click / Button LB";
	[SerializeField] private string m_shootText = "Shoot: \t\tLeft Click / Button RB";
	[SerializeField] private string m_reloadText = "Reload: \t\tR / Button Y";
	[SerializeField] private string m_fireModeText = "Fire Mode: \tQ / Right Analogue Button";

	// General
	[SerializeField, Range (0f, 1f)] private float m_uiFontSize = 0.8f;
	[SerializeField, Range (0f, 1f)] private float m_controlsFontSize = 0.05f;
	[SerializeField] private Color m_backgroundColor = Color.gray;
	[SerializeField] private Color m_contentColor = Color.white;
		
	
	// Member variables
	private PausePhase m_phase = PausePhase.Main;
	private float m_timeScale = 0f;
	private int m_controlsFont;


	// Component references
	private GUIManager m_gui;


	// External references
	private GameObject m_player;
	private AudioListener m_listener;

	
	
	// Functions
	public void Initialise()
	{
		m_gui = GetComponent<GUIManager>();
		m_player = GameObject.FindGameObjectWithTag (Tags.player);
		m_listener = m_player.GetComponent<AudioListener>();

		if (!m_player)
		{
			Debug.LogError ("Unable to determine PauseGUI: .m_player");
		}
				
		
		m_controlsFont = Screen.width < Screen.height ?
						(int) (Screen.width * m_controlsFontSize * 0.75f) :
						(int) (Screen.height * m_controlsFontSize * 0.75f);

		m_continueButtonPosition.Initialise();
		m_restartButtonPosition.Initialise();
		m_controlsButtonPosition.Initialise();
		m_quitButtonPosition.Initialise();

		m_returnButtonPosition.Initialise();
		m_controlsHeadingTextPosition.Initialise();
		m_controlsTextPosition.Initialise();
	}


	private void OnEnable()
	{
		if (m_listener)
		{
			m_listener.enabled = false;
		}

		m_timeScale = Time.timeScale;
		Time.timeScale = 0f;
	}


	private void OnDisable()
	{
		if (m_listener)
		{
			m_listener.enabled = true;
		}

		Time.timeScale = m_timeScale;
		m_phase = PausePhase.Main;
	}


	private void OnGUI()
	{
		GUI.contentColor = m_contentColor;
		GUI.backgroundColor = m_backgroundColor;

		switch (m_phase)
		{
			case PausePhase.Main:
				DrawMainPhase();
				break;

			case PausePhase.Controls:
				DrawControlsPhase();
				break;
		}
	}


	private void DrawMainPhase()
	{
		// Cache the styles
		GUIStyle button = GUI.skin.button;

		// Switch back to InGameGUI if continue is pressed
		if (GUI.Button (m_continueButtonPosition.rectangle, m_continueButtonText,
		                GUIManager.ConfigureGUIStyle (button, m_continueButtonPosition, m_continueButtonText, m_uiFontSize, true)))
		{
			m_gui.SwitchToInGame();
		}

		// Restart the level from scratch
		if (GUI.Button (m_restartButtonPosition.rectangle, m_restartButtonText,
		                GUIManager.ConfigureGUIStyle (button, m_restartButtonPosition, m_restartButtonText, m_uiFontSize, true)))
		{
			Application.LoadLevel (Application.loadedLevel);
		}

		// Show the controls menu
		if (GUI.Button (m_controlsButtonPosition.rectangle, m_controlsButtonText,
		                GUIManager.ConfigureGUIStyle (button, m_controlsButtonPosition, m_controlsButtonText, m_uiFontSize, true)))
		{
			m_phase = PausePhase.Controls;
		}

		// Quit the game
		if (GUI.Button (m_quitButtonPosition.rectangle, m_quitButtonText,
		                GUIManager.ConfigureGUIStyle (button, m_quitButtonPosition, m_quitButtonText, m_uiFontSize, true)))
		{
			Application.Quit();
		}
	}


	private void DrawControlsPhase()
	{
		// Cache the styles
		GUIStyle button = GUI.skin.button;
		
		// Return to previous menu when clicked
		if (GUI.Button (m_returnButtonPosition.rectangle, m_returnButtonText,
		                GUIManager.ConfigureGUIStyle (button, m_returnButtonPosition, m_returnButtonText, m_uiFontSize, true)))
		{
			m_phase = PausePhase.Main;
		}

		// Draw all labels
		GUI.Label (m_controlsHeadingTextPosition.rectangle, m_controlsHeadingText, 
		           GUIManager.ConfigureGUIStyle (button, m_controlsHeadingTextPosition, m_controlsHeadingText, m_uiFontSize, true));

		button.fontSize = m_controlsFont;
		button.alignment = TextAnchor.MiddleLeft;

		GUI.Label (m_controlsTextPosition.rectangle, GetControlsText(), button);
	}


	private string GetControlsText()
	{
		return (m_movementText + "\n" +
				m_crouchText + "\n" +
		        m_playerRotationText + "\n" +
		        m_jumpText + "\n" +
		        m_interactText + "\n" +
		        m_torchText + "\n" +
		        m_changeWeaponText + "\n" +
		        m_aimText + "\n" +
		        m_shootText + "\n" +
		        m_reloadText + "\n" +
		        m_fireModeText);
	}
}
