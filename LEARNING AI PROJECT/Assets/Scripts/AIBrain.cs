using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    
}

class Node
{//inputs for this are prevous layers outputs
    float[] weights;
    float bias;

    internal float output;// = (input * weight) + bias

    void MathsStuf(float[] inputs, float[] _weights, float _bias)
    {
        output = 0;

        for (int i = 0; i < inputs.Length; i++)
        {
            output += inputs[i] * _weights[i];
        }
        output += bias;
    }

    // used to setup weights and bias for the node using using Xavier/Glorot
    public Node(int numInputs)
    {
        weights = new float[numInputs];
        float variance = Mathf.Sqrt(1.0f / numInputs);

        for (int i = 0; i < numInputs; i++)
        {
            weights[i] = (Random.Range(0,1) * 2 * variance - variance);
        }

        bias = (Random.Range(0,1) * 2 * variance - variance);
    }

    float Sigmoid(float x)
    {
        return 1.0f / (1.0f + Mathf.Exp(-x));
    }
}

class Layer
{
    Node[] nodes;

    Node[] inputNodes;

    public float[] GetOutputs()
    {
        float[] outputs = new float[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            outputs[i] = nodes[i].output;
        }
        return outputs;
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