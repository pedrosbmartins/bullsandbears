using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Terminal : MonoBehaviour {

    private float bootDelayBase = 0.8f; // not const so you can speedup boot with <enter>

    public RectTransform Screen;
    public ScrollRect Scroll;
    public RectTransform Content;
    public TextLine TextLinePrefab;
    public Trader TraderPrefab;

    public Trader Trader;
    public InputLine InputLine;
    public Monitor Monitor;

    private List<Command> commandList;

    private bool resetFlag = false;

    private void Awake() {
        InputLine.OnSubmit += HandleSubmittedInput;
        commandList = GenerateCommandList();
    }

    private void Start() {
        InputLine.Hide();
        StartCoroutine(DisplayBootMessage());
    }

    private void Update() {
        if (Input.GetKey(KeyCode.Return)) {
            bootDelayBase = 0;
        }
    }

    private void DisplayTextLine(string text, bool displayMargin = false) {
        TextLine textLine = Instantiate(TextLinePrefab, Content, false);
        textLine.Set(text);
        if (displayMargin) DisplayMargin();
        InputLine.transform.SetAsLastSibling();
        ScrollPanelToBottom();
    }

    private void ScrollPanelToBottom() {
        Canvas.ForceUpdateCanvases();
        Scroll.verticalNormalizedPosition = 0;
        Canvas.ForceUpdateCanvases();
    }

    private void DisplayMargin() {
        DisplayTextLine("");
    }

    private void DisplayInputLine(string input) {
        DisplayTextLine(String.Format("> {0}", input));
    }

    private void HandleSubmittedInput(string input) {
        DisplayInputLine(input);
        if (resetFlag && input == "yes") {
            ResetGameData();
        }
        else {
            resetFlag = false;
            CheckForCommandMatch(input);
        }
    }

    private void CheckForCommandMatch(string input) {
        foreach (var command in commandList) {
            var regex = new Regex(command.Matcher, RegexOptions.None);
            var matches = regex.Matches(input);
            if (matches.Count > 0) {
                HandleCommand(command.Action, matches);
                return;
            }
        }
        // couldn't find a match
        DisplayTextLine("No command found", true);
    }

    private void HandleCommand(CommandAction action, MatchCollection matches) {
        switch (action) {
            case CommandAction.Color:
                HandleColorCommand(matches);
                break;
            case CommandAction.Help:
                DisplayAllCommands();
                break;
            case CommandAction.Reset:
                HandleResetCommand();
                break;
            case CommandAction.Trader:
                HandleTraderCommand();
                break;
            case CommandAction.Shutdown:
                HandleShutdownCommand();
                break;
            default:
                DisplayTextLine("Error: Invalid command action", true);
                break;
        }
    }

    private void HandleColorCommand(MatchCollection matches) {
        GroupCollection groups = matches[0].Groups;

        int r, g, b;
        bool parsed;
        parsed = int.TryParse(groups[2].Value, out r);
        parsed = int.TryParse(groups[3].Value, out g) && parsed;
        parsed = int.TryParse(groups[4].Value, out b) && parsed;

        if (parsed) {
            ChangeTerminalColor(r, g, b);
        }
        else {
            DisplayTextLine("Error: Could not parse terminal color", true);
        }
    }

    private void ChangeTerminalColor(int r, int g, int b) {
        if (Monitor != null) {
            var color = new Color(r / 255f, g / 255f, b / 255f);
            if (IsVisibilityLow(color)) {
                DisplayTextLine(
                    String.Format("Selected color has low visibility, please choose another", r, g, b),
                    true
                );
                return;
            }
            DisplayTextLine("Terminal color changed", true);
            Monitor.ChangeTerminalColor(color);
            GameData.SetTerminalColor(color);
        }
    }

    private bool IsVisibilityLow(Color color) {
        float threshold = 30f / 255f;
        return color.r <= threshold &&
               color.g <= threshold &&
               color.b <= threshold;
    }

    private void DisplayAllCommands() {
        commandList.ForEach(command => {
            string formattedName = command.Name.PadRight(11);
            DisplayTextLine(String.Format("{0}  {1}", formattedName, command.Description));
        });
        DisplayMargin();
    }

    private void HandleTraderCommand() {
        StartCoroutine(TraderBoot());
    }

    private IEnumerator TraderBoot() {
        InputLine.Hide();
        DisplayTextLine("Starting trader program...");
        yield return new WaitForSeconds(2f);
        RunTraderProgram();
        InputLine.Show();
        DisplayMargin();
    }

    private void RunTraderProgram() {
        gameObject.SetActive(false);
        Trader = Instantiate(TraderPrefab, Screen, false);
        Trader.OnExitProgram += HandleTraderExit;
        Trader.OnRebootProgram += HandleTraderReboot;
    }

    private void HandleTraderExit() {
        Trader.OnExitProgram -= HandleTraderExit;
        Destroy(Trader.gameObject);
        gameObject.SetActive(true);
        InputLine.Focus();
    }

    private void HandleTraderReboot() {
        Trader.OnRebootProgram -= HandleTraderReboot;
        Destroy(Trader.gameObject);
        RunTraderProgram();
    }

    private void HandleShutdownCommand() {
        StartCoroutine(Shutdown());
    }

    private IEnumerator Shutdown() {
        InputLine.Hide();
        DisplayTextLine("Shutting down the system...");
        yield return new WaitForSeconds(2f);
        Application.Quit();
    }

    private void HandleResetCommand() {
        DisplayTextLine("Are you sure you want to reset all data?");
        DisplayTextLine("(Type 'yes', or ignore)", true);
        resetFlag = true;
    }

    private void ResetGameData() {
        GameData.Reset();
        if (Monitor != null) {
            Monitor.ChangeTerminalColor(GameData.GetTerminalColor());
        }
        DisplayTextLine("Data has been reset", true);
    }

    private IEnumerator DisplayBootMessage() {
        yield return new WaitForSeconds(bootDelayBase);
        DisplayTextLine("Bulls & Bears Terminal Version 1.0");
        yield return new WaitForSeconds(bootDelayBase / 2);
        DisplayTextLine("(C) Copyleft Bulls & Bears Inc. 1986", true);
        // yeah, I know this repeating lines look terrible, 
        // but it is meant to better speedup boot when pressing <enter>
        yield return new WaitForSeconds(bootDelayBase);
        yield return new WaitForSeconds(bootDelayBase);
        DisplayTextLine("Booting from disk...");
        yield return new WaitForSeconds(bootDelayBase);
        yield return new WaitForSeconds(bootDelayBase);
        DisplayTextLine("Ok", true);
        yield return new WaitForSeconds(bootDelayBase / 2);
        DisplayTextLine("(type 'trader' to start, or 'help' for available commands)");
        InputLine.Show();
        InputLine.Focus();
    }

    private List<Command> GenerateCommandList() {
        return new List<Command>() {
            new Command("color r,g,b", @"^(color|c) (\d{1,3}),(\d{1,3}),(\d{1,3})$", "Set terminal color (0-255)", CommandAction.Color),
            new Command("help", @"^(help|h)$", "Display all available commands", CommandAction.Help),
            new Command("reset", @"^(reset|r)$", "Reset all data", CommandAction.Reset),
            new Command("shutdown", @"^(shutdown|s)$", "Shutdown the system", CommandAction.Shutdown),
            new Command("trader", @"^(trader|t)$", "Run trader program", CommandAction.Trader),       
        };
    }

}

public class Command {

    public readonly string Name;
    public readonly string Matcher;
    public readonly string Description;
    public readonly CommandAction Action;

    public Command(string name, string matcher, string description, CommandAction action) {
        Name = name;
        Matcher = matcher;
        Description = description;
        Action = action;
    }

}

public enum CommandAction { Color, Help, Reset, Shutdown, Trader };
