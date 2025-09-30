using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreePlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    public float MoveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0,Input.GetAxis("Vertical"));
        Vector3 moveVector = transform.TransformDirection(move) * MoveSpeed;
        rb.linearVelocity = moveVector * Time.deltaTime * MoveSpeed;
    }
}
