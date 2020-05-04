using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    // Start is called before the first frame update
    new public Rigidbody rigibody;
    public PortalPhysicsObject portalPhysicsObject;
    public Player player;
    private void Awake()
    {
        rigibody = GetComponent<Rigidbody>();
        portalPhysicsObject = GetComponent<PortalPhysicsObject>();
    }
    public void Hold(Player player)
    {
        gameObject.SetActive(false);
        this.player = player;
    }
    public void Release(Vector3 position,Vector3 velocity)
    {
        gameObject.SetActive(true);
        rigibody.MovePosition(position);
        rigibody.velocity = velocity;
        if (player == null)
            return;
        player = null;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (player!= null)
        {
            player.holdObject = null;
            rigibody.useGravity = true;
            player = null;
        }
    }
}
