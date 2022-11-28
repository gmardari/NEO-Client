using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour
{

    private Camera cam;
    public float camYOffset;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(EOManager.player != null)
        {
            Vector3 playerPos = EOManager.player.transform.position;
            Vector3 camPos = cam.transform.position;
            cam.transform.position = new Vector3(playerPos.x, playerPos.y + camYOffset, camPos.z);
        }
    }
}
