using System;
using System.Collections;
using UnityEngine;

namespace ServicesPackage
{
    public class ServiceCoroutineRunner : MonoBehaviour
    {
        private static ServiceCoroutineRunner _instance;

        public static ServiceCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ServiceCoroutineRunner");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<ServiceCoroutineRunner>();
                }
                return _instance;
            }
        }

        public static Coroutine StartSafe(IEnumerator routine)
        {
            return Instance.StartCoroutine(SafeWrapper(routine));
        }

        public static void RunAfter(float delay, Action action)
        {
            Instance.StartCoroutine(RunAfterRoutine(delay, action));
        }


        private static IEnumerator SafeWrapper(IEnumerator routine)
        {
            while (true)
            {
                object current;

                try
                {
                    if (!routine.MoveNext())
                    {
                        yield break;
                    }

                    current = routine.Current;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ServiceCoroutineRunner] Coroutine Error: {ex}");
                    yield break;
                }

                yield return current;
            }
        }

        private static IEnumerator RunAfterRoutine(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
