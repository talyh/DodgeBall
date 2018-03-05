using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AIAgentController : MonoBehaviour
{
    [SerializeField]
    private bool _debugMode;
    [SerializeField]
    private Agent _agent;

    private Transform _target;

    [SerializeField]
    private float _scanRadius = 10;
    [SerializeField]
    private float _destinationBuffer = 5;

    private bool _alignedToTarget = false;

    private LayerMask _scanLayer;

    private Rigidbody _rb;

    // Use this for initialization
    void Start()
    {
        _scanLayer = 1 << LayerMask.NameToLayer("Agent") |
                     1 << LayerMask.NameToLayer("Interactable");

        _rb = _agent.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_target && Vector3.Distance(_agent.transform.position, _target.position) < _destinationBuffer)
        {
            _agent.StopMoving();
            _agent.StopTurning();
            return;
        }

        if (!_target)
        {
            Scan();
        }
        else
        {
            _agent.MoveToTarget(_target, _debugMode);
        }
    }

    private void Scan()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _scanRadius, _scanLayer);

        foreach (Collider coll in hitColliders)
        {
            if (coll.transform != transform)
            {
                if (Vector3.Distance(transform.position, coll.transform.position) > _destinationBuffer)
                {
                    _target = coll.transform;
                    break;
                }
            }
        }

        if (_debugMode)
        {
            Debug.Log("Target locked: " + _target.gameObject.name);
        }
    }

    private void OnDrawGizmos()
    {
        if (_debugMode)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, _scanRadius);
        }
    }
}
