using UnityEngine;


/// <summary>
/// GUIManager provides a means for switching between all the different types of GUIs that exist in the game.
/// </summary>
[RequireComponent (typeof (InGameGUI))]
[RequireComponent (typeof (PauseGUI))]
public class GUIManager : MonoBehaviour 
{
	// Unity modifiable variablues
	[SerializeField] private bool m_pauseAvailable = true;	// Determines whether the pause menu can be displayed or not


	// Component references
	private InputManager m_input;
	private InGameGUI m_inGameGUI;
	private PauseGUI m_pauseGUI;



	// Functions
	private void Awake()
	{
		m_input = GetComponent<InputManager>();

		m_inGameGUI = GetComponent<InGameGUI>();
		m_pauseGUI = GetComponent<PauseGUI>();

		m_inGameGUI.Initialise();
		m_pauseGUI.Initialise();

		SwitchToInGame();
	}


	private void Update()
	{
		if (m_input.pause && !m_input.prevPause && m_pauseAvailable)
		{
			m_pauseGUI.enabled = !m_pauseGUI.enabled;

			if (m_pauseGUI.enabled)
			{
				SwitchToPause();
			}

			else
			{
				SwitchToInGame();
			}
		}
	}


	public void SwitchToInGame()
	{
		m_pauseGUI.enabled = false;
		m_inGameGUI.enabled = true;
	}


	public void SwitchToPause()
	{
		if (m_pauseAvailable)
		{
			m_pauseGUI.enabled = true;
			m_inGameGUI.enabled = false;
		}
	}


	public static GUIStyle ConfigureGUIStyle (GUIStyle modify, GUISector position, string text, float fontSize, bool centre)
	{
		modify.alignment = centre ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
		modify.fontSize = CalculateFontSize (position.rectangle.width, position.rectangle.height, text, fontSize);
		return modify;
	}


	private static int CalculateFontSize (float width, float height, string text, float fontSize)
	{
		float size;
		if (width <= height)
		{
			size = width / text.Length;

			if (size > height)
			{
				return CalculateFontSize (height, width, text, fontSize);
			}

			else
			{
				size *= 0.75f;
			}
		}

		else
		{
			size = height * fontSize;

			if (size * text.Length / 4f > width)
			{
				return CalculateFontSize (height, width, text, fontSize);
			}

			else
			{
				size *= 0.75f;
			}
		}

		return (int) size;
	}
}