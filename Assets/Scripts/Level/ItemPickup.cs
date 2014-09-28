using UnityEngine;


/// <summary>
/// This script is used to allow for item pickups which the player can collect, they apply an effect to the player which is deteremined
/// through enumerations.
/// </summary>
[RequireComponent (typeof (Collider))]
public sealed class ItemPickup : PlayerPickup
{
	// Unity modifiable values
	[SerializeField] private ItemType m_itemType = ItemType.Ammo;		// The type of item pickup
	[SerializeField] private WeaponType m_weaponType = WeaponType.None;	// The weapon to enable

	[SerializeField, Range (-10000, 10000)] private int m_value = 6;	// How much the pickup effects the player


	// Member variables
	private PlayerInventory m_inventory;



	// Functions
	protected override void Awake()
	{
		collider.isTrigger = true;
		m_inventory = GameObject.FindGameObjectWithTag (Tags.player).GetComponent<PlayerInventory>();

		if (!m_inventory)
		{
			Debug.LogError ("Unable to determine ItemPickup: .m_inventory.");
		}
	}


	protected override void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag (Tags.player))
		{
			bool destroy = true;

			switch (m_itemType)
			{
				case ItemType.Ammo:
					destroy = AddAmmo();
					break;

				case ItemType.Collectible:
					// TODO: Add collectibles to the game	
					break;

				case ItemType.Key:
					// TODO: Add keys and locked doors to the game
					break;

				case ItemType.Skill:
					// TODO: Add obtainable skills in the game
					break;

				case ItemType.Weapon:
					EnableWeapon();
					break;
			}
			
			if (destroy)
			{
				DestroySelf();
			}
		}
	}


	private bool AddAmmo()
	{
		return m_inventory.AddAmmo (m_weaponType, m_value);
	}


	private void EnableWeapon()
	{
		switch (m_weaponType)
		{
			case WeaponType.None:
				Debug.LogError ("Unable to enable WeaponType.None in PlayerInventory");
				break;

			case WeaponType.Pistol:
				m_inventory.pistolEnabled = true;
				break;
		}

		AddAmmo();
	}
}
