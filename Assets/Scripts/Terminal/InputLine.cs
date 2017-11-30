using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputLine : MonoBehaviour {

    [SerializeField] private InputField inputField;

    public event Action<string> OnSubmit = delegate { };

    public void Focus() {
        inputField.ActivateInputField();
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void OnEditEnd() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            OnSubmit(inputField.text);
            inputField.text = "";
        }
        Focus();
    }

}
