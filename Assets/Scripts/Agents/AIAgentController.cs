using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

[RequireComponent(typeof(Agent))]
public class AIAgentController : MonoBehaviour
{
    [SerializeField]
    private bool _debugMode;

    [SerializeField]
    private Agent _agent;

    private int _courtAreaMask;

    public Transform _target; // TODO change

    [SerializeField]
    private float _scanRadius = 10;

    [SerializeField]
    private float _destinationBuffer = 5;

    // private bool _alignedToTarget = false;

    private LayerMask _scanLayer;

    private List<Vector3> _pathNodes;

    private bool _movingToTarget;


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

        DetermineWalkMask();
        DetermineScanMask();

        _pathNodes = new List<Vector3>();
    }

    private void DetermineWalkMask()
    {
        _courtAreaMask = 1 << NavMesh.GetAreaFromName(_agent.team.ToString());
    }

    private void DetermineScanMask()
    {
        // _scanLayer = 1 << LayerMask.NameToLayer("Agent") |
        //                      1 << LayerMask.NameToLayer("Interactable");
    }

    private void Update()
    {
        if (_agent.hasBall)
        {
            _target = null;
        }
    }

    private void FixedUpdate()
    {
        if (_debugMode)
        {
            Supporting.Log("--------------- New Frame -----------------");
            Supporting.Log(string.Format("{0} at {1}", gameObject.name, transform.position));
        }

        if (!_target)
        {
            // Scan();
            _agent.MoveForwards();
        }
        else
        {
            MoveToTarget();
        }

        if (_debugMode)
        {
            Supporting.Log("-------------------------------------------");
        }
    }

    private void MoveToTarget()
    {
        DeterminePath(_target.position);

        if (_pathNodes.Count > 0)
        {
            if (_debugMode)
            {
                Supporting.Log(string.Format("{0} found path to target {1}", gameObject.name, _target.name));
            }

            MoveToPoint(_pathNodes[1]);
        }
        else
        {
            if (_debugMode)
            {
                Supporting.Log(string.Format("{0} couldn't find path to target {1}", gameObject.name, _target.name), 2);
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

                if (_debugMode)
                {
                    Supporting.Log(string.Format("{0} path: Waypoint {1} >> {2}", gameObject.name, _pathNodes.IndexOf(pathNode), pathNode));
                }
            }
        }
    }

    public void MoveToPoint(Vector3 destination)
    {
        destination.y = transform.position.y;

        if (_debugMode)
        {
            Supporting.Log(string.Format("{0} navigating to target: {1}", gameObject.name, destination));
        }

        // Vector3 toDestination = destination - transform.position;
        // float distanceToDestination = toDestination.magnitude;

        float distance = Vector3.Distance(destination, transform.position);

        if (_debugMode)
        {
            Supporting.Log(string.Format("{0} distance to target: {1}", gameObject.name, distance));
        }

        if (distance < _destinationBuffer)
        {
            if (_debugMode)
            {
                Supporting.Log(string.Format("{0} arrived at {1}", gameObject.name, destination));
            }

            _pathNodes.Remove(destination);

            if (_pathNodes.Count <= 1)
            {
                _agent.Stop();
                _target = null;
            }

            return;
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
        // Collider[] hitColliders = Physics.OverlapSphere(transform.position, _scanRadius, _scanLayer);

        // foreach (Collider coll in hitColliders)
        // {
        //     if (coll.transform != transform)
        //     {
        //         if (Vector3.Distance(transform.position, coll.transform.position) > _destinationBuffer)
        //         {
        //             _target = coll.transform;
        //             break;
        //         }
        //     }
        // }

        // if (_debugMode)
        // {
        //Debug.Log("Target locked: " + _target.gameObject.name);
        // }
    }

    private void OnDrawGizmos()
    {
        if (_debugMode)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, _scanRadius);

            if (_pathNodes != null)
            {
                foreach (Vector3 position in _pathNodes)
                {
                    Gizmos.DrawCube(position, new Vector3(1, 1, 1));
                }
            }
        }
    }
}
