using UnityEngine;

public class ObjectTeleporter : MonoBehaviour
{
    public Transform miniatureSpawnPoint;
    public float scaleRatio = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            other.transform.position = miniatureSpawnPoint.position;

            other.transform.localScale *= scaleRatio;

            if (rb != null)
            {
                rb.linearVelocity *= scaleRatio;
            }
        }
    }
}