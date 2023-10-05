using System;
using System.Diagnostics;
using UnityEngine;

using Random = UnityEngine.Random;

// to use as a guide https://github.com/john-sorrentino/Neural-Network-Evolution-Sim-Tutorial/blob/main/NN.cs https://www.youtube.com/watch?v=yyS5hjyOFDo&list=PLlIPoiD-ZBaw9x3NRoquGkFBl9RpAasys&index=2&ab_channel=JohnSorrentino


public class AIBrain
{
    //internal int[] networkShape;

    internal Layer[] network;


    public AIBrain(int[] networkShape)
    {
        network = new Layer[networkShape.Length - 1];
        for (int i = 0; i < network.Length; i++)
        {
            network[i] = new Layer(networkShape[i]);
        }

        //for (int i = 0; i < network.Length; i++)
        //{

        //}
    }

    internal float[] Process(float[] inputs)
    {
        float[] tmp = new float[1];
        for (int i = 0; i < network.Length; i++)
        {
            if (i == 0)
            {
                tmp = network[i].ForwardPass(inputs);
            }
            else
            {
                tmp = network[i].ForwardPass(tmp);
            }

        }
        return tmp;
    }

}

class Node
{//inputs for this are prevous layers outputs

    internal float[] weights;
    private float bias = 0;

    internal float output;// = (input * weight) + bias


    //public Node(Layer previousLayer)
    //{
    //    weights = new float[] { previousLayer.nodes.Length };
    //}

    internal float ForwardPass(float[] inputs, float[] _weights)
    {
        //float output = 0f;

        for (int i = 0; i < inputs.Length; i++)
        {
            output += inputs[i] * _weights[i];
        }

        return ActivationFunction.Sigmoid(output + bias);
    }

    // used to setup weights and bias for the node using using Xavier/Glorot
    public Node(int numInputs)
    {
        weights = new float[numInputs];
        float variance = Mathf.Sqrt(1.0f / numInputs);

        for (int i = 0; i < numInputs; i++)
        {
            weights[i] = (Random.Range(0f, 1f) * 2 * variance - variance);
        }

        //bias = (Random.Range(0f, 1f) * 2 * variance - variance);
    }

}


class Layer
{
    internal Node[] nodes;
    internal Layer previousLayer;//dont think this ever gets set and is therefore the issue


    public Layer(int _numNodes)
    {
        nodes = new Node[_numNodes];

        for (int i = 0; i < _numNodes; i++)
        {
            nodes[i] = new Node(_numNodes);
        }
    }

    internal float[] ForwardPass(float[] inputs)
    {
        int numNodes = nodes.Length;
        float[] layerOutputs = new float[numNodes];

        for (int i = 0; i < numNodes; i++)
        {
            if (previousLayer != null)
            {
                layerOutputs[i] = nodes[i].ForwardPass(inputs, previousLayer.nodes[i].weights);
            }
        }

        return layerOutputs;
    }
}

//class Layer
//{
//    internal Node[] nodes;

//    public Layer(int numNodes, int numInputsPerNode)
//    {
//        nodes = new Node[numNodes];

//        for (int i = 0; i < numNodes; i++)
//        {
//            nodes[i] = new Node(numInputsPerNode);
//        }
//    }

//    // Apply the layer's forward pass
//    internal float[] ForwardPass(float[] inputs)
//    {
//        int numNodes = nodes.Length;
//        float[] layerOutputs = new float[numNodes];

//        for (int i = 0; i < numNodes; i++)
//        {
//            layerOutputs[i] = nodes[i].ForwardPass(inputs, nodes[i].weights);
//        }

//        return layerOutputs;
//    }
//}
//class Layer
//{
//    internal Node[] nodes;


//    internal int numberOfInputs;
//    internal int numberOfNeurons;

//    public Layer(int n_inputs, int n_neurons)
//    {
//        numberOfInputs = n_inputs;
//        numberOfNeurons = n_neurons;
//    }

//    void Calculate()
//    {

//    }

//    public void Forward(float[] inputsArray)
//    {


//        for (int i = 0; i < n_neurons; i++)
//        {
//            //sum of weights times inputs
//            for (int j = 0; j < n_inputs; j++)
//            {
//                nodeArray[i] += weightsArray[i, j] * inputsArray[j];
//            }

//            //add the bias
//            nodeArray[i] += biasesArray[i];
//        }
//    }
//    //Node[] inputNodes;

//    //public float[] GetForwardPassOutputs()
//    //{
//    //float[] outputs = new float[nodes.Length - 1];
//    //for (int i = 0; i < nodes.Length; i++)
//    //{
//    //outputs[i] = nodes[i].ForwardPass(previouslayer.nodes[i].output, nodes[i].weights);
//    //}
//    //return outputs;
//    //}

//}


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