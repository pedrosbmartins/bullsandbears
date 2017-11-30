using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MessageItem : MonoBehaviour {

    [SerializeField] private Text metaField;
    [SerializeField] private Text messageField;

    public void Setup(string type, string message) {
        metaField.text = FormatMetaData(type);
        messageField.text = message;
    }

    public void UpdateMessage(string message) {
        messageField.text = message;
    }

    private string FormatMetaData(string type) {
        return String.Format("> [{0}]", type);
    }

}
