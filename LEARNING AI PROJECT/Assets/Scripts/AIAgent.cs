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

    private void FixedUpdate()//this would need t5op be fixed update if i wanted to do teh time speedyupy thing
    {
        if (hascrashed)
        {
            //brain.AddFitness(-50);//bad dont crash into walls
            //^^^^^^^ THIS ISNT HELPING ME YOU PILOCK
            return;
        }
        var leftRay = Physics.Raycast(transform.position, -transform.right, out var hitLeft, Mathf.Infinity, layersToCrashInto);
        var rightRay = Physics.Raycast(transform.position, transform.right, out var hitRight, Mathf.Infinity, layersToCrashInto);
        var forwardRay = Physics.Raycast(transform.position, transform.forward, out var hitForward, Mathf.Infinity, layersToCrashInto);

        var input = new float[] { hitLeft.distance, hitRight.distance, hitForward.distance };
        //Debug.Log(brain.Process(input));
        var aiOutput = brain.Process(input);

        var forwardBackwards = aiOutput[0];
        var leftRight = aiOutput[1];

        gameObject.GetComponent<AICar>().AICarInput(leftRight, forwardBackwards);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if ((layersToCrashInto & (1 << collision.collider.gameObject.layer)) != 0)
        {
            hascrashed = true;
        }
    }

}
