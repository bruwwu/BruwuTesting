using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreePlayerMovement : MonoBehaviour
{
    CharacterController characterController;
    public float MoveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0,Input.GetAxis("Vertical"));
        Vector3 moveVector = transform.TransformDirection(move) * MoveSpeed;
        characterController.Move(moveVector * Time.deltaTime * MoveSpeed);
    }
}
