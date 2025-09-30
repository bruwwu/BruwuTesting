using UnityEngine;
using Vuforia;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public TMP_Text infoText;    // Texto que cambiará según el estado de Vuforia

    private bool isScanned = false;


    void Update()
    {
        // Verificar si un ImageTarget está siendo escaneado
        if (isScanned)
        {
            infoText.text = "¡Escaneaste la imagen!";
        }
        else
        {
            infoText.text = "Escanea una imagen de Vuforia para interactuar.";
        }
    }

    // Función que maneja el clic en el botón
    void OnButtonClick()
    {
        // Lógica para cuando se presiona el botón (puedes cambiar el estado de algo en Vuforia)
        Debug.Log("Botón presionado: Cambiando algo en Vuforia.");
    }

    // Este método puede ser llamado desde el ImageTargetBehaviour
    public void OnTargetFound()
    {
        isScanned = true;
    }

    // Este método puede ser llamado desde el ImageTargetBehaviour
    public void OnTargetLost()
    {
        isScanned = false;
    }
}
