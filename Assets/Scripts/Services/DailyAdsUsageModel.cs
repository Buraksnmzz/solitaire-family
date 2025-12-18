using System;

[Serializable]
public class DailyAdsUsageModel : IModel
{
    public int remainingAds;
    public int lastResetDayKey;
}