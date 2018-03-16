using System.Collections;
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

    public Ball _ball;
    public Ball ball
    {
        get { return _ball; }
    }
    public bool hasBall
    {
        get { return _ball; }
    }
    [SerializeField]
    private Vector3 _ballOffset;

    [SerializeField]
    private float _throwForce = 15;
    public float throwForce
    {
        get { return _throwForce; }
    }

    [SerializeField]
    private float _accuracy = 0.5f;

    public Transform outArea
    {
        get { return GameController.instance.GetOutArea(this); }
    }

    private bool _hit;
    public bool hit
    {
        get { return _hit; }
    }

    public bool _defending;
    [SerializeField]
    private float _defenseTime = 1;
    public float _timeDefending;

    private StateManager _stateManager = new StateManager();
    public StateManager stateManager
    {
        get { return _stateManager; }
    }
    public State currentState
    {
        get { return _stateManager.currentState; }
    }

    [SerializeField]
    private float _reactionTime = 0.3f;
    public float reactionTime
    {
        get { return _reactionTime; }
    }

    private AgentController _controller;
    public bool playerControlled
    {
        get { return _controller ? _controller.GetType() == typeof(PlayerController) : false; }
    }
    [SerializeField]
    private GameObject _playerControlMarker;
    public GameObject playerControlMarker
    {
        get { return _playerControlMarker; }
    }
    public Transform target
    {
        get { return !playerControlled && _controller ? (_controller as AIAgentController).target : null; }
    }

    public string TEMP_STATE;
    public bool TEMP_COND_AGENT_HAS_BALL;
    public bool TEMP_COND_AGENT_HIT;
    public bool TEMP_COND_BALL_THROWN;
    public bool TEMP_COND_TARGETTING_OPPONENT;

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

        if (!_meshRenderer)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        if (!_controller)
        {
            _controller = GetComponent<AIAgentController>();
        }

        _angularSpeed = _angularMaxSpeed;
        _linearSpeed = _linearMaxSpeed;

        _boundaries = GameController.instance.GetTeamBoundaries(this);

        InitializeFSM();
    }

    private void InitializeFSM()
    {
        stateManager.Add(State.States.Wander);
        stateManager.Add(State.States.Attack);
        stateManager.Add(State.States.Defend);
        stateManager.Add(State.States.Out);

        stateManager.Add(new WanderToAttack());
        stateManager.Add(new DefendToAttack());
        stateManager.Add(new AttackToWander());
        stateManager.Add(new DefendToWander());
        stateManager.Add(new WanderToDefend());
        stateManager.Add(new AttackToOut());
        stateManager.Add(new DefendToOut());
        stateManager.Add(new WanderToOut());

        stateManager.Add(Condition.Conditions.AgentHasBall);
        stateManager.Add(Condition.Conditions.AgentHit);
        stateManager.Add(Condition.Conditions.TargettingOpponent);
        stateManager.Add(Condition.Conditions.BallThrownByOpponent);

        stateManager.Init(this);

        _stateManager.SetInitialState(State.States.Wander);
    }

    public void SetTeam(GameController.Teams team)
    {
        _team = team;
        EnrollInTeam();
        DetermineAgentColor();
    }

    private void EnrollInTeam()
    {
        GameController.instance.EnrollTeamMember(this);
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

    private void SetMaterial(Material newMaterial)
    {
        _meshRenderer.material = newMaterial;
    }

    public void SetController(AgentController controller)
    {
        _controller = controller;
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     System.Delegate[] subscribers = gotHit.GetInvocationList();
        //     foreach (System.Delegate subscriber in subscribers)
        //     {
        //         Debug.Log(name + " " + subscriber.Target + " " + subscriber.Method);
        //     }
        // }

        if (_stateManager.currentState == null)
        {
            Debug.Break();
        }

        TEMP_STATE = _stateManager.currentState.ToString();
        TEMP_COND_AGENT_HAS_BALL = _stateManager.GetCondition(Condition.Conditions.AgentHasBall).IsTrue();
        TEMP_COND_AGENT_HIT = _stateManager.GetCondition(Condition.Conditions.AgentHit).IsTrue();
        TEMP_COND_BALL_THROWN = _stateManager.GetCondition(Condition.Conditions.BallThrownByOpponent).IsTrue();
        TEMP_COND_TARGETTING_OPPONENT = _stateManager.GetCondition(Condition.Conditions.TargettingOpponent).IsTrue();

        if (!GameController.instance.gameStarted)
        {
            return;
        }

        stateManager.Update();

        if (_hit)
        {
            return;
        }

        if (debugMode)
        {
            Debug.DrawRay(transform.position, transform.forward * 30, Color.blue);
        }

        if (_ball)
        {
            if (debugMode)
            {
                Supporting.Log(string.Format("{0} carrying {1} \n {0} position: {2} - {1} position: {3} - offset {4}",
                    name, _ball.name, transform.position, _ball.transform.position, _ballOffset));
            }

            _ball.Carry(transform, _ballOffset);
        }

        if (currentState != _stateManager.GetState(State.States.Defend) || _timeDefending >= _defenseTime)
        {
            StopDefending();
        }

        // Ensure players stand upright, even if they collide with each other
        transform.rotation = Quaternion.Euler(new Vector3(0, transform.localEulerAngles.y, 0));
    }

    public bool GoingOut()
    {
        if (_hit)
        {
            return false;
        }

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

    public void CrossLine()
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

        if (debugMode)
        {
            Supporting.Log(string.Format("{0} picked up {1}", name, ball.name), 2);
        }

        if (ball)
        {
            _ball = ball;
            _ball.transform.position = transform.forward;
            _ball.transform.rotation = transform.rotation;
            if (tookball != null)
            {
                tookball();
            }

            GameController.instance.KeepTrack(ball, this);
        }
    }

    // called by the State Machine
    public void Throw()
    {
        _controller.Throw();
    }

    // called by the Controller, now with a target to throw at
    public void Throw(Transform target)
    {
        if (_hit)
        {
            return;
        }

        if (debugMode)
        {
            Supporting.Log(string.Format("{0} threw {1} at {2}", name, _ball.name, target.name), 2);
        }

        if (_ball)
        {
            float throwAccuracy = Random.Range(_accuracy, 1);
            _ball.Throw(target, throwForce * throwAccuracy);
            _ball = null;
        }
    }

    public void TakeHit()
    {
        if (!_hit)
        {
            _hit = true;
            Stop();
            GameController.instance.RemoveFromTeam(this);
            SetMaterial(GameController.instance.defaultMaterial);

            if (gotHit != null)
            {
                //System.Delegate[] subscribers = gotHit.GetInvocationList();     
                // Supporting.Log(string.Format("{0} {1} {2}", name, subscribers[0].Target, subscribers[0].Method));
                gotHit();
            }
        }
    }

    // called when the agent arrives at the Out area
    public void SitOut()
    {
        _controller = null;
        StartCoroutine(FaceGame());
        StartCoroutine(WatchGame());
    }

    private IEnumerator FaceGame()
    {
        while (!WatchingGame())
        {
            TurnLeft();
            yield return null;
        }
    }

    private IEnumerator WatchGame()
    {
        yield return new WaitUntil(() => WatchingGame());
        Stop();
        _rb.isKinematic = true;
    }

    private bool WatchingGame()
    {
        int alignment = transform.parent.localEulerAngles.y >= 180 && transform.parent.localEulerAngles.y < 360 ? -1 : 1;
        float angle = Vector3.Angle(transform.forward, alignment * transform.parent.right);
        angle = angle < 0 ? 360 + angle : angle;

        if (debugMode)
        {
            Supporting.Log(string.Format("{0} turning to watch game. Current angle {1}. Rotating towards: {2}", name, angle, alignment * transform.parent.right));
            Debug.DrawRay(transform.position, transform.forward * 15, Color.magenta);
            Debug.DrawRay(transform.position, alignment * transform.parent.right * 15, Color.cyan);
        }

        return angle + _angularMaxSpeed % 360 >= 0 && angle + _angularMaxSpeed % 360 <= 10;
    }

    public void Attack()
    {
        if (_controller)
        {
            _controller.Attack();
        }
    }

    public void Wander()
    {
        if (_controller)
        {
            _controller.Wander();
        }
    }

    public void Defend()
    {
        if (_controller)
        {
            _controller.Defend();
        }
    }

    public void Out()
    {
        if (_controller)
        {
            _controller.Out();
        }
    }

    public void SetDefending()
    {
        if (!_defending)
        {
            _defending = true;
        }
        else
        {
            _timeDefending += Time.deltaTime;

            if (_timeDefending >= _defenseTime)
            {
                StopDefending();
            }
        }
    }

    public void StopDefending()
    {
        _defending = false;
        _timeDefending = 0;
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
                if ((!ball.thrown && !ball.taken && !hasBall) || _defending)
                {
                    ball.Stop();
                    Stop();
                    Pickup(ball);
                }
                else if (ball != _ball && this.team != GameController.instance.WhoThrewBall(ball).team)
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

    private void OnDisable()
    {
        if (tookball != null)
        {
            tookball = null;
        }

        if (gotHit != null)
        {
            gotHit = null;
        }
    }
}
