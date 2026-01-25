using UnityEngine;

public class GeneradorCircuitos : MonoBehaviour
{
    [Header("Prefabs de pista")]
    public PiezaCircuito[] trackPrefabs; // tus prefabs (recta, curva izquierda, curva derecha)
    public int numberOfPieces = 10;   // número de piezas a generar

    [Header("Opciones de colocación")]
    public int maxAttempts = 10;      // intentos antes de rendirse si no se puede colocar

    private PiezaCircuito lastPiece;     // última pieza colocada

    void Start()
    {
        GenerateTrack();
    }

    void GenerateTrack()
    {
        if (trackPrefabs.Length == 0) return;

        // 1️⃣ Coloca la primera pieza en el origen
        lastPiece = Instantiate(trackPrefabs[0]);
        lastPiece.transform.position = Vector3.zero;
        lastPiece.transform.rotation = Quaternion.identity;

        // 2️⃣ Generar las siguientes piezas
        for (int i = 1; i < numberOfPieces; i++)
        {
            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < maxAttempts)
            {
                attempts++;

                // Elegir prefab aleatorio
                PiezaCircuito newPiece = Instantiate(trackPrefabs[Random.Range(0, trackPrefabs.Length)]);

                // Alinear con la pieza anterior
                AlignPiece(newPiece, lastPiece);

                // Comprobar solapamiento
                if (CanPlacePiece(newPiece))
                {
                    lastPiece = newPiece;
                    placed = true;
                }
                else
                {
                    Destroy(newPiece); // descartar si colisiona
                }
            }

            // Si tras varios intentos no se coloca, terminamos el circuito
            if (!placed)
            {
                Debug.LogWarning("No se pudo colocar una nueva pieza. Circuito terminado antes de llegar al número deseado.");
                break;
            }
        }
    }

    void AlignPiece(PiezaCircuito newPiece, PiezaCircuito previousPiece)
    {
        // Rotación
        Quaternion rot = previousPiece.puntoSalida.rotation * Quaternion.Inverse(newPiece.puntoEntrada.rotation);
        newPiece.transform.rotation = rot * newPiece.transform.rotation;

        // Posición
        Vector3 offset = previousPiece.puntoSalida.position - newPiece.puntoEntrada.position;
        newPiece.transform.position += offset;
    }

    bool CanPlacePiece(PiezaCircuito newPiece)
    {
        Collider newCollider = newPiece.GetComponent<Collider>();
        if (newCollider == null) return true; // sin collider no hay solapamiento

        Vector3 center = newCollider.bounds.center;
        Vector3 size = newCollider.bounds.size;
        Quaternion rot = newPiece.transform.rotation;

        // Comprueba si hay colisión con cualquier otro collider en la escena
        Collider[] hits = Physics.OverlapBox(center, size / 2, rot);
        foreach (var hit in hits)
        {
            // Ignorar colisiones con la propia pieza
            if (!hit.transform.IsChildOf(newPiece.transform))
            {
                return false; // hay solapamiento
            }
        }

        return true; // espacio libre
    }
}
