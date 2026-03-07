using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Ya NO se asigna desde el Inspector: se carga dinámicamente cuando el agente está listo
    private Transform carTransform;

    [Range(1, 10)]
    public float followSpeed = 2;
    [Range(1, 10)]
    public float lookSpeed = 5;

    public Vector3 offset = new Vector3(0f, 5f, -10f);

    private void Start()
    {
        // Si el agente ya existe en escena, enlazarlo directamente
        if (Agente1_0.Instance != null)
            OnAgentReady();
        else
            Agente1_0.OnAgentReady += OnAgentReady;
    }

    private void OnDestroy()
    {
        Agente1_0.OnAgentReady -= OnAgentReady;
    }

    private void OnAgentReady()
    {
        carTransform = Agente1_0.Instance.transform;
        Agente1_0.OnAgentReady -= OnAgentReady;

        Debug.Log("[CameraFollow] Transform del coche cargado dinámicamente.");
    }

    void LateUpdate()
    {
        if (carTransform == null) return;

        // 1️⃣ Calcular la posición objetivo detrás del coche
        Vector3 targetPosition = carTransform.position + carTransform.TransformDirection(offset);

        // 2️⃣ Mover la cámara suavemente hacia esa posición
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // 3️⃣ Hacer que la cámara mire siempre al coche
        Quaternion targetRotation = Quaternion.LookRotation(carTransform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }
}