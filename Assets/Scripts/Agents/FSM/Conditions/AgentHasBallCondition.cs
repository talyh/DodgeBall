using System.Collections;
using System.Collections.Generic;

public class AgentHasBall : Condition
{
    protected override void AdditionalInit()
    {
        _name = Conditions.AgentHasBall;
    }

    public override bool IsTrue()
    {
        return agent.hasBall;
    }

    protected override void AdditionalShutDown()
    {
    }
}