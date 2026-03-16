using System;
using System.Collections.Generic;

namespace TopSpeed.Game
{
    internal sealed partial class Game
    {
        private void TryShowDeviceChoiceDialog()
        {
            if (!IsMenuState(_state))
                return;
            if (_textInputPromptActive || _inputMapping.IsActive || _shortcutMapping.IsActive)
                return;
            if (_choices.HasActiveChoiceDialog)
                return;
            if (_multiplayerCoordinator.Questions.HasActiveOverlayQuestion || _dialogs.HasActiveOverlayDialog)
                return;
            if (!_input.TryGetPendingJoystickChoices(out var discovered) || discovered.Count == 0)
                return;

            var items = new Dictionary<int, string>();
            var guidByChoiceId = new Dictionary<int, Guid>();

            for (var i = 0; i < discovered.Count; i++)
            {
                var choiceId = i + 1;
                var choice = discovered[i];
                var label = choice.IsRacingWheel
                    ? $"{choice.DisplayName} (Racing wheel)"
                    : choice.DisplayName;
                items[choiceId] = label;
                guidByChoiceId[choiceId] = choice.InstanceGuid;
            }

            ShowChoiceDialog(
                "Choose controller",
                "Multiple game controllers were detected. Select one controller to use.",
                items,
                cancelable: false,
                cancelLabel: null,
                onResult: result =>
                {
                    if (result.IsCanceled)
                        return;

                    if (!guidByChoiceId.TryGetValue(result.ChoiceId, out var instanceGuid))
                        return;

                    if (_input.TrySelectJoystick(instanceGuid))
                    {
                        if (items.TryGetValue(result.ChoiceId, out var label))
                            _speech.Speak($"Controller selected. {label}.");
                        else
                            _speech.Speak("Controller selected.");
                        return;
                    }

                    _speech.Speak("Unable to activate the selected controller.");
                });
        }
    }
}
