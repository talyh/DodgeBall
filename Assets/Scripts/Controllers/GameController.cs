using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : Singleton<GameController>
{
    public enum Teams { Red = 0, Blue }
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


    [SerializeField]
    private Material _defaultMaterial;
    public Material defaultMaterial
    {
        get { return _defaultMaterial; }
    }


    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        _redTeam = new List<Agent>();
        _blueTeam = new List<Agent>();
    }

    private void DetermineBoundaries(Teams team)
    {
        Bounds boundaries;
        switch (team)
        {
            case Teams.Red:
                boundaries = _redTeamArea.GetComponent<MeshRenderer>().bounds;
                _redTeamAreaBoundaries.minX = _redTeamArea.transform.position.x - boundaries.size.x / 2;
                _redTeamAreaBoundaries.maxX = _redTeamArea.transform.position.x + boundaries.size.x / 2;
                _redTeamAreaBoundaries.minZ = _redTeamArea.transform.position.z - boundaries.size.z / 2;
                _redTeamAreaBoundaries.maxZ = _redTeamArea.transform.position.z + boundaries.size.z / 2;
                break;
            case Teams.Blue:
                boundaries = _blueTeamArea.GetComponent<MeshRenderer>().bounds;
                _blueTeamAreaBoundaries.minX = _blueTeamArea.transform.position.x - boundaries.size.x / 2;
                _blueTeamAreaBoundaries.maxX = _blueTeamArea.transform.position.x + boundaries.size.x / 2;
                _blueTeamAreaBoundaries.minZ = _blueTeamArea.transform.position.z - boundaries.size.z / 2;
                _blueTeamAreaBoundaries.maxZ = _blueTeamArea.transform.position.z + boundaries.size.z / 2;
                break;
            default:
                break;
        }
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
}
