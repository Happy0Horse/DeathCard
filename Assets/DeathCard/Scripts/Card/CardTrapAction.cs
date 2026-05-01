using UnityEngine;
using static PlayerAnimation;

public class CardTrapAction : MonoBehaviour
{
    [Header("Trap Materials")]
    public Material spikeTrapMaterial;
    public Material explosiveMaterial;
    public Material stunMaterial;

    private CardData _lastRequestedCard;

    public void RequestTrap(CardData data) => _lastRequestedCard = data;

    public void Execute(HexCell target)
    {
        if (_lastRequestedCard == null) return;
        Debug.Log("Created a trap");

        GameObject trapObj = new GameObject($"Trap_{_lastRequestedCard.trapType}");
        trapObj.transform.position = target.transform.position + Vector3.up * 1.5f;

        BoxCollider col = trapObj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1.5f, 1f, 1.5f);

        PlacedTrap trapLogic = trapObj.AddComponent<PlacedTrap>();
        Material selectedMat = GetMaterial(_lastRequestedCard.trapType);

        trapLogic.Initialize(
            _lastRequestedCard.trapType,
            _lastRequestedCard.damage,
            _lastRequestedCard.range,
            target,
            selectedMat
        );

        _lastRequestedCard = null;
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