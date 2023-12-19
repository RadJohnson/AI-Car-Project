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
    private Vector3 currentRotation;

    void Start()
    {
        currentRotation = transform.eulerAngles;
    }

    void Update()
    {
        cars = aiManager.GetCarGameObjects(); // consider optimizing this

        // Switch car
        if (Input.GetMouseButtonDown(0))
        {
            currentCarIndex = (currentCarIndex + 1) % cars.Count;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            currentCarIndex--;
            if (currentCarIndex < 0)
            {
                currentCarIndex = cars.Count - 1;
            }
        }
        //Debug.Log(currentCarIndex);

        // Adjust zoom with scroll wheel
        radius -= Input.mouseScrollDelta.y;
        radius = Mathf.Clamp(radius, 2, 8);

        // Rotate camera with mouse movement
        if (Input.GetMouseButton(2)) // Middle mouse button
        {
            currentRotation.y += Input.GetAxis("Mouse X");
            currentRotation.x -= Input.GetAxis("Mouse Y");
        }

        // Calculate new position and rotation
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        Vector3 direction = rotation * -Vector3.forward;
        Vector3 desiredPosition = cars[currentCarIndex].transform.position + direction * radius;

        // Apply smooth transition
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.LookAt(cars[currentCarIndex].transform);
    }

    private void Reset()
    {
        aiManager = FindObjectOfType<AIManager>();
        smoothSpeed = 0.25f;
        radius = 5f;
    }
}