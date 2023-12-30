using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpectatorCamera : MonoBehaviour
{
    [SerializeField] private AIManager aiManager;
    [SerializeField] internal List<GameObject> cars;
    [SerializeField] private Vector2 mouseSensitivity;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private float radius;
    private int currentCarIndex = 0;
    private Vector3 currentRotation;

    void Start()
    {
        currentRotation = transform.eulerAngles;
    }

    private void Update()
    {
        cars = aiManager.GetCarGameObjects(); // consider optimizing this so that it only gets called on reset could be done by moveing this to the manager or by having it as an event

        // Switch car
        if (Input.GetMouseButtonDown(0))
        {
            currentCarIndex++;
            if (currentCarIndex > cars.Count)
                currentCarIndex = 0;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            currentCarIndex--;
            if (currentCarIndex < 0)
                currentCarIndex = cars.Count - 1;
        }
        //Debug.Log(currentCarIndex);

        radius -= Input.mouseScrollDelta.y;
        radius = Mathf.Clamp(radius, 2, 8);

        if (Input.GetMouseButton(2)) // Middle mouse button
        {
            currentRotation.y += Input.GetAxis("Mouse X") * mouseSensitivity.x;
            currentRotation.x -= Input.GetAxis("Mouse Y") * mouseSensitivity.y;
        }

    }

    private void FixedUpdate()
    {
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        Vector3 direction = rotation * -Vector3.forward;
        Vector3 desiredPosition = cars[currentCarIndex].transform.position + direction * radius;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.LookAt(cars[currentCarIndex].transform);
    }

    private void Reset()
    {
        aiManager = FindObjectOfType<AIManager>();
        mouseSensitivity = new Vector2(10, 10);
        smoothSpeed = 0.25f;
        radius = 5f;
    }
}