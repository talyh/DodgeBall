using System.Collections;
using System.Collections.Generic;

public class BallThrownByOpponent : Condition
{
    protected override void AdditionalInit()
    {
        _name = Conditions.BallThrownByOpponent;
    }

    public override bool IsTrue()
    {
        return GameController.instance.BallThrown();
    }

    protected override void AdditionalShutDown()
    {
    }
}