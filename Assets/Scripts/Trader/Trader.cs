using System;
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
    public AudioClip MarketClosingMusic;

    public delegate void ExitProgramHandler();
    public event ExitProgramHandler OnExitProgram = delegate { };

    public delegate void RebootProgramHandler();
    public event RebootProgramHandler OnRebootProgram = delegate { };

    private AudioSource Music;

    private bool isDisplayingDayCount = false;
    private bool isModalOpened = false; 
    private bool isProgressSaved = false;

    private float fastMusicPitch = 1.2f;
    private float normalMusicPitch = 1f;

    private void Awake() {
        Music = GetComponent<AudioSource>();
        Market.OnDayEnded += HandleMarketDayEnded;
        Player.OnAllPositionsClosed += HandleAllPositionsClosed;
    }

    private void Start() {
        DisplayDayCount();
    }

    private void DisplayDayCount() {
        DayDisplay dayDisplay = Instantiate(DayDisplayPrefab, gameObject.transform, false);
        isDisplayingDayCount = true;
        dayDisplay.OnHide += RunTraderProgram;
    }

    private void RunTraderProgram() {
        isDisplayingDayCount = false;
        DisplayInitialMessages();
        Market.Initialize();
    }

    private void Update() {
        CheckMusicPitch();
        CheckKeyboardInput();
    }

    private void CheckMusicPitch() {
        if (Music.pitch == normalMusicPitch && 
            Market.CurrentState == MarketState.DayStarted && 
            TwoHoursToEndDay()) {
            MessageCentral.Instance.DisplayMessages("Message", new string[] { "The market is closing in two hours!" }, true);
            Music.pitch = fastMusicPitch;
        }
    }

    private bool TwoHoursToEndDay() {
        DateTime closingTime = Market.TimeTracker.MarketCloseTime;
        TimeSpan hoursToEndDay = closingTime.Subtract(Market.GetCurrentTime());
        return hoursToEndDay <= TimeSpan.FromHours(2);
    }

    /**
     * Possible inputs:
     * 
     * Key    Action       Context                   Responsible
     * F1     Help         Any                       (marketpanel)
     * F2     Buy          Market.DayStarted and     (marketpanel)
     *                     MarketPanel.RowSelected
     * F3     Sell         Market.DayStarted and     (marketpanel)
     *                     MarketPanel.RowSelected
     *                     
     * Enter  OpenMarket   Market.Idle               ######## here ########
     *        NewDay       Market.Closed             ######## here ########
     *        SubmitModal  isModalOpened             (modal)
     *        SkipDayCount isDisplayingDayCount      (daydisplay)
     * 
     * Esc    ExitModal    isModalOpened             (modal)
     *        ExitProgram  MarketPanel.Idle          ######## here ########
     *        DiselectRow  RowSelected               (marketpanel)
     *  
     * UpDown ChangeRows   Any                       (marketpanel)
     * < >    Quantity     isModalOpened             (buymodal)
     * 
     */
    private void CheckKeyboardInput() {
        if (isModalOpened || isDisplayingDayCount) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return)) {
            if (Market.CurrentState == MarketState.Idle) {
                Market.BeginDay();
            }
            else if (Market.CurrentState == MarketState.Closed) {
                RebootProgram();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Escape)) {
            if (MarketPanel.CurrentContext == MarketPanelContext.Idle) {
                DisplayExitModal();
            }
            else {
                MarketPanel.CheckInput();
            }
        }
        else {
            MarketPanel.CheckInput();
        }
    }

    private void DisplayExitModal() {
        AlertModal alertModal = Instantiate(AlertModalPrefab, ModalContainer, false);
        string message = isProgressSaved ? "Exit to terminal?" : "You'll lose this day progress. Continue?";
        alertModal.SetTitle("EXIT");
        alertModal.SetMessage(message);
        alertModal.OnExit += HandleModalExit;
        alertModal.OnSubmit += ExitProgram;
        isModalOpened = true;
    }

    private void HandleMarketDayEnded() {
        Music.pitch = normalMusicPitch;
        MarketPanel.DestroyOpenedModals();
        DisplayMarketEndedModal();
    }

    private void DisplayMarketEndedModal() {
        AlertModal alertModal = Instantiate(AlertModalPrefab, ModalContainer, false);
        alertModal.SetTitle("MARKET CLOSED");
        alertModal.SetMessage("Any open positions will be closed");
        alertModal.OnExit += HandleMarketClosed;
        alertModal.OnSubmit += HandleMarketClosed;
        isModalOpened = true;
    }

    private void HandleMarketClosed() {
        HandleModalExit();
        Player.CloseAllPositions();
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
