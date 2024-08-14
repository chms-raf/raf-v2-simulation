using UnityEngine;
using UnityEngine.EventSystems;

public class UIDebugger : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("UI Element Clicked: " + gameObject.name);
    }
}
