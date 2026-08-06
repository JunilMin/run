using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MindfulRun.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        [Header("Horizontal")]
        [SerializeField] float maxSpeed = 8f;
        [SerializeField] float groundAcceleration = 70f;
        [SerializeField] float groundDeceleration = 85f;
        [SerializeField] float airAcceleration = 42f;
        [SerializeField] float airDeceleration = 30f;
        [Header("Jump")]
        [SerializeField] float jumpSpeed = 13.5f;
        [SerializeField] float coyoteTime = .12f;
        [SerializeField] float jumpBufferTime = .12f;
        [SerializeField] float jumpCutMultiplier = .45f;
        [SerializeField] float fallGravityMultiplier = 2.2f;
        [SerializeField] float maxFallSpeed = 20f;
        [Header("Grounding")]
        [SerializeField] LayerMask groundMask = ~0;
        [SerializeField] Vector2 groundCheckSize = new(.55f, .12f);
        [SerializeField] float groundCheckDistance = .05f;

        Rigidbody2D body;
        Collider2D bodyCollider;
        StickFigureAnimator figure;
        float moveInput, lastGroundedAt = float.NegativeInfinity, jumpPressedAt = float.NegativeInfinity;
        bool jumpHeld, controlsEnabled = true, wasGrounded;
        public bool IsGrounded { get; private set; }
        public bool ControlsEnabled => controlsEnabled;
        public float HorizontalInput => moveInput;
        public event Action<float> MovementInput;
        public event Action Jumped;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>(); bodyCollider = GetComponent<Collider2D>();
            figure = GetComponentInChildren<StickFigureAnimator>(); body.freezeRotation = true;
        }

        void Update()
        {
            ReadInput();
            var bounds = bodyCollider.bounds;
            IsGrounded = Physics2D.BoxCast(new Vector2(bounds.center.x, bounds.min.y + .03f), groundCheckSize, 0, Vector2.down, groundCheckDistance, groundMask);
            if (IsGrounded) lastGroundedAt = Time.time;
            if (controlsEnabled && Time.time - jumpPressedAt <= jumpBufferTime && Time.time - lastGroundedAt <= coyoteTime)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
                jumpPressedAt = float.NegativeInfinity; lastGroundedAt = float.NegativeInfinity; Jumped?.Invoke();
            }
            if (!jumpHeld && body.linearVelocity.y > 0f) body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * jumpCutMultiplier);
            figure?.SetMotion(body.linearVelocity, IsGrounded, IsGrounded && !wasGrounded);
            wasGrounded = IsGrounded;
        }

        void FixedUpdate()
        {
            float target = moveInput * maxSpeed;
            float rate = Mathf.Abs(moveInput) > .01f ? (IsGrounded ? groundAcceleration : airAcceleration) : (IsGrounded ? groundDeceleration : airDeceleration);
            float x = Mathf.MoveTowards(body.linearVelocity.x, target, rate * Time.fixedDeltaTime);
            float y = Mathf.Max(body.linearVelocity.y, -maxFallSpeed);
            body.gravityScale = y < 0f ? fallGravityMultiplier : 1f;
            body.linearVelocity = new Vector2(x, y);
        }

        void ReadInput()
        {
            if (!controlsEnabled || Keyboard.current == null) { moveInput = 0f; jumpHeld = false; return; }
            var keys = Keyboard.current;
            float next = (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1f : 0f) - (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1f : 0f);
            if (!Mathf.Approximately(next, moveInput) && Mathf.Abs(next) > .01f) MovementInput?.Invoke(next);
            moveInput = next; jumpHeld = keys.spaceKey.isPressed;
            if (keys.spaceKey.wasPressedThisFrame) jumpPressedAt = Time.time;
        }

        public void SetControlsEnabled(bool enabled) { controlsEnabled = enabled; if (!enabled) moveInput = 0f; }
        public void PlayDeath() { controlsEnabled = false; figure?.PlayDeath(); }
    }
}
