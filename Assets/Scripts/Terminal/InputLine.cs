using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputLine : MonoBehaviour {

    public InputField Field;

    public delegate void OnSubmitHandler(string command);
    public event OnSubmitHandler OnSubmit = delegate {};

	public void Focus() {
        Field.ActivateInputField();
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void OnEditEnd() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            OnSubmit(Field.text);
            Field.text = "";
            Focus();
        }
        else {
            Focus();
        }
    }

}
