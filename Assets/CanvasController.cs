using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : Singleton<CanvasController>
{

    [SerializeField]
    private Text _redScore;
    [SerializeField]
    private Text _blueScore;

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
}
