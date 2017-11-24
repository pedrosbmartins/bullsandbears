using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Modal : MonoBehaviour {

    public delegate void ModalExitHandler();
    public event ModalExitHandler OnExit = delegate {};

    public delegate void SellModalSubmitHandler();
    public event SellModalSubmitHandler OnSubmit = delegate {};

    public Text Title;
    public Text Message;
    public Button OkButton;

    protected Color defaultButtonColor = Color.white;
    protected Color pressedButtonColor = Color.gray;

    public void SetTitle(string title) {
        Title.text = title;
    }

    public void SetMessage(string message) {
        Message.text = message;
    }

    protected virtual void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            OkButton.image.color = pressedButtonColor;
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
        OkButton.image.color = defaultButtonColor;
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
        OnOkButtonClicked();
    }

    protected virtual void OnOkButtonClicked() {
        OnSubmit();
    }

}
