using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
public class Agent : MonoBehaviour
{
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

    void Start()
    {
        Setup();
        DetermineAgentColor();
        EnrollInTeam();

        _angularSpeed = _angularMaxSpeed;
        _linearSpeed = _linearMaxSpeed;
    }

    void Setup()
    {
        if (!_rb)
        {
            _rb = GetComponent<Rigidbody>();
        }

        if (_meshRenderer)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
    }

    void DetermineAgentColor()
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

    public void MoveForwards()
    {
        _rb.velocity = transform.forward * _linearSpeed;
    }

    public void MoveBackwards()
    {
        _rb.velocity = -transform.forward * _linearSpeed;
    }

    public void StrafeRight()
    {
        _rb.velocity = transform.right * _linearSpeed;
    }

    public void StrafeLeft()
    {
        _rb.velocity = -transform.right * _linearSpeed;
    }

    public void TurnRight()
    {
        _rb.angularVelocity = transform.up * _angularSpeed;
    }

    public void TurnLeft()
    {
        _rb.angularVelocity = -transform.up * _angularSpeed;
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

    private void OnCollisionEnter(Collision coll)
    {
        StopMoving();
        StopTurning();

        if (coll.gameObject.tag == GameController.Tags.Ball.ToString())
        {
            Ball ball = coll.gameObject.GetComponent<Ball>();

            if (ball)
            {
                ball.Attach(this);
            }
        }
    }
}
