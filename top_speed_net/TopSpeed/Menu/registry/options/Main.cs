using System.Collections.Generic;

namespace TopSpeed.Menu
{
    internal sealed partial class MenuRegistry
    {
        private MenuScreen BuildOptionsMenu()
        {
            var items = new List<MenuItem>
            {
                new MenuItem("Game settings", MenuAction.None, nextMenuId: "options_game"),
                new MenuItem("Volume settings", MenuAction.None, nextMenuId: "options_volume",
                    onActivate: () =>
                    {
                        _settings.SyncAudioCategoriesFromMusicVolume();
                        _audio.ApplyAudioSettings();
                    }),
                new MenuItem("Controls", MenuAction.None, nextMenuId: "options_controls"),
                new MenuItem("Race settings", MenuAction.None, nextMenuId: "options_race"),
                new MenuItem("Server settings", MenuAction.None, nextMenuId: "options_server"),
                new MenuItem("Restore default settings", MenuAction.None, nextMenuId: "options_restore"),
                BackItem()
            };
            return _menu.CreateMenu("options_main", items);
        }
    }
}
