using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GeneradorCircuitoSimple : MonoBehaviour
{
    public PiezaCircuito[] piezas;
    public Transform parent;

    public float distanciaObjetivo = 200f;
    public float margenDistancia = 5f;

    public float margenCierre = 2f;
    public float margenAngulo = 10f;

    public int maxIntentosPorPieza = 10;
    public int maxPiezas = 200;
    public int operacionesPorFrame = 20;

    List<PiezaCircuito> colocadas = new();

    void Start()
    {
        StartCoroutine(Generar());
    }

    IEnumerator Generar()
    {
        // -------- PIEZA INICIAL --------
        PiezaCircuito first = Instantiate(piezas[0], parent);
        first.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        colocadas.Add(first);

        float distancia = first.longitud;
        Vector3 startPos = first.puntoEntrada.position;
        Vector3 startDir = first.puntoEntrada.right;

        int ops = 0;

        // -------- CRECIMIENTO --------
        while (distancia < distanciaObjetivo - margenDistancia &&
               colocadas.Count < maxPiezas)
        {
            bool placed = false;

            for (int i = 0; i < maxIntentosPorPieza && !placed; i++)
            {
                PiezaCircuito prefab = piezas[Random.Range(0, piezas.Length)];
                PiezaCircuito p = Instantiate(prefab, parent);
                p.gameObject.SetActive(false);

                AlignPiece(p, colocadas[^1]);

                if (CanPlace(p))
                {
                    p.gameObject.SetActive(true);
                    colocadas.Add(p);
                    distancia += p.longitud;
                    placed = true;
                }
                else
                    Destroy(p.gameObject);
            }

            if (!placed) break;

            if (++ops >= operacionesPorFrame)
            {
                ops = 0;
                yield return null;
            }
        }

        // -------- INTENTO DE CIERRE --------
        for (int intentos = 0; intentos < 50; intentos++)
        {
            PiezaCircuito prefab = piezas[Random.Range(0, piezas.Length)];
            PiezaCircuito p = Instantiate(prefab, parent);
            p.gameObject.SetActive(false);

            AlignPiece(p, colocadas[^1]);

            if (CanPlace(p) && IsClosed(p, startPos, startDir))
            {
                p.gameObject.SetActive(true);
                colocadas.Add(p);
                Debug.Log("✅ Circuito cerrado");
                yield break;
            }

            Destroy(p.gameObject);
            yield return null;
        }

        Debug.LogWarning("❌ No se pudo cerrar el circuito");
    }

    // -------- GEOMETRÍA --------
    bool IsClosed(PiezaCircuito p, Vector3 pos, Vector3 dir)
    {
        float d = Vector3.Distance(p.puntoSalida.position, pos);
        float a = Vector3.Angle(p.puntoSalida.right, dir);
        return d <= margenCierre && a <= margenAngulo;
    }

    void AlignPiece(PiezaCircuito n, PiezaCircuito prev)
    {
        Vector3 prevDir = prev.puntoSalida.right;
        Vector3 newDir = n.puntoEntrada.right;

        float angle = Vector3.SignedAngle(newDir, prevDir, Vector3.up);
        n.transform.Rotate(Vector3.up, angle, Space.World);

        n.transform.position +=
            prev.puntoSalida.position - n.puntoEntrada.position;
    }

    bool CanPlace(PiezaCircuito p)
    {
        Collider[] newCols = p.GetComponentsInChildren<Collider>();
        Collider[] oldCols = parent.GetComponentsInChildren<Collider>();

        foreach (var a in newCols)
            foreach (var b in oldCols)
            {
                if (a.transform == b.transform) continue;

                if (Physics.ComputePenetration(
                    a, a.transform.position, a.transform.rotation,
                    b, b.transform.position, b.transform.rotation,
                    out _, out _))
                    return false;
            }

        return true;
    }
}
