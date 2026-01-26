using UnityEngine;

public class GeneradorCircuitos : MonoBehaviour
{
    [Header("Prefabs de pista")]
    public PiezaCircuito[] trackPrefabs; // prefabs (recta, curvas, etc.)
    public int numberOfPieces = 10;      // número de piezas a generar

    [Header("Opciones de colocación")]
    public int maxAttempts = 10;         // intentos antes de rendirse

    [Header("Organización")]
    public Transform circuitoParent;     // Empty "Circuito"

    private PiezaCircuito lastPiece;     // última pieza colocada

    void Start()
    {
        GenerateTrack();
    }

    void GenerateTrack()
    {
        if (trackPrefabs.Length == 0) return;

        if (circuitoParent == null)
        {
            Debug.LogError("No se ha asignado el Circuito Parent.");
            return;
        }

        // 1️⃣ Colocar la primera pieza en el origen
        lastPiece = Instantiate(trackPrefabs[0], circuitoParent);
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

                PiezaCircuito prefab =
                    trackPrefabs[Random.Range(0, trackPrefabs.Length)];

                // 1️⃣ Instancia temporal (desactivada)
                PiezaCircuito newPiece =
                    Instantiate(prefab, circuitoParent);

                newPiece.gameObject.SetActive(false);

                // 2️⃣ Alinear con la pieza anterior
                AlignPiece(newPiece, lastPiece);

                // 3️⃣ Comprobar solapamiento
                if (CanPlacePiece(newPiece))
                {
                    newPiece.gameObject.SetActive(true);
                    lastPiece = newPiece;
                    placed = true;
                }
                else
                {
                    Destroy(newPiece);
                }
            }

            // Si no se puede colocar tras varios intentos, se termina
            if (!placed)
            {
                Debug.LogWarning(
                    "No se pudo colocar una nueva pieza. Circuito terminado antes de lo esperado."
                );
                break;
            }
        }

        CleanDisabledPieces();
    }

    void AlignPiece(PiezaCircuito newPiece, PiezaCircuito previousPiece)
    {
        // Dirección de salida y entrada (plano horizontal)
        Vector3 prevDir = previousPiece.puntoSalida.right;
        Vector3 newDir = newPiece.puntoEntrada.right;

        prevDir.y = 0;
        newDir.y = 0;

        prevDir.Normalize();
        newDir.Normalize();

        // Calcular solo rotación en Y
        float angleY = Vector3.SignedAngle(newDir, prevDir, Vector3.up);

        // Aplicar rotación
        newPiece.transform.Rotate(Vector3.up, angleY, Space.World);

        // Ajustar posición
        Vector3 offset =
            previousPiece.puntoSalida.position -
            newPiece.puntoEntrada.position;

        newPiece.transform.position += offset;
    }

    bool CanPlacePiece(PiezaCircuito newPiece)
    {
        Collider newCollider = newPiece.GetComponent<Collider>();
        if (newCollider == null) return true;

        Vector3 center = newCollider.bounds.center;
        Vector3 size = newCollider.bounds.size;
        Quaternion rot = newPiece.transform.rotation;

        Collider[] hits = Physics.OverlapBox(center, size / 2, rot);

        foreach (var hit in hits)
        {
            // Ignorar colisiones con la propia pieza
            if (!hit.transform.IsChildOf(newPiece.transform))
            {
                return false;
            }
        }

        return true;
    }

    void CleanDisabledPieces()
    {
        for (int i = circuitoParent.childCount - 1; i >= 0; i--)
        {
            Transform child = circuitoParent.GetChild(i);

            if (!child.gameObject.activeSelf)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
