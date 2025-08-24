using UnityEngine;

public class FootIKController : MonoBehaviour
{
    public Transform leftFootIK;         // Asigna el objeto "left_foot_ik" desde el Inspector.
    public Transform rightFootIK;        // Asigna el objeto "right_foot_ik" desde el Inspector.
    public Transform leftFootHint;       // Asigna el objeto "left_foot_ik_hint" desde el Inspector.
    public Transform rightFootHint;      // Asigna el objeto "right_foot_ik_hint" desde el Inspector.

    public LayerMask groundLayer;        // Capa del suelo para el raycast.
    public float footOffset = 0.1f;      // Altura desde el suelo.
    public float raycastDistance = 1.0f; // Distancia máxima del raycast.

    private void LateUpdate()
    {
        if (leftFootIK != null && rightFootIK != null)
        {
            AdjustFoot(leftFootIK, leftFootHint);
            AdjustFoot(rightFootIK, rightFootHint);
        }
    }

    private void AdjustFoot(Transform footIK, Transform footHint)
    {
        // Realizar un raycast desde el pie hacia abajo.
        if (Physics.Raycast(footIK.position + Vector3.up * raycastDistance, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            // Ajustar la posición del pie al suelo con un pequeño offset.
            Vector3 targetPosition = hit.point + Vector3.up * footOffset;
            footIK.position = targetPosition;

            // Ajustar la rotación del pie según la inclinación del suelo.
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(footIK.forward, hit.normal), hit.normal);
            footIK.rotation = targetRotation;

            // Ajustar el hint si está asignado.
            if (footHint != null)
            {
                Vector3 hintPosition = footHint.position;
                hintPosition.y = targetPosition.y;
                footHint.position = hintPosition;
            }
        }
    }
}
