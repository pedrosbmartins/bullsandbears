using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Modal : MonoBehaviour {

    public delegate void ModalExitHandler();
    public event ModalExitHandler OnExit = delegate {};

    public Text Title;
    public Button OkButton;

    protected Color defaultButtonColor = Color.white;
    protected Color pressedButtonColor = Color.gray;

    public abstract void Setup(string info);

    protected virtual void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            OkButton.image.color = pressedButtonColor;
        }
        else if (Input.GetKeyUp(KeyCode.Return)) {
            StartCoroutine(OkButtonClick());
        }
        else if (Input.GetKeyUp(KeyCode.Escape)) {
            Destroy(gameObject);
            OnExit();
        }
    }

    protected IEnumerator OkButtonClick() {
        OkButton.image.color = defaultButtonColor;
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
        OnOkButtonClicked();
    }

    protected abstract void OnOkButtonClicked();

}
