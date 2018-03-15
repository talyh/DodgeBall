using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

[RequireComponent(typeof(Agent))]
public class AIAgentController : Controller
{
    [Header("Agent Configuration")]
    [SerializeField]
    private Agent _agent;

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
            _courtAreaMask = 1 << NavMesh.GetAreaFromName(GameController.Teams.Out.ToString())
                | 1 << NavMesh.GetAreaFromName(_agent.team.ToString());
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

    private void ClearTarget()
    {
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
        if (_agent.hit)
        {
            DetermineWalkMask();
            Vector3 targetPosition = _target.position;
            targetPosition.z -= (GameController.instance.teamSize - GameController.instance.RemainingTeamCount(_agent)) * _agent.transform.localScale.z;
            DeterminePath(targetPosition);
        }
        else
        {
            DeterminePath(_target.position);

            if (_agent.debugMode)
            {
                Supporting.Log(string.Format("{0} decided to move to {1} to throw at {2}", name, _target.position, _target.name));
            }
        }

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

            _pathNodes.Remove(destination);

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
                    Ball ball = _target.GetComponent<Ball>();
                    if (ball)
                    {
                        if (_agent.debugMode)
                        {
                            Supporting.Log(string.Format("{0} picked up {1}", name, ball.name));
                        }

                        _agent.Pickup(ball);
                        _target = null;
                    }
                    else if (_target == _agent.outArea)
                    {
                        if (_agent.debugMode)
                        {
                            Supporting.Log(string.Format("{0} arrived at {1}", name, _agent.outArea.name));
                        }

                        NavMeshObstacle obstacle = gameObject.GetComponent<NavMeshObstacle>();
                        obstacle.carving = true;

                        _agent.GoOut();

                        this.enabled = false;
                    }
                    else
                    {
                        if (_agent.debugMode)
                        {
                            Supporting.Log(string.Format("{0} distance to {1} is {2}", name, finalDestination, distance));
                        }
                    }
                }

                return;
            }
        }

        if (_agent.debugMode)
        {
            Supporting.Log(string.Format("{0} navigating to: {1} -  distance: {2}", name, destination, distance));
        }

        Vector3 toDestination = destination - transform.position;
        toDestination.Normalize();
        float lookAtToDestinationDot = Vector3.Dot(transform.forward, toDestination);
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

    public void Wander()
    {
        if (_target)
        {
            MoveToTarget();
            DetermineThrow();
        }
        else
        {
            if (!_agent.hit)
            {
                Scan();
            }
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

    private bool ValidTarget(Agent agent)
    {
        if (agent.team == _agent.team || agent.team == GameController.Teams.Out)
        {
            return false;
        }

        return true;
    }

    private bool ValidTarget(Ball ball)
    {
        if (ball.courtSide != _agent.team)
        {
            return false;
        }

        return true;
    }

    private void DetermineThrow()
    {
        if (!_agent.hasBall || _target.tag != GameController.Tags.Agent.ToString())
        {
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, _target.position);

        if (distanceToTarget >= _agent.minThrowingDistance)
        {
            Throw();
        }
    }

    public void Throw()
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

    private void GoOut()
    {
        _agent.Stop();
        _target = _agent.outArea;
    }

    public void Defend(float reactionTime)
    {
        _agent.Stop();
        WaitABit(reactionTime);
        Wander();
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
    }

    private void OnDisable()
    {
        _agent.tookball -= ClearTarget;
        _agent.gotHit -= GoOut;
    }
}
