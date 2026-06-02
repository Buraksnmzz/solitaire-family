//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace ServicesPackage
//{
//    public static class SignalBusService
//    {
//        private static readonly Dictionary<Type, Delegate> _signals =
//           new Dictionary<Type, Delegate>(64);

//        private static readonly Dictionary<Type, object> _lastSignals = new();

//        public static void Subscribe<T>(Action<T> callback)
//        {
//            if (_signals.TryGetValue(typeof(T), out var del))
//            {
//                _signals[typeof(T)] = (Action<T>)del + callback;
//            }
//            else
//            {
//                _signals[typeof(T)] = callback;
//            }
//        }

//        public static void Unsubscribe<T>(Action<T> callback)
//        {
//            if (_signals.TryGetValue(typeof(T), out var del))
//            {
//                _signals[typeof(T)] = (Action<T>)del - callback;
//            }
//        }

//        public static void Fire<T>(T payload, bool warnIfNoListeners = false)
//        {
//            if (!UnityMainThread.IsMainThread)
//            {
//                UnityMainThread.Enqueue(() => Fire(payload, warnIfNoListeners));
//                return;
//            }

//            if (_signals.TryGetValue(typeof(T), out var del))
//            {
//                ((Action<T>)del)?.Invoke(payload);
//            }
//            else if (warnIfNoListeners)
//            {
//                Debug.LogWarning($"[SignalBus] No listeners for {typeof(T).Name}");
//            }
//        }
//    }
//}
using System;
using System.Collections.Generic;

namespace ServicesPackage
{
    public static class SignalBusService
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();
        private static readonly Dictionary<Type, object> _stickySignals = new();

     
        public static void Fire<T>(T signal, bool sticky = false)
        {
            var type = typeof(T);

            if (sticky)
                _stickySignals[type] = signal;

            if (_subscribers.TryGetValue(type, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    try
                    {
                        ((Action<T>)list[i])?.Invoke(signal);
                    }
                    catch (Exception e)
                    {
                        ServicesLogger.LogError($"[SignalBus] Error delivering {type.Name}: {e}");
                    }
                }
            }
        }

        public static void Subscribe<T>(Action<T> callback, bool replaySticky = false)
        {
            var type = typeof(T);

            if (!_subscribers.TryGetValue(type, out var list))
                _subscribers[type] = list = new List<Delegate>();

            list.Add(callback);

            if (replaySticky && _stickySignals.TryGetValue(type, out var last))
            {
                try
                {
                    callback((T)last);
                }
                catch (Exception e)
                {
                    ServicesLogger.LogError($"[SignalBus] Error replaying {type.Name}: {e}");
                }
            }
        }

        public static void Unsubscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var list))
                list.Remove(callback);
        }

        public static void Clear()
        {
            _subscribers.Clear();
            _stickySignals.Clear();
        }
    }
}