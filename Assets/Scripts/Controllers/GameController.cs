using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : Singleton<GameController>
{
    public const int TEAM_SIZE = 4;

    public enum Teams { None = -1, Red = 0, Blue, Out }
    public enum Tags { Ball = 0, Agent, MiddleLine }
    public enum Layers { Ball, Agent }

    private List<Agent> _redTeam;
    [SerializeField]
    private Material _redTeamMaterial;
    public Material redTeamMaterial
    {
        get { return _redTeamMaterial; }
    }
    [SerializeField]
    private Transform _redTeamArea;
    private Boundaries _redTeamAreaBoundaries;
    public Boundaries redTeamAreaBoundaries
    {
        get { return _redTeamAreaBoundaries; }
    }

    private List<Agent> _blueTeam;
    [SerializeField]
    private Material _blueTeamMaterial;
    public Material blueTeamMaterial
    {
        get { return _blueTeamMaterial; }
    }
    [SerializeField]
    private Transform _blueTeamArea;

    private Boundaries _blueTeamAreaBoundaries;
    public Boundaries blueTeamAreaBoundaries
    {
        get { return _blueTeamAreaBoundaries; }
    }

    [SerializeField]
    private Material _defaultMaterial;
    public Material defaultMaterial
    {
        get { return _defaultMaterial; }
    }

    public Teams teamWithBall
    {
        get { return CheckTeamWithBall(); }
    }

    private Dictionary<Ball, Agent> _balls;

    private Dictionary<Teams, int> _scores;


    private void Start()
    {
        Setup();
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
                    SetScore(Teams.Blue, TEAM_SIZE - _redTeam.Count);
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
                    SetScore(Teams.Red, TEAM_SIZE - _blueTeam.Count);
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

    public Boundaries GetTeamBoundaries(Agent teamMember)
    {
        if (CheckTeamForMember(teamMember, ref _redTeam))
        {
            DetermineBoundaries(Teams.Red);
            return _redTeamAreaBoundaries;
        }
        else if (CheckTeamForMember(teamMember, ref _blueTeam))
        {
            DetermineBoundaries(Teams.Blue);
            return _blueTeamAreaBoundaries;
        }

        return new Boundaries();
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
}
