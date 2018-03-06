using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtBounds : MonoBehaviour
{

    private void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.tag == GameController.Tags.Ball.ToString())
        {
            coll.GetComponent<Ball>().Respawn();
            // TODO consider scoring against the team that lead the nall to go out
        }
        else if (coll.gameObject.tag == GameController.Tags.Agent.ToString())
        {
            coll.GetComponent<Agent>().GoOut();
        }
    }
}
