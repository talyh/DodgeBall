using System.Collections;
using System.Collections.Generic;

public class Attack : State
{
    public Attack()
    {
        _name = States.Attack;
    }

    public override void OnEnter()
    {
    }

    public override void Update()
    {
        agent.Attack();
    }

    public override void OnExit()
    {
    }
}