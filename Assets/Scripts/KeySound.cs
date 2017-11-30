using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySound : MonoBehaviour {

    [SerializeField] private AudioClip keyDownClip;
    [SerializeField] private AudioClip keyUpClip;

    [SerializeField] private AudioClip alternativeKeyDownClip;
    [SerializeField] private AudioClip alternativeKeyUpClip;

    private void Update () {
        if (GameData.GetSFXOn()) {
            CheckKeyPress();
        }
    }

    private void CheckKeyPress() {
        pressableKeys.ForEach(key => {
            if (Input.GetKeyDown(key)) {
                AudioSource.PlayClipAtPoint(keyDownClip, Vector2.one);
            }
            if (Input.GetKeyUp(key)) {
                AudioSource.PlayClipAtPoint(keyUpClip, Vector2.one);
            }
        });
        pressableAlternativeKeys.ForEach(key => {
            if (Input.GetKeyDown(key)) {
                AudioSource.PlayClipAtPoint(alternativeKeyDownClip, Vector2.one);
            }
            if (Input.GetKeyUp(key)) {
                AudioSource.PlayClipAtPoint(alternativeKeyUpClip, Vector2.one);
            }
        });
    }

    private List<KeyCode> pressableKeys = new List<KeyCode>() {
        KeyCode.Q, KeyCode.W, KeyCode.E,  KeyCode.R,  KeyCode.T,  KeyCode.Y,
        KeyCode.U, KeyCode.I, KeyCode.O,  KeyCode.P,  KeyCode.A,  KeyCode.S,
        KeyCode.D, KeyCode.F, KeyCode.G,  KeyCode.H,  KeyCode.J,  KeyCode.K,
        KeyCode.L, KeyCode.Z, KeyCode.X,  KeyCode.C,  KeyCode.V,  KeyCode.B,
        KeyCode.N, KeyCode.M,
        KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha9, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
        KeyCode.F1, KeyCode.F2, KeyCode.F3,
        KeyCode.Return, KeyCode.Backspace, KeyCode.Escape
    };

    private List<KeyCode> pressableAlternativeKeys = new List<KeyCode>() {
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow
    };

}
