using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EventBus
{
    private static Dictionary<Type, Delegate> _listeners;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        _listeners = new();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        _listeners.Clear();
    }

    public static void Subscribe<T>(Action<T> listener)
    {
        var type = typeof(T);

        if (_listeners.ContainsKey(type))
            _listeners[type] = Delegate.Combine(_listeners[type], listener);
        else
            _listeners[type] = listener;
    }

    public static void Unsubscribe<T>(Action<T> listener)
    {
        var type = typeof(T);

        if (!_listeners.ContainsKey(type)) return;

        _listeners[type] = Delegate.Remove(_listeners[type], listener);

        if (_listeners[type] == null)
            _listeners.Remove(type);
    }

    public static void Publish<T>(T eventData)
    {
        var type = typeof(T);

        if (!_listeners.ContainsKey(type)) return;

        var action = _listeners[type] as Action<T>;
        if (action == null)
        {
            Debug.LogWarning($"[EventBus] {type.Name} 캐스트 실패");
            return;
        }
        action.Invoke(eventData);
    }
}