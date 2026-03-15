using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class BaseCreatureController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction checkInAction;
    public float moveSpeed = 3f;
    public float SpeedMultiplier { get; private set; } = 1f;
    private bool wasHoldingCheckIn;
    private float checkInHoldStartTime;
    private float checkInHoldDuration;
    private Rigidbody2D rb;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        checkInAction = playerInput.actions["Check In"];
        rb = GetComponent<Rigidbody2D>();

        InitializeCharacters();
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        checkInAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        checkInAction?.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        UpdateMovement(move);

        bool isHolding = checkInAction.IsPressed();

        if (isHolding && !wasHoldingCheckIn)
        {
            checkInHoldStartTime = Time.time;
            OnCheckInStateChanged(true);
        }

        if (!isHolding && wasHoldingCheckIn)
        {
            checkInHoldDuration = Time.time - checkInHoldStartTime;
            OnCheckInStateChanged(false);
        }

        wasHoldingCheckIn = isHolding;
    }

    protected virtual void InitializeCharacters()
    {

    }

    protected virtual void UpdateMovement(Vector2 moveInput)
    {
        Vector2 newPosition = rb.position + moveInput * moveSpeed * SpeedMultiplier * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        SpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    protected virtual void OnCheckInStateChanged(bool isPressed)
    {

    }

    protected virtual void OnAbilityActivated(string abilityName)
    {

    }
}