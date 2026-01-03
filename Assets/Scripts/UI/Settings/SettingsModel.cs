using System.Collections.Generic;
using UnityEngine;

public class SettingsModel : IModel
{
    public bool IsSoundOn { get; set; } = true;
    public bool IsMusicOn { get; set; } = true;
    public bool IsHapticOn { get; set; } = true;
    public bool IsNoAds { get; set; } = false;
    public SystemLanguage CurrentLanguage { get; set; } = SystemLanguage.Unknown;
}
