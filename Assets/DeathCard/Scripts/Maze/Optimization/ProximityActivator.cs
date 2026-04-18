using UnityEngine;

public class ProximityActivator : MonoBehaviour
{
    public enum TargetFeature { Visuals, Shadows }
    public TargetFeature feature;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Decoration"))
        {
            var culler = other.GetComponent<DecorationCuller>();
            if (culler == null) return;

            if (feature == TargetFeature.Visuals) culler.SetVisible(true);
            else culler.SetShadows(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Decoration"))
        {
            var culler = other.GetComponent<DecorationCuller>();
            if (culler == null) return;

            if (feature == TargetFeature.Visuals) culler.SetVisible(false);
            else culler.SetShadows(false);
        }
    }
}