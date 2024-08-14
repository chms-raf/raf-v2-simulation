/// |-----------------------------------------Raycaster World-----------------------------------------------------|
///      Author: Kaden Wince
/// Description: This class overrides the normal GraphicRaycaster on the main canvas to send the raycast out of
///              the middle of the screen instead of where the cursor is located.
/// |-------------------------------------------------------------------------------------------------------------|
    
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaycasterWorld : GraphicRaycaster {
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) {
        // Set middle screen pos or you can set variable on start and use it
        eventData.position = new(Screen.width / 2, Screen.height / 2);
        base.Raycast(eventData, resultAppendList);
    }
}
