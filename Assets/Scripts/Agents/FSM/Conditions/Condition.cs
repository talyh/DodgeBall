using System.Collections;
using System.Collections.Generic;

public abstract class Condition
{
    public enum Conditions { Invalid = -1, AgentHasBall, AgentHit, TargettingOpponent, BallThrownByOpponent }

    protected Conditions _name;
    public Conditions name
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

    static public Condition Resolve(Condition.Conditions name)
    {
        switch (name)
        {
            case Condition.Conditions.AgentHasBall:
                return new AgentHasBall();
            case Condition.Conditions.AgentHit:
                return new AgentHit();
            case Condition.Conditions.TargettingOpponent:
                return new TargettingOpponent();
            case Condition.Conditions.BallThrownByOpponent:
                return new BallThrownByOpponent();
            default:
                Supporting.Log("Couldn't resolve condition to be added", 1);
                return null;
        }
    }

    public abstract bool IsTrue();

    public void ShutDown()
    {
        AdditionalShutDown();
    }

    protected virtual void AdditionalShutDown()
    { }
}