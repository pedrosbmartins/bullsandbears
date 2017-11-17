using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyModal : MarketModal {

    public delegate void BuyModalSubmitHandler(int quantity);
    public event BuyModalSubmitHandler OnSubmit;

    public Slider QuantitySlider;
    public Text QuantityField;

    private const int DEFAULT_QUANTITY = 100;

    private void Awake() {
        QuantitySlider.onValueChanged.AddListener(OnQuantityChange);
        QuantityField.text = DEFAULT_QUANTITY.ToString();
    }

    protected override void Update() {
        base.Update();

        if (Input.GetKeyUp(KeyCode.RightArrow)) {
            QuantitySlider.value += 1;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
            QuantitySlider.value -= 1;
        }
    }

    public override void Setup(string stockSymbol) {
        Title.text = $"BUY {stockSymbol}";
    }

    protected override void OnOkButtonClicked() {
        if (OnSubmit != null) OnSubmit(int.Parse(QuantityField.text));
    }

    private void OnQuantityChange(float value) {
        int quantity = DEFAULT_QUANTITY + (DEFAULT_QUANTITY * (int)value);
        QuantityField.text = quantity.ToString();
    }

}
