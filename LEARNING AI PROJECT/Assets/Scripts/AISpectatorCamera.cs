using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpectatorCamera : MonoBehaviour
{
    [SerializeField] private AIManager aiManager;
    [SerializeField] internal List<GameObject> cars;
    [SerializeField] internal float smoothSpeed;
    [SerializeField] internal float radius;
    private int currentCarIndex = 0;

    void Update()
    {
        cars = aiManager.GetCarGameObjects();//this could be made more efficient by listening to an event

        // Switch car
        if (Input.GetMouseButtonDown(0))
        {
            currentCarIndex++;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            currentCarIndex--;
        }
        if (currentCarIndex < 0)
        {
            currentCarIndex = cars.Count - 1;
        }
        else if (currentCarIndex > cars.Count - 1)
        {
            currentCarIndex = 0;
        }
        Debug.Log(currentCarIndex);

        radius -= Input.mouseScrollDelta.y;
        radius = Mathf.Clamp(radius, 2, 8);

        Vector3 targetPosition = cars[currentCarIndex].transform.position;
        Vector3 direction = (transform.position - targetPosition).normalized;
        Vector3 desiredPosition = targetPosition + direction * radius;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(cars[currentCarIndex].transform);
    }


    private void Reset()
    {
        aiManager = GameObject.FindObjectOfType<AIManager>();
        smoothSpeed = 0.25f;
        radius = 5f;
    }
}
