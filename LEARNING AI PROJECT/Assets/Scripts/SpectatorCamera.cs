using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    AIManager aiManager;
    [SerializeField] internal List<GameObject> cars;
    [SerializeField] internal float smoothSpeed;
    [SerializeField] internal float radius; // The distance from the target car
    private int currentCarIndex = 0;

    void Update()
    {
        //cars = aiManager.GetCarGameObjects();

        if (Input.GetKeyDown(KeyCode.C)) // Switch car
        {
            currentCarIndex = (currentCarIndex + 1) % cars.Count;// cahnge this so that it makes more sense
        }

        Vector3 targetPosition = cars[currentCarIndex].transform.position;
        Vector3 direction = (transform.position - targetPosition).normalized;
        Vector3 desiredPosition = targetPosition + direction * radius;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(cars[currentCarIndex].transform);
    }


    private void Reset()
    {
        smoothSpeed = 0.25f;
        radius = 5f;
    }
}
