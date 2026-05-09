using UnityEngine;

public class DomeInitializer : MonoBehaviour
{
    private static SacrificeDome _pendingFirstDome;

    public static void RegisterFirstDome(SacrificeDome dome)
    {
        _pendingFirstDome = dome;
        TryInitialize();
    }

    private static void TryInitialize()
    {
        if (_pendingFirstDome == null) return;
        if (GameManager.Instance == null) return;

        int round = GameManager.Instance.GetCurrentRound();
        SkipDomes(_pendingFirstDome, round);
        _pendingFirstDome = null;
    }

    private static void SkipDomes(SacrificeDome first, int round)
    {
        SacrificeDome current = first;
        while (current != null)
        {
            if (current.domeIndex < round)
            {
                SacrificeDome next = current.innerDome;
                current.MarkSkipped();
                Object.Destroy(current.gameObject);
                current = next;
            }
            else
            {
                current.isFirstDome = true;
                current.EnableDome();
                break;
            }
        }
    }
}