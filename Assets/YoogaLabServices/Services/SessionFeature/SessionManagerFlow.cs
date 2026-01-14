using System;
using UnityEngine;

namespace ServicesPackage
{
    public class SessionManagerFlow 
    {
        private readonly SessionManagerContext ctx;

        public SessionManagerFlow(SessionManagerContext context)
        {
            ctx = context;
        }

        public void Initialize()
        {
            ctx.Session = PlayerPrefs.GetInt(SessionManagerContext.SessionKey, 0) + 1;
            ctx.IsFirstSession = ctx.Session == 1;

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string storedInstallTimeStr = PlayerPrefs.GetString(SessionManagerContext.InstallTimeKey, "0");

            long installTime = 0;
            if (!long.TryParse(storedInstallTimeStr, out installTime) || installTime <= 0)
            {
                installTime = now;
                PlayerPrefs.SetString(SessionManagerContext.InstallTimeKey, now.ToString());
            }

            ctx.DaysSinceInstall = (now - installTime) / (1000 * 3600 * 24);

            PlayerPrefs.SetInt(SessionManagerContext.SessionKey, ctx.Session);
            PlayerPrefs.Save();
            ServicesLogger.Log($"[SessionManager] Days since install: {ctx.DaysSinceInstall}");
        }
    }
}
