using System.Collections;
using UnityEngine;

public class CardUtillityAction : MonoBehaviour
{

    [Header("Galaxy Settings")]
    public Material galaxyMaterial;
    public float growthSpeed = 0.5f;
    public float returnSpeed = 0.2f;
    public float stayDuration = 2.0f;

    private Coroutine _activeRoutine;

    private void Awake()
    {
        ResetGalaxy();
    }

    private void OnApplicationQuit()
    {
        ResetGalaxy();
    }

    public void Execute(CardData data)
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);

        switch (data.utilityType)
        {
            case PlayerAnimation.UtilityType.GalaxyVoid:
                _activeRoutine = StartCoroutine(GalaxySequence());
                break;
            case PlayerAnimation.UtilityType.Heal:
                Debug.Log("Heal logic would go here");
                break;
        }
    }

    private IEnumerator GalaxySequence()
    {
        yield return StartCoroutine(AnimateProperty(1f, growthSpeed));
        yield return new WaitForSeconds(stayDuration);
        yield return StartCoroutine(AnimateProperty(0f, returnSpeed));
        _activeRoutine = null;
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
}