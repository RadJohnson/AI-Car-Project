using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class AIAgent : MonoBehaviour
{
    internal AIBrain brain;
    [SerializeField]private LayerMask layersToCrashInto;
    private bool hascrashed;

    public void Initialize(AIBrain _brain)
    {
        brain = _brain;
    }

    private void Update()
    {

        var leftRay = Physics.Raycast(transform.position, -transform.right, out var hitLeft, Mathf.Infinity, layersToCrashInto);
        var rightRay = Physics.Raycast(transform.position, transform.right, out var hitRight, Mathf.Infinity, layersToCrashInto);
        var forwardRay = Physics.Raycast(transform.position, transform.forward, out var hitForward, Mathf.Infinity, layersToCrashInto);

        var input = new float[] { hitLeft.distance, hitRight.distance, hitForward.distance };
        //Debug.Log(brain.Process(input));
        var aiOutput = brain.Process(input);
        gameObject.GetComponent<AICar>().AICarInput(aiOutput[0], aiOutput[1]);

    }




}
