using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PlayerControl : MonoBehaviour
{
    private InputActionAsset inputActions;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _wallSlideAction;
    [SerializeField] private PlayerStats _stats;



    private Rigidbody2D _rb;
    private CapsuleCollider2D _cd;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;

    private float _time;

    #region Start and Initialization
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _cd = GetComponent<CapsuleCollider2D>();

        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _dashAction = InputSystem.actions.FindAction("Dash");
        _wallSlideAction = InputSystem.actions.FindAction("WallSlide");

    }

    private void Update()
    {
        GatherInputActions();
        _time += Time.deltaTime;
    }

    private void GatherInputActions()
    {
        var _moveInput = _moveAction.ReadValue<Vector2>();
        var _jumpInput = _jumpAction.triggered;
        var _jumpHeld = _jumpAction.IsPressed();
        var _dashInput = _dashAction.triggered;
        var _wallSlideInput = _wallSlideAction.IsPressed();

        _frameInput = new FrameInput
        {
            Move = _moveInput,
            JumpDown = _jumpInput,
            JumpHeld = _jumpHeld,
            DashDown = _dashInput,
            WallHeld = _wallSlideInput

        };

        if (_frameInput.Move != Vector2.zero)
        {
            _lastDirection = _moveInput.normalized;

        }

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _wallJumpToConsume = true;
        }

        if (_frameInput.DashDown)
        {
            _dashConsume = true;
        }
    }

    #endregion

    #region Collisions


    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollisionEnter(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        HandleCollisionExit(collision);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollisionStay(collision);
    }

    #endregion

    #region Hadnle Collison
    private void HandleCollisionEnter(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            DetectCollisionDirection(collision);
            _dashConsume = false;
            if (_isgrounded)
            {
                _jumpToConsume = false;
                _canWallSlide = true;
            }
            if (_isLeftWall || _isRightWall)
            {
                _wallJumping = false;
                _wallJumpToConsume = false;
            }
        }
    }

    private void HandleCollisionExit(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            ResetCollisions();
            _frameLeftGrounded = _time;
            if (_isDashing) _dashCount = 0;
        }
    }

    private void HandleCollisionStay(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            DetectCollisionDirection(collision);
            _coyoteUsable = true;
            _jumpToConsume = false;
            if(_isgrounded) _dashCount = 1;
        }
    }
    #endregion

    #region Detect Collision Direction

    private bool _isgrounded;
    private bool _isRoofed;
    private bool _isLeftWall;
    private bool _isRightWall;
    private ContactPoint2D[] _contacts = new ContactPoint2D[10];
    private void DetectCollisionDirection(Collision2D collision)
    {
        int contactCount = collision.GetContacts(_contacts);
        for (int i = 0; i < contactCount; i++)
        {
            var contact = _contacts[i];
            if (contact.normal.y > 0.5f) _isgrounded = true;
            if (contact.normal.y < -0.5f) _isRoofed = true;
            if (contact.normal.x > 0.5f) _isRightWall = true;
            if (contact.normal.x < -0.5f) _isLeftWall = true;
        }
    }

    private void ResetCollisions()
    {
        _isgrounded = false;
        _isRoofed = false;
        _isLeftWall = false;
        _isRightWall = false;
        _issliding = false;
    }

    #endregion


    #region FixedUpdate
    private void FixedUpdate()
    {
        HandleJump();
        HandleGravity();
        HandleMovement();
        HandleDash();
        HandleWallSliding();
        HandleWallJump();

        ApplyMovement();
    }
    #endregion

    #region Jump

    private bool _jumpToConsume;
    private bool _coyoteUsable;
    private float _frameLeftGrounded;
    private bool CanUseCoyote => _coyoteUsable && !_isgrounded && _time < _frameLeftGrounded + _stats._CoyoteTime;
    private void HandleJump()
    {
        if (!_jumpToConsume) return;
        if (_isgrounded || CanUseCoyote) ExecuteJump();
    }

    private void ExecuteJump()
    {
        _coyoteUsable = false;
        _frameVelocity.y = _stats._jumpForce;
    }


    #endregion

    #region Wall Jump
    private bool _wallJumpToConsume;
    private bool _wallJumping;
    private void HandleWallJump()
    {
        if (!_wallJumpToConsume) return;
        if (!_isLeftWall && !_isRightWall) return;
        ExecuteWallJump();

    }

    private void ExecuteWallJump()
    {
        _wallJumping = true;
        if (_isLeftWall)
        {
            _frameVelocity.x = -_stats.WallJumpForceX;
        }
        else if (_isRightWall)
        {
            _frameVelocity.x = _stats.WallJumpForceX;
        }
        _frameVelocity.y = _stats.WallJumpForceY;
    }


    #endregion

    #region Wall Sliding

    private bool _issliding;
    private bool _canWallSlide;
    private void HandleWallSliding()
    {
        if (!_frameInput.WallHeld)
        {
            _issliding = false;
            return;
        }
        if (_issliding || !_canWallSlide) return;
        if (_isLeftWall || _isRightWall) ExcuteSlide();
        else return;
    }

    private void ExcuteSlide()
    {
        _canWallSlide = false;
        StartCoroutine(WallSlideDuration());
    }

    private IEnumerator WallSlideDuration()
    {
        _issliding = true;
        Debug.Log("Wall Slide Started");
        yield return new WaitForSeconds(_stats.WallSlideDuration);
        Debug.Log("Wall Slide Ended");
        _issliding = false;
    }


    #endregion

    #region Dash
    private int _dashCount;
    private bool _isDashing;
    private bool _dashConsume;
    private void HandleDash()
    {
        if (_isRoofed) return;
        if (!_dashConsume) return;
        if (_dashCount < 1) return;
        ExecuteDash();
    }

    private void ExecuteDash()
    {
        _dashCount--;
        _dashConsume = false;
        _isDashing = true;
        _frameVelocity = _lastDirection * _stats.DashPower;

        StartCoroutine(EndDash());
    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(_stats.DashDuration);
        _isDashing = false;
    }

    #endregion



    #region Movement
    private Vector2 _lastDirection;
    private void HandleMovement()
    {
        if (_frameInput.Move.x == 0)
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats._deceleration * Time.deltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats._moveSpeed, _stats._acceleration * Time.deltaTime);
        }
    }
    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if (_isgrounded && _frameVelocity.y <= 0f) _frameVelocity.y = _stats.GroundingForce;
        else if(_issliding) _frameVelocity.y = _stats.WallSlideSpeed;
        else
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_frameVelocity.y > 0f && !_frameInput.JumpHeld && !_isDashing && !_wallJumping)
            {
                inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            }
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion


    private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

    public struct FrameInput
    {
        public Vector2 Move;
        public bool JumpDown;
        public bool JumpHeld;
        public bool DashDown;
        public bool WallHeld;
    }



}
