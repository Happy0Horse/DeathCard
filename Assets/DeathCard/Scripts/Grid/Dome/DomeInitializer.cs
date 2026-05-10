using UnityEngine;
using Mirror;

public class DomeInitializer : MonoBehaviour
{
    private static SacrificeDome _pendingFirstDome;

    public static void RegisterFirstDome(SacrificeDome dome)
    {
        _pendingFirstDome = dome;
    }

    public static void InitializeForRound(int round)
    {
        if (_pendingFirstDome == null) return;
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