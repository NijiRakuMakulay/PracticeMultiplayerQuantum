using System;
using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalInput : MonoBehaviour {
    [SerializeField]PlayerInput controls;
    InputAction jumpButton;
    InputAction fireButton;
    InputAction XMove;
    InputAction YMove;

    private void OnEnable() {
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    public void PollInput(CallbackPollInput callback) {
        Quantum.Input i = new Quantum.Input();
        jumpButton = controls.currentActionMap.FindAction("Jump");
        fireButton = controls.currentActionMap.FindAction("Fire");
        XMove = controls.currentActionMap.FindAction("HorizontalMovement");
        YMove = controls.currentActionMap.FindAction("VerticalMovement");
        i.Jump = (short)(jumpButton.ReadValue<float>());
        i.FireTrigger = (short)(fireButton.ReadValue<float>());
        i.DirectionX = (short)(XMove.ReadValue<float>());
        i.DirectionY = (short)(YMove.ReadValue<float>());
        callback.SetInput(i, DeterministicInputFlags.Repeatable);
    }
}
