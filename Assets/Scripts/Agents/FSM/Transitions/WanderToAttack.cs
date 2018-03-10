using System.Collections;
using System.Collections.Generic;

public class WanderToAttack : Transition
{
    public WanderToAttack()
    {
        _baseState = State.States.Wander;
        _targetState = State.States.Attack;
    }

    public override State GetNextState()
    {
        if (agent.currentState.name != _baseState)
        {
            return null;
        }

        bool hit = agent.stateManager.GetCondition(Condition.Conditions.AgentHit).IsTrue();
        bool hasBall = agent.stateManager.GetCondition(Condition.Conditions.AgentHasBall).IsTrue();
        bool targettingOpponent = agent.stateManager.GetCondition(Condition.Conditions.TargettingOpponent).IsTrue();

        if (!hit && hasBall && targettingOpponent)
        {
            return agent.stateManager.GetState(_targetState);
        }

        return null;
    }
}