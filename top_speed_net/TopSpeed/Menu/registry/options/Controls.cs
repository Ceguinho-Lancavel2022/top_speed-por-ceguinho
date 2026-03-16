using System.Collections.Generic;
using TopSpeed.Input;
using TopSpeed.Shortcuts;

namespace TopSpeed.Menu
{
    internal sealed partial class MenuRegistry
    {
        private const string ShortcutGroupsMenuId = "options_controls_shortcuts";
        private const string ShortcutBindingsMenuId = "options_controls_shortcut_bindings";

        private string _activeShortcutGroupId = string.Empty;

        private MenuScreen BuildOptionsControlsMenu()
        {
            var items = new List<MenuItem>
            {
                new MenuItem(() => $"Select device: {DeviceLabel(_settings.DeviceMode)}", MenuAction.None, nextMenuId: "options_controls_device"),
                new CheckBox(
                    "Force feedback",
                    () => _settings.ForceFeedback,
                    value => _settingsActions.UpdateSetting(() => _settings.ForceFeedback = value),
                    hint: "Enables force feedback or vibration if your controller supports it. Press ENTER to toggle."),
                new RadioButton(
                    "Progressive keyboard input",
                    new[]
                    {
                        "Off",
                        "Fastest (0.25 seconds)",
                        "Fast (0.50 seconds)",
                        "Moderate (0.75 seconds)",
                        "Slowest (1.00 second)"
                    },
                    () => (int)_settings.KeyboardProgressiveRate,
                    value => _settingsActions.UpdateSetting(() => _settings.KeyboardProgressiveRate = (KeyboardProgressiveRate)value),
                    hint: "When enabled, throttle, brake, and steering ramp in over time instead of jumping instantly to full value. Press LEFT or RIGHT to change."),
                new MenuItem("Map keyboard keys", MenuAction.None, nextMenuId: "options_controls_keyboard"),
                new MenuItem("Map joystick keys", MenuAction.None, nextMenuId: "options_controls_joystick"),
                new MenuItem(
                    "Map menu shortcuts",
                    MenuAction.None,
                    nextMenuId: ShortcutGroupsMenuId,
                    onActivate: RebuildShortcutGroupsMenu),
                BackItem()
            };
            return _menu.CreateMenu("options_controls", items);
        }

        private MenuScreen BuildOptionsControlsDeviceMenu()
        {
            var items = new List<MenuItem>
            {
                new MenuItem("Keyboard", MenuAction.Back, onActivate: () => _settingsActions.SetDevice(InputDeviceMode.Keyboard)),
                new MenuItem("Joystick", MenuAction.Back, onActivate: () => _settingsActions.SetDevice(InputDeviceMode.Joystick)),
                new MenuItem("Both", MenuAction.Back, onActivate: () => _settingsActions.SetDevice(InputDeviceMode.Both)),
                BackItem()
            };
            return _menu.CreateMenu("options_controls_device", items, "Select input device");
        }

        private MenuScreen BuildOptionsControlsKeyboardMenu()
        {
            return _menu.CreateMenu("options_controls_keyboard", BuildMappingItems(InputMappingMode.Keyboard));
        }

        private MenuScreen BuildOptionsControlsJoystickMenu()
        {
            var items = new List<MenuItem>
            {
                new RadioButton(
                    "Throttle pedal direction",
                    new[] { "Auto", "Normal", "Inverted" },
                    () => (int)_settings.JoystickThrottleInvertMode,
                    value => _settingsActions.UpdateSetting(() => _settings.JoystickThrottleInvertMode = (PedalInvertMode)value),
                    hint: "Auto detects wheel pedal direction from resting position. Use LEFT or RIGHT to change."),
                new RadioButton(
                    "Brake pedal direction",
                    new[] { "Auto", "Normal", "Inverted" },
                    () => (int)_settings.JoystickBrakeInvertMode,
                    value => _settingsActions.UpdateSetting(() => _settings.JoystickBrakeInvertMode = (PedalInvertMode)value),
                    hint: "Auto detects wheel pedal direction from resting position. Use LEFT or RIGHT to change."),
                new RadioButton(
                    "Steering dead zone",
                    new[] { "Default (1 degree)", "2 degrees", "3 degrees", "4 degrees", "5 degrees" },
                    () =>
                    {
                        var deadZone = _settings.JoystickSteeringDeadZone;
                        if (deadZone < 1 || deadZone > 5)
                            deadZone = 1;
                        return deadZone - 1;
                    },
                    value =>
                    {
                        var deadZone = value + 1;
                        if (deadZone < 1 || deadZone > 5)
                            deadZone = 1;
                        _settingsActions.UpdateSetting(() => _settings.JoystickSteeringDeadZone = deadZone);
                    },
                    hint: "Sets how much small steering movement is ignored around center. Default is 1 degree. Use LEFT or RIGHT to change.")
            };

            items.AddRange(BuildMappingItems(InputMappingMode.Joystick, includeBack: false));
            items.Add(BackItem());
            return _menu.CreateMenu("options_controls_joystick", items);
        }

        private MenuScreen BuildOptionsControlsShortcutGroupsMenu()
        {
            var items = new List<MenuItem>
            {
                new MenuItem("Global shortcuts", MenuAction.None),
                BackItem()
            };
            return _menu.CreateMenu(ShortcutGroupsMenuId, items, title: string.Empty);
        }

        private MenuScreen BuildOptionsControlsShortcutBindingsMenu()
        {
            var items = new List<MenuItem>
            {
                new MenuItem("No shortcuts in this group.", MenuAction.None),
                BackItem()
            };
            return _menu.CreateMenu(ShortcutBindingsMenuId, items, title: string.Empty);
        }

        private List<MenuItem> BuildMappingItems(InputMappingMode mode, bool includeBack = true)
        {
            var items = new List<MenuItem>();
            foreach (var action in _raceInput.KeyMap.Actions)
            {
                var definition = action;
                items.Add(new MenuItem(
                    () => $"{definition.Label}: {_mapping.FormatMappingValue(definition.Action, mode)}",
                    MenuAction.None,
                    onActivate: () => _mapping.BeginMapping(mode, definition.Action)));
            }

            if (includeBack)
                items.Add(BackItem());
            return items;
        }

        private void RebuildShortcutGroupsMenu()
        {
            var groups = _menu.GetShortcutGroups();
            var items = new List<MenuItem>();
            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                items.Add(new MenuItem(
                    group.Name,
                    MenuAction.None,
                    onActivate: () => OpenShortcutGroup(group)));
            }

            items.Add(BackItem());
            _menu.UpdateItems(ShortcutGroupsMenuId, items, preserveSelection: true);
        }

        private void OpenShortcutGroup(ShortcutGroup group)
        {
            _activeShortcutGroupId = group.Id;
            if (!RebuildShortcutBindingsMenu())
            {
                _ui.SpeakMessage($"{group.Name} has no shortcuts.");
                return;
            }

            _menu.Push(ShortcutBindingsMenuId);
        }

        private bool RebuildShortcutBindingsMenu()
        {
            var items = new List<MenuItem>();
            if (string.IsNullOrWhiteSpace(_activeShortcutGroupId))
            {
                items.Add(new MenuItem("No shortcut group selected.", MenuAction.None));
                items.Add(BackItem());
                _menu.UpdateItems(ShortcutBindingsMenuId, items, preserveSelection: true);
                return false;
            }

            var bindings = _menu.GetShortcutBindings(_activeShortcutGroupId);
            for (var i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                var actionId = binding.ActionId;
                var displayName = binding.DisplayName;
                var description = binding.Description;
                items.Add(new MenuItem(
                    () => $"{displayName}: {GetShortcutKeyText(actionId, binding.Key)}",
                    MenuAction.None,
                    onActivate: () => _mapping.BeginShortcutMapping(_activeShortcutGroupId, actionId, displayName),
                    hint: description));
            }

            if (items.Count == 0)
                return false;

            items.Add(BackItem());
            _menu.UpdateItems(ShortcutBindingsMenuId, items, preserveSelection: true);
            return true;
        }

        private string GetShortcutKeyText(string actionId, SharpDX.DirectInput.Key fallbackKey)
        {
            if (_menu.TryGetShortcutBinding(actionId, out var binding))
                return FormatShortcutKey(binding.Key);
            return FormatShortcutKey(fallbackKey);
        }

        private static string FormatShortcutKey(SharpDX.DirectInput.Key key)
        {
            return (int)key <= 0 ? "none" : key.ToString();
        }
    }
}
