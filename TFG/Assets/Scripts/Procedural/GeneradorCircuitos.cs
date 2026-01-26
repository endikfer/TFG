using System.Collections.Generic;
using UnityEngine;

public class GeneradorCircuitos : MonoBehaviour
{
    [Header("Prefabs de pista")]
    public PiezaCircuito[] trackPrefabs; // prefabs (recta, curvas, etc.)
    public int numberOfPieces = 20;

    [Header("Opciones de colocación")]
    public int maxAttempts = 10;

    [Header("Organización")]
    public Transform circuitoParent;

    [Header("Reglas de curvas")]
    public int maxConsecutivas = 2;     // Máximo de curvas seguidas del mismo lado
    public int maxConCurvasIntercaladas = 4; // Máximo de curvas hacia el mismo lado contando rectas

    [Header("Probabilidades dinámicas")]
    [Range(0, 1)] public float baseRectaProb = 0.7f;
    [Range(0, 1)] public float baseCurvaIzqProb = 0.15f;
    [Range(0, 1)] public float baseCurvaDerProb = 0.15f;
    //[Range(0, 1)] public float reduccionPorRepeticion = 0.5f; // factor de reducción

    private List<PiezaCircuito> piezasAsignadas = new List<PiezaCircuito>();

    void Start()
    {
        GenerateTrack();
    }

    void GenerateTrack()
    {
        if (trackPrefabs.Length == 0 || circuitoParent == null)
        {
            Debug.LogError("No se ha asignado trackPrefabs o circuitoParent.");
            return;
        }

        // Primera pieza
        PiezaCircuito firstPiece = Instantiate(trackPrefabs[0], circuitoParent);
        firstPiece.transform.position = Vector3.zero;
        firstPiece.transform.rotation = Quaternion.identity;
        piezasAsignadas.Add(firstPiece);

        for (int i = 1; i < numberOfPieces; i++)
        {
            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < maxAttempts)
            {
                attempts++;

                PiezaCircuito prefab = ChooseNextPiece();

                PiezaCircuito newPiece = Instantiate(prefab, circuitoParent);
                newPiece.gameObject.SetActive(false);

                AlignPiece(newPiece, piezasAsignadas[piezasAsignadas.Count - 1]);

                if (CanPlacePiece(newPiece))
                {
                    newPiece.gameObject.SetActive(true);
                    piezasAsignadas.Add(newPiece);
                    placed = true;
                }
                else
                {
                    Destroy(newPiece);
                }
            }

            if (!placed)
            {
                Debug.LogWarning("No se pudo colocar una nueva pieza. Circuito terminado antes de lo esperado.");
                break;
            }
        }

        CleanDisabledPieces();
    }

    // =================== ELECCIÓN DE PIEZA ===================
    PiezaCircuito ChooseNextPiece()
    {
        float rectaProb = baseRectaProb;
        float curvaIzqProb = baseCurvaIzqProb;
        float curvaDerProb = baseCurvaDerProb;

        // ==================================================
        // 1. NO MÁS DE maxConsecutivas CURVAS SEGUIDAS MISMO LADO
        // (bloque continuo FINAL, no acumulado)
        // ==================================================
        int consecIzq = 0;
        int consecDer = 0;

        // Primero miramos la última pieza
        if (piezasAsignadas.Count > 0)
        {
            var lastTipo = piezasAsignadas[piezasAsignadas.Count - 1].tipo;

            if (lastTipo == PiezaCircuito.TipoPieza.CurvaIzquierda)
            {
                consecIzq = 1;

                for (int i = piezasAsignadas.Count - 2; i >= 0; i--)
                {
                    if (piezasAsignadas[i].tipo == PiezaCircuito.TipoPieza.CurvaIzquierda)
                        consecIzq++;
                    else
                        break;
                }
            }
            else if (lastTipo == PiezaCircuito.TipoPieza.CurvaDerecha)
            {
                consecDer = 1;

                for (int i = piezasAsignadas.Count - 2; i >= 0; i--)
                {
                    if (piezasAsignadas[i].tipo == PiezaCircuito.TipoPieza.CurvaDerecha)
                        consecDer++;
                    else
                        break;
                }
            }
        }

        if (consecIzq >= maxConsecutivas) curvaIzqProb = 0f;
        if (consecDer >= maxConsecutivas) curvaDerProb = 0f;

        // ==================================================
        // 2. NO MÁS DE maxConCurvasIntercaladas CURVAS MISMO LADO
        // (contando rectas intercaladas)
        // ==================================================
        int totalIzq = 0;
        int totalDer = 0;

        for (int i = piezasAsignadas.Count - 1; i >= 0; i--)
        {
            if (piezasAsignadas[i].tipo == PiezaCircuito.TipoPieza.CurvaIzquierda)
                totalIzq++;
            else if (piezasAsignadas[i].tipo == PiezaCircuito.TipoPieza.CurvaDerecha)
                totalDer++;

            if (totalIzq >= maxConCurvasIntercaladas ||
                totalDer >= maxConCurvasIntercaladas)
                break;
        }

        if (totalIzq >= maxConCurvasIntercaladas) curvaIzqProb = 0f;
        if (totalDer >= maxConCurvasIntercaladas) curvaDerProb = 0f;

        // ==================================================
        // 3. PROBABILIDADES DINÁMICAS (histórico completo)
        // ==================================================
        int rectas = 0;
        int izq = 0;
        int der = 0;

        foreach (var p in piezasAsignadas)
        {
            if (p.tipo == PiezaCircuito.TipoPieza.Recta) rectas++;
            else if (p.tipo == PiezaCircuito.TipoPieza.CurvaIzquierda) izq++;
            else if (p.tipo == PiezaCircuito.TipoPieza.CurvaDerecha) der++;
        }

        // Bases para reducción exponencial (puedes ajustar)
        float baseRectaFactor = 0.7f; // suave para rectas
        float baseCurvaFactor = 0.4f;  // agresiva para curvas

        if (rectaProb > 0f) rectaProb *= Mathf.Pow(baseRectaFactor, rectas);
        if (curvaIzqProb > 0f) curvaIzqProb *= Mathf.Pow(baseCurvaFactor, izq);
        if (curvaDerProb > 0f) curvaDerProb *= Mathf.Pow(baseCurvaFactor, der);

        // ==================================================
        // 4. NORMALIZACIÓN SEGURA
        // ==================================================
        float total = rectaProb + curvaIzqProb + curvaDerProb;

        if (total <= 0f)
        {
            rectaProb = 1f;
            curvaIzqProb = curvaDerProb = 0f;
            total = 1f;
        }

        rectaProb /= total;
        curvaIzqProb /= total;
        curvaDerProb /= total;

        // ==================================================
        // 5. ELECCIÓN FINAL
        // ==================================================
        float r = Random.value;
        float acc = rectaProb;

        PiezaCircuito.TipoPieza chosen =
            r <= acc ? PiezaCircuito.TipoPieza.Recta :
            r <= (acc += curvaIzqProb) ? PiezaCircuito.TipoPieza.CurvaIzquierda :
            PiezaCircuito.TipoPieza.CurvaDerecha;

        foreach (var prefab in trackPrefabs)
            if (prefab.tipo == chosen)
                return prefab;

        return trackPrefabs[0];
    }



    // =================== ALINEACIÓN ===================
    void AlignPiece(PiezaCircuito newPiece, PiezaCircuito previousPiece)
    {
        Vector3 prevDir = previousPiece.puntoSalida.right;
        Vector3 newDir = newPiece.puntoEntrada.right;

        prevDir.y = 0;
        newDir.y = 0;

        prevDir.Normalize();
        newDir.Normalize();

        float angleY = Vector3.SignedAngle(newDir, prevDir, Vector3.up);
        newPiece.transform.Rotate(Vector3.up, angleY, Space.World);

        Vector3 offset = previousPiece.puntoSalida.position - newPiece.puntoEntrada.position;
        newPiece.transform.position += offset;
    }

    // =================== SOLAPAMIENTO ===================
    bool CanPlacePiece(PiezaCircuito newPiece)
    {
        BoxCollider[] newColliders = newPiece.GetComponents<BoxCollider>();
        Collider[] placedColliders = circuitoParent.GetComponentsInChildren<Collider>();

        foreach (var colA in newColliders)
        {
            if (!colA.enabled) continue;

            foreach (var colB in placedColliders)
            {
                if (colB.transform == newPiece.transform || !colB.enabled) continue;

                if (Physics.ComputePenetration(
                    colA, colA.transform.position, colA.transform.rotation,
                    colB, colB.transform.position, colB.transform.rotation,
                    out Vector3 dir, out float dist))
                {
                    return false;
                }
            }
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
