using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 4f;
    [SerializeField, Min(0f)] private float jumpForce = 6f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField, Min(0.01f)] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Animator Parameters")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string jumpParam = "Jump";

    private Rigidbody _rb;
    private Animator _animator;
    private Vector3 _moveInput;
    private bool _jumpRequested;
    private bool _isGrounded;
    private readonly Collider[] _groundHits = new Collider[12];

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (groundCheck == null)
        {
            groundCheck = transform;
        }
    }

    private void Update()
    {
        Vector2 move = ReadMoveInput();
        _moveInput = new Vector3(move.x, 0f, move.y).normalized;

        _isGrounded = CheckGrounded();

        if (ReadJumpPressed() && _isGrounded)
        {
            _jumpRequested = true;
        }

        Vector3 planarVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _animator.SetFloat(speedParam, planarVelocity.magnitude);
        _animator.SetBool(jumpParam, !_isGrounded);
    }

    private void FixedUpdate()
    {
        Vector3 desiredVelocity = _moveInput * moveSpeed;
        Vector3 currentVelocity = _rb.linearVelocity;
        _rb.linearVelocity = new Vector3(desiredVelocity.x, currentVelocity.y, desiredVelocity.z);

        if (_jumpRequested)
        {
            _jumpRequested = false;
            Vector3 velocity = _rb.linearVelocity;
            velocity.y = 0f;
            _rb.linearVelocity = velocity;
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    private bool CheckGrounded()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            groundCheck.position,
            groundCheckRadius,
            _groundHits,
            groundMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _groundHits[i];
            if (hit == null)
            {
                continue;
            }

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private Vector2 ReadMoveInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            float hKeys = 0f;
            float vKeys = 0f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) hKeys -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) hKeys += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vKeys -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vKeys += 1f;

            if (Mathf.Abs(hKeys) > Mathf.Abs(h))
            {
                h = hKeys;
            }

            if (Mathf.Abs(vKeys) > Mathf.Abs(v))
            {
                v = vKeys;
            }
        }
#endif

        return new Vector2(Mathf.Clamp(h, -1f, 1f), Mathf.Clamp(v, -1f, 1f));
    }

    private bool ReadJumpPressed()
    {
        bool jumpPressed = Input.GetButtonDown("Jump");

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpPressed = true;
        }
#endif

        return jumpPressed;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
