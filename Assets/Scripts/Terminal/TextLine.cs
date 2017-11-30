using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextLine : MonoBehaviour {

    [SerializeField] private Text textField;

	public void Set(string text) {
        textField.text = text;
    }

}
