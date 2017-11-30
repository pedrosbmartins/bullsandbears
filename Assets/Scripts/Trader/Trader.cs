using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Trader : MonoBehaviour {

    [SerializeField] private AlertModal alertModalPrefab;
    [SerializeField] private DayDisplay dayDisplayPrefab;
    [SerializeField] private MarketPanel marketPanelPrefab;
    [SerializeField] private StockPanel stockPanelPrefab;
    [SerializeField] private MessagePanel messagePanelPrefab;
    [SerializeField] private AccountPanel accountPanelPrefab;

    [SerializeField] private RectTransform leftUIPanelContainer;
    [SerializeField] private RectTransform rightUIPanelContainer;

    public event Action OnExitProgram = delegate { };
    public event Action OnRebootProgram = delegate { };

    private StockMarket market;
    private Player player;
    private AudioSource music;
    private MarketPanel marketPanel;

    private bool isDisplayingDayCount = false;
    private bool isModalOpened = false; 
    private bool isProgressSaved = false;

    private float normalMusicPitch = 1f;
    private float highMusicPitch = 1.2f;

    private void Awake() {
        market = GetComponent<StockMarket>();
        player = GetComponent<Player>();
        music = GetComponent<AudioSource>();

        market.OnDayEnded += HandleMarketDayEnded;
        player.OnAllPositionsClosed += HandleAllPositionsClosed;

        InstantiatePanels();
    }

    private void InstantiatePanels() {
        Instantiate(accountPanelPrefab, leftUIPanelContainer, false);
        marketPanel = Instantiate(marketPanelPrefab, leftUIPanelContainer, false);
        Instantiate(messagePanelPrefab, rightUIPanelContainer, false);
        Instantiate(stockPanelPrefab, rightUIPanelContainer, false);
    }

    private void Start() {
        if (GameData.GetMusicOn()) {
            music.Play();
        }
        DisplayDayCount();
    }

    private void DisplayDayCount() {
        DayDisplay dayDisplay = Instantiate(dayDisplayPrefab, gameObject.transform, false);
        isDisplayingDayCount = true;
        dayDisplay.OnFinish += InitializeTraderProgram;
    }

    private void InitializeTraderProgram() {
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
            if (music.pitch == normalMusicPitch && IsMarketActiveAndDayEnding()) {
                // increase pitch when market day is ending
                // to give player a sense of rushing
                MessageCentral.Instance.DisplayMessages("Message", new string[] { "The market is closing in two hours!" }, true);
                music.pitch = highMusicPitch;
            }
            else if (music.pitch == highMusicPitch && market.CurrentState == MarketState.DayEnded) {
                music.pitch = normalMusicPitch;
            }
        }
    }

    private bool IsMarketActiveAndDayEnding() {
        return market.CurrentState == MarketState.DayStarted 
            && TwoHoursToEndDay();
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
     * ---    ------       -------                   -----------
     * F1     Help         Any                       MarketPanel
     * F2     Buy          Market.DayStarted and     MarketPanel
     *                     MarketPanel.RowSelected
     * F3     Sell         Market.DayStarted and     MarketPanel
     *                     MarketPanel.RowSelected
     * F4     Short        Market.DayStarted and     MarketPanel
     *                     MarketPanel.RowSelected
     *                     
     * Enter  OpenMarket   Market.Idle               Trader (this script)
     *        NewDay       Market.Closed             Trader (this script)
     *        SubmitModal  isModalOpened             Modals
     *        SkipDayCount isDisplayingDayCount      DayDisplay
     * 
     * Esc    ExitModal    isModalOpened             Modals
     *        ExitProgram  MarketPanel.Idle          Trader (this script)
     *        DiselectRow  RowSelected               MarketPanel
     *  
     * UpDown ChangeRows   Any                       MarketPanel
     * < >    Quantity     isModalOpened             BuyModal
     * 
     */
    private void CheckKeyboardInput() {
        CheckResetDataKeys();

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

    private void CheckResetDataKeys() {
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyUp(KeyCode.Delete)) {
            GameData.Reset();
            Debug.Log("Game Data has been reset");
        }
    }

    private void DisplayModal(Modal prefab, string title, string message, Action onSubmit, Action onExit) {
        Modal modal = Instantiate(prefab, transform.root, false);
        modal.SetTitle(title);
        modal.SetMessage(message);
        modal.OnSubmit += onSubmit;
        modal.OnExit += onExit;
        isModalOpened = true;
    }

    private void DisplayExitModal() {
        string message = isProgressSaved ? "Exit to terminal?" : "You'll lose this day progress. Continue?";
        DisplayModal(alertModalPrefab, "EXIT", message, ExitProgram, HandleModalExit);
    }

    private void DisplayMarketEndedModal() {
        var title = "MARKET CLOSED";
        var message = "Any open positions will be closed";
        DisplayModal(alertModalPrefab, title, message, HandleMarketEndedModalClosed, HandleMarketEndedModalClosed);
    }

    private void HandleMarketEndedModalClosed() {
        HandleModalExit();
        player.CloseAllPositions();
    }

    private void HandleModalExit() {
        isModalOpened = false;
    }

    private void HandleMarketDayEnded() {
        GetComponent<NewsSource>().Stop();
        marketPanel.DestroyOpenedModals();
        DisplayMarketEndedModal();
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
        player.SaveAccountBalance();
        GameAchievements.Check(player.Account.Balance);
        GameData.IncrementDayCount();
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
