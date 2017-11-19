using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trader : MonoBehaviour {

    public StockMarket Market;
    public Player Player;
    public MarketPanel MarketPanel;

    private void Awake() {
        Player.OnAllPositionsClosed += HandleAllPositionsClosed;
    }

    private void Start() {
        DisplayInitialMessages();
        Market.Initialize();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            if (Market.CurrentState == MarketState.PreOpen) {
                Market.BeginDay();
            }   
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) {
            if (MarketPanel.CurrentContext == MarketPanelContext.Idle) {
                Debug.Log("escape");
            }
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
        Player.SaveBalance();
        GameData.IncrementDayCount();
    }

}
