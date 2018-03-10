using System.Collections;
using System.Collections.Generic;

public class Wander : State
{
    public Wander()
    {
        _name = States.Wander;
    }

    public override void OnEnter()
    {
    }

    public override void Update()
    {
        agent.Wander();
    }

    public override void OnExit()
    {
    }
}