using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5f;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9f;

    Rigidbody rb;

    /// <summary>
    /// Functions to override movement speed. Will use the last added override.
    /// </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Keyboard.current == null)
            return;

        // ===== RUN INPUT =====
        IsRunning = canRun && Keyboard.current.leftShiftKey.isPressed;

        float targetMovingSpeed = IsRunning ? runSpeed : speed;

        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // ===== MOVEMENT INPUT =====
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.aKey.isPressed) moveX -= 1f;
        if (Keyboard.current.dKey.isPressed) moveX += 1f;
        if (Keyboard.current.sKey.isPressed) moveY -= 1f;
        if (Keyboard.current.wKey.isPressed) moveY += 1f;

        Vector2 inputVector = new Vector2(moveX, moveY).normalized;

        Vector3 move = new Vector3(
            inputVector.x * targetMovingSpeed,
            rb.linearVelocity.y,
            inputVector.y * targetMovingSpeed
        );

        rb.linearVelocity = transform.rotation * move;
    }
}