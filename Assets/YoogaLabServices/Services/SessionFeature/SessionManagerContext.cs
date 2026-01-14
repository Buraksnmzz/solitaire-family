namespace ServicesPackage
{
    public class SessionManagerContext
    {
        public int Session { get; set; }
        public bool IsFirstSession { get; set; }
        public long DaysSinceInstall { get; set; }

        public const string InstallTimeKey = "install_t";
        public const string SessionKey = "session";
    }
}

