using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

[RequireComponent(typeof(Agent))]
public class AIAgentController : MonoBehaviour
{
    [Header("Agent Configuration")]
    [SerializeField]
    private Agent _agent;

    private int _courtAreaMask;

    public Transform _target; // TODO change

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

    // private bool _alignedToTarget = false;

    private List<Vector3> _pathNodes;

    // private bool _movingToTarget;

    private bool _wandering;
    private int _wanderingAction;
    private float _timeWandering;
    [SerializeField]
    private float _maxTimeWandering;

    [Header("Engaging")]
    [SerializeField]
    private float _minThrowingDistance = 2;
    [SerializeField]
    private float _maxThrowingDistance = 8;


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

        DetermineWalkMask();

        _pathNodes = new List<Vector3>();
    }

    private void DetermineWalkMask()
    {
        _courtAreaMask = 1 << NavMesh.GetAreaFromName(_agent.team.ToString());
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
        if (_agent.debugMode)
        {
            Supporting.Log(string.Format("{0} is at {1}", name, transform.position));
        }

        if (_target)
        {
            MoveToTarget();
            DetermineThorw();
        }
        else
        {
            Scan();
        }

        if (!_target)
        {
            NavMeshPath path = new NavMeshPath();

            Vector3 origin = _agent.transform.position;
            Vector3 destination = transform.TransformPoint(_agent.transform.forward);

            bool pathForward = NavMesh.CalculatePath(origin, destination, _courtAreaMask, path);

            if (!pathForward)
            {
                if (_agent.debugMode)
                {
                    Supporting.Log(string.Format("{0} couldn't find a path forward from {1} to {2} in area {3}",
                            name, origin, destination, Supporting.GetNavMeshIndex(_courtAreaMask), 2));
                }

                return;
            }

            if (!_wandering || _timeWandering >= _maxTimeWandering)
            {
                _agent.Stop();
                _wanderingAction = _agent.Wander();
                _wandering = true;
                _timeWandering = 0;
            }
            else
            {
                _agent.Wander(_wanderingAction);
                _timeWandering += Time.deltaTime;
            }
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

        // Vector3 toDestination = destination - transform.position;
        // float distanceToDestination = toDestination.magnitude;

        float distance = Vector3.Distance(destination, transform.position);

        if (distance < _destinationBuffer)
        {
            if (_agent.debugMode)
            {
                Supporting.Log(string.Format("{0} arrived at {1}", name, destination));
            }

            _pathNodes.Remove(destination);

            if (_pathNodes.Count <= 1)
            {
                _agent.Stop();
                _target = null;
            }

            return;
        }

        if (_agent.debugMode)
        {
            Supporting.Log(string.Format("{0} navigating to: {1} -  distance: {1}", name, destination, distance));
        }

        Vector3 toDestination = destination - transform.position;
        toDestination.Normalize();
        // float lookAtToDestinationDot = Vector3.Dot(transform.forward, toDestination);
        float rightToDestinationDot = Vector3.Dot(transform.right, toDestination);
        // float toDestinationAngle = Mathf.Rad2Deg * Mathf.Acos(lookAtToDestinationDot);

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

    private void Scan()
    {
        DetermineScanMask();

        // if my team has the ball, don't bother scanning for it
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

                    if (agent)
                    {
                        if (agent.team == _agent.team)
                        {
                            return;
                        }
                    }

                    _target = coll.transform;
                    _wandering = false;

                    if (_agent.debugMode)
                    {
                        Supporting.Log(string.Format("{0} locked onto target {1}", name, _target.name));
                    }

                    break;
                }
            }
        }
    }

    private void DetermineThorw()
    {
        if (!_agent.hasBall || _target.tag != GameController.Tags.Agent.ToString())
        {
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, _target.position);

        if (distanceToTarget > _minThrowingDistance && distanceToTarget < _maxThrowingDistance)
        {
            Throw();
        }
    }

    private void Throw()
    {
        _agent.Throw(_target);
        _target = null;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (_agent.debugMode)
        {
            Supporting.Log(string.Format("{0} triggered {1}", name, coll.name));
        }

        if (coll.gameObject.tag == GameController.Tags.MiddleLine.ToString())
        {
            _agent.GoOut();
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
                foreach (Vector3 position in _pathNodes)
                {
                    Gizmos.DrawCube(position, new Vector3(1, 1, 1));
                }
            }
        }
    }

    private void OnDisable()
    {
        _agent.tookball -= ClearTarget;
    }
}
