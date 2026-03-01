using System;
using System.Collections.Generic;
using TopSpeed.Input;

namespace TopSpeed.Menu
{
    internal sealed partial class MenuRegistry
    {
        private MenuScreen BuildOptionsVolumeSettingsMenu()
        {
            var items = new List<MenuItem>
            {
                BuildVolumeSlider(
                    "Master audio volume",
                    () => _settings.AudioVolumes.MasterPercent,
                    value => _settings.AudioVolumes.MasterPercent = value,
                    "Controls the overall audio volume for the game. Set lower to reduce every sound category."),
                BuildVolumeSlider(
                    "Vehicle engine sounds",
                    () => _settings.AudioVolumes.PlayerVehicleEnginePercent,
                    value => _settings.AudioVolumes.PlayerVehicleEnginePercent = value,
                    "Controls your own engine and throttle sounds, including engine start and stop."),
                BuildVolumeSlider(
                    "Vehicle event sounds",
                    () => _settings.AudioVolumes.PlayerVehicleEventsPercent,
                    value => _settings.AudioVolumes.PlayerVehicleEventsPercent = value,
                    "Controls events related to your own vehicle, such as horn, back-fire, and other vehicle events."),
                BuildVolumeSlider(
                    "Other vehicles engine sounds",
                    () => _settings.AudioVolumes.OtherVehicleEnginePercent,
                    value => _settings.AudioVolumes.OtherVehicleEnginePercent = value,
                    "Controls engine-related sounds for bots and other players, including engine start and stop."),
                BuildVolumeSlider(
                    "Other vehicles event sounds",
                    () => _settings.AudioVolumes.OtherVehicleEventsPercent,
                    value => _settings.AudioVolumes.OtherVehicleEventsPercent = value,
                    "Controls horns, crashes, bumps, brakes, and similar event sounds for bots and other players."),
                BuildVolumeSlider(
                    "Surface loop sounds",
                    () => _settings.AudioVolumes.SurfaceLoopsPercent,
                    value => _settings.AudioVolumes.SurfaceLoopsPercent = value,
                    "Controls road and surface loops like asphalt, gravel, etc."),
                BuildVolumeSlider(
                    "Music volume",
                    () => _settings.AudioVolumes.MusicPercent,
                    value =>
                    {
                        _settings.AudioVolumes.MusicPercent = value;
                        _settings.SyncMusicVolumeFromAudioCategories();
                    },
                    "Controls menu and race music volume. This stays synchronized with the menu music volume setting."),
                BuildVolumeSlider(
                    "Online server event sounds",
                    () => _settings.AudioVolumes.OnlineServerEventsPercent,
                    value => _settings.AudioVolumes.OnlineServerEventsPercent = value,
                    "Controls server and multiplayer event sounds such as connection and other events."),
                BackItem()
            };

            return _menu.CreateMenu("options_volume", items);
        }

        private Slider BuildVolumeSlider(string label, Func<int> getter, Action<int> setter, string hint)
        {
            return new Slider(
                label,
                "0-100",
                getter,
                value => _settingsActions.UpdateSetting(() =>
                {
                    _settings.AudioVolumes ??= new AudioVolumeSettings();
                    setter(value);
                    _settings.AudioVolumes.ClampAll();
                    _settings.SyncMusicVolumeFromAudioCategories();
                }),
                onChanged: _ => _audio.ApplyAudioSettings(),
                hint: $"{hint} Use LEFT or RIGHT to change by 1, PAGE UP or PAGE DOWN to change by 10, HOME for maximum, END for minimum.");
        }
    }
}
