using System.Collections;
using System.Collections.Generic;

public abstract class Transition
{
    private Agent _agent = null;
    protected Agent agent
    {
        get { return _agent; }
    }

    protected State.States _baseState = State.States.Invalid;
    public State.States baseState
    {
        get { return _baseState; }
    }
    protected State.States _targetState = State.States.Invalid;
    public State.States targetState
    {
        get { return _targetState; }
    }

    protected List<Condition> _conditions = new List<Condition>();

    public void Init(Agent agent)
    {
        _agent = agent;

        SetConditions();
        InitializeConditions();
        AdditionalInit();
    }

    protected virtual void AdditionalInit()
    { }

    protected virtual void SetConditions()
    { }

    private void InitializeConditions()
    {
        foreach (Condition c in _conditions)
        {
            c.Init(agent);
        }
    }

    public virtual State GetNextState()
    {
        if (agent.currentState.name != _baseState)
        {
            return null;
        }

        foreach (Condition c in _conditions)
        {
            if (!c.IsTrue())
            {
                return null;
            }
        }

        return agent.stateManager.GetState(_targetState);
    }

    public void ShutDown()
    {
        AdditionalShutDown();
    }

    protected virtual void AdditionalShutDown()
    { }
}