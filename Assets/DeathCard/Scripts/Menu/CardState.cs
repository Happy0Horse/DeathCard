using UnityEngine;

public class CardState : MonoBehaviour
{
    [SerializeField] public GameObject stateA;
    [SerializeField] public GameObject stateB;

    public void SetState(bool isStateA)
    {
        if (stateA) stateA.SetActive(isStateA);
        if (stateB) stateB.SetActive(!isStateA);
    }
}
