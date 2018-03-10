using System.Collections;
using System.Collections.Generic;

public class StateManager
{
    private Agent _agent = null;
    private State _currentState = null;
    public State currentState
    {
        get { return _currentState; }
    }
    private List<State> _states = new List<State>();
    private List<Transition> _transitions = new List<Transition>();
    private List<Condition> _conditions = new List<Condition>();


    public void Init(Agent agent)
    {
        // store a reference to the agent so we can access its attributes and methods
        _agent = agent;

        // initialize all states and transitions added by the agent
        InitializeStates();
        InitializeTransitions();
        InitializeConditions();
    }

    private void InitializeStates()
    {
        foreach (State s in _states)
        {
            s.Init(_agent);
        }
    }

    private void InitializeTransitions()
    {
        foreach (Transition t in _transitions)
        {
            t.Init(_agent);
        }
    }

    private void InitializeConditions()
    {
        foreach (Condition c in _conditions)
        {
            c.Init(_agent);
        }
    }

    public void Add(State.States newState)
    {
        // ensure no duplicate entries are added to the list
        if (GetState(newState) != null)
        {
            return;
        }

        // determine what's the State to add, based on the enum value
        // if a State is determined, add it to the list
        State toAdd = ResolveState(newState);
        if (toAdd != null)
        {
            _states.Add(toAdd);
            // Supporting.Log(string.Format("State {0} added", toAdd.GetType().ToString()), 2);
        }
    }

    public void Add(Transition transition)
    {
        // ensure no duplicate entries are added to the list
        if (GetTransition(transition.baseState, transition.targetState) != null)
        {
            return;
        }

        _transitions.Add(transition);
        // Supporting.Log(string.Format("Transition {0} added", transition.GetType().ToString()), 2);
    }

    public void Add(Condition.Conditions condition)
    {
        // ensure no duplicate entries are added to the list
        if (GetCondition(condition) != null)
        {
            return;
        }

        // determine what's the State to add, based on the enum value
        // if a State is determined, add it to the list
        Condition toAdd = ResolveCondition(condition);
        if (toAdd != null)
        {
            _conditions.Add(toAdd);
            // Supporting.Log(string.Format("Transition {0} added", condition.GetType().ToString()), 2);
        }
    }

    private State ResolveState(State.States toResolve)
    {
        // determine if the State already exists in the StateManager
        // if it does, return the entry from the states list
        // if it doesn't, return a new instance of the State
        State lookup = GetState(toResolve);
        if (lookup != null)
        {
            return lookup;
        }

        return State.Resolve(toResolve);
    }

    private Condition ResolveCondition(Condition.Conditions toResolve)
    {
        // determine if the Condition already exists in the StateManager
        // if it does, return the entry from the condition list
        // if it doesn't, return a new instance of the Condition
        Condition lookup = GetCondition(toResolve);
        if (lookup != null)
        {
            return lookup;
        }

        return Condition.Resolve(toResolve);
    }

    public void SetInitialState(State.States initialState)
    {
        _currentState = ResolveState(initialState);
    }

    public void Update()
    {
        // each frame, execute the Update loop of the current state and check for transitions
        if (_currentState != null)
        {
            _currentState.Update();
        }

        Transition();
    }

    private void Transition()
    {
        // check the Transitions that have the currentState as base
        // if any return a valid value, transition to that target State,
        // processing both the End of the current State and Start of the next one
        State newState = null;

        foreach (Transition t in GetTransitions(_currentState.name))
        {
            newState = t.GetNextState();
            if (newState != null)
            {
                _currentState.OnExit();
                _currentState = newState;
                _currentState.OnEnter();
                break;
            }
        }
    }

    // find the the instance of a State based on its name
    public State GetState(State.States name)
    {
        return _states.Find(state => state.name == name);
    }

    // find the instance of a Transition, based on its base and target States
    public Transition GetTransition(State.States baseState, State.States targetState)
    {
        return _transitions.Find(transition => transition.baseState == baseState && transition.targetState == targetState);
    }

    // find all Transitions initated from a given State
    public Transition[] GetTransitions(State.States baseState)
    {
        return _transitions.FindAll(transition => transition.baseState == baseState).ToArray();
    }

    // find the instance of a Condition, based on its name
    public Condition GetCondition(Condition.Conditions name)
    {
        return _conditions.Find(condition => condition.name == name);
    }

    private void OnDestroy()
    {
        ShutDownStates();
        ShutDownTransitions();
        ShutDownConditions();
    }

    private void ShutDownStates()
    {
        foreach (State s in _states)
        {
            s.ShutDown();
        }
    }

    private void ShutDownTransitions()
    {
        foreach (Transition t in _transitions)
        {
            t.ShutDown();
        }
    }

    private void ShutDownConditions()
    {
        foreach (Condition c in _conditions)
        {
            c.ShutDown();
        }
    }
}
