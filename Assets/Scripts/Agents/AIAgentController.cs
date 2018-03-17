using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

[RequireComponent(typeof(Agent))]
public class AIAgentController : AgentController
{
    private int _courtAreaMask;

    public Transform _target;
    public Transform target
    {
        get { return _target; }
    }

    [Header("Scanning")]
    [SerializeField]
    private float _scanRadiusForBall = 5;
    [SerializeField]
    private float _scanRadiusForOpponent = 20;
    private float _currentScanRadius;
    private LayerMask _currentScanLayer;

    [Header("Movement")]
    [SerializeField]
    private float _destinationBuffer = 5;

    private List<Vector3> _pathNodes;

    private bool _waiting;
    private float _timeAttacking;
    private float _timeWandering;
    private float _wanderingDuration = 5;
    private Vector3 _wanderingDestination;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        if (!_agent)
        {
            _agent = GetComponent<Agent>();
        }

        _agent.tookball += ClearTarget;
        _agent.gotHit += GoOut;

        DetermineWalkMask();

        _pathNodes = new List<Vector3>();
    }

    private void DetermineWalkMask()
    {
        _courtAreaMask = 1 << NavMesh.GetAreaFromName(_agent.team.ToString());

        if (_agent.hit)
        {
            _courtAreaMask = NavMesh.AllAreas;
        }
    }

    private void DetermineScanMask()
    {
        if (!_agent.hasBall)
        {
            _currentScanLayer = 1 << LayerMask.NameToLayer(GameController.Layers.Ball.ToString());
        }
        else
        {
            _currentScanLayer = 1 << LayerMask.NameToLayer(GameController.Layers.Agent.ToString());
        }
    }

    private void DetermineScanRadius()
    {
        if (!_agent.hasBall)
        {
            _currentScanRadius = _scanRadiusForBall;
        }
        else
        {
            _currentScanRadius = _scanRadiusForOpponent;
        }
    }

    public void ClearTarget()
    {
        Debug.LogFormat("{0} losing track of {1}", name, _target);
        _target = null;
    }

    private void FixedUpdate()
    {
        if (!GameController.instance.gameStarted)
        {
            return;
        }

        if (_waiting)
        {
            return;
        }

        if (_agent.debugMode)
        {
            Supporting.Log(string.Format("{0} is at {1}", name, transform.position));
        }

        if (_agent.GoingOut() && !_agent.hit)
        {
            _agent.Stop();
            _agent.TurnBack();
        }
    }

    private void MoveToTarget()
    {
        DeterminePath(_target.position);

        if (_pathNodes.Count > 0)
        {
            if (_agent.debugMode)
            {
                Supporting.Log(string.Format("{0} found path to target {1}", name, _target.name));
            }

            MoveToPoint(_pathNodes[1]);
        }
        else
        {
            if (_agent.debugMode)
            {
                Supporting.Log(string.Format("{0} couldn't find path to target {1}", name, _target.name), 2);
            }
        }
    }

    private void DeterminePath(Vector3 target)
    {
        _pathNodes.Clear();

        if (_agent.debugMode)
        {
            Supporting.Log(string.Format("{0} attempting to generate path to {1}", name, target));
        }

        NavMeshPath path = new NavMeshPath();
        bool pathFound = NavMesh.CalculatePath(_agent.transform.position, target, _courtAreaMask, path);

        if (pathFound)
        {
            foreach (Vector3 pathNode in path.corners)
            {
                _pathNodes.Add(pathNode);

                if (_agent.debugMode)
                {
                    Supporting.Log(string.Format("{0} path: Waypoint {1} >> {2}", name, _pathNodes.IndexOf(pathNode), pathNode));
                }
            }
        }
    }

    public void MoveToPoint(Vector3 destination)
    {
        destination.y = transform.position.y;

        float distance = Vector3.Distance(destination, transform.position);

        if (distance < _destinationBuffer)
        {
            if (_agent.debugMode)
            {
                Supporting.Log(string.Format("{0} arrived at {1}", name, destination));
            }

            if (_pathNodes.Contains(destination))
            {
                _pathNodes.Remove(destination);
            }

            if (_pathNodes.Count >= 1)
            {
                Vector3 finalDestination = _pathNodes[_pathNodes.Count - 1];
                distance = Vector3.Distance(transform.position, finalDestination);

                if (_agent.debugMode)
                {
                    Supporting.Log(string.Format("{0}'s final destination {1} is at {2} away", name, finalDestination, distance));
                }

                if (distance <= _destinationBuffer)
                {
                    _agent.Stop();
                    if (_target)
                    {
                        if (_agent.hit)
                        {
                            if (_agent.debugMode)
                            {
                                Supporting.Log(string.Format("{0} arrived at {1}", name, _agent.outArea.name));
                            }

                            NavMeshObstacle obstacle = gameObject.GetComponent<NavMeshObstacle>();
                            obstacle.carving = true;

                            _agent.SitOut();

                            this.enabled = false;
                        }
                        else
                        {
                            Ball ball = _target.GetComponent<Ball>();
                            if (ball)
                            {
                                _agent.Pickup(ball);
                            }
                        }
                    }
                    else
                    {
                        if (_agent.debugMode)
                        {
                            Supporting.Log(string.Format("{0} distance to {1} is {2}", name, finalDestination, distance));
                        }
                    }

                    return;
                }
            }
        }

        if (_agent.debugMode)
        {
            Supporting.Log(string.Format("{0} navigating to: {1} -  distance: {2}", name, destination, distance));
        }

        Vector3 toDestination = destination - transform.position;
        toDestination.Normalize();
        float rightToDestinationDot = Vector3.Dot(transform.right, toDestination);

        bool shouldTurnRight = rightToDestinationDot > Mathf.Epsilon;

        if (shouldTurnRight)
        {
            _agent.TurnRight();
        }
        else
        {
            _agent.TurnLeft();
        }

        _agent.MoveForwards();
    }

    public Vector3 GenerateRandomPoint(int counter = 0)
    {
        if (counter > 10)
        {
            _agent.Stop();
            return Vector3.zero;
        }

        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 50;
        randomDirection.y = 0;
        randomDirection += transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, 50, _courtAreaMask);

        Vector3 point = navHit.position;

        DeterminePath(point);

        if (_pathNodes.Count > 0)
        {
            return point;
        }
        else
        {
            counter++;
            return GenerateRandomPoint(counter);
        }
    }

    public override void Wander()
    {
        if (!_agent.hit && !_target)
        {
            Scan();
        }

        if (!_target)
        {
            if (_timeWandering >= _wanderingDuration)
            {
                _timeWandering = 0;
                _wanderingDestination = GenerateRandomPoint();
            }
            else
            {
                _timeWandering += Time.deltaTime;
                MoveToPoint(_wanderingDestination);
            }
        }
        else
        {
            MoveToTarget();
        }
    }

    private void Scan()
    {
        DetermineScanMask();

        // if I'm looking for ball and my team has the it, don't bother looking anymore
        if (_currentScanLayer == LayerMask.NameToLayer(GameController.Layers.Ball.ToString()) &&
            GameController.instance.teamWithBall == _agent.team)
        {
            return;
        }

        DetermineScanRadius();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _currentScanRadius, _currentScanLayer);

        foreach (Collider coll in hitColliders)
        {
            if (coll.transform != transform)
            {
                if (Vector3.Distance(transform.position, coll.transform.position) > _destinationBuffer)
                {
                    Agent agent = coll.GetComponent<Agent>();
                    Ball ball = coll.GetComponent<Ball>();

                    if (agent && ValidTarget(agent) || ball && ValidTarget(ball))
                    {
                        _target = coll.transform;

                        if (_agent.debugMode)
                        {
                            Supporting.Log(string.Format("{0} locked onto target {1}", name, _target.name));
                        }

                        break;
                    }
                }
            }
        }
    }

    private bool ValidTarget(Agent target)
    {
        if (target.team == _agent.team || target.hit)
        {
            return false;
        }

        return true;
    }

    private bool ValidTarget(Ball ball)
    {
        if (_agent.debugMode)
        {
            Supporting.Log(string.Format("{0} assessing ball", name));
        }


        if (ball.courtSide != _agent.team || ball.taken)
        {
            return false;
        }

        return true;
    }

    public override void Attack()
    {
        if (!_agent.hasBall || (_target && _target.tag != GameController.Tags.Agent.ToString()))
        {
            return;
        }

        if (!_target)
        {
            Scan();
            return;
        }

        if (_timeAttacking >= _agent.reactionTime)
        {
            _timeAttacking = 0;
            Throw();
        }
        else
        {
            _timeAttacking += Time.deltaTime;
            MoveToTarget();
        }
    }

    public override void Throw()
    {
        _agent.Throw(_target);
        _target = null;
        _agent.Stop();
        StartCoroutine(WaitABit(3));
    }

    private IEnumerator WaitABit(float seconds)
    {
        _waiting = true;
        yield return new WaitForSeconds(seconds);
        _waiting = false;
    }

    public void GoOut()
    {
        _agent.gotHit -= GoOut;
        _agent.Stop();

        DetermineWalkMask();

        Vector3 outPosition = _agent.outArea.position;
        outPosition.z -= (GameController.instance.teamSize - GameController.instance.RemainingTeamCount(_agent)) * (_agent.transform.localScale.z + 2);

        GameObject spot = new GameObject();
        spot.transform.parent = _agent.outArea;
        spot.transform.position = outPosition;
        spot.name = string.Format("Spot for {0}", _agent.name);

        _target = spot.transform;
        _agent.transform.parent = _target;
    }

    public override void Out()
    {
        MoveToTarget();
    }

    public override void Defend()
    {
        Defend(_agent.reactionTime);
    }

    private void Defend(float reactionTime)
    {
        _agent.Stop();
        WaitABit(UnityEngine.Random.Range(0, _agent.reactionTime));

        if (!_waiting)
        {
            _agent.SetDefending();
        }
    }

    private void OnDrawGizmos()
    {
        if (_agent.debugMode)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, _currentScanRadius);

            if (_pathNodes != null)
            {
                for (int i = 0; i < _pathNodes.Count; i++)
                {
                    Gizmos.DrawSphere(_pathNodes[i], 0.3f);
                    if (i < _pathNodes.Count - 1)
                    {
                        Debug.DrawLine(_pathNodes[i], _pathNodes[i + 1], Color.black);
                    }
                }
            }
        }

        if (_agent.hit)
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawCube(_target.position, _target.lossyScale);
        }
    }

    private void OnDisable()
    {
        _agent.tookball -= ClearTarget;
    }
}
