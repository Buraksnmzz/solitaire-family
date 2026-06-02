using Firebase;
using System;
using System.Threading.Tasks;

namespace ServicesPackage
{
    public class FirebaseCoreContext 
    {
        public bool isInitialized { get; set; } = false;
        internal Task<DependencyStatus> dependencyTask;
        internal DateTime _lastPausedTime;
        internal bool _wasPaused { get; set; } = false;
    }
}