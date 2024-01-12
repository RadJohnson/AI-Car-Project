using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CurrentCarUI : MonoBehaviour
{
    [SerializeField] private AISpectatorCamera spectatorCamera;

    [SerializeField] private TextMeshProUGUI carsIndexUI;

    void Update()
    {
        
        carsIndexUI.text = "Agent: " + spectatorCamera.currentCarIndex.ToString();
    }

    private void Reset()
    {
        FindObjectOfType<Camera>().TryGetComponent(out spectatorCamera);
        TryGetComponent(out carsIndexUI);
    }
}
