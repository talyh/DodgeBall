using System.Collections;
using System.Collections.Generic;

public class DefendToOut : Transition
{
    public DefendToOut()
    {
        _baseState = State.States.Defend;
        _targetState = State.States.Out;
    }

    public override State GetNextState()
    {
        if (agent.currentState.name != _baseState)
        {
            return null;
        }

        bool hit = agent.stateManager.GetCondition(Condition.Conditions.AgentHit).IsTrue();

        if (hit)
        {
            return agent.stateManager.GetState(_targetState);
        }

        return null;
    }
}