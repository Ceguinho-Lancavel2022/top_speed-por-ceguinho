namespace TopSpeed.Race.Panels
{
    internal sealed class ControlVehiclePanel : IVehicleRacePanel
    {
        public string Name => "Control panel";
        public bool AllowsDrivingInput => true;
        public bool AllowsAuxiliaryInput => true;

        public void Update(float elapsed)
        {
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void Dispose()
        {
        }
    }
}
