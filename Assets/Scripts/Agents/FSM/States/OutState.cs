using System.Collections;
using System.Collections.Generic;

public class Out : State
{
    public Out()
    {
        _name = States.Out;
    }

    public override void OnEnter()
    {
    }

    public override void Update()
    {
        agent.Out();
    }

    public override void OnExit()
    {
    }
}