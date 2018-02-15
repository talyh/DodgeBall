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

    public Transform _target;

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
            MoveToTarget();
        }
    }

    private void MoveToTarget()
    {


        Vector3 intendedPosition = transform.position + transform.forward;


        Vector3 desiredPosition = _target.position;

        Vector3 steering = desiredPosition - intendedPosition;
        steering = steering / _rb.mass;
        steering *= _agent.angularMaxSpeed;

        Vector3 resultingVelocity = intendedPosition - transform.position + steering;
        resultingVelocity.y = 0;
        resultingVelocity *= _agent.linearMaxSpeed;


        _rb.velocity = resultingVelocity * Time.deltaTime;
        transform.LookAt(_target);

        if (_debugMode)
        {
            Debug.Log("----------------------- " + Time.frameCount + " -----------------------");
            Debug.Log("transform.position: " + transform.position);
            Debug.Log("intendedPosition: " + intendedPosition);
            Debug.DrawRay(transform.position, (intendedPosition - transform.position).normalized * 30, Color.blue);
            Debug.Log("desiredPosition: " + desiredPosition);
            Debug.DrawRay(transform.position, (desiredPosition - transform.position).normalized * 30, Color.red);
            Debug.Log("steering: " + steering);
            Debug.DrawRay(transform.position, steering.normalized * 30, Color.green);
            Debug.Log("resultingVelocity: " + resultingVelocity);
            Debug.DrawRay(transform.position, resultingVelocity.normalized * 30, Color.magenta);
        }
    }

    private void Scan()
    {
        // Collider[] hitColliders = Physics.OverlapSphere(transform.position, _scanRadius);

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
