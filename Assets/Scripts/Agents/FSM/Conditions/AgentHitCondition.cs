using System.Collections;
using System.Collections.Generic;

public class AgentHit : Condition
{
    protected override void AdditionalInit()
    {
        _name = Conditions.AgentHit;
    }

    public override bool IsTrue()
    {
        return agent.hit;
    }

    protected override void AdditionalShutDown()
    {
    }
}