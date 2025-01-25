using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputDirection {
    Up,
    Down,
    Left,
    Right
}

public class PlayerInputControls : EntityInput {
    public PlayerInputActions playerContols;

    private InputAction inputUp;
    private InputAction inputDown;
    private InputAction inputLeft;
    private InputAction inputRight;

    private void Awake() {
        playerContols = new PlayerInputActions();
    }

    private void OnEnable() {
        inputUp = playerContols.Player.InputUp;
        inputUp.Enable();
        inputUp.performed += UpInputPerformed;
        inputDown = playerContols.Player.InputDown;
        inputDown.Enable();
        inputDown.performed += DownInputPerformed;
        inputLeft = playerContols.Player.InputLeft;
        inputLeft.Enable();
        inputLeft.performed += LeftInputPerformed;
        inputRight = playerContols.Player.InputRight;
        inputRight.Enable();
        inputRight.performed += RightInputPerformed;
    }

    private void OnDisable() {
        inputUp.Disable();
        inputUp.performed -= UpInputPerformed;
        inputDown.Disable();
        inputDown.performed -= DownInputPerformed;
        inputLeft.Disable();
        inputLeft.performed -= LeftInputPerformed;
        inputRight.Disable();
        inputRight.performed -= RightInputPerformed;
    }

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
    }

    private void UpInputPerformed(InputAction.CallbackContext context) {
        GameManager.instance.CheckInput(index, InputDirection.Up);
    }

    private void DownInputPerformed(InputAction.CallbackContext context) {
        GameManager.instance.CheckInput(index, InputDirection.Down);
    }

    private void LeftInputPerformed(InputAction.CallbackContext context) {
        GameManager.instance.CheckInput(index, InputDirection.Left);
    }

    private void RightInputPerformed(InputAction.CallbackContext context) {
        GameManager.instance.CheckInput(index, InputDirection.Right);
    }
}
