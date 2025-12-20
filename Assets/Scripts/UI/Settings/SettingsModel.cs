using System.Collections.Generic;
public class SettingsModel : IModel
{
    public bool IsSoundOn { get; set; } = true;
    public bool IsMusicOn { get; set; } = true;
    public bool IsHapticOn { get; set; } = true;
    public bool IsNoAds { get; set; } = false;
}
