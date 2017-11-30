using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum CommandAction { Color, Help, Reset, Shutdown, Sound, Trader };

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

public class CommandHandler : MonoBehaviour {

    private Terminal terminal;

    private void Awake() {
        terminal = GetComponent<Terminal>();
    }

    public void CheckMatch(string input) {
        foreach (var command in commandList) {
            var regex = new Regex(command.Matcher, RegexOptions.None);
            var matches = regex.Matches(input);
            if (matches.Count > 0) {
                HandleAction(command.Action, matches);
                return;
            }
        }
        // couldn't find a match
        terminal.DisplayTextLine("No command found", true);
    }

    private void HandleAction(CommandAction action, MatchCollection matches) {
        switch (action) {
            case CommandAction.Color:
                HandleColorAction(matches);
                break;
            case CommandAction.Help:
                DisplayAllCommands();
                break;
            case CommandAction.Reset:
                terminal.StartResetMode();
                break;
            case CommandAction.Shutdown:
                terminal.Shutdown();
                break;
            case CommandAction.Sound:
                HandleSoundCommand(matches);
                break;
            case CommandAction.Trader:
                terminal.StartTraderProgram();
                break;
            default:
                terminal.DisplayTextLine("Error: Invalid command action", true);
                break;
        }
    }

    private void HandleColorAction(MatchCollection matches) {
        GroupCollection groups = matches[0].Groups;

        int r, g, b;
        bool parsed;
        parsed = int.TryParse(groups[2].Value, out r);
        parsed = int.TryParse(groups[3].Value, out g) && parsed;
        parsed = int.TryParse(groups[4].Value, out b) && parsed;

        if (parsed) {
            terminal.ChangeColor(r, g, b);
        }
        else {
            terminal.DisplayTextLine("Error: Could not parse terminal color", true);
        }
    }

    private void DisplayAllCommands() {
        commandList.ForEach(command => {
            string formattedName = command.Name.PadRight(11);
            terminal.DisplayTextLine(String.Format("{0}  {1}", formattedName, command.Description));
        });
        terminal.DisplayMargin();
    }

    private void HandleSoundCommand(MatchCollection matches) {
        GroupCollection groups = matches[0].Groups;
        string option = groups[2].Value;

        var music = false;
        var sfx = false;
        string message;

        switch (option) {
            case "on":
                music = true;
                sfx = true;
                message = "Sound has been turned on";
                break;
            case "off":
                message = "Sound has been turned off";
                break;
            case "music":
                music = true;
                message = "Only music has been turned on";
                break;
            case "sfx":
                message = "Only sound effects have been turned on";
                sfx = true;
                break;
            default:
                message = "There was an error with the sound command";
                break;
        }

        GameData.SetMusicOn(music);
        GameData.SetSFXOn(sfx);

        terminal.DisplayTextLine(message, true);
    }

    private List<Command> commandList = new List<Command>() {
        new Command("color r,g,b", @"^(color|c) (\d{1,3}),(\d{1,3}),(\d{1,3})$", "Set terminal color (0-255)",     CommandAction.Color),
        new Command("help",        @"^(help|h)$",                                "Display all available commands", CommandAction.Help),
        new Command("reset",       @"^(reset|r)$",                               "Reset all data",                 CommandAction.Reset),
        new Command("shutdown",    @"^(shutdown|sd)$",                           "Shutdown the system",            CommandAction.Shutdown),
        new Command("sound <opt>", @"^(sound|s) (on|off|music|sfx)$",            "Set sound (on,off,music,sfx)",   CommandAction.Sound),
        new Command("trader",      @"^(trader|t)$",                              "Run trader program",             CommandAction.Trader),
    };

}
