using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageCentral {

    private static MessageCentral _instance = null;

    public static MessageCentral Instance {
        get {
            if (_instance == null) {
                _instance = new MessageCentral();
            }
            return _instance;
        }
    }

    private MessagePanel panel;

    public MessageCentral() {
        var messagePanel = GameObject.FindWithTag("MessagePanel");
        if (messagePanel != null) {
            panel = messagePanel.GetComponent<MessagePanel>();
        }
        else {
            Debug.Log("Message Central: no MessagePanel object found");
        }
    }

    public void DisplayMessage(string type, string message) {
        if (panel != null) {
            panel.DisplayMessage(type, message);
        }
        else {
            Log(type, new string[] { message });
        }
    }

    public void DisplayMessages(string type, string[] messages, bool separator = false, bool delayFirst = false) {
        if (panel != null) {
            panel.StartMessageQueue(type, messages, separator, delayFirst);
        }
        else {
            Log(type, messages);
        }
    }

    private void Log(string type, string[] messages) {
        for (int i = 0; i < messages.Length; i++) {
            Debug.Log(
                string.Format("{0}: {1}", type, messages[i])
            );
        }
    }

    public void Destroy() {
        _instance = null;
    }

}
