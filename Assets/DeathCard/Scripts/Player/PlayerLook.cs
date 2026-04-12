using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class PlayerLook : NetworkBehaviour
{
    public Transform cameraTransform;
    public float mouseSensitivity = 200f;

    private Vector2 lookInput;
    private float xRotation = 0f;

    void Start()
    {
        if (!isLocalPlayer) return;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        Look();
    }

    void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
}