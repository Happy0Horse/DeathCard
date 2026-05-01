using UnityEngine;
using static PlayerAnimation;

public class CardTrapAction : MonoBehaviour
{
    [Header("Trap Prefabs")]
    public GameObject spikeTrapPrefab;
    public GameObject explosivePrefab;
    public GameObject stunPrefab;

    [Header("Trap Materials")]
    public Material spikeTrapMaterial;
    public Material explosiveMaterial;
    public Material stunMaterial;

    private CardData _lastRequestedCard;

    public void RequestTrap(CardData data) => _lastRequestedCard = data;

    public void Execute(HexCell target)
    {
        if (_lastRequestedCard == null) return;

        float hexSurfaceY = 0;
        if (target.TryGetComponent(out MeshRenderer mesh))
        {
            hexSurfaceY = mesh.bounds.extents.y;
        }

        GameObject trapObj = new GameObject($"Trap_{_lastRequestedCard.trapType}");
        trapObj.transform.position = target.transform.position + new Vector3(0, hexSurfaceY, 0);

        BoxCollider col = trapObj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1.5f, 1.5f, 1.5f);
        col.center = new Vector3(0, 0.75f, 0);

        PlacedTrap trapLogic = trapObj.AddComponent<PlacedTrap>();

        trapLogic.Initialize(
            _lastRequestedCard.trapType,
            _lastRequestedCard.damage,
            _lastRequestedCard.effectiveRange,
            _lastRequestedCard.effectDuration,
            target,
            GetPrefab(_lastRequestedCard.trapType),
            GetMaterial(_lastRequestedCard.trapType)
        );

        _lastRequestedCard = null;
    }

    private GameObject GetPrefab(PlayerAnimation.TrapType type)
    {
        switch (type)
        {
            case PlayerAnimation.TrapType.Spikes: return spikeTrapPrefab;
            case PlayerAnimation.TrapType.Explosive: return explosivePrefab;
            case PlayerAnimation.TrapType.Stun: return stunPrefab;
            default: return null;
        }
    }

    private Material GetMaterial(PlayerAnimation.TrapType type)
    {
        switch (type)
        {
            case PlayerAnimation.TrapType.Spikes: return spikeTrapMaterial;
            case PlayerAnimation.TrapType.Explosive: return explosiveMaterial;
            case PlayerAnimation.TrapType.Stun: return stunMaterial;
            default: return spikeTrapMaterial;
        }
    }
}