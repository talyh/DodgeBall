using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool _thrown;
    public bool thrown
    {
        get { return _thrown; }
    }

    public bool taken
    {
        get { return GameController.instance.CheckBallIsTaken(this); }
    }

    private Vector3 _spawnPosition;

    private Rigidbody _rb = null;

    private Transform _target;
    private float _desiredAirTime = 1.0f;

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

        _spawnPosition = transform.position;
        GameController.instance.KeepTrack(this, null);
    }

    public void Carry(Transform agent, Vector3 offset)
    {
        transform.position = agent.position + offset;
    }

    public void Throw(Transform target, float throwForce)
    {
        if (!_thrown)
        {
            _thrown = true;
            Stop();
            _target = target;
            _target.GetComponent<Agent>().gotHit += Hit;
            _desiredAirTime = 60 / throwForce;
            _rb.velocity = CalculateInitialVelocityMovingTarget();
        }
    }

    public void Respawn()
    {
        Stop();
        transform.position = _spawnPosition;
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
        float yDisplacement = displacement.y;
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
        _thrown = false;
        _target.GetComponent<Agent>().gotHit -= Hit;
        _target = null;
    }

    private void OnCollisionEnter(Collision coll)
    {
        Stop();

        if (coll.gameObject.tag != GameController.Tags.Agent.ToString())
        {
            _thrown = false;
        }
    }

    private void OnDisable()
    {
        if (_target)
        {
            _target.GetComponent<Agent>().gotHit -= Hit;
        }
    }
}

