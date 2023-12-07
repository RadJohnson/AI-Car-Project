using System.Collections.Generic;
using UnityEngine;
public class AIAgent : MonoBehaviour
{
    internal AIBrain brain;
    [SerializeField] private LayerMask layersToCrashInto;
    private bool hascrashed;

    [SerializeField] internal List<GameObject> checkpoints;

    [SerializeField] internal int checkpointsPassedThrough;
    public void Initialize(AIBrain _brain)
    {
        brain = _brain;
    }

    private void FixedUpdate()//this would need to be fixed update if i wanted to do the time speedy upy thing which i am not sure is possible to be honest
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

        var input = new float[] 
        { hitLeft.distance, hitRight.distance, hitForward.distance, (transform.position - checkpoints[checkpointsPassedThrough + 1].transform.position).magnitude };
        //Debug.Log(brain.Process(input));
        var aiOutput = brain.Process(input);

        var forwardBackwards = aiOutput[0];
        var leftRight = aiOutput[1];

        gameObject.GetComponent<AICar>().AICarInput(leftRight, forwardBackwards);
    }
    internal void CheckpointChecker(GameObject checkpoint)
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoint == checkpoints[i])
            {
                if (i == checkpointsPassedThrough)
                {
                    checkpointsPassedThrough++;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((layersToCrashInto & (1 << collision.collider.gameObject.layer)) != 0)
        {
            hascrashed = true;
        }
    }
}
