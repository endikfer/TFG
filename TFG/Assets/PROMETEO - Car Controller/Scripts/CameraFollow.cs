using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform carTransform;
	[Range(1, 10)]
	public float followSpeed = 2;
	[Range(1, 10)]
	public float lookSpeed = 5;

    public Vector3 offset = new Vector3(0f, 5f, -10f);

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
