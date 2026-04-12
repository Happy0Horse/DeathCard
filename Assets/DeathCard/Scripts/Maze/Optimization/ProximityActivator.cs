using UnityEngine;

public class ProximityActivator : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Sphere touched: " + other.name + " with tag: " + other.tag);
        if (other.CompareTag("Decoration"))
        {
            var culler = other.GetComponent<DecorationCuller>();
            if (culler != null) culler.SetVisible(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Decoration"))
        {
            var culler = other.GetComponent<DecorationCuller>();
            if (culler != null) culler.SetVisible(false);
        }
    }
}