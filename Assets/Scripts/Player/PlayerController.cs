using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Controller
{

    [SerializeField]
    private Agent _agent;

    private void Update()
    {
        if (!GameController.instance.gameStarted)
        {
            return;
        }

        if (!_agent)
        {
            return;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            _agent.MoveForwards();
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _agent.StrafeLeft();
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            _agent.MoveBackwards();
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _agent.StrafeRight();
        }
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftShift))
        {
            _agent.TurnLeft();
        }
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.RightShift))
        {
            _agent.TurnRight();
        }

        if (Input.GetKeyUp(KeyCode.W)
            || Input.GetKeyUp(KeyCode.UpArrow)
            || Input.GetKeyUp(KeyCode.A)
            || Input.GetKeyUp(KeyCode.LeftArrow)
            || Input.GetKeyUp(KeyCode.S)
            || Input.GetKeyUp(KeyCode.DownArrow)
            || Input.GetKeyUp(KeyCode.Q)
            || Input.GetKeyUp(KeyCode.RightArrow)
            )
        {
            _agent.StopMoving();
        }
        if (Input.GetKeyUp(KeyCode.Q)
            || Input.GetKeyUp(KeyCode.LeftShift)
            || Input.GetKeyUp(KeyCode.E)
            || Input.GetKeyUp(KeyCode.RightShift)
            )
        {
            _agent.StopTurning();
        }
    }

    public void SetAgent(Agent agent)
    {
        if (_agent)
        {
            _agent.gotHit -= ChooseNewAgent;
        }

        _agent = agent;
        _agent.gotHit += ChooseNewAgent;
    }

    private void ChooseNewAgent()
    {
        _agent.playerControlMarker.SetActive(false);
        int teamCount = GameController.instance.RemainingTeamCount(_agent);

        if (teamCount > 0)
        {
            int random = Random.Range(0, teamCount);
            GameController.instance.SetPlayerControlled(_agent.team, random);
        }
    }

    private void OnDisable()
    {
        if (_agent)
        {
            _agent.gotHit -= ChooseNewAgent;
        }
    }
}