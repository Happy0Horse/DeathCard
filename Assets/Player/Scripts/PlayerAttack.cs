using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public Transform cameraTransform;
    public float attackDistance = 3f;

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackDistance))
        {
            Infectable obj = hit.collider.GetComponent<Infectable>();

            if (obj != null)
            {
                obj.Infect();
            }
        }
    }
}