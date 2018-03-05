using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
public class Agent : MonoBehaviour
{
    [SerializeField]
    private Transform _redPost;
    [SerializeField]
    private Transform _bluePost;

    [SerializeField]
    private Material _redTeam;
    [SerializeField]
    private Material _blueTeam;
    [SerializeField]
    private Material _defaultMaterial;

    [SerializeField]
    private float _linearMaxSpeed;
    public float linearMaxSpeed
    {
        get { return _linearMaxSpeed; }
    }
    [SerializeField]
    private float _angularMaxSpeed;
    public float angularMaxSpeed
    {
        get { return _angularMaxSpeed; }
    }

    [SerializeField]
    private float _linearAcceleration;
    [SerializeField]
    private float _angularAcceleration;

    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private MeshRenderer _meshRenderer;

    public float currentSpeed
    {
        get { return _rb.velocity.magnitude; }
    }

    public float currentAngularSpeed
    {
        get { return _rb.angularVelocity.magnitude; }
    }

    void Update()
    {
        DetermineAgentColor();
    }

    void DetermineAgentColor()
    {
        float distanceToRedPost = Vector3.Distance(transform.position, _redPost.position);
        float distanceToBluePost = Vector3.Distance(transform.position, _bluePost.position);

        if (Mathf.Abs(distanceToBluePost - distanceToRedPost) < 5)
        {
            SetMaterial(_defaultMaterial);
        }
        else if (distanceToBluePost > distanceToRedPost)
        {
            SetMaterial(_redTeam);
        }
        else
        {
            SetMaterial(_blueTeam);
        }
    }

    void SetMaterial(Material newMaterial)
    {
        _meshRenderer.material = newMaterial;
    }

    public void MoveForwards()
    {
        _rb.velocity = transform.forward * _linearMaxSpeed;
    }

    public void MoveBackwards()
    {
        _rb.velocity = -transform.forward * _linearMaxSpeed;
    }

    public void StrafeRight()
    {
        _rb.velocity = transform.right * _linearMaxSpeed;
    }

    public void StrafeLeft()
    {
        _rb.velocity = -transform.right * _linearMaxSpeed;
    }

    public void TurnRight()
    {
        _rb.angularVelocity = transform.up * _angularMaxSpeed;
    }

    public void TurnLeft()
    {
        _rb.angularVelocity = -transform.up * _angularMaxSpeed;
    }

    public void StopMoving()
    {
        _rb.velocity = Vector3.zero;
    }

    public void StopTurning()
    {
        _rb.angularVelocity = Vector3.zero;
    }

    public void MoveToTarget(Transform target, bool debugMode = false)
    {
        Vector3 intendedPosition = transform.position + transform.forward;

        Vector3 desiredPosition = target.position;

        Vector3 steering = desiredPosition - intendedPosition;
        steering = steering / _rb.mass;
        steering *= angularMaxSpeed;

        Vector3 resultingVelocity = intendedPosition - transform.position + steering;
        resultingVelocity.y = 0;
        resultingVelocity *= linearMaxSpeed;

        _rb.velocity = resultingVelocity * Time.fixedDeltaTime;
        transform.LookAt(target);

        if (debugMode)
        {
            Debug.Log("----------------------- Frame " + Time.frameCount + " -----------------------");
            Debug.Log("transform.position: " + transform.position);
            Debug.Log("intendedPosition: " + intendedPosition);
            Debug.DrawRay(transform.position, (intendedPosition - transform.position).normalized * 30, Color.blue);
            Debug.Log("desiredPosition: " + desiredPosition);
            Debug.DrawRay(transform.position, (desiredPosition - transform.position).normalized * 30, Color.red);
            Debug.Log("steering: " + steering);
            Debug.DrawRay(transform.position, steering.normalized * 30, Color.green);
            Debug.Log("resultingVelocity: " + resultingVelocity + " (scaled to Time.fixedDeltaTime)");
            Debug.DrawRay(transform.position, resultingVelocity.normalized * 30, Color.magenta);
        }
    }
}
