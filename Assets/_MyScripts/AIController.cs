using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class AIController : MonoBehaviour
{
    CharacterMotions characterMotions;
    [Header("AI settings")]
    public bool use16Raycasts = false;
    public float rayDistance = 20f;
    public NNScriptableObject networkScriptable;
    public NeuralNetwork myNeuralNetwork;
    public enum AILearnMethod { PlayerSelf, GeneticExternal};
    public AILearnMethod learnMethod = AILearnMethod.GeneticExternal;
    public bool freezeMotions = false;
    public float[] networkEvaluated;

    private void Start() {
        characterMotions = GetComponent<CharacterMotions>();
        myNeuralNetwork = new NeuralNetwork(networkScriptable.inputLayerDim, networkScriptable.hiddenLayers, networkScriptable.outputLayerDim, networkScriptable.hiddenLayersFunction, networkScriptable.outputLayersFunction);
    }

    private void FixedUpdate() {
        float[] raycastInputs = GetRaycastInputs();

        float[] nearestEnemyInputs = GetNearestEnemyInputs();

        float[] selfInputs = GetSelfInputs();

        float[] superInputs = GetSuperInputs();

        float[] finalInputs = AssembleInputs(raycastInputs, nearestEnemyInputs, selfInputs, superInputs);
        

        networkEvaluated = myNeuralNetwork.Evaluate(finalInputs);

        if (!freezeMotions) {
            NNOutputsToMotions(networkEvaluated);
        }
    }

    int CompareHitsByDistance(RaycastHit2D hit1, RaycastHit2D hit2) {
        return (int)(hit1.distance - hit2.distance);
    }

    CharacterMotions GetNearestEnemy() {
        CharacterMotions[] allCharacters = FindObjectsOfType<CharacterMotions>();
        CharacterMotions result = null;
        foreach(CharacterMotions character in allCharacters) {
            if(result == null) {
                result = character;
            }
            else {
                bool enemy = character.team != characterMotions.team;
                if (enemy) {
                    float distanceFromResult = (result.transform.position - transform.position).magnitude;
                    float distanceFromActual = (character.transform.position - transform.position).magnitude;
                    if(distanceFromActual < distanceFromResult) {
                        result = character;
                    }
                }
            }
        }
        return result;
    }

    float[] GetRaycastInputs() {
        //TODO: attualmente blocco i 16 raycast
        use16Raycasts = false;
        int numOfRaycasts = use16Raycasts ? 16 : 8;
        float raycastAngle = 360f / numOfRaycasts;
        List<float> raycastInputs = new List<float>();

        for (int i = 0; i < numOfRaycasts; i++) {
            float radians = raycastAngle * i * Mathf.Deg2Rad;
            Vector2 rayDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            List<RaycastHit2D> hits = new List<RaycastHit2D>(Physics2D.RaycastAll(transform.position, rayDirection, rayDistance));
            hits.Sort(CompareHitsByDistance);
            float platformDistance = 0;
            float outOfMapDistance = 0;
            foreach (RaycastHit2D hit in hits) {
                if (platformDistance == 0 && hit.collider.GetComponent<Platform>() != null) {
                    platformDistance = hit.distance;
                }
                if (outOfMapDistance == 0 && hit.collider.GetComponent<OutOfMap>() != null) {
                    outOfMapDistance = hit.distance;
                }
            }
            raycastInputs.Add(1f - platformDistance / rayDistance);
            //Debug.DrawLine(transform.position, transform.position + new Vector3(rayDirection.x, rayDirection.y, 0) * platformDistance, Color.black);
            raycastInputs.Add(1f - outOfMapDistance / rayDistance);
            //Debug.DrawLine(transform.position, transform.position + new Vector3(rayDirection.x, rayDirection.y, 0) * outOfMapDistance, Color.red);
        }
        if (raycastInputs.Count != numOfRaycasts * 2) {
            throw new Exception("raycastInputs must be double the numOfRaycasts");
        }

        return raycastInputs.ToArray();
    }

    float[] GetNearestEnemyInputs() {
        //Nearest enemy (rayDistace non limita la visione del nemico)
        float[] nearestEnemyInfos = new float[5];
        CharacterMotions nearestEnemy = GetNearestEnemy();
        Vector2 enemyOffset = nearestEnemy.transform.position - transform.position;
        float enemyDistance = enemyOffset.magnitude;
        nearestEnemyInfos[0] = Mathf.Clamp01(1 - enemyDistance / rayDistance);
        float enemyAngle = (float)(Math.Atan2(enemyOffset.y, enemyOffset.x) * Mathf.Rad2Deg);
        enemyAngle /= 360f;
        nearestEnemyInfos[1] = enemyAngle;
        Vector2 enemyVelocity = nearestEnemy.GetComponent<Rigidbody2D>().velocity;
        float enemyVelocityMagnitude = Mathf.Clamp01(enemyVelocity.magnitude / 100f);
        nearestEnemyInfos[2] = enemyVelocityMagnitude;
        float enemyVelocityAngle = (float)(Math.Atan2(enemyVelocity.y, enemyVelocity.x) * Mathf.Rad2Deg);
        enemyVelocityAngle /= 360f;
        nearestEnemyInfos[3] = enemyVelocityAngle;
        float enemyHitPercentage = (float)nearestEnemy.damage / nearestEnemy.maxDamage;
        nearestEnemyInfos[4] = enemyHitPercentage;

        return nearestEnemyInfos;
    }

    float[] GetSelfInputs() {
        //self
        float[] selfInfos = new float[6];
        Vector2 selfVelocity = GetComponent<Rigidbody2D>().velocity;
        float selfVelocityMagnitude = Mathf.Clamp01(selfVelocity.magnitude / 100f);
        selfInfos[0] = selfVelocityMagnitude;
        float selfVelocityAngle = (float)(Math.Atan2(selfVelocity.y, selfVelocity.x) * Mathf.Rad2Deg);
        selfVelocityAngle /= 360f;
        selfInfos[1] = selfVelocityAngle;
        float selfHitPercentage = (float)characterMotions.damage / characterMotions.maxDamage;
        selfInfos[2] = selfHitPercentage;
        selfInfos[3] = characterMotions.dashReadyIn <= 0f ? 1 : 0;
        selfInfos[4] = characterMotions.punchReadyIn <= 0f ? 1 : 0;
        selfInfos[5] = characterMotions.jumpReadyIn <= 0f ? 1 : 0;
        return selfInfos;
    }

    float[] GetSuperInputs() {
        float[] super = new float[4] { 0, 0, 0, 0 };
        return super;
    }

    float[] AssembleInputs(float[] raycastInputs, float[] nearestEnemyInputs, float[] selfInputs, float[] superInputs) {
        //finalInputs (escluso bias)
        float[] finalInputs = new float[31];
        raycastInputs.CopyTo(finalInputs, 0);
        nearestEnemyInputs.CopyTo(finalInputs, raycastInputs.Length);
        selfInputs.CopyTo(finalInputs, raycastInputs.Length + nearestEnemyInputs.Length);
        superInputs.CopyTo(finalInputs, raycastInputs.Length + nearestEnemyInputs.Length + selfInputs.Length);

        int requiredLength = myNeuralNetwork.inputNeurons.neurons.Length - 1;
        if (raycastInputs.Length + nearestEnemyInputs.Length + selfInputs.Length + superInputs.Length != requiredLength) {
            throw new Exception("Total inputs must have length " + requiredLength);
        }
        return finalInputs;
    }

    void NNOutputsToMotions(float[] outputs) {
        if(outputs.Length != 7) {
            throw new Exception("Outputs length must be equal to 8");
        }

        //controllo horizontal movements nei primi 0-2 bits
        int maxIndexHMovement = -1;
        float maxValueHMovement = -1;
        for(int i=0; i < 3; i++) {
            if(outputs[i] >= maxValueHMovement) {
                maxIndexHMovement = i;
                maxValueHMovement = outputs[i];
            }
        }
        if(maxIndexHMovement != 2) {
            int moveDirection = maxIndexHMovement == 0 ? 1 : -1;
            characterMotions.HorizontalMove(moveDirection);
        }
        else {
            characterMotions.ContrastMovement();
        }

        //controllo azione nei bits da 3 a 6
        int maxIndexAction = -1;
        float maxValueAction = -1;
        for(int i= 3; i < 7; i++) {
            if(outputs[i] >= maxValueAction) {
                maxValueAction = outputs[i];
                maxIndexAction = i;
            }
        }
        switch (maxIndexAction) {
            case 3: {
                    characterMotions.Punch();
                    break;
                }
            case 4: {
                    characterMotions.Dash();
                    break;
                }
            case 5: {
                    characterMotions.StartRapidDown();
                    break;
                }
            case 6: {
                    characterMotions.Jump();
                    break;
                }
            default: {
                    throw new Exception("Output not implemented");
                }
        }

    }
}
