using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    public static MainThreadDispatcher instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RunOnMainThread(System.Action action)
    {
        lock (queue)
        {
            queue.Enqueue(action);
        }
    }

    private System.Collections.Queue queue = new System.Collections.Queue();

    private void Update()
    {
        lock (queue)
        {
            while (queue.Count > 0)
            {
                ((System.Action)queue.Dequeue()).Invoke();
            }
        }
    }
}