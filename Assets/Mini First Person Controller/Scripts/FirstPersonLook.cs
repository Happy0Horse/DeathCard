using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;

    public float sensitivity = 125f;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;

    void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Mouse.current == null)
            return;

        // Отримуємо рух миші (новий Input System)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // НОВИЙ Input System вже дає delta за кадр,
        // тому множимо на Time.deltaTime для стабільності
        Vector2 rawFrameVelocity = Vector2.Scale(
            mouseDelta * sensitivity * Time.deltaTime,
            Vector2.one
        );

        frameVelocity = Vector2.Lerp(
            frameVelocity,
            rawFrameVelocity,
            1f / smoothing
        );

        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90f, 90f);

        // Повороти
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }
}