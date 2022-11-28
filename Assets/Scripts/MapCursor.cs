using EO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapCursor : MonoBehaviour
{
    //public UIController uiController;
    //private GameObject map;

    private SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        //map = GameObject.FindWithTag("Map");


        renderer.enabled = false;
    }

    //TODO: Don't show cursor if above UI
    // Update is called once per frame
    void Update()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        Vector2Int cellPos = EOManager.eo_map.WorldToCell(worldPos);

        Vector2Int? c = null;

        if (EOManager.eo_map.IsLoaded && !EOManager.eo_map.OutOfMapBounds(cellPos) && !EventSystem.current.IsPointerOverGameObject())
        {
            c = cellPos;
            renderer.enabled = true;
            transform.position = EOManager.eo_map.CellToWorld(cellPos);

            if(Input.GetMouseButtonDown(0))
            {
                Cell cell = EOManager.eo_map.GetCell(c.Value);

                if (cell != null)
                {
                    Item item = cell.PopItem();
                    if(item != null)
                    {
                        //TODO: Implement pick up item method
                        //EOManager.Singleton.PickupItem(item);
                    }
                }
            }
           
        }
        else
        {
            renderer.enabled = false;

        }

        EOManager.eo_map.cursorPos = c;
        //uiController.SetCursorPosition(c);

    }
}
