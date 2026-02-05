using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class Generador : MonoBehaviour
{
    public PiezaCircuito piezaInicial;
    public PiezaCircuito[] piezas;
    public Transform circuitoParent;

    public float distanciaObjetivo = 200f;
    public float margenDistancia = 5f;


    private Vector3 startPos;
    private Vector3 startDir;


    private List<PiezaCircuito> piezasAsignadas = new List<PiezaCircuito>();



    void Start()
    {
        StartCoroutine(Generar());
    }

    IEnumerator Generar()
    {
        if (piezas.Length == 0 || circuitoParent == null || piezaInicial == null)
        {
            Debug.LogError("No se ha asignado trackPrefabs, circuitoParent o PiezaInicial.");
            yield return null;
        }

        // Primera pieza
        PiezaCircuito firstPiece = Instantiate(piezaInicial);
        firstPiece.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        piezasAsignadas.Add(firstPiece);

        startPos = firstPiece.puntoEntrada.position;
        startDir = firstPiece.puntoEntrada.right;


        float distancia = firstPiece.longitud;
        bool circuitoCerrado = false;

        // Piezas siguientes
        while (distancia < distanciaObjetivo - margenDistancia)
        {
            PiezaCircuito prefab = piezas[Random.Range(0, piezas.Length)];
            PiezaCircuito p = Instantiate(prefab, circuitoParent);
            p.gameObject.SetActive(false);

            AlignPiece(p, piezasAsignadas[piezasAsignadas.Count - 1]);

            if (CanPlace(p))
            {
                if (CanCloseCircuit(p))
                {
                    circuitoCerrado = true;
                }

                p.gameObject.SetActive(true);
                piezasAsignadas.Add(p);
                distancia += p.longitud;
            }
            else
                Destroy(p.gameObject);
        }






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
        Collider[] oldCols = circuitoParent.GetComponentsInChildren<Collider>();

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

    bool CanCloseCircuit(PiezaCircuito last)
    {
        if (last.puntoSalida.position == piezaInicial.puntoEntrada.position && last.puntoSalida.rotation == piezaInicial.puntoEntrada.rotation)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
