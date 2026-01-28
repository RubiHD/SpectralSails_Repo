using UnityEngine;

public class FloatingPickup : MonoBehaviour
{
    public float floatAmplitude = 0.2f;   // Qué tanto sube y baja
    public float floatSpeed = 2f;         // Velocidad del movimiento
    public float rotationSpeed = 50f;     // Grados por segundo

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Movimiento vertical tipo "flotar"
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotación vertical (sobre el eje Y)
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
