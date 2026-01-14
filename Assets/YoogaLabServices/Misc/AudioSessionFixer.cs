using System.Runtime.InteropServices;
using UnityEngine;

namespace ServicesPackage
{
    public static class AudioSessionFixer
    {
    #if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void RestoreAudioSession();
    #else
        private static void RestoreAudioSession() { }
    #endif

        public static void FixAudio()
        {
            Debug.Log("[AudioSessionFixer] Attempting to restore audio session...");
            RestoreAudioSession();
        }
    }
}