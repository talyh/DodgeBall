using System.Collections;
using System.Collections.Generic;

public class WanderToDefend : Transition
{
    public WanderToDefend()
    {
        _baseState = State.States.Wander;
        _targetState = State.States.Defend;
    }

    public override State GetNextState()
    {
        if (agent.currentState.name != _baseState)
        {
            return null;
        }

        bool hit = agent.stateManager.GetCondition(Condition.Conditions.AgentHit).IsTrue();
        bool hasBall = agent.stateManager.GetCondition(Condition.Conditions.AgentHasBall).IsTrue();
        bool ballThrownByOpponent = agent.stateManager.GetCondition(Condition.Conditions.BallThrownByOpponent).IsTrue();


        if (!hit && !hasBall && ballThrownByOpponent)
        {
            return agent.stateManager.GetState(_targetState);
        }

        return null;
    }
}