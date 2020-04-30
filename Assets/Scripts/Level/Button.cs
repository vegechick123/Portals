using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    // Start is called before the first frame update
    public PortalBlock portalBlock;
    new private Rigidbody  rigidbody;
    Vector3 originPositon;
    float height;
    bool down;
    private void Start()
    {
        height = 0;
        down = false;
        rigidbody = GetComponent<Rigidbody>();
        originPositon = rigidbody.position;
    }
    private void Update()
    {
        if (down)
            height = Mathf.Clamp(height - Time.deltaTime, 0, 1);
        else
            height = Mathf.Clamp(height + Time.deltaTime, 0, 1);
        rigidbody.MovePosition(originPositon + new Vector3(0, height,0));
    }
    private void OnTriggerEnter(Collider other)
    {
        down = true;
        portalBlock.Destory();
    }
    private void OnTriggerExit(Collider other)
    {
        down = true;
        portalBlock.Restore();
    }
}
