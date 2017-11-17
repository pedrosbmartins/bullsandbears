using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyHelperItem : MonoBehaviour {

    public Text KeyField;
    public Text ActionField;

    public void SetBinding(KeyBinding keyBinding) {
        KeyField.text = keyBinding.Key.ToString();
        ActionField.text = keyBinding.Action.ToString();
    }

}
