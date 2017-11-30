using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingItem : MonoBehaviour {

    [SerializeField] private Text keyField;
    [SerializeField] private Text actionField;

    public void SetBinding(KeyBinding keyBinding) {
        keyField.text = keyBinding.Key.ToString();
        actionField.text = keyBinding.Action.ToString();
    }

}
