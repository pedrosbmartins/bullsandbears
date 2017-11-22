using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trader : MonoBehaviour {

    public StockMarket Market;
    public Player Player;
    public MarketPanel MarketPanel;
    public RectTransform ModalContainer;
    public AlertModal AlertModalPrefab;
    public DayDisplay DayDisplayPrefab;

    public delegate void ExitProgramHandler();
    public event ExitProgramHandler OnExitProgram = delegate { };

    public delegate void RebootProgramHandler();
    public event RebootProgramHandler OnRebootProgram = delegate { };

    private bool isDisplayingDayCount = true;
    private bool isModalOpened = false;
    private bool isProgressSaved = false;

    private void Awake() {
        Player.OnAllPositionsClosed += HandleAllPositionsClosed;
    }

    private void Start() {
        DayDisplay dayDisplay = Instantiate(DayDisplayPrefab, gameObject.transform, false);
        dayDisplay.OnHide += StartTraderProgram;
    }

    private void StartTraderProgram() {
        isDisplayingDayCount = false;
        DisplayInitialMessages();
        Market.Initialize();
    }

    private void Update() {
        if (!isModalOpened && !isDisplayingDayCount) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                if (Market.CurrentState == MarketState.PreOpen) {
                    Market.BeginDay();
                }
                else if (Market.CurrentState == MarketState.Closed) {
                    RebootProgram();
                }
            }

            if (MarketPanel.CurrentContext == MarketPanelContext.Idle) {
                if (Input.GetKeyUp(KeyCode.Escape)) {
                    AlertModal alertModal = Instantiate(AlertModalPrefab, ModalContainer, false);
                    string message = isProgressSaved ? "Exit to terminal?" : "You'll lose this day progress. Continue?";
                    alertModal.SetTitle("EXIT");
                    alertModal.SetMessage(message);
                    alertModal.OnExit += HandleModalExit;
                    alertModal.OnSubmit += ExitProgram;
                    isModalOpened = true;
                }
            }

            MarketPanel.CheckInput();
        }
    }

    private void OnDestroy() {
        MessageCentral.Instance.Destroy();
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

    private void RebootProgram() {
        isModalOpened = false;
        OnRebootProgram();
    }

}
