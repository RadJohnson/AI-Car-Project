using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    [SerializeField] internal AIAgent agentPrefab;
    [SerializeField] private Transform startPoint;
    [SerializeField] private float timeBetweenGenerations = 15f;
    private bool isTraining = false;
    private int populationSize = 50;
    private int generationNumber = 0;

    //first 3 inputs are for the distance from walls and 4th is for distance to next checkpoint
    private int[] networkShape = { 4, 10, 10, 2 };// Make sure that the first and last numbers in the shape are the same as the number of inputs that outputs you want

    private List<AIBrain> nets;
    private List<AIAgent> agentList;

    [SerializeField] private List<GameObject> checkpoints;

    [SerializeField] private string brainFilePath;


    private void Update()
    {
        if (isTraining == false)
        {
            Debug.Log($"Starting Generation {generationNumber}");

            if (generationNumber % 20 == 0)//increases the time by 5 seconds (at the moment) every 20 generations
                timeBetweenGenerations += 5f;

            if (generationNumber == 0)
            {
                CreateTrainingPool();
            }
            else
            {
                //This may need to change so that the AI get given more score based on number of checkpoints passed through
                for (int i = 0; i < populationSize; i++)
                {
                    //seems like it should now be working


                    if (!agentList[i].hascrashed)
                        nets[i].AddFitness((float)(agentList[i].checkpointsPassedThrough + (float)CumulativeCheckpointDistance(agentList[i].checkpointsPassedThrough)));
                    else 
                        nets[i].AddFitness(0);

                    //nets[i].AddFitness(CumulativeCheckpointDistance(agentList[i].checkpointsPassedThrough));

                    //nets[i].AddFitness((float)agentList[i].checkpointsPassedThrough +
                    //    -(agentList[i].gameObject.transform.position - checkpoints[agentList[i].checkpointsPassedThrough + 1].transform.position).magnitude);
                    //Want to probably add a bit for distance to the next checkpoint and from the most recently passed through for better granularity

                    //nets[i].AddFitness((agentList[i].transform.position - startPoint.position /*+ agentList[i].transform.position + endpoint.position*/).magnitude);
                }

                nets.Sort();//puts the fitest at the bottom of this which is why next 2 for loops are the way they are
                //nets.Reverse();//not needed unless I want it to become worse overtime


                for (int i = populationSize - 1, agentNum = 0; i > populationSize - 11; i--, agentNum++)//may want to only save the first few networks rather than all of them
                //for (int i = 0; i < 10; i++)//may want to only save the first few networks rather than all of them
                {
                    var contentToSave = JsonUtility.ToJson(nets[i], true);
                    string filePath = Path.Combine(Application.persistentDataPath, $"Generation_{generationNumber} NeuralNetwork_{agentNum}.json");//want to now change the numbers in the file name so it is more logical when lookign at the save files
                    //change application.persistant datapath to a coppied file path since it is currently saving to appdata\locallow\(unity project counter)
                    //Debug.Log(filePath);

                    File.WriteAllText(filePath, contentToSave);
                }

                /*
                for (int i = 0; i < populationSize; i++)//this if I have time will be chaged so that the second half of the agents get the previous version coppied onto them
                {
                    nets[i] = new AIBrain(nets[i]);
                    nets[i].MutateWeights();
                }
                */


                for (int i = 0; i < populationSize / 2; i++)//this if I have time will be chaged so that the second half of the agents get the previous version coppied onto them
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
            agent.checkpoints = checkpoints;
            agent.Initialize(nets[i], generationNumber);
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
        if (brainFilePath == "")
        {
            for (int i = 0; i < populationSize; i++)
            {
                AIBrain net = new AIBrain(networkShape);
                nets.Add(net);
            }
        }
        else
        {// might want to save all of a generation and put them all in again on their own for learning continuation
            string brain = File.ReadAllText(brainFilePath);

            generationNumber = JsonUtility.FromJson<AIBrain>(brain).generation;

            for (int i = 0; i < populationSize; i++)
            {
                AIBrain net = new AIBrain(JsonUtility.FromJson<AIBrain>(brain));
                nets.Add(net);
            }
        }
    }

    float CumulativeCheckpointDistance(int checkPointsPassedThrough)
    {
        float returnVal = 0;
        for (int i = 1; i < checkPointsPassedThrough; i++)
        {
            returnVal += (checkpoints[i - 1].transform.position - checkpoints[i].transform.position).magnitude;
        }
        return returnVal;
    }

    void Timer()
    {
        isTraining = false;
    }
}
