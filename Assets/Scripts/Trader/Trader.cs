using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Trader : MonoBehaviour {

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

    private StockMarket market;
    private Player player;
    private AudioSource music;
    private MarketPanel marketPanel;

    private bool isDisplayingDayCount = false;
    private bool isModalOpened = false; 
    private bool isProgressSaved = false;

    private float fastMusicPitch = 1.2f;
    private float normalMusicPitch = 1f;

    private List<Achievement> CreateAchievementList() {
        return null;
    }

    private void Awake() {
        market = GetComponent<StockMarket>();
        player = GetComponent<Player>();

        music = GetComponent<AudioSource>();
        market.OnDayEnded += HandleMarketDayEnded;
        player.OnAllPositionsClosed += HandleAllPositionsClosed;

        Instantiate(AccountPanelPrefab, LeftUIPanelContainer, false);
        marketPanel = Instantiate(MarketPanelPrefab, LeftUIPanelContainer, false);
        Instantiate(MessagePanelPrefab, RightUIPanelContainer, false);
        Instantiate(StockPanelPrefab, RightUIPanelContainer, false);
    }

    private void Start() {
        if (GameData.GetMusicOn()) {
            music.Play();
        }
        DisplayDayCount();
    }

    private void DisplayDayCount() {
        DayDisplay dayDisplay = Instantiate(DayDisplayPrefab, gameObject.transform, false);
        isDisplayingDayCount = true;
        dayDisplay.OnHide += RunTraderProgram;
    }

    private void RunTraderProgram() {
        isDisplayingDayCount = false;
        DisplayWelcomeMessages();
        market.Initialize();
    }

    private void Update() {
        CheckMusicPitch();
        CheckKeyboardInput();
    }

    private void CheckMusicPitch() {
        if (GameData.GetMusicOn()) {
            if (music.pitch == normalMusicPitch &&
                market.CurrentState == MarketState.DayStarted &&
                TwoHoursToEndDay()) {
                MessageCentral.Instance.DisplayMessages("Message", new string[] { "The market is closing in two hours!" }, true);
                music.pitch = fastMusicPitch;
            }
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
     * F4     Short        Market.DayStarted and     (marketpanel)
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
        CheckResetData();

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
            if (marketPanel.CurrentContext == MarketPanelContext.Idle) {
                DisplayExitModal();
            }
            else {
                marketPanel.CheckInput();
            }
        }
        else {
            marketPanel.CheckInput();
        }
    }

    private void CheckResetData() {
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyUp(KeyCode.Delete)) {
            GameData.Reset();
            Debug.Log("Game Data has been reset");
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
        music.pitch = normalMusicPitch;
        GetComponent<NewsSource>().Stop();
        marketPanel.DestroyOpenedModals();
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

    private void DisplayWelcomeMessages() {
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
        GameAchievements.Check(player.Account.Balance);
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
