using UnityEngine;


/// <summary>
/// A base class for all pickups that exist in the game for the player.
/// </summary>
[RequireComponent (typeof (Collider))]
public abstract class PlayerPickup : MonoBehaviour 
{
	// Unity modifiable values
	[SerializeField] protected Explosion m_specialEffect;				// The Explosion to leave behind upon picking up the health
	[SerializeField] protected AudioClip m_audioOnCollect;				// The audio to play on trigger by the player
	[SerializeField, Range (0f, 1f)] protected float m_volume = 0.5f;


	// Use this for initialization
	protected virtual void Awake()
	{
		collider.isTrigger = true;
	}


	protected abstract void OnTriggerEnter (Collider other);


	protected void DestroySelf()
	{
		if (m_specialEffect)
		{
			Instantiate (m_specialEffect, transform.position, transform.rotation);
		}
		
		if (m_audioOnCollect)
		{
			AudioSource.PlayClipAtPoint (m_audioOnCollect, transform.position, m_volume);
		}
		
		Destroy (gameObject);
	}
}
