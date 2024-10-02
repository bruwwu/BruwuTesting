using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidad")]
    public float MoveSpeed;
    private CharacterController characterController;
    public float newPosition;
    [Header("Lerp")]
    public float targetZ;
    public float lerpVelocity = 10f;


    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        targetZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, 0); 
        
        characterController.Move(move * Time.deltaTime * MoveSpeed);

         if(Input.GetKeyDown(KeyCode.W)){
            Vector3 newPos = transform.position;

            if(newPos.z < 2f){
            targetZ += 2.2f; //sumar 1.5 a Z, la gran diferencia fue poner el + xd
            }
            
        }
        else if(Input.GetKeyDown(KeyCode.S)){
            Vector3 newPos = transform.position;

            if(newPos.z > -2f){
            targetZ -= 2.2f; // restarle 1.5 a Z
            }
        }
        Vector3 currentPosition = transform.position;
        currentPosition.z = Mathf.Lerp(currentPosition.z, targetZ, Time.deltaTime * lerpVelocity);
        transform.position = currentPosition;
    }

    /*IEnumerator coolDownW(float waitTime = 0.6f){
        
    }*/
}