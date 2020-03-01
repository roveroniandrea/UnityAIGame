using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Neuron
{
    [System.NonSerialized]
    NeuronLayer previousNeurons;

    public float[] outputWeights;
    public int indexInLayer;
    ActivationFunctions.FunctionType functionType;
    float latestDotProduct = 0f;
    float latestError = 0f;
    float[] receivedInputs;


    public Neuron(NeuronLayer previousNeurons, int indexInLayer, ActivationFunctions.FunctionType functionType, int numOutputWeights) {
        SetPreviousNeurons(previousNeurons);
        //se indexInLayer = -1 il neurone è bias
        this.indexInLayer = indexInLayer;
        this.functionType = functionType;

        //outputWeights (se == 0 allora outputNeuron)
        if(numOutputWeights == 0) {
            outputWeights = null;
        }
        else {
            outputWeights = new float[numOutputWeights];
            InitOutputWeights();
        }
    }

    void InitOutputWeights() {
        for(int i=0; i < outputWeights.Length; i++) {
            outputWeights[i] = Random.value * 2f - 1f;
        }
    }

    public float Evaluate(float[] inputs) {
        if(isPreviousNeuronNull()) {
            if(indexInLayer == -1) {
                //bias neuron
                receivedInputs = null;
                return 1f;
            }
            else {
                //input neuron
                if(inputs.Length != 1) {
                    throw new System.Exception("Inputs must have length 1 for input neurons");
                }
                receivedInputs = inputs;
                return inputs[0];
            }
        }
        else {
            if(inputs.Length != previousNeurons.neurons.Length) {
                throw new System.Exception("inputs and previousNeurons must have same length");
            }
            float dotProduct = 0f;
            for(int i=0; i < inputs.Length; i++) {
                dotProduct += inputs[i] * previousNeurons.neurons[i].outputWeights[indexInLayer];
            }
            latestDotProduct = dotProduct;
            receivedInputs = inputs;
            return ActivationFunctions.Evaluate(functionType, dotProduct);
        }
    }

    public float CalculateError(float[] nextLayerErrors) {
        if(indexInLayer == -1 || isPreviousNeuronNull()) {
            //bias o input neuron
            latestError = 0f;
        }
        if(outputWeights == null || outputWeights.Length == 0) {
            //outputNeuron
            if(nextLayerErrors.Length != 1) {
                throw new System.Exception("nextLayerErrors must have length 1 for outputNeurons");
            }
            latestError = nextLayerErrors[0];
        }
        else {
            if(nextLayerErrors.Length != outputWeights.Length) {
                throw new System.Exception("nextLayerErrors and outputWeights must have same length");
            }
            float error = 0f;
            for(int i=0; i < outputWeights.Length; i++) {
                error += outputWeights[i] * nextLayerErrors[i];
            }
            latestError = error;
        }
        return latestError;
    }

    public void AdjustWeightsBackpropagation() {
        if(indexInLayer == -1 || isPreviousNeuronNull()) {
            //bias o input neuron
            return;
        }
        if(previousNeurons.neurons.Length != receivedInputs.Length) {
            throw new System.Exception("previousNeurons and receivedInputs must have same length");
        }

        //evito valori troppo alti nella derivata
        float manyNeuronsContrast = previousNeurons.neurons[0].outputWeights.Length;
        for(int i=0; i < receivedInputs.Length; i++) {
            previousNeurons.neurons[i].outputWeights[indexInLayer] += latestError * ActivationFunctions.EvaluateDerivative(functionType, latestDotProduct / manyNeuronsContrast) * receivedInputs[i];
        }
    }

    public void SetPreviousNeurons(NeuronLayer previousNeurons) {
        this.previousNeurons = previousNeurons;
    }

    bool isPreviousNeuronNull() {
        return previousNeurons == null || previousNeurons.isNull();
    }
}

public static class ActivationFunctions {
    public enum FunctionType { None, Sigmoid, Tanh, ReLU, LeakyReLU}
    public static float alfaReLU = 0.1f;

    public static float Evaluate(FunctionType functionType, float value) {
        switch (functionType) {
            case FunctionType.Sigmoid: {
                    return SigmoidFunction(value);
                }
            case FunctionType.Tanh: {
                    return TanhFunction(value);
                }
            case FunctionType.ReLU: {
                    return ReLUFunction(value);
                }
            case FunctionType.LeakyReLU: {
                    return LeakyReLUFunction(value);
                }
            default: {
                    throw new System.Exception("Function not implemented");
                }
        }
    }

    public static float EvaluateDerivative(FunctionType functionType, float value) {
        switch (functionType) {
            case FunctionType.Sigmoid: {
                    return SigmoidDerivativeFunction(value);
                }
            case FunctionType.Tanh: {
                    return TanhDerivativeFunction(value);
                }
            case FunctionType.ReLU: {
                    return ReLUDerivativeFunction(value);
                }
            case FunctionType.LeakyReLU: {
                    return LeakyReLUDerivativeFunction(value);
                }
            default: {
                    throw new System.Exception("Derivative function not implemented");
                }
        }
    }

    static float SigmoidFunction(float value) {
        return 1f / (1f + Mathf.Exp(-value));
    }

    static float TanhFunction(float value) {
        return (Mathf.Exp(2 * value) - 1f) / (Mathf.Exp(2 * value) + 1f);
    }

    static float ReLUFunction(float value) {
        return Mathf.Max(0f, value);
    }

    static float LeakyReLUFunction(float value) {
        return Mathf.Max(alfaReLU * value, value);
    }

    static float SigmoidDerivativeFunction(float value) {
        return Mathf.Exp(value) / Mathf.Pow(Mathf.Exp(value) + 1f, 2);
    }

    static float TanhDerivativeFunction(float value) {
        return 4f / Mathf.Pow(Mathf.Exp(-value) + Mathf.Exp(value), 2);
    }

    static float ReLUDerivativeFunction(float value) {
        return value <= 0f ? 0f : 1f;
    }

    static float LeakyReLUDerivativeFunction(float value) {
        return value <= 0f ? alfaReLU : 1f;
    }
}
