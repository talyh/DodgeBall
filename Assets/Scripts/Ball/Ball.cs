using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Ball : MonoBehaviour
{
    public bool debugMode;

    private bool _thrown;
    public bool thrown
    {
        get { return _thrown; }
    }

    public bool taken
    {
        get { return GameController.instance.CheckBallIsTaken(this); }
    }

    private Vector3 _spawnPosition;

    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private SphereCollider _collider;

    private Transform _target;
    private float _desiredAirTime = 1.0f;
    private float _timeOnAir;

    private GameController.Teams _courtSide;
    public GameController.Teams courtSide
    {
        get { return _courtSide; }
    }

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        if (!_rb)
        {
            _rb = GetComponent<Rigidbody>();
        }

        if (!_collider)
        {
            _collider = GetComponent<SphereCollider>();
        }

        _spawnPosition = transform.position;
        GameController.instance.KeepTrack(this, null);
    }

    private void Update()
    {
        if (thrown)
        {
            _timeOnAir += Time.deltaTime;
        }

        if (_timeOnAir >= _desiredAirTime * 0.5)
        {
            _collider.enabled = true;
        }

        DetermineCourtSide();

        if (_courtSide == GameController.Teams.Out)
        {
            Respawn();
        }
    }

    public void Carry(Transform agent, Vector3 offset)
    {
        _rb.isKinematic = true;
        _collider.enabled = false;
        transform.rotation = agent.rotation;
        transform.position = agent.position + agent.forward + offset;
    }

    public void Throw(Transform target, float throwForce)
    {
        if (!_thrown)
        {
            _rb.isKinematic = false;
            _thrown = true;
            Stop();
            _target = target;
            _target.GetComponent<Agent>().gotHit += Hit;
            _desiredAirTime = 60 / throwForce;
            _rb.velocity = CalculateInitialVelocityMovingTarget();

            if (debugMode)
            {
                Supporting.Log(string.Format("{0} thrown. desiredAirTime = {1} // velocity = {2}", name, _desiredAirTime, _rb.velocity), 2);
            }
        }
    }

    public void Respawn()
    {
        if (debugMode)
        {
            Supporting.Log(string.Format("{0} respawned", name), 2);
        }

        Stop();

        transform.position = _spawnPosition;
        _thrown = false;
    }

    public void StopMoving()
    {
        _rb.velocity = Vector3.zero;
    }

    public void StopTurning()
    {
        _rb.angularVelocity = Vector3.zero;
    }

    public void Stop()
    {
        StopMoving();
        StopTurning();

        _timeOnAir = 0;
    }

    private Vector3 CalculateInitialVelocityMovingTarget()
    {
        Vector3 targetVelocity = _target.GetComponent<Rigidbody>().velocity;
        Vector3 targetDisplacement = targetVelocity * _desiredAirTime;
        Vector3 targetPosition = _target.position + targetDisplacement;
        return CalculateInitialVelocity(targetPosition);
    }

    private Vector3 CalculateInitialVelocity(Vector3 targetPosition)
    {
        Vector3 displacement = targetPosition - this.transform.position;
        displacement.y = 0.0f;
        float horizontalDisplacement = displacement.magnitude;
        if (horizontalDisplacement < Mathf.Epsilon)
        {
            return Vector3.zero;
        }

        float horizontalSpeed = horizontalDisplacement / _desiredAirTime;

        float time = horizontalDisplacement / horizontalSpeed;
        time *= 0.5f;
        Vector3 initialYVelocity = Physics.gravity * time * -1.0f;
        displacement.Normalize();
        Vector3 initialHorizontalVelocity = displacement * horizontalSpeed;
        return initialHorizontalVelocity + initialYVelocity;
    }

    private void Hit()
    {
        if (_target)
        {
            _thrown = false;
            _target.GetComponent<Agent>().gotHit -= Hit;
            _target = null;
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        Stop();

        if (coll.gameObject.tag != GameController.Tags.Agent.ToString())
        {
            _thrown = false;
        }
    }

    // private void OnDisable()
    // {
    //     if (_target)
    //     {
    //         _target.GetComponent<Agent>().gotHit -= Hit;
    //     }
    // }

    private void DetermineCourtSide()
    {
        if (transform.position.z > GameController.instance.redTeamAreaBoundaries.minZ
            && transform.position.z < GameController.instance.redTeamAreaBoundaries.maxZ)
        {
            _courtSide = GameController.Teams.Red;
        }
        else if (transform.position.z > GameController.instance.blueTeamAreaBoundaries.minZ
             && transform.position.z < GameController.instance.blueTeamAreaBoundaries.maxZ)
        {
            _courtSide = GameController.Teams.Blue;
        }
        else
        {
            _courtSide = GameController.Teams.Out;
        }
    }
}

