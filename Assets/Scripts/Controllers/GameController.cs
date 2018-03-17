using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : Singleton<GameController>
{
    private bool _gameStarted;
    public bool gameStarted
    {
        get { return _gameStarted; }
    }

    public enum Teams { None = -1, Red = 0, Blue, Out }
    public enum Tags { Ball = 0, Agent, MiddleLine }
    public enum Layers { Ball, Agent }

    [Header("Red Team")]
    [SerializeField]
    private GameObject _redTeamStructure;
    [SerializeField]
    private Material _redTeamMaterial;
    public Material redTeamMaterial
    {
        get { return _redTeamMaterial; }
    }
    [SerializeField]
    private Transform _redTeamArea;
    [SerializeField]
    private Transform _redTeamOutArea;
    private Boundaries _redTeamAreaBoundaries;
    public Boundaries redTeamAreaBoundaries
    {
        get { return _redTeamAreaBoundaries; }
    }
    private List<Agent> _redTeam;

    [Header("Blue Team")]
    [SerializeField]
    private GameObject _blueTeamStructure;
    [SerializeField]
    private Material _blueTeamMaterial;
    public Material blueTeamMaterial
    {
        get { return _blueTeamMaterial; }
    }
    [SerializeField]
    private Transform _blueTeamArea;
    [SerializeField]
    private Transform _blueTeamOutArea;
    private Boundaries _blueTeamAreaBoundaries;
    public Boundaries blueTeamAreaBoundaries
    {
        get { return _blueTeamAreaBoundaries; }
    }
    private List<Agent> _blueTeam;


    [Header("General")]
    [SerializeField]
    private Material _defaultMaterial;
    public Material defaultMaterial
    {
        get { return _defaultMaterial; }
    }

    [SerializeField]
    private int _teamSize = 6;
    public int teamSize
    {
        get { return _teamSize; }
    }

    [SerializeField]
    private GameObject _teamMemberPrefab;

    [SerializeField]
    private PlayerController _playerController;

    public Teams teamWithBall
    {
        get { return CheckTeamWithBall(); }
    }

    private Dictionary<Ball, Agent> _balls;

    private Dictionary<Teams, int> _scores;

    private void Start()
    {
        Setup();
        GenerateTeams();
    }

    private void Setup()
    {
        if (_redTeam == null)
        {
            _redTeam = new List<Agent>();
        }

        if (_blueTeam == null)
        {
            _blueTeam = new List<Agent>();
        }

        if (_balls == null)
        {
            _balls = new Dictionary<Ball, Agent>();
        }

        if (_scores == null)
        {
            _scores = new Dictionary<Teams, int>();
        }

        if (_playerController == null)
        {
            GameObject.FindObjectOfType<PlayerController>();
        }
    }

    private void GenerateTeams()
    {
        GenerateTeam(Teams.Red);
        GenerateTeam(Teams.Blue);
    }

    private void GenerateTeam(Teams team)
    {
        for (int i = 0; i < _teamSize; i++)
        {
            Vector3 randomPosition = GetRandomPositionInCourt(team);
            float randomRotation = Random.Range(0, 360);

            GameObject teamMember = Instantiate(_teamMemberPrefab, randomPosition, Quaternion.Euler(new Vector3(0, randomRotation, 0)));
            Agent agent = teamMember.GetComponent<Agent>();

            agent.SetTeam(team);

            if (team == Teams.Red)
            {
                teamMember.transform.parent = _redTeamStructure.transform;
            }
            else if (team == Teams.Blue)
            {
                teamMember.transform.parent = _blueTeamStructure.transform;
            }

            agent.name = string.Format("{0} player {1}", team, i);
        }
    }

    private Vector3 GetRandomPositionInCourt(Teams courtSide)
    {

        float randomX = Random.Range(GetTeamBoundaries(courtSide).minX + Agent.maxDistanceToBoundaries, GetTeamBoundaries(courtSide).maxX - Agent.maxDistanceToBoundaries);
        float randomZ = Random.Range(GetTeamBoundaries(courtSide).minZ + Agent.maxDistanceToBoundaries, GetTeamBoundaries(courtSide).maxZ - Agent.maxDistanceToBoundaries);

        List<Agent> team = null;
        switch (courtSide)
        {
            case Teams.Red:
                team = _redTeam;
                break;
            case Teams.Blue:
                team = _blueTeam;
                break;
            default:
                Supporting.Log(string.Format("Couldn't resolve team for {0}", courtSide), 1);
                break;

        }

        Vector3 randomPosition = new Vector3(randomX, 0, randomZ);

        if (team.Find(agent => Vector3.Distance(agent.transform.position, randomPosition) < 3))
        {
            return GetRandomPositionInCourt(courtSide);
        }

        return randomPosition;
    }

    public void SetPlayerTeam(Teams team)
    {
        List<Agent> teamMembers = null;

        switch (team)
        {
            case Teams.Red:
                teamMembers = _redTeam;
                break;
            case Teams.Blue:
                teamMembers = _blueTeam;
                break;
            default:
                Supporting.Log(string.Format("Couldn't determine the container for team {0}", team));
                break;
        }

        if (teamMembers != null)
        {
            int random = Random.Range(0, teamMembers.Count);
            Agent selected = teamMembers[random];
            SetPlayerControlled(selected);

            _gameStarted = true;
        }
    }

    private void SetPlayerControlled(Agent teamMember)
    {
        teamMember.gameObject.GetComponent<AIAgentController>().enabled = false;
        teamMember.SetController(_playerController);
        teamMember.playerControlMarker.SetActive(true);
        _playerController.SetAgent(teamMember);
    }

    public void SetPlayerControlled(Teams team, int position)
    {
        List<Agent> teamMembers = null;

        switch (team)
        {
            case Teams.Red:
                teamMembers = _redTeam;
                break;
            case Teams.Blue:
                teamMembers = _blueTeam;
                break;
            default:
                Supporting.Log(string.Format("Couldn't determine the container for team {0}", team));
                break;
        }

        if (teamMembers != null)
        {
            SetPlayerControlled(teamMembers[position]);
        }
    }

    private void DetermineBoundaries(Teams team)
    {
        switch (team)
        {
            case Teams.Red:
                SetBoundaries(ref _redTeamAreaBoundaries, _redTeamArea, _redTeamArea.GetComponent<MeshRenderer>().bounds);
                break;
            case Teams.Blue:
                SetBoundaries(ref _blueTeamAreaBoundaries, _blueTeamArea, _blueTeamArea.GetComponent<MeshRenderer>().bounds);
                break;
            default:
                break;
        }
    }

    private void SetBoundaries(ref Boundaries boundaries, Transform area, Bounds bounds)
    {
        boundaries.minX = area.position.x - bounds.size.x / 2;
        boundaries.maxX = area.position.x + bounds.size.x / 2;
        boundaries.minZ = area.position.z - bounds.size.z / 2;
        boundaries.maxZ = area.position.z + bounds.size.z / 2;
    }

    public void EnrollTeamMember(Agent teamMember)
    {
        switch (teamMember.team)
        {
            case Teams.Red:
                if (!CheckTeamForMember(teamMember, ref _redTeam))
                {
                    _redTeam.Add(teamMember);
                }
                else
                {
                    Supporting.Log(string.Format("{0} cannot be added twice to Red Team", teamMember.gameObject), 1);
                }
                break;
            case Teams.Blue:
                if (!CheckTeamForMember(teamMember, ref _blueTeam))
                {
                    _blueTeam.Add(teamMember);
                }
                else
                {
                    Supporting.Log(string.Format("{0} cannot be added twice to Blue Team", teamMember.gameObject), 1);
                }
                break;
            default:
                Supporting.Log(string.Format("Couldn't determine team for {0}", teamMember.gameObject), 1);
                break;
        }
    }

    public void RemoveFromTeam(Agent teamMember)
    {
        switch (teamMember.team)
        {
            case Teams.Red:
                if (CheckTeamForMember(teamMember, ref _redTeam))
                {
                    _redTeam.Remove(teamMember);
                    SetScore(Teams.Blue, teamSize - _redTeam.Count);
                }
                else
                {
                    Supporting.Log(string.Format("{0} cannot be found on Red Team. Couldn't Remove.", teamMember.gameObject), 1);
                }
                break;
            case Teams.Blue:
                if (CheckTeamForMember(teamMember, ref _blueTeam))
                {
                    _blueTeam.Remove(teamMember);
                    SetScore(Teams.Red, teamSize - _blueTeam.Count);
                }
                else
                {
                    Supporting.Log(string.Format("{0} cannot be found on Blue Team. Couldn't Remove.", teamMember.gameObject), 1);
                }
                break;
            default:
                Supporting.Log(string.Format("Couldn't determine team for {0}", teamMember.gameObject), 1);
                break;
        }
    }

    private void SetScore(Teams team, int score)
    {
        if (_scores.ContainsKey(team))
        {
            _scores[team] = score;
        }
        else
        {
            _scores.Add(team, score);
        }

        CanvasController.instance.SetScore(team, score);
    }

    private bool CheckTeamForMember(Agent teamMember, ref List<Agent> team)
    {
        if (team == null)
        {
            Setup();
        }

        return team.Contains(teamMember);
    }

    public Boundaries GetTeamBoundaries(Teams team)
    {
        switch (team)
        {
            case Teams.Red:
                DetermineBoundaries(Teams.Red);
                return _redTeamAreaBoundaries;
            case Teams.Blue:
                DetermineBoundaries(Teams.Blue);
                return _blueTeamAreaBoundaries;
            default:
                return new Boundaries();
        }
    }

    public Boundaries GetTeamBoundaries(Agent teamMember)
    {
        return GetTeamBoundaries(teamMember.team);
    }

    public Transform GetOutArea(Agent teamMember)
    {
        switch (teamMember.team)
        {
            case Teams.Red:
                return _redTeamOutArea;
            case Teams.Blue:
                return _blueTeamOutArea;
            default:
                return null;
        }
    }

    private Teams CheckTeamWithBall()
    {
        if (_blueTeam.Exists(agent => agent.hasBall))
        {
            return Teams.Blue;
        }
        else if (_redTeam.Exists(agent => agent.hasBall))
        {
            return Teams.Red;
        }

        return Teams.None;
    }

    public bool CheckBallIsTaken(Ball ball)
    {
        List<Agent> combined = _blueTeam;
        combined.Concat(_redTeam);

        return combined.Exists(agent => agent.ball == ball);
    }

    public void KeepTrack(Ball ball, Agent agent)
    {
        if (_balls == null)
        {
            _balls = new Dictionary<Ball, Agent>();
        }

        if (!_balls.ContainsKey(ball))
        {
            _balls.Add(ball, agent);
        }
        else
        {
            _balls[ball] = agent;
        }
    }

    public Agent WhoThrewBall(Ball ball)
    {
        return _balls[ball];
    }

    public int RemainingTeamCount(Agent agent)
    {
        switch (agent.team)
        {
            case Teams.Red:
                return _redTeam.Count;
            case Teams.Blue:
                return _blueTeam.Count;
            default:
                return -1;
        }
    }

    public KeyValuePair<Ball, Agent>[] BallThrown()
    {
        return _balls.Where(entry => entry.Key.thrown).ToArray();
    }
}
