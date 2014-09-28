using UnityEngine;


/// <summary>
/// An in-game gui which displays information such as hints, dialogue, player health and current ammo levels.
/// </summary>
[RequireComponent (typeof (GUIManager))]
public class InGameGUI : MonoBehaviour 
{
	// Unity modifiable values
	[SerializeField] private GUISector m_healthHeadingPosition;
	[SerializeField] private GUISector m_healthPosition;
	
	[SerializeField] private GUISector m_weaponHeadingPosition;
	[SerializeField] private GUISector m_ammoPosition;
	
	[SerializeField] private GUISector m_hintBoxPosition;
	[SerializeField] private GUISector m_dialogueBoxPosition;
	
	[SerializeField] private string m_healthHeading = "Health";
	[SerializeField, Range (0f, 1f)] private float m_uiFontSize = 0.8f;
	[SerializeField, Range (0f, 1f)] private float m_dialogueFontSize = 0.1f;
	[SerializeField] private Color m_backgroundColor = Color.gray;
	[SerializeField] private Color m_contentColor = Color.white;
	
	
	// Public variables
	public string hintText = "";
	public string dialogueText = "";

	public bool displayPlayer = true;
	public bool displayHint = false;
	public bool displayDialogue = false;
	
	
	// Member variables
	private string m_healthText = "";
	private string m_weaponHeading = "Weapon";
	private string m_ammoText = "";
	private bool m_displayWeapon = false;
	private int m_dialogueFont = 0;
	private GUIStyle m_style = new GUIStyle();	// Used for caching purposes
	
	
	
	// External references
	private Health m_health;				// Used to get the health information from the player
	private PlayerInventory m_inventory;	// Used to get the weapon information from the player
	
		

	// Functions
	public void Initialise()
	{	
		GameObject player = GameObject.FindGameObjectWithTag (Tags.player);
		m_health = player.GetComponent<Health>();
		m_inventory = player.GetComponent<PlayerInventory>();
		
		if (!m_health || !m_inventory)
		{
			Debug.LogError ("Unable to determine GUIManager: .m_health || .m_inventory.");
		}
		
		m_style.normal.textColor = m_contentColor;
		m_healthHeadingPosition.Initialise();
		m_healthPosition.Initialise();
		m_weaponHeadingPosition.Initialise();
		m_ammoPosition.Initialise();
		m_hintBoxPosition.Initialise();
		m_dialogueBoxPosition.Initialise();
		
		m_dialogueFont = Screen.width < Screen.height ?
						(int) (Screen.width * m_dialogueFontSize * 0.75f) :
						(int) (Screen.height * m_dialogueFontSize * 0.75f);
	}


	private void OnGUI()
	{
		// Prepare GUI styles
		GUI.skin.button.wordWrap = true;
		
		GUI.backgroundColor = m_backgroundColor;
		GUI.contentColor = m_contentColor;
		
		
		// Obtain and draw all text
		ObtainText();
		DrawHealth();
		DrawWeapon();
		DrawHint();
		DrawDialogue();
	}


	private void ObtainText()
	{
		m_healthText = ((int) m_health.totalHealth).ToString();
		
		RangedWeapon weapon = m_inventory.activeWeapon;
		if (weapon)
		{
			m_displayWeapon = true;
			m_weaponHeading = m_inventory.activeWeaponType + " (" + weapon.fireMode + ")";
			m_ammoText = weapon.currentClip + " / " + weapon.ammo;
		}
		
		else
		{
			m_displayWeapon = false;
		}
	}
	
	
	private void DrawHealth()
	{
		if (displayPlayer)
		{
			// Draw health heading
			GUI.Label (m_healthHeadingPosition.rectangle, m_healthHeading, 
			           GUIManager.ConfigureGUIStyle (m_style, m_healthHeadingPosition, m_healthHeading, m_uiFontSize, true));
			
			// Draw health text
			GUI.Label (m_healthPosition.rectangle, m_healthText, 
			           GUIManager.ConfigureGUIStyle (m_style, m_healthPosition, m_healthText, m_uiFontSize, true));
		}
	}
	
	
	private void DrawWeapon()
	{
		if (displayPlayer && m_displayWeapon)
		{
			// Draw weapon heading
			GUI.Label (m_weaponHeadingPosition.rectangle, m_weaponHeading, 
			           GUIManager.ConfigureGUIStyle (m_style, m_weaponHeadingPosition, m_weaponHeading, m_uiFontSize, true));
			
			
			// Draw ammo
			GUI.Label (m_ammoPosition.rectangle, m_ammoText, 
			           GUIManager.ConfigureGUIStyle (m_style, m_ammoPosition, m_ammoText, m_uiFontSize, true));
		}
	}
	
	
	private void DrawHint()
	{		
		if (displayHint)
		{
			// Draw hint text as a box
			GUI.Label (m_hintBoxPosition.rectangle, hintText, 
			           GUIManager.ConfigureGUIStyle (GUI.skin.button, m_hintBoxPosition, hintText, m_uiFontSize, true));
		}
	}
	
	
	private void DrawDialogue()
	{
		if (displayDialogue)
		{
			// Left-align the dialogue text
			GUI.skin.button.alignment = TextAnchor.MiddleLeft;
			GUI.skin.button.fontSize = m_dialogueFont;
			
			// Draw dialogue text as a box
			GUI.Label (m_dialogueBoxPosition.rectangle, dialogueText, "button");
		}
	}
}
