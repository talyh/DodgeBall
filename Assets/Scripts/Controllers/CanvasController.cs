using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : Singleton<CanvasController>
{
    [SerializeField]
    private GameObject _teamSelection;

    [SerializeField]
    private Text _redScore;
    [SerializeField]
    private Text _blueScore;
    [SerializeField]
    private Text _winner;
    [SerializeField]
    private Color _red;
    [SerializeField]
    private Color _blue;

    public void SetScore(GameController.Teams team, int score)
    {
        switch (team)
        {
            case GameController.Teams.Red:
                _redScore.text = score.ToString();
                break;
            case GameController.Teams.Blue:
                _blueScore.text = score.ToString();
                break;
            default:
                Supporting.Log(string.Format("Couldn't set score of {0} for team {1}", score, team.ToString()), 1);
                break;
        }
    }

    public void SetPlayerTeamChoice(string team)
    {
        if (team.ToUpper() == GameController.Teams.Red.ToString().ToUpper())
        {
            GameController.instance.SetPlayerTeam(GameController.Teams.Red);
        }
        else if (team.ToUpper() == GameController.Teams.Blue.ToString().ToUpper())
        {
            GameController.instance.SetPlayerTeam(GameController.Teams.Blue);
        }
        else
        {
            Supporting.Log(string.Format("Couldn't determine the right team for {0}", team), 1);
        }

        _teamSelection.SetActive(false);
    }

    public void ShowWinner(GameController.Teams winner)
    {
        _winner.text = string.Format("{0} Wins!", winner.ToString());

        switch (winner)
        {
            case GameController.Teams.Red:
                _winner.color = _red;
                break;
            case GameController.Teams.Blue:
                _winner.color = _blue;
                break;
            default:
                Supporting.Log(string.Format("Couldn't resolve team for {0}", winner), 1);
                break;
        }

        _winner.transform.parent.gameObject.SetActive(true);
    }
}
