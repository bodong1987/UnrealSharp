// ReSharper disable EventNeverSubscribedTo.Global
namespace UnrealSharp.Utils.Misc;

/// <summary>
/// State interface
/// </summary>
public interface IState
{
    /// <summary>
    /// Called when [state enter].
    /// </summary>
    void OnStateEnter();

    /// <summary>
    /// Called when [state leave].
    /// </summary>
    void OnStateLeave();

    /// <summary>
    /// Called when [state override].
    /// </summary>
    void OnStateOverride();

    /// <summary>
    /// Called when [state resume].
    /// </summary>
    void OnStateResume();

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    string Name { get; }
}

/// <summary>
/// Class AutoRegisterStateAttribute.
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Class)]
public class AutoRegisterStateAttribute : Attribute;

/// <summary>
/// Class BaseState.
/// </summary>
public abstract class BaseState : IState
{
    /// <summary>
    /// Called when [state enter].
    /// </summary>
    public virtual void OnStateEnter() { }

    /// <summary>
    /// Called when [state leave].
    /// </summary>
    public virtual void OnStateLeave() { }

    /// <summary>
    /// Called when [state override].
    /// </summary>
    public virtual void OnStateOverride() { }

    /// <summary>
    /// Called when [state resume].
    /// </summary>
    public virtual void OnStateResume() { }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    public virtual string Name => GetType().Name;
}

/// <summary>
/// Class AbstractImportableState.    
/// Implements the <see cref="IState" />
/// </summary>    
/// <seealso cref="IState" />
public abstract class AbstractImportableState : IState
{
    /// <summary>
    /// Called when [state enter].
    /// </summary>
    public virtual void OnStateEnter() { }

    /// <summary>
    /// Called when [state leave].
    /// </summary>
    public virtual void OnStateLeave() { }

    /// <summary>
    /// Called when [state override].
    /// </summary>
    public virtual void OnStateOverride() { }

    /// <summary>
    /// Called when [state resume].
    /// </summary>
    public virtual void OnStateResume() { }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    public virtual string Name => GetType().Name;
}

/// <summary>
/// State Machine
/// </summary>
public class StateMachine
{
    /// <summary>
    /// The registered state
    /// </summary>
    private readonly Dictionary<string, IState> _registeredState = new();

    /// <summary>
    /// The state stack
    /// </summary>
    private readonly Stack<IState> _stateStack = new();

    /// <summary>
    /// Gets the state of the tar.
    /// </summary>
    /// <value>The state of the tar.</value>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public IState? TargetState { get; private set; } 

    /// <summary>
    /// when ForbidReentry is true, do nothing if change to a state already in stack
    /// </summary>
    /// <value></value>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public bool ForbidReentry { get; set; } = true;

    /// <summary>
    /// Occurs when [state changing].
    /// </summary>
    public event EventHandler<StateChangingEventArgs>? StateChanging;

    /// <summary>
    /// Occurs when [state changed].
    /// </summary>
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Occurs when [state override].
    /// </summary>
    public event EventHandler<StateOverrideEventArgs>? StateOverride;
    /// <summary>
    /// Occurs when [state resumed].
    /// </summary>
    public event EventHandler<StateResumedEventArgs>? StateResumed;

    /// <summary>
    /// Registers the state.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="state">The state.</param>
    public void RegisterState(string name, IState state)
    {
        _registeredState.TryAdd(name, state);
    }
        
    /// <summary>
    /// Registers the state.
    /// </summary>
    /// <typeparam name="TStateImplType">The type of the t state implementation type.</typeparam>
    /// <param name="state">The state.</param>
    /// <param name="name">The name.</param>
    public void RegisterState<TStateImplType>(TStateImplType state, string name)
        where TStateImplType : IState
    {
        RegisterState(name, state);
    }

    /// <summary>
    /// Unregisters the state.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>IState.</returns>
    public IState? UnregisterState(string name)
    {
        return !_registeredState.Remove(name, out var state) ? default : state;
    }

    /// <summary>
    /// Gets the state.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>IState.</returns>
    public IState? GetState(string name)
    {
        return _registeredState.GetValueOrDefault(name);
    }

    /// <summary>
    /// Gets the state.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name">The name.</param>
    /// <returns>T.</returns>
    public T? GetState<T>(string name)
        where T : class, IState
    {
        return GetState(name) as T;
    }

    /// <summary>
    /// Gets the name of the state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>System.String.</returns>
    public string? GetStateName(IState state)
    {
        var etr = _registeredState.GetEnumerator();
        while (etr.MoveNext())
        {
            var pair = etr.Current;

            if (pair.Value == state)
            {
                return pair.Key;
            }
        }

        return null;
    }

    /// <summary>
    /// Pushes the specified state.
    /// </summary>
    /// <param name="state">The state.</param>
    public void Push(IState state)
    {
        IState? overrideState = null;

        if (_stateStack.Count > 0)
        {
            overrideState = _stateStack.Peek();
            overrideState.OnStateOverride();
        }

        _stateStack.Push(state);

        state.OnStateEnter();

        StateOverride?.Invoke(this, new StateOverrideEventArgs(overrideState, state));
    }

    /// <summary>
    /// Pushes the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    public void Push(string name)
    {
        if (!_registeredState.TryGetValue(name, out var state))
        {
            return;
        }

        Push(state);
    }

    /// <summary>
    /// Pops the state.
    /// </summary>
    /// <returns>IState.</returns>
    public IState? PopState()
    {
        if (_stateStack.Count <= 0)
        {
            return default;
        }

        var state = _stateStack.Pop();
        state.OnStateLeave();
        IState? resumeState = null;

        if (_stateStack.Count > 0)
        {
            resumeState = _stateStack.Peek();
            resumeState.OnStateResume();
        }

        StateResumed?.Invoke(this, new StateResumedEventArgs(state, resumeState));

        return state;
    }

    /// <summary>
    /// Changes the state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>IState.</returns>
    public IState? ChangeState(IState? state)
    {
        if (state == null)
        {
            return default;
        }

        if (ForbidReentry && state == TopState)
        {
            return null;
        }

        TargetState = state;

        IState? oldState = default;
        if (_stateStack.Count > 0)
        {
            oldState = _stateStack.Pop();

            if (StateChanging != null)
            {
                var changingEvt = new StateChangingEventArgs(oldState, state);
                StateChanging(this, changingEvt); //-V3083
            }

            oldState.OnStateLeave();
        }

        _stateStack.Push(state);
        state.OnStateEnter();

        StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, state)); //-V3083

        return oldState;
    }

    /// <summary>
    /// Changes the state.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>IState.</returns>
    public IState? ChangeState(string name)
    {
        if (!_registeredState.TryGetValue(name, out var state))
        {
            Logger.LogError("failed find state with name ：" + name);
            return default;
        }

        if (ForbidReentry && name == TopStateName())
        {
            return null;
        }

        return ChangeState(state);
    }

    /// <summary>
    /// Gets the state of the top.
    /// </summary>
    /// <returns>IState.</returns>
    public IState? GetTopState()
    {
        return _stateStack.Count <= 0 ? default : _stateStack.Peek();
    }

    /// <summary>
    /// Gets the state of the top.
    /// </summary>
    /// <value>The state of the top.</value>
    public IState? TopState => GetTopState();


    /// <summary>
    /// Tops the name of the state.
    /// </summary>
    /// <returns>System.String.</returns>
    public string? TopStateName()
    {
        if (_stateStack.Count <= 0)
        {
            return null;
        }

        var state = _stateStack.Peek();
        return GetStateName(state);
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
        while (_stateStack.Count > 0)
        {
            _stateStack.Pop().OnStateLeave();
        }
    }

    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>The count.</value>
    public int Count => _stateStack.Count;
}

/// <summary>
/// Class StateChangingEventArgs.
/// Implements the <see cref="EventArgs" />
/// </summary>
/// <seealso cref="EventArgs" />
public class StateChangingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the state of the leave.
    /// </summary>
    /// <value>The state of the leave.</value>
    public readonly IState LeaveState;

    /// <summary>
    /// Gets the target state.
    /// </summary>
    public readonly IState EnterState;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateChangingEventArgs"/> class.
    /// </summary>
    /// <param name="leaveState">State of the leave.</param>
    /// <param name="enterState">Target state.</param>
    public StateChangingEventArgs(IState leaveState, IState enterState)
    {
        LeaveState = leaveState;
        EnterState = enterState;
    }
}

/// <summary>
/// Class StateChangedEventArgs.
/// Implements the <see cref="EventArgs" />
/// </summary>
/// <seealso cref="EventArgs" />
public class StateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the state of the leave.
    /// </summary>
    /// <value>The state of the leave.</value>
    public readonly IState? LeaveState;

    /// <summary>
    /// Gets the target state.
    /// </summary>
    /// <value>The target state.</value>
    public readonly IState EnterState;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateChangedEventArgs"/> class.
    /// </summary>
    /// <param name="leaveState">State of the leave.</param>
    /// <param name="enterState">Target state</param>
    public StateChangedEventArgs(IState? leaveState, IState enterState)
    {
        LeaveState = leaveState;
        EnterState = enterState;
    }
}

/// <summary>
/// Class StatePushedEventArgs.
/// Implements the <see cref="EventArgs" />
/// </summary>
/// <seealso cref="EventArgs" />
public class StateOverrideEventArgs : EventArgs
{
    /// <summary>
    /// Gets the state of the leave.
    /// </summary>
    /// <value>The state of the leave.</value>
    public readonly IState? OverrideState;

    /// <summary>
    /// Gets the target state.
    /// </summary>
    /// <value>The target state.</value>
    public readonly IState EnterState;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateOverrideEventArgs" /> class.
    /// </summary>
    /// <param name="overrideState">State of the override.</param>
    /// <param name="enterState">Target state</param>
    public StateOverrideEventArgs(IState? overrideState, IState enterState)
    {
        OverrideState = overrideState;
        EnterState = enterState;
    }
}

/// <summary>
/// Class StateResumedEventArgs.
/// Implements the <see cref="EventArgs" />
/// </summary>
/// <seealso cref="EventArgs" />
public class StateResumedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the state of the leave.
    /// </summary>
    /// <value>The state of the leave.</value>
    public readonly IState LeaveState;

    /// <summary>
    /// Gets the target state.
    /// </summary>
    /// <value>The target state.</value>
    public readonly IState? ResumeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateResumedEventArgs"/> class.
    /// </summary>
    /// <param name="leaveState">State of the leave.</param>
    /// <param name="resumeState">State of the resume.</param>
    public StateResumedEventArgs(IState leaveState, IState? resumeState)
    {
        LeaveState = leaveState;
        ResumeState = resumeState;
    }
}