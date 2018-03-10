using System.Collections;
using System.Collections.Generic;

public class Defend : State
{
    public Defend()
    {
        _name = States.Defend;
    }

    public override void OnEnter()
    {
    }

    public override void Update()
    {
        agent.Defend();
    }

    public override void OnExit()
    {
    }
}