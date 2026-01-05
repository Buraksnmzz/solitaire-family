using UnityEngine;

namespace UI.Signals
{
    public class LanguageChangeRequestedSignal : ISignal
    {
        public SystemLanguage Language;

        public LanguageChangeRequestedSignal(SystemLanguage language)
        {
            Language = language;
        }
    }
}
