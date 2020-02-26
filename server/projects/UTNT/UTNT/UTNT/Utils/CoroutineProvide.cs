using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Wing.Tools.Utils
{
    public class CoroutineProvider : MonoBehaviour
    {
        public event Action OnUpdate;

        private static CoroutineProvider m_instance;
        private static GameObject obj;
        private List<Action> _actions = new List<Action>();

        public void Init()
        {

        }

        public static CoroutineProvider Instance
        {
            get
            {
                if (m_instance == null)
                {
                    obj = new GameObject("CoroutineProvide");
                    m_instance = obj.AddComponent<CoroutineProvider>();
                }

                return m_instance;
            }
        }

        public void Delay(float delay,Action action)
        {
            StartCoroutine(DoDelay(action, delay));
        }

        public void DoInMainThread(Action action)
        {
            lock (_actions)
            {
                _actions.Add(action);
            }
        }

        void Update()
        {
            if(OnUpdate!=null)
            {
                OnUpdate();
            }

            if (_actions.Count > 0)
            {
                var acts = new List<Action>();
                lock (_actions)
                {
                    acts.AddRange(_actions);
                    _actions.Clear();
                }

                acts.ForEach(act =>
                {
                    try
                    {
                        act.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                });
            }
        }

        IEnumerator DoDelay(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (action != null)
            {
                action();
            }
        }
    }
}
