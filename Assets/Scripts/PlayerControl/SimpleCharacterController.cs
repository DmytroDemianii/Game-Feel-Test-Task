using DG.Tweening;
using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");
    private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
    private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");
    private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
    private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");

    [Header("Components")]
    [SerializeField] private PlayerCollisionHandler _collisionHandler;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Transform _modelTransform;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _gravityMultiplier = 2f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Refined Movement")]
    [SerializeField] private float _acceleration = 25f;
    [SerializeField] private float _deceleration = 25f;
    [SerializeField] private float _airControl = 0.5f;

    [Header("Jump Feel Settings")]
    [SerializeField] private float _fallMultiplier = 4f;
    [SerializeField] private float _coyoteTime = 0.15f;
    [SerializeField] private float _jumpBufferTime = 0.15f;
    [SerializeField] private float _repulsionEffect = 1f;
    [SerializeField] private float _headBumpResetDelay = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _groundedOffset = -0.14f;
    [SerializeField] private float _groundedRadius = 0.18f;
    [SerializeField] private bool _isGrounded = true;

    private Vector3 _velocity;
    private float _speed2D;
    private Vector3 _moveDirection;
    private int _currentGait;
    private float _strafeDirectionX = 0f;
    private float _strafeDirectionZ = 1f;
    private bool _isWalking = false;
    private bool _isStopped = true;
    private bool _movementInputHeld = false;

    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;

    private void Start()
    {
        _collisionHandler.OnHeadBump += ResetVerticalVelocity;
        _inputReader.onJumpPerformed += OnJump;
    }

    private void Update()
    {
        UpdateTimers();
        GroundedCheck();
        CalculateMoveDirection();
        HandleJump();
        CheckIfStopped();
        FaceMoveDirection();
        ApplyGravity();
        Move();
        UpdateAnimator();
    }

    private void UpdateTimers()
    {
        if (_isGrounded)
        {
            _coyoteTimeCounter = _coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        if (_jumpBufferCounter > 0)
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void CalculateMoveDirection()
    {
        _moveDirection = new Vector3(_inputReader._moveComposite.x, 0f, 0f);
        _movementInputHeld = _moveDirection.magnitude > 0.01f;

        float targetSpeed = _moveDirection.x * _moveSpeed;
        float currentAccel = _isGrounded ? (_movementInputHeld ? _acceleration : _deceleration) : (_acceleration * _airControl);

        _velocity.x = Mathf.MoveTowards(_velocity.x, targetSpeed, currentAccel * Time.deltaTime);
        _velocity.z = 0f;

        _speed2D = Mathf.Abs(_velocity.x);
        CalculateGait();
    }

    private void HandleJump()
    {
        if (_jumpBufferCounter > 0 && _coyoteTimeCounter > 0)
        {
            _velocity.y = _jumpForce;
            _jumpBufferCounter = 0;
            _coyoteTimeCounter = 0;
            _animator.SetBool(_isJumpingAnimHash, true);
        }
    }

    private void OnJump()
    {
        _jumpBufferCounter = _jumpBufferTime;
    }

    private void ApplyGravity()
    {
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
        else
        {
            float currentGravityMult = _velocity.y < 0 ? _fallMultiplier : _gravityMultiplier;
            _velocity.y += Physics.gravity.y * currentGravityMult * Time.deltaTime;
        }

        if (_velocity.y <= 0f)
        {
            _animator.SetBool(_isJumpingAnimHash, false);
        }
    }

    private void ResetVerticalVelocity()
    {
        if (_velocity.y > 0)
        {
            _velocity.y = _repulsionEffect;
        }
    }

    private void CalculateGait()
    {
        if (_speed2D < 0.01f) 
        {
            _currentGait = 0;
        }
        else
        {
            _currentGait = 1;
        }
    }

    private void CheckIfStopped()
    {
        _isStopped = _moveDirection.magnitude == 0 && _speed2D < 0.5f;
        _isWalking = !_isStopped && _isGrounded;
    }

    private void FaceMoveDirection()
    {
        if (_modelTransform == null) return;

        if (_moveDirection.magnitude > 0.01f)
        {
            Vector3 faceDirection = new Vector3(_velocity.x, 0f, 0f);
            if (faceDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(faceDirection);
                _modelTransform.rotation = Quaternion.Slerp(_modelTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void Move()
    {
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(
            _controller.transform.position.x,
            _controller.transform.position.y - _groundedOffset,
            _controller.transform.position.z
        );
        _isGrounded = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayerMask, QueryTriggerInteraction.Ignore);
    }

    private void UpdateAnimator()
    {
        _animator.SetFloat(_moveSpeedHash, _speed2D);
        _animator.SetInteger(_currentGaitHash, _currentGait);
        _animator.SetBool(_isGroundedHash, _isGrounded);
        _animator.SetFloat(_strafeDirectionXHash, _strafeDirectionX);
        _animator.SetFloat(_strafeDirectionZHash, _strafeDirectionZ);
        _animator.SetFloat(_isStrafingHash, 0f);
        _animator.SetBool(_isWalkingHash, _isWalking);
        _animator.SetBool(_isStoppedHash, _isStopped);
        _animator.SetBool(_movementInputHeldHash, _movementInputHeld);
    }

    private void OnDestroy()
    {
        _collisionHandler.OnHeadBump -= ResetVerticalVelocity;
        _inputReader.onJumpPerformed -= OnJump;
    }
}