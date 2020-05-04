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
    private void FixedUpdate()
    {
        if (down)
            height = Mathf.Clamp(height -0.3f * Time.deltaTime, -0.09f, 0);
        else
            height = Mathf.Clamp(height + 0.3f*Time.deltaTime, -0.09f, 0);
        rigidbody.MovePosition(originPositon +height*transform.up);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Cube")
        {
            down = true;
            portalBlock.Destory();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Cube")
        {
            down = false;
            portalBlock.Restore();
        }
    }
}
