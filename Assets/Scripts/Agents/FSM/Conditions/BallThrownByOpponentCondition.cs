using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BallThrownByOpponent : Condition
{
    protected override void AdditionalInit()
    {
        _name = Conditions.BallThrownByOpponent;
    }

    public override bool IsTrue()
    {
        return GameController.instance.BallThrown().Where(entry => entry.Value.team != agent.team).Count() > 0;
    }

    protected override void AdditionalShutDown()
    {
    }
}