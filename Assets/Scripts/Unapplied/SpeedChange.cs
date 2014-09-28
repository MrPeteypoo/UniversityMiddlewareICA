using UnityEngine;



/// <summary>
/// A generic class designed to be a composite of another class/struct to provide a clean way of speeding a process up in a configurable
/// manner. SpeedChange can also be used to slow a process down.
/// </summary>
[System.Serializable]
public sealed class SpeedChange
{
	public bool changeSpeed = false;					// Increase speed when margin is surpassed
	[Range (0.001f, 100f)] public float margin = 1f;	// The margin at which the speed change should occur
	[Range (0.001f, 100f)] public float factor = 1f;	// How much to speed up by
}
