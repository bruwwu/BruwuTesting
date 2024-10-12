using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidad")]
    public float MoveSpeed;
    private CharacterController characterController;
    public float newPosition;
    Vector3 playerInput;
    private float maxZValue = 3.0f;

    [Header("Jump")]
    public float gravityValue = -9.81f;
    private Vector3 playerVelocity;
    public float jumpHeight = 4f;
    bool groundedChar;

    [Header("Lerp")]
    public float targetZ = 0f;
    public float lerpVelocity = 10f;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Detección del jugador en el suelo
        groundedChar = characterController.isGrounded;

        // Detección del input del jugador
        playerInput = new Vector3(Input.GetAxis("Horizontal"), 0, 0).normalized;
        MovePlayer();  // Función de movimiento 
        ZSwitch();     // Función de cambio de profundidad en Z
        PlayerJump();  // Función de salto
    }

    void PlayerJump()
    {
        // Si el jugador está en el suelo y presiona espacio, saltar
        if (groundedChar && Input.GetKeyDown(KeyCode.Space))
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);  // Iniciar el salto
        }

        // Aplicar gravedad si no está en el suelo
        if (!groundedChar)
        {
            playerVelocity.y += gravityValue * Time.deltaTime;  // Añadir gravedad mientras esté en el aire
        }
        else if (playerVelocity.y < 0)
        {
            // Si está en el suelo y está cayendo, resetear la velocidad vertical
            playerVelocity.y = 0f;
        }

        // Mover el jugador en función de la velocidad vertical
        characterController.Move(playerVelocity * Time.deltaTime);
    }

    void MovePlayer()
    {
        // Movimiento en el plano X usando el CharacterController
        Vector3 moveVector = transform.TransformDirection(playerInput) * MoveSpeed;
        characterController.Move(moveVector * Time.deltaTime * MoveSpeed);
    }

    void ZSwitch()
    {
        // Detectar si el jugador quiere moverse hacia adelante en Z
        if (Input.GetKeyDown(KeyCode.W))
        {
            // Mover solo si estamos en 0 o -3, para no exceder 3
            if (targetZ == 0f || targetZ == -3f)
            {
                targetZ += 3f;  // Cambiar de capa a la siguiente en Z (0 -> 3 o -3 -> 0)
            }
        }
        // Detectar si el jugador quiere moverse hacia atrás en Z
        else if (Input.GetKeyDown(KeyCode.S))
        {
            // Mover solo si estamos en 0 o 3, para no exceder -3
            if (targetZ == 0f || targetZ == 3f)
            {
                targetZ -= 3f;  // Cambiar de capa a la anterior en Z (3 -> 0 o 0 -> -3)
            }
        }

        // Suavizar el movimiento en Z usando Lerp
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(transform.position.z, targetZ, Time.deltaTime * lerpVelocity));
        Vector3 moveDirection = targetPosition - transform.position;
        characterController.Move(new Vector3(0, 0, moveDirection.z));
    }
}
