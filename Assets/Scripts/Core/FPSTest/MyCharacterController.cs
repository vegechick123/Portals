using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCharacterController : MonoBehaviour
{
    public bool isGrounded;
    private Rigidbody rigibody;
    private Collider colider;
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if(collision.rigidbody.position.y < rigibody.position.y);
    //    isGrounded = true;
    //}
    private void Awake()
    {
        rigibody = GetComponent<Rigidbody>();
    }
    //private void FixedUpdate()
    //{
    //    Physics.CapsuleCast()
    //}
    // Start is called before the first frame update
    public CollisionFlags Move(Vector3 distance)
    {
        //rigibody.MovePosition(rigibody.position+distance);
        return CollisionFlags.Above;
    }
}
