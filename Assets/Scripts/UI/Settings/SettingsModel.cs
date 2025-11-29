using System.Collections.Generic;
public class SettingsModel : IModel
{
    public bool IsSoundOn { get; set; } = true;
    public bool IsMusicOn { get; set; } = true;
    public bool IsHapticOn { get; set; } = true;
    public string Language { get; set; } = "en";
    public bool IsLargeFont { get; set; }
}
