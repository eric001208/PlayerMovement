using UnityEngine;

[CreateAssetMenu]
public class PlayerStats : ScriptableObject
{
    [Header("LAYERS")]
    [Tooltip("Set this to the layer your player is on")]
    public LayerMask PlayerLayer;

    [Header("MOVEMENT")]
    [Tooltip("The speed at which the player moves.")]
    public float _moveSpeed = 5f;
    public float _maxSpeed = 10f;
    public float _acceleration = 80f;
    public float _deceleration = 50f;
    [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
    public float GroundingForce = -1.5f;
    [Tooltip("The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
    public float GrounderDistance = 0.05f;

    [Header("JUMPING")]
    [Tooltip("The force applied when the player jumps.")]
    public float _jumpForce = 20f;
    public float _CoyoteTime = 0.15f;
    public float FallAcceleration = 110;
    public float JumpEndEarlyGravityModifier = 3;
    public float MaxFallSpeed = 20;
    
    [Header("WALL JUMPING")]
    public float WallJumpForceY = 25f;
    public float WallJumpForceX = 10f;

    [Header("WALLSLIDING")]
    public float WallSlideSpeed = -1.5f;
    public float WallSlideDuration = 1f;

    [Header("DASHING")]
    public float DashPower = 35f;
    public float DashDuration = 0.2f;
}
