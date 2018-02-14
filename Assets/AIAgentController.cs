using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AIAgentController : MonoBehaviour
{
    [SerializeField]
    private Agent _agent;

    private Transform _target;

    [SerializeField]
    private float _scanRadius = 10;
    [SerializeField]
    private float _destinationBuffer = 5;

    private bool _alignedToTarget = false;
    private bool arrived
    {
        get { return _target ? false : Vector3.Distance(_agent.transform.position, _target.position) < _destinationBuffer; }
    }

    private LayerMask _scanLayer;

    // Use this for initialization
    void Start()
    {
        _scanLayer = 1 << LayerMask.NameToLayer("Agent") |
                     1 << LayerMask.NameToLayer("Interactable");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_target && arrived)
        {
            return;
        }

        if (!_target)
        {
            Scan();
        }
        else
        {
            CheckAlignmentToTarget();

            if (!_alignedToTarget)
            {
                StartCoroutine(TurnToTarget());
            }

            MoveToTarget();
        }
    }

    private void MoveToTarget()
    {
        if (!_alignedToTarget)
        {
            return;
        }

        _agent.MoveForwards();
    }

    private void Scan()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _scanRadius);

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
    }

    IEnumerator TurnToTarget()
    {
        bool targetToLeft = _target.position.x - transform.position.x < 1;

        while (!_alignedToTarget)
        {
            Debug.DrawLine(transform.position, _target.position, Color.red);
            Debug.DrawRay(transform.position, transform.forward * 10, Color.blue);

            if (targetToLeft)
            {
                _agent.TurnLeft();
            }
            else
            {
                _agent.TurnRight();
            }

            CheckAlignmentToTarget();

            yield return null;
        }

        _agent.StopTurning();
    }

    void CheckAlignmentToTarget()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit);

        _alignedToTarget = hit.transform == _target;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, _scanRadius);
    }
}
