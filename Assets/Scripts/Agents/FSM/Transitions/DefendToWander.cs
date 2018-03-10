using System.Collections;
using System.Collections.Generic;

public class DefendToWander : Transition
{
    public DefendToWander()
    {
        _baseState = State.States.Defend;
        _targetState = State.States.Wander;
    }

    public override State GetNextState()
    {
        if (agent.currentState.name != _baseState)
        {
            return null;
        }

        bool hit = agent.stateManager.GetCondition(Condition.Conditions.AgentHit).IsTrue();
        bool hasBall = agent.stateManager.GetCondition(Condition.Conditions.AgentHasBall).IsTrue();

        if (!hit && !hasBall)
        {
            return agent.stateManager.GetState(_targetState);
        }

        return null;
    }
}