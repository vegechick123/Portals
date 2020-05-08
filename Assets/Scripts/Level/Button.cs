using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    // Start is called before the first frame update
    public PortalBlock portalBlock;
    public PortalBlock portalBlock2;
    new private Rigidbody  rigidbody;
    Vector3 originPositon;
    float height;
    float tick = 1;
    bool down;
    virtual public void Start()
    {
        height = 0;
        down = false;
        rigidbody = GetComponent<Rigidbody>();
        originPositon = rigidbody.position;
    }
    virtual public void FixedUpdate()
    {
        tick += Time.fixedDeltaTime;
        if(tick>0.1&&down)
        {
            down = false;
            OutTouch();
        }
        if (down)
            height = Mathf.Clamp(height -0.3f * Time.deltaTime, -0.09f, 0);
        else
            height = Mathf.Clamp(height + 0.3f*Time.deltaTime, -0.09f, 0);
        rigidbody.MovePosition(originPositon +height*transform.up);
    }
    virtual public void EnterTouch()
    {
        portalBlock.Destory();
        if (portalBlock2 != null)
            portalBlock2.Destory();
    }
    virtual public void OutTouch()
    {
        portalBlock.Restore();
        if (portalBlock2 != null)
            portalBlock2.Restore();
    }
    public void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Cube")
        {
            tick = 0;
            if (down != true)
            {
                down = true;
                EnterTouch();
            }
        }
    }
}
