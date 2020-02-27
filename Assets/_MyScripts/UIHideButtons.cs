using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHideButtons : MonoBehaviour
{
    public GameObject[] hideIfKeyboard;
    public bool editorForceButtonEnable;
    // Start is called before the first frame update
    void Start()
    {
        if(!(Application.isEditor && editorForceButtonEnable)) {
            if (!Input.touchSupported) {
                foreach (GameObject hide in hideIfKeyboard) {
                    hide.SetActive(false);
                }
            }
        }
    }
}
