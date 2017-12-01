using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Terminal : MonoBehaviour {

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private InputLine inputLine;

    [SerializeField] private TextLine textLinePrefab;
    [SerializeField] private Trader traderPrefab;

    private Monitor monitor;
    private RectTransform programsInterfaceContainer;
    private CommandHandler commandHandler;
    private Trader trader;

    private float bootDelayBase = 0.8f;
    private bool resetFlag = false;

    private void Awake() {
        monitor = GetComponentInParent<Monitor>();
        commandHandler = GetComponent<CommandHandler>();

        programsInterfaceContainer = transform.parent.GetComponent<RectTransform>();

        inputLine.OnSubmit += HandleSubmittedInput;
    }

    private void Start() {
        inputLine.Hide();
        StartCoroutine(DisplayBootMessage());
    }

    private void Update() {
        if (Input.GetKey(KeyCode.Return)) {
            bootDelayBase = 0; // speeds up boot
        }
    }

    public void DisplayTextLine(string text, bool displayMargin = false) {
        TextLine textLine = Instantiate(textLinePrefab, contentRect, false);
        textLine.Set(text);
        if (displayMargin) DisplayMargin();
        inputLine.transform.SetAsLastSibling();
        ScrollPanelToBottom();
    }

    public void DisplayMargin() {
        DisplayTextLine("");
    }

    private void DisplayInputLine(string input) {
        DisplayTextLine(String.Format("> {0}", input));
    }

    private void ScrollPanelToBottom() {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
        Canvas.ForceUpdateCanvases();
    }

    private void HandleSubmittedInput(string input) {
        DisplayInputLine(input);
        if (resetFlag && input == "yes") {
            ResetGameData();
        }
        else {
            resetFlag = false;
            commandHandler.CheckMatch(input);
        }
    }

    public void ChangeColor(int r, int g, int b) {
        if (monitor != null) {
            var color = new Color(r / 255f, g / 255f, b / 255f);
            if (IsVisibilityLow(color)) {
                DisplayTextLine(
                    String.Format("Selected color has low visibility, please choose another", r, g, b),
                    true
                );
                return;
            }
            DisplayTextLine("Terminal color changed", true);
            monitor.ChangeTerminalColor(color);
            GameData.SetTerminalColor(color);
        }
    }

    private bool IsVisibilityLow(Color color) {
        float threshold = 30f / 255f;
        return color.r <= threshold &&
               color.g <= threshold &&
               color.b <= threshold;
    }

    public void StartTraderProgram() {
        StartCoroutine(TraderBoot());
    }

    private IEnumerator TraderBoot() {
        inputLine.Hide();
        DisplayTextLine("Starting trader program...");
        yield return new WaitForSeconds(2f);
        RunTraderProgram();
        inputLine.Show();
        DisplayMargin();
    }

    private void RunTraderProgram() {
        gameObject.SetActive(false);
        trader = Instantiate(traderPrefab, programsInterfaceContainer, false);
        trader.OnExitProgram += HandleTraderExit;
        trader.OnRebootProgram += HandleTraderReboot;
    }

    private void HandleTraderExit() {
        trader.OnExitProgram -= HandleTraderExit;
        Destroy(trader.gameObject);
        trader = null;
        gameObject.SetActive(true);
        inputLine.Focus();
        ScrollPanelToBottom();
    }

    private void HandleTraderReboot() {
        trader.OnRebootProgram -= HandleTraderReboot;
        Destroy(trader.gameObject);
        RunTraderProgram();
    }

    public void Shutdown() {
        StartCoroutine(ShutdownCoroutine());
    }

    private IEnumerator ShutdownCoroutine() {
        inputLine.Hide();
        DisplayTextLine("Shutting down the system...");
        yield return new WaitForSeconds(2f);
        Application.Quit();
    }

    public void StartResetMode() {
        DisplayTextLine("Are you sure you want to reset all data?");
        DisplayTextLine("(Type 'yes', or ignore)", true);
        resetFlag = true;
    }

    private void ResetGameData() {
        GameData.Reset();
        if (monitor != null) {
            monitor.ChangeTerminalColor(GameData.GetTerminalColor());
        }
        DisplayTextLine("Data has been reset", true);
    }

    private IEnumerator DisplayBootMessage() {
        yield return new WaitForSeconds(bootDelayBase);
        DisplayTextLine("Bulls & Bears Terminal Version 1.0");
        yield return new WaitForSeconds(bootDelayBase / 2);
        DisplayTextLine("(C) Copyleft Bulls & Bears Inc. 1986", true);
        // yeah, I know this repeating lines look terrible, but it's meant to 
        // better speedup boot when pressing <enter> (which sets bootDelayBase to 0)
        // using 2 * bootDelayBase would basically 'lock' the boot for this amount of time
        yield return new WaitForSeconds(bootDelayBase);
        yield return new WaitForSeconds(bootDelayBase);
        DisplayTextLine("Booting from disk...");
        yield return new WaitForSeconds(bootDelayBase);
        yield return new WaitForSeconds(bootDelayBase);
        DisplayTextLine("Ok", true);
        yield return new WaitForSeconds(bootDelayBase / 2);
        DisplayTextLine("(type 'trader' to start, or 'help' for available commands)");
        inputLine.Show();
        inputLine.Focus();
    }

}