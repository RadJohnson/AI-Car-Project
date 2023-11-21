using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public AIAgent agentPrefab;
    public Transform startPoint,endpoint;
    [SerializeField] private float timeBetweenGenerations = 15f;
    private bool isTraining = false;
    private int populationSize = 50;
    private int generationNumber = 0;

    private int[] networkShape = { 3, 10, 10, 2 };// Make sure that the first and last numbers in the shape are the same as the number of inputs that outputs you want

    private List<AIBrain> nets;

    private List<AIAgent> agentList;

    //void Start()
    //{
    //    if (isTraining == false)
    //    {
    //        Debug.Log($"Starting Generation {generationNumber}");
    //        if (generationNumber == 0)
    //        {
    //            CreateTrainingPool();
    //        }
    //
    //        SpawnAgents();
    //
    //    }
    //
    //}

    
    private void Update()
    {
        if (isTraining == false)
        {
            Debug.Log($"Starting Generation {generationNumber}");
            if (generationNumber == 0)
            {
                CreateTrainingPool();
            }
            else
            {
                for (int i = 0; i < populationSize; i++)
                {
                    nets[i].AddFitness((agentList[i].transform.position - startPoint.position /*+ agentList[i].transform.position + endpoint.position*/).magnitude);
                }

                nets.Sort();
                //nets.Reverse();

                //for (int i = 0; i < populationSize; i++)
                //{
                //    var contentToSave = JsonUtility.ToJson(nets[i], true);// issue with this line here
                //    string filePath = Path.Combine(Application.persistentDataPath,$"Generation_{generationNumber} NeuralNetwork_{i}.json");//change application.persistant datapath to a coppied file path
                //
                //    // Write the content to the file
                //    File.WriteAllText(filePath, contentToSave);
                //}

                for (int i = 0; i < populationSize / 2; i++)// this could be an issue
                {
                    nets[i] = new AIBrain(nets[i + (populationSize / 2)]);
                    nets[i].MutateWeights();
                    nets[i + (populationSize / 2)] = new AIBrain(nets[i + (populationSize / 2)]);
                }
                
                for (int i = 0; i < populationSize; i++)
                {
                    nets[i].SetFitness(0f);
                }
            }
            generationNumber++;
            isTraining = true;
            Invoke("Timer", timeBetweenGenerations);
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
            AIAgent agent = Instantiate(agentPrefab, startPoint.position, transform.rotation);
            agent.Initialize(nets[i]);
            agentList.Add(agent);
        }
    }

    void CreateTrainingPool()
    {
        //population must be even and will be set to 50 as a minimum
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
            nets.Add(net);
        }
    }

    void Timer()
    {
        isTraining = false;
    }
}
