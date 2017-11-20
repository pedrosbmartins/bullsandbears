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

    private MessagePanel Panel;

    public MessageCentral() {
        var messagePanel = GameObject.Find("MessagePanel");
        if (messagePanel != null) {
            Panel = messagePanel.GetComponent<MessagePanel>();
        }
        else {
            Debug.Log("Message Central: no MessagePanel object found");
        }
    }

    public void DisplayMessage(string type, string message) {
        if (Panel != null) {
            Panel.DisplayMessage(type, message);
        }
    }

    public void DisplayMessages(string type, string[] messages, bool separator = false, bool delayFirst = false) {
        if (Panel != null) {
            Panel.StartMessageQueue(type, messages, separator, delayFirst);
        }
    }

    public void Destroy() {
        _instance = null;
    }

}
