using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trader : MonoBehaviour {

    public StockMarket Market;
    public Player Player;
    public MarketPanel MarketPanel;
    public RectTransform ModalContainer;
    public ExitModal ExitModal;

    public delegate void ExitProgramHandler();
    public event ExitProgramHandler OnExitProgram = delegate {};

    private bool isModalOpened = false;
    private bool isProgressSaved = false;

    private void Awake() {
        Player.OnAllPositionsClosed += HandleAllPositionsClosed;
    }

    private void Start() {
        DisplayInitialMessages();
        Market.Initialize();
    }

    private void Update() {
        if (!isModalOpened) {
            if (Market.CurrentState == MarketState.PreOpen &&
                Input.GetKeyDown(KeyCode.Return)) {
                Market.BeginDay();
            }

            if (MarketPanel.CurrentContext == MarketPanelContext.Idle) {
                if (Input.GetKeyUp(KeyCode.Escape)) {
                    ExitModal modal = Instantiate(ExitModal, ModalContainer, false);
                    modal.SetMessage(isProgressSaved);
                    modal.OnExit += HandleModalExit;
                    modal.OnSubmit += ExitProgram;
                    isModalOpened = true;
                }
            }

            MarketPanel.CheckInput();
        }
    }

    private void DisplayInitialMessages() {
        var messages = new string[] {
            "Good morning, day traders",
            "The market will soon be open",
            "Press [space] to skip messages",
            "Press [F1] to display help",
            "Or press [enter] to start trading",
        };
        MessageCentral.Instance.DisplayMessages("Message", messages, false, true);
    }

    private void HandleAllPositionsClosed() {
        SaveData();
        DisplayDayEndedMessages();
    }

    private void DisplayDayEndedMessages() {
        var messages = new string[] {
            "Press [enter] to start a new day",
            "Or press [esc] to quit",
        };
        MessageCentral.Instance.DisplayMessages("Message", messages, true);
    }

    private void SaveData() {
        isProgressSaved = true;
        Player.SaveBalance();
        GameData.IncrementDayCount();
    }

    private void HandleModalExit() {
        isModalOpened = false;
    }

    private void ExitProgram() {
        isModalOpened = false;
        OnExitProgram();
    }

}
