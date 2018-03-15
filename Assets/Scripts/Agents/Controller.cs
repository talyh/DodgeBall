using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AgentController : MonoBehaviour
{
    [SerializeField]
    protected Agent _agent;

    public abstract void Wander();

    public abstract void Attack();

    public abstract void Throw();

    public abstract void Defend();
}