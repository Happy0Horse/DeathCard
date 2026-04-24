using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class PlayerAttack : NetworkBehaviour
{
    public Transform cameraTransform;
    public float attackDistance = 3f;

    public void OnAttack(InputValue value)
    {
        if (!isLocalPlayer) return;
        if (!value.isPressed) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackDistance))
        {
            Infectable obj = hit.collider.GetComponent<Infectable>();
            if (obj != null)
                CmdInfect(obj.gameObject);
        }
    }

    [Command]
    void CmdInfect(GameObject target)
    {
        Infectable obj = target.GetComponent<Infectable>();
        if (obj != null)
            obj.Infect();
    }
}