using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SellModal : Modal {

    public delegate void SellModalSubmitHandler();
    public event SellModalSubmitHandler OnSubmit;

    public override void Setup(string stockSymbol) {
        Title.text = String.Format("SELL {0}", stockSymbol);
    }

    protected override void OnOkButtonClicked() {
        if (OnSubmit != null) OnSubmit();
    }

}
