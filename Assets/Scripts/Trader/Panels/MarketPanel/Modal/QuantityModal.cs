using System;
using UnityEngine;
using UnityEngine.UI;

public class QuantityModal : Modal {

    public new event Action<int> OnSubmit = delegate { };

    [SerializeField] private Slider quantitySlider;
    [SerializeField] private Text quantityField;

    private const int DefaultQuantity = 100;

    private void Awake() {
        quantitySlider.onValueChanged.AddListener(OnQuantityChange);
        quantityField.text = DefaultQuantity.ToString();
    }

    protected override void Update() {
        base.Update();

        if (Input.GetKeyUp(KeyCode.RightArrow)) {
            quantitySlider.value += 1;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
            quantitySlider.value -= 1;
        }
    }

    protected override void OnOkButtonClicked() {
        OnSubmit(int.Parse(quantityField.text));
    }

    private void OnQuantityChange(float value) {
        int quantity = DefaultQuantity + (DefaultQuantity * (int)value);
        quantityField.text = quantity.ToString();
    }

}
