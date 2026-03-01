using System.Collections.Generic;

namespace TopSpeed.Menu
{
    internal sealed partial class MenuRegistry
    {
        private MenuScreen BuildOptionsServerSettingsMenu()
        {
            var items = new List<MenuItem>
            {
                new MenuItem(() => $"Default server port: {FormatServerPort(_settings.DefaultServerPort)}", MenuAction.None, onActivate: _server.BeginServerPortEntry),
                BackItem()
            };
            return _menu.CreateMenu("options_server", items);
        }
    }
}
