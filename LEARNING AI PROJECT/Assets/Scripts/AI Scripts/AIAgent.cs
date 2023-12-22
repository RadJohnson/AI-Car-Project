using System.Collections.Generic;
using UnityEngine;
public class AIAgent : MonoBehaviour
{
    internal AIBrain brain;
    [SerializeField] private LayerMask layersToCrashInto;
    internal bool hascrashed;

    [SerializeField] internal List<GameObject> checkpoints;

    [SerializeField] internal int checkpointsPassedThrough;
    float distanceToNextCheckpoint = 0f;

    public void Initialize(AIBrain _brain, int _generation)
    {
        brain = _brain;
        brain.generation = _generation;
    }

    private void FixedUpdate()
    {
        if (hascrashed)
        {
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            return;
        }

        var leftRay = Physics.Raycast(transform.position, -transform.right, out var hitLeft, Mathf.Infinity, layersToCrashInto);
        var rightRay = Physics.Raycast(transform.position, transform.right, out var hitRight, Mathf.Infinity, layersToCrashInto);
        var forwardRay = Physics.Raycast(transform.position, transform.forward, out var hitForward, Mathf.Infinity, layersToCrashInto);


        if (checkpointsPassedThrough + 1 < checkpoints.Count)
        {
            distanceToNextCheckpoint = (transform.position - checkpoints[checkpointsPassedThrough + 1].transform.position).magnitude;
        }

        var input = new float[]
        { hitLeft.distance, hitRight.distance, hitForward.distance, distanceToNextCheckpoint };

        var aiOutput = brain.Process(input);

        var forwardBackwards = aiOutput[0];
        var leftRight = aiOutput[1];

        gameObject.GetComponent<AICar>().AICarInput(leftRight, forwardBackwards);//can make this more efficient by geting the componenet at the start then using that here
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
