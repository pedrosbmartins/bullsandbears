using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MessageItem : MonoBehaviour {

    public Text MetaField;
    public Text MessageField;

    public void Setup(string type, string message) {
        MetaField.text = FormatMetaData(type);
        MessageField.text = message;
    }

    public void UpdateMessage(string message) {
        MessageField.text = message;
    }

    private string FormatMetaData(string type) {
        return String.Format("> [{0}]", type);
    }

}
