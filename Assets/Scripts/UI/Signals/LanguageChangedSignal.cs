using UnityEngine;

namespace UI.Signals
{
    public class LanguageChangedSignal : ISignal
    {
        public SystemLanguage Language;

        public LanguageChangedSignal(SystemLanguage language)
        {
            Language = language;
        }
    }
}
