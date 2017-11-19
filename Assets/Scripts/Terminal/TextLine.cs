using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextLine : MonoBehaviour {

    public Text Field;

	public void Set(string text) {
        Field.text = text;
    }

}
