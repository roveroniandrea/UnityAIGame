using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIdamage : MonoBehaviour
{
    public RectTransform imageContainer;
    public RectTransform image;
    public Text damageText;

    public void InitDamage(float maxDamage, int scaleX) {
        transform.localScale = new Vector3(scaleX, 1, 1);
        damageText.transform.localScale = new Vector3(scaleX, 1, 1);
        UpdateDamage(0f, maxDamage);
    }

    public void UpdateDamage(float damage, float maxDamage) {
        float right = imageContainer.rect.width - damage * imageContainer.rect.width / maxDamage;
        image.offsetMax = new Vector2(-right, image.offsetMax.y);
        damageText.text = damage + " / " + maxDamage;
    }
}
