using System;

/// <summary>
/// Interface for the event dispatcher service
/// </summary>
public interface IEventDispatcherService : IService
{
    /// <summary>
    /// Add a listener for a specific event type
    /// </summary>
    /// <typeparam name="T">Type of event to listen for</typeparam>
    /// <param name="listener">Callback to invoke when event occurs</param>
    void AddListener<T>(Action<T> listener) where T : ISignal;

    /// <summary>
    /// Remove a listener for a specific event type
    /// </summary>
    /// <typeparam name="T">Type of event to stop listening for</typeparam>
    /// <param name="listener">Callback to remove</param>
    void RemoveListener<T>(Action<T> listener) where T : ISignal;

    /// <summary>
    /// Dispatch an event to all listeners
    /// </summary>
    /// <param name="signal">The event data to dispatch</param>
    void Dispatch(ISignal signal);
}