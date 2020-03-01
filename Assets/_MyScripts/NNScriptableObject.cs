using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNetwork", menuName = "NeuralNetwork")]
public class NNScriptableObject : ScriptableObject
{
    public bool use16Raycasts = false;
    public int inputLayerDim = 1;
    public int[] hiddenLayers;
    public int outputLayerDim = 1;
    public ActivationFunctions.FunctionType hiddenLayersFunction;
    public ActivationFunctions.FunctionType outputLayersFunction;
}
