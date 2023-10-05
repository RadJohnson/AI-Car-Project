using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public AIAgent agentPrefab;
    public Transform startPoint;
    [SerializeField] private float timeBetweenGenerations = 15f;
    private bool isTraining = false;
    private int populationSize = 50;
    private int generationNumber = 0;

    private int[] networkShape = { 3, 10, 10, 2 };

    private List<AIBrain> nets;

    private List<AIAgent> agentList;

    void Start()
    {
        if (isTraining == false)
        {
            Debug.Log($"Starting Generation {generationNumber}");
            if (generationNumber == 0)
            {
                CreateTrainingPool();
            }

            SpawnAgents();

        }

    }

    private void SpawnAgents()
    {
        if (agentList != null)
        {
            for (int i = 0; i < agentList.Count; i++)
            {
                Destroy(agentList[i].gameObject);
            }
        }
        agentList = new List<AIAgent>();

        for (int i = 0; i < populationSize; i++)
        {
            AIAgent actor = Instantiate(agentPrefab, startPoint.position, transform.rotation);
            actor.Initialize(nets[i]);
            agentList.Add(actor);
        }
    }

    void CreateTrainingPool()
    {
        //population must be even, just setting it to 20 incase it's not
        if (populationSize % 2 != 0)
        {
            if (populationSize < 50)
                populationSize = 50;
            else
                populationSize++;
        }
        nets = new List<AIBrain>();
        for (int i = 0; i < populationSize; i++)
        {
            AIBrain net = new AIBrain(networkShape);
            //net.
            nets.Add(net);
        }
    }
}
