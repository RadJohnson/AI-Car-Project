using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AIManager : MonoBehaviour
{
    [SerializeField] internal AIAgent agentPrefab;
    [SerializeField] private Transform startPoint;
    [SerializeField, Tooltip("Time in Seconds")] private float timeBetweenGenerations;
    [SerializeField, Tooltip("Time in Seconds")] private float maxTimeBetweenGenerations;
    private bool isTraining = false;
    private int populationSize = 50;
    private int generationNumber = 0;

    //first 3 inputs are for the distance from walls and 4th is for distance to next checkpoint
    private int[] networkShape = { 4, 10, 10, 2 };// Make sure that the first and last numbers in the shape are the same as the number of inputs that outputs you want
    private List<AIBrain> nets;
    internal List<AIAgent> agentList;

    [SerializeField] private List<GameObject> checkpoints;
    [SerializeField] private string savedGenerationFilePath;

    private void Update()
    {
        if (!isTraining)
        {
            Debug.Log($"Starting Generation {generationNumber}");
            if (generationNumber % 50 == 0)
            {
                if (timeBetweenGenerations < maxTimeBetweenGenerations)
                {
                    timeBetweenGenerations = Mathf.Min(timeBetweenGenerations + 5f, maxTimeBetweenGenerations);
                }
            }

            if (generationNumber == 0)
            {
                CreateTrainingPool();
            }
            else
            {
                CalcualteFitness();

                nets.Sort();
                nets.Reverse();

                SaveNetworks(nets, generationNumber, populationSize, true);

                MutateGeneration();

                foreach (var net in nets)
                {
                    net.SetFitness(0f);
                }
            }
            generationNumber++;
            isTraining = true;
            Invoke("Timer", timeBetweenGenerations);
            SpawnAgents();
        }
    }

    private void CalcualteFitness()
    {
        for (int i = 0; i < populationSize; i++)
        {
            if (!agentList[i].hascrashed)
                nets[i].AddFitness((float)(agentList[i].checkpointsPassedThrough + CumulativeCheckpointDistance(agentList[i].checkpointsPassedThrough)));
            else
                nets[i].SetFitness(nets[i].GetFitness() - 10);
        }
    }

    private void MutateGeneration()
    {
        for (int i = 0; i < populationSize / 2; i++)
        {
            nets[i + populationSize / 2] = new AIBrain(nets[i]);
            nets[i] = new AIBrain(nets[i]);
            nets[i].MutateWeights();
        }
    }

    private void SpawnAgents()
    {
        if (agentList != null)
        {
            foreach (var agent in agentList)
            {
                Destroy(agent.gameObject);
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

    private void CreateTrainingPool()
    {
        AdjustPopulationSize(out populationSize);
        nets = new List<AIBrain>();

        if (string.IsNullOrEmpty(savedGenerationFilePath))
        {
            for (int i = 0; i < populationSize; i++)
            {
                AIBrain net = new AIBrain(networkShape);
                nets.Add(net);
            }
        }
        else
        {
            nets = LoadNetworks(savedGenerationFilePath, populationSize, true);
            generationNumber = nets.Count > 0 ? nets[0].generation : 0;
        }
    }

    private void AdjustPopulationSize(out int _populationSize)
    {
        _populationSize = 0;
        //population must be even and will be set to 50 as a minimum
        if (_populationSize < 50)
        {
            _populationSize = 50;
        }
        else if (_populationSize % 2 != 0)
        {
            _populationSize++;
        }
    }

    private float CumulativeCheckpointDistance(int checkPointsPassedThrough)
    {
        float distance = 0;
        for (int i = 1; i < checkPointsPassedThrough; i++)
        {
            distance += (checkpoints[i - 1].transform.position - checkpoints[i].transform.position).magnitude;
        }

        // Add distance from the last passed checkpoint to the current position
        if (checkPointsPassedThrough > 0 && checkPointsPassedThrough <= checkpoints.Count)
        {
            distance += (transform.position - checkpoints[checkPointsPassedThrough - 1].transform.position).magnitude;
        }

        return distance;
    }

    private void Timer()
    {
        isTraining = false;
    }

    internal List<GameObject> GetCarGameObjects()
    {
        return agentList.Select(agent => agent.gameObject).ToList();
    }

    void SaveNetworks(List<AIBrain> _nets, int _generationNumber, int _saveCount, bool _saveInFolder)
    {
        string baseFilePath = Path.Combine(Application.persistentDataPath, SceneManager.GetActiveScene().name);
        if (_saveInFolder)
        {
            baseFilePath = Path.Combine(baseFilePath, $"Generation_{_generationNumber}");
            Directory.CreateDirectory(baseFilePath);
        }

        for (int i = 0; i < _saveCount; i++)
        {
            var contentToSave = JsonUtility.ToJson(_nets[i], true);
            string filePath = Path.Combine(baseFilePath, $"NeuralNetwork_{i}.json");
            File.WriteAllText(filePath, contentToSave);
        }
    }

    List<AIBrain> LoadNetworks(string _path, int _populationSize, bool _loadAllFromFolder)
    {
        List<AIBrain> _nets = new List<AIBrain>();

        if (_loadAllFromFolder)
        {
            string[] fileEntries = Directory.GetFiles(_path, "*.json");
            foreach (string fileName in fileEntries)
            {
                string brain = File.ReadAllText(fileName);
                AIBrain net = JsonUtility.FromJson<AIBrain>(brain);
                _nets.Add(net);
            }
        }
        else
        {
            string brain = File.ReadAllText(_path);
            AIBrain singleNet = JsonUtility.FromJson<AIBrain>(brain);
            for (int i = 0; i < _populationSize; i++)
            {
                _nets.Add(new AIBrain(singleNet));
            }
        }

        return _nets;
    }
}
