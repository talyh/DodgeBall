using System.Collections;
using System.Collections.Generic;

public abstract class State
{
    public enum States { Invalid = -1, Wander, Attack, Defend }

    protected States _name = States.Invalid;
    public States name
    {
        get { return _name; }
    }
    private Agent _agent = null;
    protected Agent agent
    {
        get { return _agent; }
    }

    public void Init(Agent agent)
    {
        _agent = agent;

        AdditionalInit();
    }

    protected virtual void AdditionalInit()
    { }

    public virtual void OnEnter()
    { }

    public virtual void Update()
    { }

    public virtual void OnExit()
    { }

    static public State Resolve(State.States name)
    {
        switch (name)
        {
            case State.States.Wander:
                return new Wander();
            case State.States.Attack:
                return new Attack();
            case State.States.Defend:
                return new Defend();
            default:
                Supporting.Log("Couldn't resolve state to be added", 1);
                return null;
        }
    }

    public void ShutDown()
    {
        AdditionalShutDown();
    }

    protected virtual void AdditionalShutDown()
    { }
}