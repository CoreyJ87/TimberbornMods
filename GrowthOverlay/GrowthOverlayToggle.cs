namespace GrowthOverlay;

internal class GrowthOverlayToggle
{
    public bool Enabled { get; private set; }
    public bool Hidden { get; private set; }

    public event EventHandler? StateChanged;

    public void EnableOverlay()
    {
        Enabled = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void DisableOverlay()
    {
        Enabled = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void HideOverlay()
    {
        Hidden = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ShowOverlay()
    {
        Hidden = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
