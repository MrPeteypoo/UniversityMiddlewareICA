using UnityEngine;



/// <summary>
/// HashIDs contains every animator hash ID relevant. Use of this class increases efficiency by using hashes instead of string arguments.
/// </summary>
public sealed class HashIDs : MonoBehaviour 
{
	// States
	public readonly int crouchMotionState = Animator.StringToHash ("Base.CrouchMotion");
	public readonly int fallState = Animator.StringToHash ("Base.Fall");
	public readonly int jumpState = Animator.StringToHash ("Base.Jump");
	public readonly int midAirState = Animator.StringToHash ("Base.MidAir");
	public readonly int uprightMotionState = Animator.StringToHash ("Base.UprightMotion");
	public readonly int deadState = Animator.StringToHash ("Base.Dying");


	// Bools
	public readonly int aimingBool = Animator.StringToHash ("Aiming");
	public readonly int crouchingBool = Animator.StringToHash ("Crouching");
	public readonly int deadBool = Animator.StringToHash ("Dead");
	public readonly int fallingBool = Animator.StringToHash ("Falling");
	public readonly int groundedBool = Animator.StringToHash ("Grounded");
	public readonly int jumpingBool = Animator.StringToHash ("Jumping");
	public readonly int openBool = Animator.StringToHash ("Open");
	public readonly int resetBool = Animator.StringToHash ("Reset");
	public readonly int shootingBool = Animator.StringToHash ("Shooting");

	// Floats
	public readonly int speedFloat = Animator.StringToHash ("Speed");
}
