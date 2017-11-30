using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Modal : MonoBehaviour {

    public event Action OnExit = delegate { };
    public event Action OnSubmit = delegate { };

    [SerializeField] private Text titleField;
    [SerializeField] private Text messageField;
    [SerializeField] private Button okButton;

    protected Color defaultButtonColor = Color.white;
    protected Color pressedButtonColor = Color.gray;

    public void SetTitle(string title) {
        titleField.text = title;
    }

    public void SetMessage(string message) {
        messageField.text = message;
    }

    protected virtual void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            okButton.image.color = pressedButtonColor;
        }
        else if (Input.GetKeyUp(KeyCode.Return)) {
            StartCoroutine(OkButtonClick());
        }
        else if (Input.GetKeyUp(KeyCode.Escape)) {
            Exit();
        }
    }

    private void Exit() {
        Destroy(gameObject);
        OnExit();
    }

    protected IEnumerator OkButtonClick() {
        okButton.image.color = defaultButtonColor;
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
        OnOkButtonClicked();
    }

    protected virtual void OnOkButtonClicked() {
        OnSubmit();
    }

}
