using System;
using TopSpeed.Input;

namespace TopSpeed.Game
{
    internal sealed partial class Game
    {
        private void SaveSettings()
        {
            _settingsManager.Save(_settings);
        }

        private void RestoreDefaults()
        {
            _settings.RestoreDefaults();
            _raceInput.SetDevice(_settings.DeviceMode);
            _input.SetDeviceMode(_settings.DeviceMode);
            _speech.ScreenReaderRateMs = _settings.ScreenReaderRateMs;
            _needsCalibration = _settings.ScreenReaderRateMs <= 0f;
            _menu.SetWrapNavigation(_settings.MenuWrapNavigation);
            _menu.SetMenuSoundPreset(_settings.MenuSoundPreset);
            _menu.SetMenuNavigatePanning(_settings.MenuNavigatePanning);
            _menu.ResetShortcutBindings();
            ApplyAudioSettings();
            SaveSettings();
            _speech.Speak("Defaults restored.");
        }

        private void SetDevice(InputDeviceMode mode)
        {
            _settings.DeviceMode = mode;
            _raceInput.SetDevice(mode);
            _input.SetDeviceMode(mode);
            SaveSettings();
        }

        private void UpdateSetting(Action update)
        {
            update();
            SaveSettings();
        }
    }
}
