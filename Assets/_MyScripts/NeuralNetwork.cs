using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NeuralNetwork: ISerializationCallbackReceiver
{
    public NeuronLayer inputNeurons;
    public NeuronLayer[] hiddenLayers;
    public NeuronLayer outputNeurons;

    public NeuralNetwork(int numInputNeurons, int[] numsHiddenNeurons, int numOutputNeurons, ActivationFunctions.FunctionType hiddenLayersFunction, ActivationFunctions.FunctionType outputLayerFunction) {
        inputNeurons = new NeuronLayer(numInputNeurons);
        for(int i = 0; i < numInputNeurons; i++) {
            //passo nessun previous layer, funzione di attivazione non serve
            inputNeurons.neurons[i] = new Neuron(null, i - 1, ActivationFunctions.FunctionType.None, numsHiddenNeurons[0] - 1);
        }

        hiddenLayers = new NeuronLayer[numsHiddenNeurons.Length];

        for(int l=0; l < numsHiddenNeurons.Length; l++) {
            hiddenLayers[l] = new NeuronLayer(numsHiddenNeurons[l]);
            for(int i = 0; i < numsHiddenNeurons[l]; i++) {
                int numOutputWeights = l == numsHiddenNeurons.Length - 1 ? numOutputNeurons : numsHiddenNeurons[l + 1] - 1;
                if(l == 0) {
                    //primo layer, quindi passerò inputNeurons come previousNeurons
                    if(i == 0) {
                        //bias neuron
                        hiddenLayers[l].neurons[i] = new Neuron(null, i - 1, ActivationFunctions.FunctionType.None, numOutputWeights);
                    }
                    else {
                        hiddenLayers[l].neurons[i] = new Neuron(inputNeurons, i - 1, hiddenLayersFunction, numOutputWeights);
                    }
                }
            }
        }

        outputNeurons = new NeuronLayer(numOutputNeurons);
        for(int i=0; i < numOutputNeurons; i++) {
            outputNeurons.neurons[i] = new Neuron(hiddenLayers[hiddenLayers.Length - 1], i, outputLayerFunction, 0);
        }
    }

    public float[] Evaluate(float[] inputs) {
        if (inputs.Length != inputNeurons.neurons.Length - 1) {
            throw new System.Exception("Input length must have same inputNeurons length (subtract bias)");
        }

        int totalLayers = 2 + hiddenLayers.Length;
        NeuronLayer[] allLayers = new NeuronLayer[totalLayers];
        allLayers[0] = inputNeurons;
        for(int i=0; i < hiddenLayers.Length; i++) {
            allLayers[i + 1] = hiddenLayers[i];
        }
        allLayers[totalLayers - 1] = outputNeurons;

        float[] previousLayerResponse = null;
        for(int l=0; l < totalLayers; l++) {
            previousLayerResponse = new float[allLayers[l].neurons.Length];
            for(int i=0; i < allLayers[l].neurons.Length; i++) {
                if(l == 0) {
                    //input layer
                    //bias se i == 0
                    float inputToFeed = i > 0 ? inputs[i - 1] : 0;
                    previousLayerResponse[i] = allLayers[l].neurons[i].Evaluate(new float[] { inputToFeed});
                }
                else {
                    previousLayerResponse[i] = allLayers[l].neurons[i].Evaluate(inputs);
                }
            }
            inputs = previousLayerResponse;
        }
        if(previousLayerResponse.Length != outputNeurons.neurons.Length) {
            throw new System.Exception("Something terrible has happened during calculations");
        }
        return previousLayerResponse;
    }

    public void BackpropagationAlgorithm(float[] errorsOfNextLayer) {
        if(errorsOfNextLayer.Length != outputNeurons.neurons.Length) {
            throw new System.Exception("errorsOfOutputNeurons and outputNeurons must have same length");
        }

        int totalLayers = 2 + hiddenLayers.Length;
        NeuronLayer[] allLayers = new NeuronLayer[totalLayers];
        allLayers[0] = inputNeurons;
        for (int i = 0; i < hiddenLayers.Length; i++) {
            allLayers[i + 1] = hiddenLayers[i];
        }
        allLayers[totalLayers - 1] = outputNeurons;

        float[] errorsOfPrecedentCalculatedLayer;
        for(int l = totalLayers - 1; l >= 0; l--) {
            int errorsLength = l == totalLayers - 1 ? allLayers[l].neurons.Length : allLayers[l].neurons.Length - 1;
            errorsOfPrecedentCalculatedLayer = new float[errorsLength];
            for(int i=0; i < allLayers[l].neurons.Length; i++) {
                if(l == totalLayers - 1) {
                    //output layer vuole un array di lunghezza 1
                    errorsOfPrecedentCalculatedLayer[i] = allLayers[l].neurons[i].CalculateError(new float[] { errorsOfNextLayer[i] });
                }
                else {
                    float err = allLayers[l].neurons[i].CalculateError(errorsOfNextLayer);
                    if(i != 0) {
                        //non è neurone di bias
                        errorsOfPrecedentCalculatedLayer[i - 1] = err;
                    }
                }
            }
            errorsOfNextLayer = errorsOfPrecedentCalculatedLayer;
        }

        for(int l = 0; l < totalLayers; l++) {
            for(int i=0; i < allLayers[l].neurons.Length; i++) {
                allLayers[l].neurons[i].AdjustWeightsBackpropagation();
            }
        }
    }

    public void OnBeforeSerialize() {}

    public void OnAfterDeserialize() {
        if(inputNeurons == null || hiddenLayers == null || outputNeurons == null) {
            return;
        }
        int totalLayers = 2 + hiddenLayers.Length;
        NeuronLayer[] allLayers = new NeuronLayer[totalLayers];
        allLayers[0] = inputNeurons;
        for (int i = 0; i < hiddenLayers.Length; i++) {
            allLayers[i + 1] = hiddenLayers[i];
        }
        allLayers[totalLayers - 1] = outputNeurons;

        for (int l=1; l < totalLayers; l++) {
            for(int i=0; i < allLayers[l].neurons.Length; i++) {
                NeuronLayer previousNeurons = allLayers[l - 1];
                if (l != totalLayers - 1 && i == 0) {
                    //bias neuron
                     previousNeurons = null;
                }
                allLayers[l].neurons[i].SetPreviousNeurons(previousNeurons);
            }
        }
    }
}

[System.Serializable]
public class NeuronLayer {
    public Neuron[] neurons;

    public NeuronLayer(int dim) {
        neurons = new Neuron[dim];
    }

    public bool isNull() {
        return neurons == null || neurons.Length == 0;
    }
}
