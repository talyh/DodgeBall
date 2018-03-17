using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtBounds : MonoBehaviour
{
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == GameController.Tags.Agent.ToString())
        {
            coll.GetComponent<Agent>().CrossLine();
        }
    }

    private void OnTriggerStay(Collider coll)
    {
        if (coll.gameObject.tag == GameController.Tags.Ball.ToString())
        {
            coll.GetComponent<Ball>().Respawn();
        }
    }
}
