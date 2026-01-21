using System.Collections.Generic;
using System;
using UnityEngine;


//[ExecuteInEditMode]
public class EventDispatcher : MonoBehaviour
{
    public static EventDispatcher Instance { get; private set; }

    private static readonly Dictionary<Type, List<object>> EventHandlers = new();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void Subscribe<T>(Action<T> callback) where T : struct
    {
        var eventType = typeof(T);

        if (!EventHandlers.ContainsKey(eventType))
        {
            EventHandlers[eventType] = new List<object>();
        }

        EventHandlers[eventType].Add(callback);
    }

    public void Unsubscribe<T>(Action<T> callback) where T : struct
    {
        var eventType = typeof(T);

        if (!EventHandlers.TryGetValue(eventType, out var handlers)) return;
        handlers.Remove(callback);
        if (handlers.Count == 0)
        {
            EventHandlers.Remove(eventType);
        }
    }

    public void SendEvent<T>(T eventData) where T : struct
    {
        var eventType = typeof(T);

        if (!EventHandlers.TryGetValue(eventType, out var eventHandler)) return;

        for (int i = eventHandler.Count - 1; i >= 0; i--)
        {
            ((Action<T>)eventHandler[i]).Invoke(eventData);
        }

    }

    public void ClearAll()
    {
        EventHandlers.Clear();
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        ClearAll();
        Destroy(this);
    }
}
