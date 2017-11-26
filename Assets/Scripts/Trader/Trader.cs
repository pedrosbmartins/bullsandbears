using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trader : MonoBehaviour {

    private StockMarket market;
    private Player player;

    public MarketPanel MarketPanel;
    public AlertModal AlertModalPrefab;
    public DayDisplay DayDisplayPrefab;

    public MarketPanel MarketPanelPrefab;
    public StockPanel StockPanelPrefab;
    public MessagePanel MessagePanelPrefab;
    public AccountPanel AccountPanelPrefab;

    public RectTransform LeftUIPanelContainer;
    public RectTransform RightUIPanelContainer;

    public event Action OnExitProgram = delegate { };
    public event Action OnRebootProgram = delegate { };

    private AudioSource Music;

    private bool isDisplayingDayCount = false;
    private bool isModalOpened = false; 
    private bool isProgressSaved = false;

    private float fastMusicPitch = 1.2f;
    private float normalMusicPitch = 1f;

    private void Awake() {
        market = GetComponent<StockMarket>();
        player = GetComponent<Player>();

        Music = GetComponent<AudioSource>();
        market.OnDayEnded += HandleMarketDayEnded;
        player.OnAllPositionsClosed += HandleAllPositionsClosed;

        Instantiate(AccountPanelPrefab, LeftUIPanelContainer, false);
        MarketPanel = Instantiate(MarketPanelPrefab, LeftUIPanelContainer, false);
        Instantiate(MessagePanelPrefab, RightUIPanelContainer, false);
        Instantiate(StockPanelPrefab, RightUIPanelContainer, false);
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
        market.Initialize();
    }

    private void Update() {
        CheckMusicPitch();
        CheckKeyboardInput();
    }

    private void CheckMusicPitch() {
        if (Music.pitch == normalMusicPitch && 
            market.CurrentState == MarketState.DayStarted && 
            TwoHoursToEndDay()) {
            MessageCentral.Instance.DisplayMessages("Message", new string[] { "The market is closing in two hours!" }, true);
            Music.pitch = fastMusicPitch;
        }
    }

    private bool TwoHoursToEndDay() {
        DateTime closingTime = market.TimeTracker.MarketCloseTime;
        TimeSpan hoursToEndDay = closingTime.Subtract(market.GetCurrentTime());
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
            if (market.CurrentState == MarketState.Idle) {
                market.BeginDay();
            }
            else if (market.CurrentState == MarketState.Closed) {
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
        AlertModal alertModal = Instantiate(AlertModalPrefab, transform.root, false);
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
        AlertModal alertModal = Instantiate(AlertModalPrefab, transform.root, false);
        alertModal.SetTitle("MARKET CLOSED");
        alertModal.SetMessage("Any open positions will be closed");
        alertModal.OnExit += HandleMarketClosed;
        alertModal.OnSubmit += HandleMarketClosed;
        isModalOpened = true;
    }

    private void HandleMarketClosed() {
        HandleModalExit();
        player.CloseAllPositions();
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
        player.SaveBalance();
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
