using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
// [RequireComponent(typeof(SpringJoint))]
[RequireComponent(typeof(MeshRenderer))]
public class Agent : MonoBehaviour
{
    public delegate void TookBall();
    public event TookBall tookball;

    public bool debugMode;

    [SerializeField]
    private GameController.Teams _team;
    public GameController.Teams team
    {
        get { return _team; }
    }

    private bool _hasBall;
    public bool hasBall
    {
        // get { return _hasBall; }
        get { return _ball; }
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
    // [SerializeField]
    // private SpringJoint _springJoint;
    // [SerializeField]
    // private float _spring = 5;
    [SerializeField]
    private MeshRenderer _meshRenderer;

    private Boundaries _boundaries;
    [SerializeField]
    private float _maxDistanceToBoundaries = 2;
    public Ball _ball; // TODO change
    [SerializeField]
    private float _throwForce = 15;
    public float throwForce
    {
        get { return _throwForce; }
    }

    void Start()
    {
        EnrollInTeam();
        Setup();
        DetermineAgentColor();
    }

    void Setup()
    {
        if (!_rb)
        {
            _rb = GetComponent<Rigidbody>();
        }

        // if (!_springJoint)
        // {
        //     _springJoint = GetComponent<SpringJoint>();
        // }

        // _springJoint.spring = 0;

        if (!_meshRenderer)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        _angularSpeed = _angularMaxSpeed;
        _linearSpeed = _linearMaxSpeed;

        _boundaries = GameController.instance.GetTeamBoundaries(this);
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

    private void LateUpdate()
    {
        Contain();
    }

    private void Contain()
    {
        if (GoingOut())
        {
            Stop();
            TurnBack();
        }
    }

    private bool GoingOut()
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

    public int Wander()
    {
        int action = Random.Range(0, 100);

        if (debugMode)
        {
            Supporting.Log(string.Format("{0} decided to {1}", name, action));
        }

        Wander(action);

        return action;
    }

    public void Wander(int action)
    {
        if (action >= 0 && action <= 20)
        {
            MoveForwards();
        }
        else if (action > 20 && action <= 40)
        {
            MoveBackwards();
        }
        else if (action > 40 && action <= 60)
        {
            StrafeLeft();
        }
        else if (action > 60 && action <= 80)
        {
            StrafeRight();
        }
        else if (action > 80 && action <= 85)
        {
            TurnRight();
        }
        else if (action > 85 && action <= 90)
        {
            TurnRight();
        }
        else if (action > 90)
        {
            Stop();
        }
    }

    public void GoOut()
    {
        Supporting.Log(string.Format("{0} is out", name));
        Stop();
    }

    public void Pickup(Rigidbody ballRB)
    {
        Ball ball = ballRB.GetComponent<Ball>();

        // _springJoint.spring = _spring;
        // _springJoint.connectedBody = ballRB;
        // _hasBall = true;
        if (ball)
        {
            _ball = ball;
            tookball();
        }
    }

    public void Throw(Transform target)
    {
        Debug.Log("THROWING");

        // Ball ball = _springJoint.connectedBody.GetComponent<Ball>();

        if (_ball)
        {
            // _springJoint.connectedBody = null;
            // _springJoint.spring = 0;
            // _hasBall = false;

            _ball.Throw(target, throwForce);
            _ball = null;
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == GameController.Tags.Ball.ToString())
        {
            Rigidbody ballRB = coll.gameObject.GetComponent<Rigidbody>();
            Ball ball = coll.gameObject.GetComponent<Ball>();

            if (ballRB & ball)
            {
                if (debugMode)
                {
                    Supporting.Log(string.Format("{0} picked up {1}", name, ball.name));
                }

                if (!ball.thrown)
                {
                    Pickup(ballRB);
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
