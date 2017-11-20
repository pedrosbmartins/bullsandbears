using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitModal : Modal {

    public delegate void ExitModalSubmitHandler();
    public event ExitModalSubmitHandler OnSubmit = delegate {};

    public Text Message;

    public override void Setup(string info) { }

    protected override void OnOkButtonClicked() {
        OnSubmit();
    }

    public void SetMessage(bool progressSaved) {
        if (progressSaved) {
            Message.text = "Exit to terminal?";
        }
        else {
            Message.text = "You'll lose this day progress. Continue?";
        }
    }

}
