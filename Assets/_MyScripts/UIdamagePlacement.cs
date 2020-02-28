using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIdamagePlacement : MonoBehaviour
{
    public UIdamage uIdamagePrefab;
    List<UIdamage> instantiatedUIdamage;
    public Vector2 cornerDistance;
    public float newLineDistance;

    private void Start() {
        instantiatedUIdamage = new List<UIdamage>();
    }

    public UIdamage CreateUIdamage(float maxDamage) {
        Rect canvasRect = GetComponent<RectTransform>().rect;
        UIdamage uIdamage = Instantiate(uIdamagePrefab, transform.position, Quaternion.identity, transform);
        instantiatedUIdamage.Add(uIdamage);
        float x = instantiatedUIdamage.Count % 2 == 0 ? cornerDistance.x : canvasRect.width - cornerDistance.x;
        float y = cornerDistance.y + (instantiatedUIdamage.Count / 2) * newLineDistance;
        Vector3 topLeftCorner = new Vector3(transform.position.x - canvasRect.width / 2f, transform.position.y + canvasRect.height / 2f, 0);
        uIdamage.transform.position = topLeftCorner + new Vector3(x, -y);
        if(instantiatedUIdamage.Count % 2 == 1) {
            uIdamage.InitDamage(maxDamage, -1);
        }
        else {
            uIdamage.InitDamage(maxDamage, 1);
        }
        return uIdamage;
    }
}
