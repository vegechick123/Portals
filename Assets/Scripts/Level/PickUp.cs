using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    // Start is called before the first frame update
    new public Rigidbody rigidbody;
    public PortalPhysicsObject portalPhysicsObject;
    public Player player;
    private Quaternion originRotation;
    private Collider colider;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        portalPhysicsObject = GetComponent<PortalPhysicsObject>();
        colider = GetComponent<Collider>();
    }
    public void Hold(Player player)
    {
        colider.enabled = false;
        gameObject.SetActive(false);
        this.player = player;
        originRotation = player.transform.rotation;
    }
    public void Release(Vector3 position,Vector3 velocity,Quaternion nowRotation)
    {
        colider.enabled = true;
        gameObject.SetActive(true);
        
        rigidbody.MoveRotation(nowRotation* Quaternion.Inverse(originRotation)* rigidbody.rotation);
        rigidbody.MovePosition(position);
        rigidbody.velocity = velocity;
        if (player == null)
            return;
        player = null;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (player!= null)
        {
            player.holdObject = null;
            rigidbody.useGravity = true;
            player = null;
        }
    }
}
