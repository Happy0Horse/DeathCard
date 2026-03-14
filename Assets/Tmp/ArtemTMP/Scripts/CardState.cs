using UnityEngine;

public class CardState : MonoBehaviour
{
    [SerializeField] private GameObject stateA;
    [SerializeField] private GameObject stateB;

    public void SetState(bool isStateA)
    {
        if (stateA) stateA.SetActive(isStateA);
        if (stateB) stateB.SetActive(!isStateA);
    }
}
