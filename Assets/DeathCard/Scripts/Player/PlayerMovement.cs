using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float standHeight = 2.0f;
    [SerializeField] private float crouchSpeed = 8f;

    public Transform cameraTransform;
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    public bool invertMovement = false;

    public bool canCrouch = false;
    private float targetHeight;

    public float maxStamina = 5f;
    public float stamina;
    public float staminaDrain = 1f;
    public float staminaRecovery = 2f;
    public float jumpStaminaCost = 1.5f;

    public Slider staminaSlider;

    public float staminaRecoveryDelay = 1.5f;

    private float staminaTimer;


    private CharacterController controller;
    private Transform _root;

    private Vector2 moveInput;
    private float yVelocity;
    private bool isSprinting;

    private bool showStaminaBar;
    private bool isCrouching = false;

    private Vector3 airVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        _root = transform.root;

        stamina = maxStamina;

        if (!isLocalPlayer)
        {
            if (staminaSlider != null)
                staminaSlider.gameObject.SetActive(false);
            return;
        }

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = stamina;
        staminaSlider.gameObject.SetActive(false);

        targetHeight = standHeight;
        controller.height = standHeight;
    }

    private void Awake()
    {
        canCrouch = LocalStorage.Instance.canCrouch;
    }
    void Update()
    {
        if (!isLocalPlayer) return;

        Move();
        UpdateStaminaUI();

        if (isCrouching)
        {
            controller.height = Mathf.Lerp(controller.height, crouchHeight, Time.deltaTime * crouchSpeed);
            controller.center = new Vector3(0, controller.height / 2f, 0);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, standHeight, Time.deltaTime * crouchSpeed);
            if (controller.height < 2)
                controller.center = new Vector3(0, 0, 0);
        }


    }

    void Move()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 inputDirection = (right * moveInput.x + forward * moveInput.y).normalized;

        bool isGrounded = controller.isGrounded;

        if (isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;

        Vector3 horizontalVelocity;

        bool canSprint = isSprinting && stamina > 0 && moveInput.magnitude > 0;

        if (isGrounded)
        {
            float currentSpeed = canSprint ? sprintSpeed : speed;
            horizontalVelocity = inputDirection * currentSpeed;
            airVelocity = horizontalVelocity;
        }
        else
        {
            horizontalVelocity = airVelocity;
        }

        if (canSprint && isGrounded)
        {
            stamina -= staminaDrain * Time.deltaTime;
            staminaTimer = staminaRecoveryDelay;
            showStaminaBar = true;
        }
        else
        {
            if (staminaTimer > 0)
                staminaTimer -= Time.deltaTime;
            else
                stamina += staminaRecovery * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        if (stamina <= 0) isSprinting = false;
        if (stamina >= maxStamina) showStaminaBar = false;

        Vector3 velocity = horizontalVelocity;
        velocity.y = yVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateStaminaUI()
    {
        staminaSlider.value = stamina;
        staminaSlider.gameObject.SetActive(showStaminaBar);
    }

    public void OnMove(InputValue value)
    {
        if (!isLocalPlayer) return;
        moveInput = value.Get<Vector2>();
        if (invertMovement) moveInput = -moveInput;
    }

    public void OnSprint(InputValue value)
    {
        if (!isLocalPlayer) return;
        isSprinting = value.isPressed;
        if (isSprinting) showStaminaBar = true;
    }

    public void OnJump(InputValue value)
    {
        if (!isLocalPlayer) return;

        if (value.isPressed && controller.isGrounded && stamina >= jumpStaminaCost)
        {
            yVelocity = jumpForce;
            stamina -= jumpStaminaCost;
            staminaTimer = staminaRecoveryDelay;
            showStaminaBar = true;

            float currentSpeed = isSprinting ? sprintSpeed : speed;
            airVelocity = (cameraTransform.right * moveInput.x + cameraTransform.forward * moveInput.y).normalized * currentSpeed;
            airVelocity.y = 0f;
        }
    }
    
    public void OnCrouch(InputValue value)
    {
        if (canCrouch)
        {
            if (value.isPressed)
            {
                if (!isCrouching)
                    StartCrouch();
                else
                    StopCrouch();

                isCrouching = !isCrouching;
            }
        }
    }
    private void StartCrouch()
    {
        targetHeight = crouchHeight;
    }

    private void StopCrouch()
    {
        float checkDistance = standHeight - controller.height;

        Vector3 rayOrigin = transform.position + Vector3.up * controller.height;
        if (Physics.Raycast(rayOrigin, Vector3.up, checkDistance + 0.1f))
        {
            isCrouching = false;
            targetHeight = standHeight;
        }
    }
}