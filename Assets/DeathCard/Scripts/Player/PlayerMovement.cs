using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    public bool invertMovement = false;

    public float maxStamina = 5f;
    public float stamina;
    public float staminaDrain = 1f;
    public float staminaRecovery = 2f;
    public float jumpStaminaCost = 1.5f;

    public float staminaRecoveryDelay = 1.5f; 
    private float staminaTimer;

    public Slider staminaSlider;

    private CharacterController controller;

    private Vector2 moveInput;
    private float yVelocity;
    private bool isSprinting;

    private bool showStaminaBar;

    private Vector3 airVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        stamina = maxStamina;

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = stamina;

        staminaSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        Move();
        UpdateStaminaUI();
    }

    void Move()
    {
        Vector3 inputDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

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
            {
                staminaTimer -= Time.deltaTime;
            }
            else
            {
                stamina += staminaRecovery * Time.deltaTime;
            }
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        if (stamina <= 0)
            isSprinting = false;

        if (stamina >= maxStamina)
            showStaminaBar = false;

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
        moveInput = value.Get<Vector2>();

        if (invertMovement)
        {
            moveInput = -moveInput;
        }
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;

        if (isSprinting)
            showStaminaBar = true;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && controller.isGrounded && stamina >= jumpStaminaCost)
        {
            yVelocity = jumpForce;

            stamina -= jumpStaminaCost;
            staminaTimer = staminaRecoveryDelay;
            showStaminaBar = true;

            float currentSpeed = isSprinting ? sprintSpeed : speed;

            airVelocity = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized
                          * currentSpeed;
        }
    }
}