using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    // Start is called before the first frame update
    new public Rigidbody rigidbody;
    public PortalPhysicsObject portalPhysicsObject;
    public Player player;
    public Matrix4x4 toMatrix;
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
        toMatrix = transform.localToWorldMatrix;
    }
    public void Release(Vector3 position,Vector3 velocity)
    {
        colider.enabled = true;
        gameObject.SetActive(true);

        transform.rotation = toMatrix.rotation;
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
