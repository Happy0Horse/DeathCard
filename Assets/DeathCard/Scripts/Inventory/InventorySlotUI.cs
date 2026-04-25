//using UnityEngine;
//using UnityEngine.EventSystems;

//public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
//{
//    public ItemData item;
//    public ContextMenuUI contextMenu;

//    public void OnPointerClick(PointerEventData eventData)
//    {
//        Debug.Log("1");
//        if (item == null) return;
//        Debug.Log("2");
//        if (eventData.button == PointerEventData.InputButton.Right)
//        {
//            Debug.Log("Right click on: " + item.name);
//            contextMenu.Show(item);
//        }
//    }
//}