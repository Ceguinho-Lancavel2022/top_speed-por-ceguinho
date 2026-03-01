using System.Collections.Generic;
using TopSpeed.Data;
using TopSpeed.Input;

namespace TopSpeed.Menu
{
    internal sealed partial class MenuRegistry
    {
        private MenuScreen BuildOptionsRaceSettingsMenu()
        {
            var items = new List<MenuItem>
            {
                new RadioButton(
                    "Copilot",
                    new[] { "off", "curves only", "all" },
                    () => (int)_settings.Copilot,
                    value => _settingsActions.UpdateSetting(() => _settings.Copilot = (CopilotMode)value),
                    hint: "Choose what information the copilot reports during the race. Use LEFT or RIGHT to change."),
                new Switch(
                    "Curve announcements",
                    "speed dependent",
                    "fixed distance",
                    () => _settings.CurveAnnouncement == CurveAnnouncementMode.SpeedDependent,
                    value => _settingsActions.UpdateSetting(() => _settings.CurveAnnouncement = value ? CurveAnnouncementMode.SpeedDependent : CurveAnnouncementMode.FixedDistance),
                    hint: "Switch between fixed distance and speed dependent curve announcements. Press ENTER to change."),
                new RadioButton(
                    "Automatic race information",
                    new[] { "off", "laps only", "on" },
                    () => (int)_settings.AutomaticInfo,
                    value => _settingsActions.UpdateSetting(() => _settings.AutomaticInfo = (AutomaticInfoMode)value),
                    hint: "Choose how much automatic race information is spoken, such as lap numbers and player positions. Use LEFT or RIGHT to change."),
                new Slider(
                    "Number of laps",
                    "1-16",
                    () => _settings.NrOfLaps,
                    value => _settingsActions.UpdateSetting(() => _settings.NrOfLaps = value),
                    hint: "Sets how many laps the session will be for single race, time trial, and multiplayer. Use LEFT or RIGHT to change by 1, PAGE UP or PAGE DOWN to change by 10, HOME for maximum, END for minimum."),
                new Slider(
                    "Number of computer players",
                    "1-7",
                    () => _settings.NrOfComputers,
                    value => _settingsActions.UpdateSetting(() => _settings.NrOfComputers = value),
                    hint: "Sets how many computer-controlled cars will race against you. Use LEFT or RIGHT to change by 1, PAGE UP or PAGE DOWN to change by 10, HOME for maximum, END for minimum."),
                new RadioButton(
                    "Single race difficulty",
                    new[] { "easy", "normal", "hard" },
                    () => (int)_settings.Difficulty,
                    value => _settingsActions.UpdateSetting(() => _settings.Difficulty = (RaceDifficulty)value),
                    hint: "Choose the difficulty level for single races. Use LEFT or RIGHT to change."),
                BackItem()
            };
            return _menu.CreateMenu("options_race", items);
        }

        private MenuScreen BuildOptionsLapsMenu()
        {
            var items = new List<MenuItem>();
            for (var laps = 1; laps <= 16; laps++)
            {
                var value = laps;
                items.Add(new MenuItem(laps.ToString(), MenuAction.Back, onActivate: () => _settingsActions.UpdateSetting(() => _settings.NrOfLaps = value)));
            }

            items.Add(BackItem());
            return _menu.CreateMenu("options_race_laps", items, "How many labs should the session be. This applys to single race, time trial and multiPlayer modes.");
        }

        private MenuScreen BuildOptionsComputersMenu()
        {
            var items = new List<MenuItem>();
            for (var count = 1; count <= 7; count++)
            {
                var value = count;
                items.Add(new MenuItem(count.ToString(), MenuAction.Back, onActivate: () => _settingsActions.UpdateSetting(() => _settings.NrOfComputers = value)));
            }

            items.Add(BackItem());
            return _menu.CreateMenu("options_race_computers", items, "Number of computer players");
        }
    }
}
