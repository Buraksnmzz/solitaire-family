using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ServicesPackage
{
    public sealed class UnityMainThread : MonoBehaviour
    {
        private static UnityMainThread _instance;
        private static readonly Queue<Action> _queue = new Queue<Action>();
        private static int _mainThreadId;

        public static bool IsMainThread =>
            Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance != null)
                return;

            var go = new GameObject("[UnityMainThread]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<UnityMainThread>();
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            Debug.Log($"[UnityMainThread] MainThreadId = {_mainThreadId}");
        }

        public static void Enqueue(Action action)
        {
            if (action == null)
                return;

            lock (_queue)
            {
                _queue.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (_queue)
            {
                while (_queue.Count > 0)
                {
                    _queue.Dequeue()?.Invoke();
                }
            }
        }
    }
}