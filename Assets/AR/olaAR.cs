using UnityEngine;
using Vuforia;
using TMPro;
using System.Collections.Generic;

public class LotteryScanner : MonoBehaviour
{
    public GameObject[] imageTargets;  // Las 5 imágenes de objetivo (ImageTargets)
    public TMP_Text messageText;       // El texto donde se mostrará el mensaje de victoria
    public TMP_Text scanCountText;     // Texto para mostrar cuántas imágenes se han escaneado

    private HashSet<string> scannedImageNames = new HashSet<string>(); // Para trackear qué imágenes ya se escanearon

    void Start()
    {
        // Inicializa el mensaje al principio
        messageText.text = "Escanea las 5 imágenes para ganar.";
        
        // Inicializa el texto del contador
        scanCountText.text = "Imágenes escaneadas: 0/5";
        
        // Desactivar el mensaje de victoria al inicio
        messageText.enabled = false;
    }

    void Update()
    {
        // Verificar si hay nuevas imágenes escaneadas
        CheckImagesScanned();
    }

    // Función para verificar y registrar imágenes escaneadas
    void CheckImagesScanned()
    {
        foreach (GameObject target in imageTargets)
        {
            ImageTargetBehaviour targetBehaviour = target.GetComponent<ImageTargetBehaviour>();
            
            // Si la imagen está siendo trackeada y aún no la hemos registrado
            if (targetBehaviour != null && targetBehaviour.TargetStatus.Status == Status.TRACKED)
            {
                string targetName = target.name;
                
                // Si no está en el conjunto, la agregamos
                if (!scannedImageNames.Contains(targetName))
                {
                    scannedImageNames.Add(targetName);
                    Debug.Log("Nueva imagen escaneada: " + targetName);
                }
            }
        }

        // Actualizar el texto que muestra cuántas imágenes se han escaneado
        scanCountText.text = "Imágenes escaneadas: " + scannedImageNames.Count + "/5";

        // Si las 5 imágenes han sido escaneadas, mostramos el mensaje de victoria
        if (scannedImageNames.Count == imageTargets.Length)
        {
            ShowVictoryMessage();
        }
    }

    // Mostrar el mensaje cuando se escanean todas las imágenes
    void ShowVictoryMessage()
    {
        messageText.text = "¡Orale, qué padre bro, ganaste!";
        messageText.enabled = true;  // Hacer visible el mensaje de victoria
    }
}