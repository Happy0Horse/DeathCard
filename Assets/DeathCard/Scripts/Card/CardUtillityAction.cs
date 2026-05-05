using System.Collections;
using UnityEngine;

public class CardUtillityAction : MonoBehaviour
{
    [Header("Galaxy Settings")]
    public Material galaxyMaterial;
    public float growthSpeed = 0.5f;
    public float returnSpeed = 0.2f;
    public float stayDuration = 5.0f;

    [Header("Heal Settings")]
    public GameObject healEffectPrefab;

    [Header("Testing")]
    public bool stunSelfForTest = true;

    private Coroutine _activeRoutine;
    private HexGridNavigator _navigator;
    private DebuffSystem _myDebuffs;
    private PlayerStat _myStats;
    private bool _internalMoving;

    private void Awake()
    {
        _navigator = GetComponent<HexGridNavigator>();
        _myDebuffs = GetComponent<DebuffSystem>();
        _myStats = GetComponent<PlayerStat>();
        ResetGalaxy();
    }

    public void Execute(CardData data)
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);

        switch (data.utilityType)
        {
            case PlayerAnimation.UtilityType.GalaxyVoid:
                _activeRoutine = StartCoroutine(GalaxySequence(data));
                break;
            case PlayerAnimation.UtilityType.Heal:
                _activeRoutine = StartCoroutine(HealSequence(data));
                break;
        }
    }

    private IEnumerator HealSequence(CardData data)
    {
        if (_myStats != null)
        {
            _myStats.Heal(data.damage);

            if (healEffectPrefab != null)
            {
                GameObject effect = Instantiate(healEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
        }

        yield return new WaitForSeconds(0.5f);
        _activeRoutine = null;
    }

    private IEnumerator GalaxySequence(CardData data)
    {
        if (GameManager.Instance != null) GameManager.Instance.SetTimerFreeze(true);

        float totalStunTime = (1f / growthSpeed) + stayDuration + (1f / returnSpeed);
        ApplyGlobalStun(totalStunTime);

        yield return StartCoroutine(AnimateProperty(1f, growthSpeed));

        float elapsed = 0;
        while (elapsed < stayDuration)
        {
            elapsed += Time.deltaTime;

            bool canInitiate = !_internalMoving && !_navigator.IsMoving;
            bool isActuallyStunned = _myDebuffs != null && _myDebuffs.IsStunned;

            if (canInitiate && !isActuallyStunned)
            {
                bool selectionResolved = false;

                _navigator.BeginSelection(data.range, (targetCell) =>
                {
                    if (targetCell != null)
                    {
                        StartCoroutine(HandleNavigatorMove(targetCell));
                    }
                    selectionResolved = true;
                });

                while (!selectionResolved && elapsed < stayDuration)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
            yield return null;
        }

        _navigator.ClearSelectionState();
        yield return StartCoroutine(AnimateProperty(0f, returnSpeed));

        if (GameManager.Instance != null) GameManager.Instance.SetTimerFreeze(false);
        _activeRoutine = null;
    }

    private IEnumerator HandleNavigatorMove(HexCell target)
    {
        _internalMoving = true;
        _navigator.MoveTo(target);

        yield return new WaitUntil(() => !_navigator.IsMoving);

        _internalMoving = false;
    }

    private void ApplyGlobalStun(float duration)
    {
        DebuffSystem[] allPlayers = FindObjectsByType<DebuffSystem>(FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            if (p == _myDebuffs && !stunSelfForTest) continue;
            p.AddDebuff(DebuffSystem.DebuffType.Stun, duration);
        }
    }

    private IEnumerator AnimateProperty(float target, float speed)
    {
        float current = galaxyMaterial.GetFloat("_Growth");
        while (!Mathf.Approximately(current, target))
        {
            current = Mathf.MoveTowards(current, target, Time.deltaTime * speed);
            galaxyMaterial.SetFloat("_Growth", current);
            yield return null;
        }
    }

    private void ResetGalaxy()
    {
        if (galaxyMaterial != null) galaxyMaterial.SetFloat("_Growth", 0f);
    }

    private void OnApplicationQuit() => ResetGalaxy();
}