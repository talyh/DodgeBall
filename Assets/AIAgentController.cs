using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgentController : MonoBehaviour
{
    [SerializeField]
    private Agent _agent;

    private Vector3 _destination = Vector3.zero;

    [SerializeField]
    private float _scanRadius = 10;
    [SerializeField]
    private float _destinationBuffer = 5;

    LayerMask scan;

    // Use this for initialization
    void Start()
    {
        scan = 1 << LayerMask.NameToLayer("Agent") |
                1 << LayerMask.NameToLayer("Interactable");
    }

    // Update is called once per frame
    void Update()
    {
        if (_destination != Vector3.zero)
        {
            MoveToDestination();
        }
        else
        {
            Scan();
        }
    }

    private void MoveToDestination()
    {
        Vector3 lookAt = _agent.transform.forward;
        lookAt.Normalize();
        Vector3 toDestination = _destination - _agent.transform.position;
        float distanceToDestination = toDestination.magnitude;
        toDestination.Normalize();
        float angleToDestination = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(lookAt, toDestination));

        Debug.DrawRay(transform.position, transform.forward * distanceToDestination, Color.blue);
        Debug.DrawRay(transform.position, _destination * distanceToDestination, Color.green);

        Debug.Log("angle: " + angleToDestination);

        if (angleToDestination < 10)
        {
            _agent.StopTurning();
            _agent.MoveForwards();
        }
        else
        {
            _agent.TurnLeft();
        }

        if (Vector3.Distance(_agent.transform.position, _destination) < 2)
        {
            _agent.StopMoving();
        }
    }

    private void Scan()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _scanRadius, scan);

        foreach (Collider coll in hitColliders)
        {
            if (Vector3.Distance(transform.position, coll.transform.position) > _destinationBuffer)
            {
                _destination = coll.transform.position;
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, _scanRadius);
    }
}
