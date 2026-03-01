using System.Collections.Generic;

namespace TopSpeed.Menu
{
    internal sealed partial class MenuRegistry
    {
        private MenuScreen BuildOptionsRestoreMenu()
        {
            var items = new List<MenuItem>
            {
                new MenuItem("Yes", MenuAction.Back, onActivate: _settingsActions.RestoreDefaults),
                new MenuItem("No", MenuAction.Back),
                BackItem()
            };
            return _menu.CreateMenu("options_restore", items, "Are you sure you would like to restore all settings to their default values?");
        }
    }
}
