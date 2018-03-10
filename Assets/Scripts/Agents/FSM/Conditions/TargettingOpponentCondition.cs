using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargettingOpponent : Condition
{
    protected override void AdditionalInit()
    {
        _name = Conditions.TargettingOpponent;
    }

    public override bool IsTrue()
    {
        if (agent.target)
        {
            Agent opponent = agent.target.GetComponent<Agent>();

            if (!opponent)
            {
                return false;
            }

            if (agent.team != opponent.team)
            {
                return true;
            }
        }

        return false;
    }

    protected override void AdditionalShutDown()
    {
    }
}