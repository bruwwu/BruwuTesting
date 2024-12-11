using UnityEngine;

public class Floater : MonoBehaviour
{
  public Rigidbody rigidBody;
public float depthBeforeSubmerged = 1f;
public float displacementAmount = 3f;
public int floaterCount = 1;
public float waterDrag = 0.99f;
public float waterAngularDrag = 0.5f;

private void FixedUpdate()
{
    rigidBody.AddForceAtPosition(Physics.gravity / floaterCount, transform.position, ForceMode.Acceleration);

    float waveHeight = WaterSim.instance.GetWaveHeight(transform.position.x);

  if (transform.position.y < waveHeight)
{
    float displacementMultiplier = Mathf.Clamp01((waveHeight - transform.position.y) / depthBeforeSubmerged) * displacementAmount;

    // Empuje hacia arriba (reducido para olas suaves)
    Vector3 upForce = new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f);
    rigidBody.AddForceAtPosition(upForce, transform.position, ForceMode.Acceleration);

    // Arrastre (más suave)
    Vector3 dragForce = displacementMultiplier * -rigidBody.velocity * waterDrag;
    rigidBody.AddForce(dragForce, ForceMode.Acceleration);

    // Arrastre angular (más lento para olas suaves)
    Vector3 angularDragForce = displacementMultiplier * -rigidBody.angularVelocity * waterAngularDrag;
    rigidBody.AddTorque(angularDragForce, ForceMode.Acceleration);
}

}

}
