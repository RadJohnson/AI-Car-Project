using System;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

using Random = UnityEngine.Random;

// to use as a guide https://github.com/john-sorrentino/Neural-Network-Evolution-Sim-Tutorial/blob/main/NN.cs https://www.youtube.com/watch?v=yyS5hjyOFDo&list=PLlIPoiD-ZBaw9x3NRoquGkFBl9RpAasys&index=2&ab_channel=JohnSorrentino


public class AIBrain : IComparable<AIBrain>
{
    internal Layer[] network;

    private float fitness;

    public AIBrain(int[] networkShape)
    {
        network = new Layer[networkShape.Length];
        for (int i = 0; i < network.Length; i++)
        {
            network[i] = new Layer(networkShape[i]);
            if (i != 0)
                //    network[i].previousLayer = new layer;
                network[i].previousLayer = network[i - 1];
        }
        /*
        for (int i = 0; i < network.Length; i++)
        {
            for (int j = 0; j < network[i].nodes.Length; j++)
            {
                if (i + 1 > network.Length - 1)
                {
                    break;
                }
                network[i].nodes[j].weights = new float[network[i + 1].nodes.Length];//index was outside the bounds of the array

                float variance = Mathf.Sqrt(1.0f / network[i + 1].nodes.Length);
                for (int k = 0; k < network[i + 1].nodes.Length; k++)
                {
                    network[i].nodes[j].weights[k] = (Random.Range(0f, 1f) * 2 * variance - variance);
                }

            }
        }
        */

        InitialiseWeights();
    }

    public AIBrain(AIBrain copyNetwork)
    {
        // Initialize the network array with the same length as the copyNetwork's network array.
        network = new Layer[copyNetwork.network.Length];

        // Copy each layer using the Layer copy constructor.
        for (int i = 0; i < copyNetwork.network.Length; i++)
        {
            // Use the copy constructor to make a deep copy of each layer.
            network[i] = new Layer(copyNetwork.network[i]);

            // Re-establish the links to the previous layers within the new network.
            // Note that previousLayer should not be deep copied; it should reference the previous layer in the new network.
            if (i != 0)
                network[i].previousLayer = network[i - 1];
        }


        /*
        for (int i = 0; i < network.Length; i++)
        {
            for (int j = 0; j < network[i].nodes.Length; j++)
            {
                if (i + 1 > network.Length - 1)
                {
                    break;
                }
                network[i].nodes[j].weights = new float[network[i + 1].nodes.Length];//index was outside the bounds of the array

                float variance = Mathf.Sqrt(1.0f / network[i + 1].nodes.Length);
                for (int k = 0; k < network[i + 1].nodes.Length; k++)
                {
                    network[i].nodes[j].weights[k] = (Random.Range(0f, 1f) * 2 * variance - variance);
                }

            }
        }
        */
        //InitialiseWeights();

        //CopyWeights(copyNetwork);
    }

    private void CopyWeights(AIBrain BrainToCoppy)
    {
        for (int i = 0; i < network.Length; i++)
        {
            for (int j = 0; j < network[i].nodes.Length; j++)
            {
                for (int k = 0; k < network[i].nodes[j].weights.Length; k++)
                {
                    network[i].nodes[j].weights[k] = BrainToCoppy.network[i].nodes[j].weights[k];
                }
            }
        }
    }

    public void AddFitness(float fit)
    {
        fitness += fit;
    }

    public void SetFitness(float fit)
    {
        fitness = fit;
    }

    public float GetFitness()
    {
        return fitness;
    }

    /// <summary>
    /// Compare two neural networks and sort based on fitness
    /// </summary>
    /// <param name="other">Network to be compared to</param>
    /// <returns></returns>
    public int CompareTo(AIBrain other)
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }

    public void MutateWeights()
    {
        for (int i = 0; i < network.Length; i++)
        {
            for (int j = 0; j < network[i].nodes.Length; j++)
            {
                for (int k = 0; k < network[i].nodes[j].weights.Length; k++)
                {
                    float randomNumber = Random.Range(0, 100);

                    float weight = network[i].nodes[j].weights[k];

                    if (randomNumber <= 2)
                    { //if 1
                      //flip sign of weight
                        weight *= -1f;
                    }
                    else if (randomNumber <= 4f)
                    { //if 2
                      //pick random weight between -1 and 1
                        weight = Random.Range(-0.5f, 0.5f);
                    }
                    else if (randomNumber <= 6f)
                    { //if 3
                      //randomly increase by 0% to 100%
                        float factor = Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                    }
                    else if (randomNumber <= 8f)
                    { //if 4
                      //randomly decrease by 0% to 100%
                        float factor = Random.Range(0f, 1f);
                        weight *= factor;
                    }

                    network[i].nodes[j].weights[k] = weight;
                }
            }
        }
    }

    internal void InitialiseWeights()
    {
        for (int i = 0; i < network.Length; i++)
        {
            for (int j = 0; j < network[i].nodes.Length; j++)
            {
                if (i + 1 > network.Length - 1)
                {
                    break;
                }
                network[i].nodes[j].weights = new float[network[i + 1].nodes.Length];//index was outside the bounds of the array

                for (int k = 0; k < network[i + 1].nodes.Length; k++)
                {
                    network[i].nodes[j].weights[k] = Random.Range(-0.5f, 0.5f);
                }
                //float variance = Mathf.Sqrt(1.0f / network[i + 1].nodes.Length);
                //for (int k = 0; k < network[i + 1].nodes.Length; k++)
                //{
                //    network[i].nodes[j].weights[k] = (Random.Range(0f, 1f) * 2 * variance - variance);
                //}

            }
        }
    }

    internal float[] Process(float[] inputs)
    {
        float[] tmp = new float[1];
        for (int i = 0; i < network.Length; i++)
        {
            if (i == 0)
            {
                tmp = network[i].ForwardPass(inputs, network);
            }
            else
            {
                tmp = network[i].ForwardPass(tmp, network);
            }
        }
        return tmp;
    }

}

class Node
{//inputs for this are prevous layers outputs

    internal float[] weights;
    //private float bias = 0;

    //internal float output;// = (input * weight) + bias


    //public Node(Layer previousLayer)
    //{
    //    weights = new float[] { previousLayer.nodes.Length };
    //}
    
    /*
    internal float ForwardPass(float[] inputs, float[] _weights)
    {
        float output = 0f;

        for (int i = 0; i < inputs.Length; i++)
        {
            for (int j = 0; j < _weights.Length; j++)
                output += inputs[i] * _weights[j];
        }

        return output;
    }
    */

    // used to setup weights and bias for the node using using Xavier/Glorot
    public Node(int nextLayersNodeCount)
    {
        weights = new float[nextLayersNodeCount];
        //float variance = Mathf.Sqrt(1.0f / nextLayersNodeCount);

        for (int i = 0; i < nextLayersNodeCount; i++)
        {
            weights[i] = Random.Range(-0.5f, 0.5f);
            //weights[i] = (Random.Range(0f, 1f) * 2 * variance - variance);
        }

        //bias = (Random.Range(0f, 1f) * 2 * variance - variance);
    }

    // Copy constructor for node
    public Node(Node copyNode)
    {
        // Allocate new memory for weights
        weights = new float[copyNode.weights.Length];
        // Copy the weights from the provided node
        for (int i = 0; i < copyNode.weights.Length; i++)
        {
            weights[i] = copyNode.weights[i];
        }
    }

}


public class Layer
{
    internal Node[] nodes;
    internal Layer previousLayer;

    public Layer(int _numNodes)
    {
        nodes = new Node[_numNodes];

        for (int i = 0; i < _numNodes; i++)
        {
            nodes[i] = new Node(_numNodes);
        }
    }

    // Copy constructor for Layer
    public Layer(Layer copyLayer)
    {
        // Initialize the nodes array with the length of the nodes in the copyLayer
        nodes = new Node[copyLayer.nodes.Length];

        // Copy each node using the Node copy constructor
        for (int i = 0; i < copyLayer.nodes.Length; i++)
        {
            nodes[i] = new Node(copyLayer.nodes[i]);
        }
    }


    internal float[] ForwardPass(float[] inputs, Layer[] network)
    {
        int numberOfNodes = nodes.Length;
        float[] layerOutputs = new float[numberOfNodes];

        for (int i = 0; i < numberOfNodes; i++)
        {
            if (previousLayer != null)
            {
                for (int j = 0; j < previousLayer.nodes.Length; j++)
                {
                    layerOutputs[i] += inputs[j] * previousLayer.nodes[j].weights[i];
                    //layerOutputs[i] += nodes[i].ForwardPass(inputs, previousLayer.nodes[j].weights);
                }
                layerOutputs[i] = ActivationFunction.Tanh(layerOutputs[i] /*+ bias*/);
            }
            else
            {
                var firstWeights = new float[network[1].nodes.Length];
                for (int j = 0; j < firstWeights.Length; j++)
                {
                    //firstWeights[j] = 1;
                    layerOutputs[i] = inputs[i] /** firstWeights[i]*/;
                }
                //for (int j = 0; j < previousLayer.nodes.Length; j++)
                //{
                    //layerOutputs[i] = nodes[i].ForwardPass(inputs, firstWeights);
                //}
            }

        }

        return layerOutputs;
    }
}


struct ActivationFunction
{
    internal static float Sigmoid(float x)
    {
        return 1.0f / (1.0f + Mathf.Exp(-x));
    }

    internal static float Tanh(float x)
    {
        return (float)Math.Tanh(x);

    }

    internal static float Relu(float x)
    {
        if (x < 0)
        {
            x = 0;
        }

        return x;
    }

    internal static float LeakyRelu(float x)
    {
        if (x < 0)
        {
            x = x / 10;
        }

        return x;
    }
}




///// <summary>
///// Mutate neural network weights
///// </summary>
//public void Mutate()
//{
//    for (int i = 0; i < weights.Length; i++)
//    {
//        for (int j = 0; j < weights[i].Length; j++)
//        {
//            for (int k = 0; k < weights[i][j].Length; k++)
//            {
//                float weight = weights[i][j][k];

//                //mutate weight value 
//                float randomNumber = UnityEngine.Random.Range(0f, 100f);

//                if (randomNumber <= 2f)
//                { //if 1
//                  //flip sign of weight
//                    weight *= -1f;
//                }
//                else if (randomNumber <= 4f)
//                { //if 2
//                  //pick random weight between -1 and 1
//                    weight = UnityEngine.Random.Range(-0.5f, 0.5f);
//                }
//                else if (randomNumber <= 6f)
//                { //if 3
//                  //randomly increase by 0% to 100%
//                    float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
//                    weight *= factor;
//                }
//                else if (randomNumber <= 8f)
//                { //if 4
//                  //randomly decrease by 0% to 100%
//                    float factor = UnityEngine.Random.Range(0f, 1f);
//                    weight *= factor;
//                }

//                weights[i][j][k] = weight;
//            }
//        }
//    }
//}

//public void AddFitness(float fit)
//{
//    fitness += fit;
//}

//public void SetFitness(float fit)
//{
//    fitness = fit;
//}

//public float GetFitness()
//{
//    return fitness;
//}

///// <summary>
///// Compare two neural networks and sort based on fitness
///// </summary>
///// <param name="other">Network to be compared to</param>
///// <returns></returns>
//public int CompareTo(NeuralNetwork other)
//{
//    if (other == null) return 1;

//    if (fitness > other.fitness)
//        return 1;
//    else if (fitness < other.fitness)
//        return -1;
//    else
//        return 0;
//}












/*
need to create struct of class for nodes

node need to contain input values and give output bassed off of math shit

do the maths on the nodes

(
def node_computation(inputs, weights, bias, activation_function):
    weighted_sum = np.dot(inputs, weights)
    activation_potential = weighted_sum + bias
    output = activation_function(activation_potential)
    return output
)
__________________________________________________________________________________________

need to define a class or struct for a group of nodes (LAYERS)


need to create a layer that can then be used for an array and an input and output version that inherits from the main layer class

*/