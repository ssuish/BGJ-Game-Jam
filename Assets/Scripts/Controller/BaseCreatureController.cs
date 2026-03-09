using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseCreatureController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction checkInAction;
    public float moveSpeed = 0.01f;
    private bool wasHoldingCheckIn;
    private float checkInHoldStartTime;
    private float checkInHoldDuration;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        checkInAction = playerInput.actions["Check In"];

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

    private void Update()
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
            Debug.Log($"Check In held for {checkInHoldDuration:F2} seconds. Tag: {gameObject.tag}", gameObject);
            OnCheckInStateChanged(false);
        }

        wasHoldingCheckIn = isHolding;
    }

    protected virtual void InitializeCharacters()
    {
        
    }

    protected virtual void UpdateMovement(Vector2 moveInput)
    {
        Vector2 position = (Vector2)transform.position + moveInput * moveSpeed * Time.deltaTime;
        transform.position = position;
    }

    protected virtual void OnCheckInStateChanged(bool isPressed)
    {

    }

    protected virtual void OnAbilityActivated(string abilityName)
    {
        
    }
}