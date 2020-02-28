using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    CharacterMotions characterMotions;
    
    bool jumpButtonClicked = false;
    bool leftButtonDown = false;
    bool rightButtonDown = false;
    bool punchButtonClicked = false;
    bool dashButtonClicked = false;
    bool rapidDownButtonClicked = false;

    // Start is called before the first frame update
    void Start()
    {
        characterMotions = GetComponent<CharacterMotions>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.W)) {
            jumpButtonClicked = true;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            dashButtonClicked = true;
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            punchButtonClicked = true;
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            rapidDownButtonClicked = true;
        }
    }

    private void FixedUpdate() {
        float inputX = GetHorizontalInputs();
        if (inputX != 0) {
            characterMotions.HorizontalMove(inputX);
        }
        else {
            characterMotions.ContrastMovement();
        }

        if (jumpButtonClicked) {
            characterMotions.Jump();
        }
        if (dashButtonClicked) {
            characterMotions.Dash();
        }
        if (punchButtonClicked) {
            characterMotions.Punch();
        }
        if (rapidDownButtonClicked) {
            characterMotions.StartRapidDown();
        }

        jumpButtonClicked = false;
        dashButtonClicked = false;
        punchButtonClicked = false;
        rapidDownButtonClicked = false;
    }

    float GetHorizontalInputs() {
        float keyboardInputs = Input.GetAxisRaw("Horizontal");
        if(keyboardInputs != 0) {
            return keyboardInputs;
        }
        if(leftButtonDown && rightButtonDown) {
            return 0;
        }
        if (leftButtonDown) {
            return -1f;
        }
        if (rightButtonDown) {
            return 1f;
        }
        return 0;
    }

    public void LeftButtonDown() {
        leftButtonDown = true;
    }

    public void RightButtonDown() {
        rightButtonDown = true;
    }

    public void LeftButtonUp() {
        leftButtonDown = false;
    }

    public void RightButtonUp() {
        rightButtonDown = false;
    }

    public void JumpButtonClicked() {
        jumpButtonClicked = true;
    }

    public void PunchButtonClicked() {
        punchButtonClicked = true;
    }

    public void DashButtonClicked() {
        dashButtonClicked = true;
    }

    public void RapidDownButtonClicked() {
        rapidDownButtonClicked = true;
    }
}
