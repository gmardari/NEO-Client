using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EntryListener : MonoBehaviour
{


    void Awake()
    {
        
    }


    public void OnPointerClick(BaseEventData data)
    {
        if (data is PointerEventData pointerData)
        {
            if (!pointerData.dragging)
            {
                if (pointerData.button == PointerEventData.InputButton.Left)
                {
                    ChestInvManager.Singleton.OnEntryClick((uint)transform.GetSiblingIndex());
                }
            }
        }


    }
}
