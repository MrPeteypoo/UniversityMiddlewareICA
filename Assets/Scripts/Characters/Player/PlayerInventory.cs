using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;					// Used for casting


/// <summary>
/// This class is used to manage the players inventory including available weapons and ammo. This class can also be used for collectibles.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
	// Unity modifable variables
	[SerializeField, Range (0f, 10f)] private float m_weaponChangeCooldown = 1f;	// How quickly the player can change weapon
	[SerializeField] private GameObject m_pistolObject;								// A reference to the object in the heirarchy
	public bool pistolEnabled = false;												// Whether the pistol can be used or not
	

	// Member variables
	private IEnumerable<WeaponType> m_weaponTypes;			// Used to provide seamless weapon toggling
	private WeaponType m_currentWeapon = WeaponType.None;	// Controls the current active weapon
	private RangedWeapon m_activeWeapon;					// A sharable reference to the current RangedWeapon
	private RangedWeapon m_pistol;							// A reference to the RangedWeapon component on m_pistolObject
	private bool m_canChangeWeapon = true;					// Prevents


	// Properties
	public RangedWeapon activeWeapon
	{
		get { return m_activeWeapon; }
	}
	
	public WeaponType activeWeaponType
	{
		get { return m_currentWeapon; }
	}


	// Functions
	private void Awake()
	{
		if (!m_pistolObject)
		{
			Debug.LogError ("Unable to determine PlayerInventory: .m_pistolObject.");
		}

		else
		{
			m_pistol = m_pistolObject.GetComponent<RangedWeapon>();
		}

		m_weaponTypes = System.Enum.GetValues (typeof (WeaponType)).Cast<WeaponType>();
	}


	private void UpdateWeapon()
	{
		switch (m_currentWeapon)
		{
			
			default:
			case WeaponType.None:
				m_pistolObject.SetActive (false);
				m_activeWeapon = null;
				m_currentWeapon = WeaponType.None;
				break;

			case WeaponType.Pistol:
				if (pistolEnabled)
				{
					m_pistolObject.SetActive (true);
					m_activeWeapon = m_pistol;
				}
				break;
		}
	}


	private IEnumerator WeaponChangeCooldown()
	{
		m_canChangeWeapon = false;
		yield return new WaitForSeconds (m_weaponChangeCooldown);
		m_canChangeWeapon = true;
	}


	public void ToggleWeapon (bool increase)
	{
		if (m_canChangeWeapon)
		{
			// Check if the current weapon is the first or last value
			if (m_currentWeapon == m_weaponTypes.First())
			{
				m_currentWeapon = m_weaponTypes.Last();
			}
			
			else if (m_currentWeapon == m_weaponTypes.Last())
			{
				m_currentWeapon = m_weaponTypes.First();
			}
			
			else
			{
				m_currentWeapon = increase ? ++m_currentWeapon : --m_currentWeapon;
			}

			UpdateWeapon();
			StartCoroutine (WeaponChangeCooldown());
		}
	}


	public bool AddAmmo (WeaponType ammoType, int amount)
	{
		// Pre-condition: ammoType is actually adding ammo
		if (ammoType == WeaponType.None)
		{
			Debug.LogError ("Attempt to add ammo of no type to PlayerInventory.");
			return false;
		}

		// Pre-condition: amount is more than or equal to 0, otherwise what is the point?
		if (amount < 0)
		{
			Debug.LogWarning ("Adding negative amount of ammo to: " + ammoType);
		}

		switch (ammoType)
		{
			case WeaponType.Pistol:
				return m_pistol.AddAmmo (amount);
			// More weapons added here
		}

		return false;
	}
}
