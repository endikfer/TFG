using System.Collections.Generic;
using UnityEngine;

public class GeneradorCircuitos : MonoBehaviour
{
    [Header("Prefabs de pista")]
    public PiezaCircuito[] trackPrefabs;
    public int numberOfPieces = 20;

    [Header("Opciones de colocación")]
    public int maxAttempts = 10;

    [Header("Organización")]
    public Transform circuitoParent;

    [Header("Reglas de curvas")]
    public int maxConsecutivas = 2;
    public int maxConCurvasIntercaladas = 4;

    [Header("Probabilidades dinámicas")]
    [Range(0, 1)] public float baseRectaProb = 0.7f;
    [Range(0, 1)] public float baseCurvaIzqProb = 0.15f;
    [Range(0, 1)] public float baseCurvaDerProb = 0.15f;

    [Header("Cierre del circuito")]
    public int piezasParaCerrar = 5;
    public int maxRetriesCierre = 10;

    private Vector3 startPos;
    private Vector3 startDir;

    private List<PiezaCircuito> piezasAsignadas = new List<PiezaCircuito>();

    void Start()
    {
        GenerateTrack();
    }

    // =================== GENERACIÓN ===================
    void GenerateTrack()
    {
        if (trackPrefabs.Length == 0 || circuitoParent == null)
        {
            Debug.LogError("No se ha asignado trackPrefabs o circuitoParent.");
            return;
        }

        // Primera pieza
        PiezaCircuito firstPiece = Instantiate(trackPrefabs[0], circuitoParent);
        firstPiece.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        piezasAsignadas.Add(firstPiece);

        startPos = firstPiece.puntoEntrada.position;
        startDir = firstPiece.puntoEntrada.right;

        bool circuitoCerrado = false; // ← Flag para mensajes
        int retriesCierre = 0;

        for (int i = 1; i < numberOfPieces; i++)
        {
            bool placed = false;
            int attempts = 0;

            bool estamosCerrando = i >= numberOfPieces - piezasParaCerrar;

            while (!placed && attempts < maxAttempts)
            {
                attempts++;

                PiezaCircuito prefab = estamosCerrando
                    ? ChooseClosingPiece()
                    : ChooseNextPiece();

                PiezaCircuito newPiece = Instantiate(prefab, circuitoParent);
                newPiece.gameObject.SetActive(false);

                AlignPiece(newPiece, piezasAsignadas[^1]);

                if (CanPlacePiece(newPiece))
                {
                    if (i == numberOfPieces - 1)
                    {
                        // Comprobamos si la última pieza realmente cierra el circuito
                        if (!CanCloseCircuit(newPiece))
                        {
                            Destroy(newPiece.gameObject);
                            continue;
                        }
                        else
                        {
                            circuitoCerrado = true; // ✅ Marcamos que el circuito se cerró
                        }
                    }

                    newPiece.gameObject.SetActive(true);
                    piezasAsignadas.Add(newPiece);
                    placed = true;
                }
                else
                {
                    Destroy(newPiece.gameObject);
                }
            }

            if (!placed)
            {
                if (estamosCerrando && retriesCierre < maxRetriesCierre)
                {
                    retriesCierre++;
                    BacktrackCierre();
                    i = numberOfPieces - piezasParaCerrar - 1;
                    break;
                }

                Debug.LogWarning("No se pudo cerrar el circuito.");
                break;
            }
        }

        CleanDisabledPieces();

        // 🔹 Mensaje final por consola
        if (circuitoCerrado)
            Debug.Log("✅ El circuito se cerró correctamente.");
        else
            Debug.LogWarning("❌ No se pudo cerrar el circuito.");
    }


    // =================== ELECCIÓN NORMAL ===================
    PiezaCircuito ChooseNextPiece()
    {
        float rectaProb = baseRectaProb;
        float curvaIzqProb = baseCurvaIzqProb;
        float curvaDerProb = baseCurvaDerProb;

        int consecIzq = 0, consecDer = 0;

        if (piezasAsignadas.Count > 0)
        {
            var lastTipo = piezasAsignadas[^1].tipo;

            for (int i = piezasAsignadas.Count - 1; i >= 0; i--)
            {
                if (piezasAsignadas[i].tipo == PiezaCircuito.TipoPieza.CurvaIzquierda)
                {
                    if (lastTipo == PiezaCircuito.TipoPieza.CurvaIzquierda) consecIzq++;
                    else break;
                }
                else if (piezasAsignadas[i].tipo == PiezaCircuito.TipoPieza.CurvaDerecha)
                {
                    if (lastTipo == PiezaCircuito.TipoPieza.CurvaDerecha) consecDer++;
                    else break;
                }
                else break;
            }
        }

        if (consecIzq >= maxConsecutivas) curvaIzqProb = 0;
        if (consecDer >= maxConsecutivas) curvaDerProb = 0;

        int rectas = 0, izq = 0, der = 0;
        foreach (var p in piezasAsignadas)
        {
            if (p.tipo == PiezaCircuito.TipoPieza.Recta) rectas++;
            else if (p.tipo == PiezaCircuito.TipoPieza.CurvaIzquierda) izq++;
            else if (p.tipo == PiezaCircuito.TipoPieza.CurvaDerecha) der++;
        }

        rectaProb *= Mathf.Pow(0.7f, rectas);
        curvaIzqProb *= Mathf.Pow(0.4f, izq);
        curvaDerProb *= Mathf.Pow(0.4f, der);

        float total = rectaProb + curvaIzqProb + curvaDerProb;
        if (total <= 0) return trackPrefabs[0];

        float r = Random.value * total;

        PiezaCircuito.TipoPieza chosen =
            r < rectaProb ? PiezaCircuito.TipoPieza.Recta :
            r < rectaProb + curvaIzqProb ? PiezaCircuito.TipoPieza.CurvaIzquierda :
            PiezaCircuito.TipoPieza.CurvaDerecha;

        foreach (var prefab in trackPrefabs)
            if (prefab.tipo == chosen)
                return prefab;

        return trackPrefabs[0];
    }

    // =================== ELECCIÓN DE CIERRE ===================
    PiezaCircuito ChooseClosingPiece()
    {
        PiezaCircuito best = null;
        float bestError = float.MaxValue;
        float currentError = ComputeCurrentClosingError();

        foreach (var prefab in trackPrefabs)
        {
            if (!IsPieceAllowed(prefab)) continue;

            PiezaCircuito test = Instantiate(prefab);
            test.gameObject.SetActive(false);

            AlignPiece(test, piezasAsignadas[^1]);

            if (!CanPlacePiece(test))
            {
                Destroy(test.gameObject);
                continue;
            }

            float error = ComputeClosingError(test);
            if (error > currentError) error += 5f;

            if (error < bestError)
            {
                bestError = error;
                best = prefab;
            }

            Destroy(test.gameObject);
        }

        return best ?? ChooseNextPiece();
    }

    bool IsPieceAllowed(PiezaCircuito prefab)
    {
        return true; // extensible más adelante
    }

    float ComputeClosingError(PiezaCircuito piece)
    {
        float dist = Vector3.Distance(piece.puntoSalida.position, startPos);
        float angle = Vector3.Angle(piece.puntoSalida.right, startDir);
        return dist + angle * 0.5f;
    }

    float ComputeCurrentClosingError()
    {
        PiezaCircuito last = piezasAsignadas[^1];
        return ComputeClosingError(last);
    }

    bool CanCloseCircuit(PiezaCircuito last)
    {
        float dist = Vector3.Distance(last.puntoSalida.position, startPos);
        float angle = Vector3.Angle(last.puntoSalida.right, startDir);
        return dist < 0.5f && angle < 10f;
    }

    void BacktrackCierre()
    {
        int borrar = Mathf.Min(piezasParaCerrar, piezasAsignadas.Count - 1);

        for (int i = 0; i < borrar; i++)
        {
            int idx = piezasAsignadas.Count - 1;
            Destroy(piezasAsignadas[idx].gameObject);
            piezasAsignadas.RemoveAt(idx);
        }
    }

    // =================== ALINEACIÓN ===================
    void AlignPiece(PiezaCircuito newPiece, PiezaCircuito prev)
    {
        Vector3 prevDir = prev.puntoSalida.right;
        Vector3 newDir = newPiece.puntoEntrada.right;

        prevDir.y = newDir.y = 0;
        float angle = Vector3.SignedAngle(newDir, prevDir, Vector3.up);
        newPiece.transform.Rotate(Vector3.up, angle, Space.World);

        Vector3 offset = prev.puntoSalida.position - newPiece.puntoEntrada.position;
        newPiece.transform.position += offset;
    }

    // =================== SOLAPAMIENTO ===================
    bool CanPlacePiece(PiezaCircuito newPiece)
    {
        var newCols = newPiece.GetComponents<BoxCollider>();
        var placedCols = circuitoParent.GetComponentsInChildren<Collider>();

        foreach (var a in newCols)
            foreach (var b in placedCols)
            {
                if (!a.enabled || !b.enabled || b.transform == newPiece.transform)
                    continue;

                if (Physics.ComputePenetration(
                    a, a.transform.position, a.transform.rotation,
                    b, b.transform.position, b.transform.rotation,
                    out _, out _))
                    return false;
            }

        return true;
    }

    // =================== LIMPIEZA ===================
    void CleanDisabledPieces()
    {
        for (int i = circuitoParent.childCount - 1; i >= 0; i--)
        {
            Transform child = circuitoParent.GetChild(i);
            if (!child.gameObject.activeSelf)
                Destroy(child.gameObject);
        }
    }
}
