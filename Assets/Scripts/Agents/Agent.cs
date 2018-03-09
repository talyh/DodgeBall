﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
public class Agent : MonoBehaviour
{
    public delegate void TookBall();
    public event TookBall tookball;

    public delegate void GotHit();
    public event GotHit gotHit;

    public bool debugMode;

    [SerializeField]
    private GameController.Teams _team;
    public GameController.Teams team
    {
        get { return _team; }
    }

    private float _linearSpeed;
    public float linearSpeed
    {
        get { return _linearSpeed; }
        set { _linearSpeed = value; }
    }
    [SerializeField]
    private float _linearMaxSpeed;
    public float linearMaxSpeed
    {
        get { return _linearMaxSpeed; }
    }

    private float _angularSpeed;
    public float angularSpeed
    {
        get { return _angularSpeed; }
        set { _angularSpeed = value; }
    }
    [SerializeField]
    private float _angularMaxSpeed;
    public float angularMaxSpeed
    {
        get { return _angularMaxSpeed; }
    }

    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private MeshRenderer _meshRenderer;

    private Boundaries _boundaries;
    [SerializeField]
    private float _maxDistanceToBoundaries = 2;

    public Ball _ball; // TODO change
    public Ball ball
    {
        get { return _ball; }
    }
    public bool hasBall
    {
        get { return _ball; }
    }

    [SerializeField]
    private float _throwForce = 15;
    public float throwForce
    {
        get { return _throwForce; }
    }
    [SerializeField]
    private Transform _outArea;
    public Transform outArea
    {
        get { return _outArea; }
    }

    public bool _hit;
    public bool hit
    {
        get { return _hit; }
    }

    private void Start()
    {
        EnrollInTeam();
        Setup();
        DetermineAgentColor();
    }

    private void Setup()
    {
        if (!_rb)
        {
            _rb = GetComponent<Rigidbody>();
        }

        if (!_meshRenderer)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        _angularSpeed = _angularMaxSpeed;
        _linearSpeed = _linearMaxSpeed;

        _boundaries = GameController.instance.GetTeamBoundaries(this);
    }

    private void DetermineAgentColor()
    {
        switch (_team)
        {
            case GameController.Teams.Red:
                SetMaterial(GameController.instance.redTeamMaterial);
                break;
            case GameController.Teams.Blue:
                SetMaterial(GameController.instance.blueTeamMaterial);
                break;
            default:
                SetMaterial(GameController.instance.defaultMaterial);
                break;
        }
    }

    void SetMaterial(Material newMaterial)
    {
        _meshRenderer.material = newMaterial;
    }

    void EnrollInTeam()
    {
        GameController.instance.EnrollTeamMember(this);
    }

    private void Update()
    {
        if (_hit)
        {
            return;
        }

        if (_ball)
        {
            Vector3 offset = _ball.transform.position - transform.position;
            _ball.Carry(transform, offset);
        }
    }

    public bool GoingOut()
    {
        float x = transform.position.x + transform.forward.x;
        float z = transform.position.z + transform.forward.z;

        if (x - _maxDistanceToBoundaries < _boundaries.minX
            || x + _maxDistanceToBoundaries > _boundaries.maxX
            || z - _maxDistanceToBoundaries < _boundaries.minZ
            || z + _maxDistanceToBoundaries > _boundaries.maxZ)
        {
            if (debugMode)
            {
                Supporting.Log(string.Format("{0} is trying to go out \n current position: {1} \n X boundaries {2} to {3} \n Z boundaries {4} to {5}",
                                 name, transform.position, _boundaries.minX, _boundaries.maxX, _boundaries.minZ, _boundaries.maxZ));
            }

            return true;
        }

        return false;

        // return GameController.instance.GoingOut(transform, _boundaries, _maxDistanceToBoundaries);
    }

    public void MoveForwards()
    {
        _rb.velocity = transform.forward * _linearSpeed;
    }


    public void TurnRight()
    {
        _rb.angularVelocity = transform.up * _angularSpeed;
    }

    public void TurnLeft()
    {
        _rb.angularVelocity = -transform.up * _angularSpeed;
    }

    public void TurnBack()
    {
        _rb.angularVelocity = -transform.up * 180;
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

    public void GoOut()
    {
        if (_hit)
        {
            return;
        }

        TakeHit();
    }

    public void Pickup(Ball ball)
    {
        if (_hit)
        {
            return;
        }

        if (ball)
        {
            _ball = ball;
            tookball();

            GameController.instance.KeepTrack(ball, this);
        }
    }

    public void Throw(Transform target)
    {
        if (_hit)
        {
            return;
        }

        if (_ball)
        {
            _ball.Throw(target, throwForce);
            _ball = null;
        }
    }

    public void TakeHit()
    {
        _hit = true;
        gotHit();
        GameController.instance.RemoveFromTeam(this);
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (_hit)
        {
            return;
        }

        if (coll.gameObject.tag == GameController.Tags.Ball.ToString())
        {
            Ball ball = coll.gameObject.GetComponent<Ball>();

            if (ball)
            {
                if (debugMode)
                {
                    Supporting.Log(string.Format("{0} picked up {1}", name, ball.name));
                }

                if (!ball.thrown && !ball.taken && !hasBall)
                {
                    Pickup(ball);
                }
                else if (ball != _ball && this != GameController.instance.WhoThrewBall(ball))
                {
                    TakeHit();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            Gizmos.color = Color.black;
            Vector3 size = new Vector3(1, 1, 1);
            float time = 0.1f;

            Vector3 cornerA = new Vector3(_boundaries.minX, 0, _boundaries.minZ);
            Gizmos.DrawCube(cornerA, size);
            if (Application.isPlaying && Time.timeSinceLevelLoad < time)
            {
                Supporting.Log(string.Format("{0}'s corner A: {1}", name, cornerA));
            }

            Vector3 cornerB = new Vector3(_boundaries.maxX, 0, _boundaries.minZ);
            Gizmos.DrawCube(cornerB, size);
            if (Application.isPlaying && Time.timeSinceLevelLoad < time)
            {
                Supporting.Log(string.Format("{0}'s corner B: {1}", name, cornerB));
            }

            Vector3 cornerC = new Vector3(_boundaries.minX, 0, _boundaries.maxZ);
            Gizmos.DrawCube(cornerC, size);
            if (Application.isPlaying && Time.timeSinceLevelLoad < time)
            {
                Supporting.Log(string.Format("{0}'s corner C: {1}", name, cornerC));
            }

            Vector3 cornerD = new Vector3(_boundaries.maxX, 0, _boundaries.maxZ);
            Gizmos.DrawCube(cornerD, size);
            if (Application.isPlaying && Time.timeSinceLevelLoad < time)
            {
                Supporting.Log(string.Format("{0}'s corner D: {1}", name, cornerD));
            }
        }
    }
}
