using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
public class Agent : MonoBehaviour
{
    [SerializeField]
    private Transform _redPost;
    [SerializeField]
    private Transform _bluePost;

    [SerializeField]
    private Material _redTeam;
    [SerializeField]
    private Material _blueTeam;
    [SerializeField]
    private Material _defaultMaterial;

    [SerializeField]
    private float _linearMaxSpeed;
    [SerializeField]
    private float _angularMaxSpeed;
    [SerializeField]
    private float _linearAcceleration;
    [SerializeField]
    private float _angularAcceleration;

    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private MeshRenderer _meshRenderer;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        DetermineAgentColor();
    }

    void DetermineAgentColor()
    {
        float distanceToRedPost = Vector3.Distance(transform.position, _redPost.position);
        float distanceToBluePost = Vector3.Distance(transform.position, _bluePost.position);

        if (Mathf.Abs(distanceToBluePost - distanceToRedPost) < 5)
        {
            SetMaterial(_defaultMaterial);
        }
        else if (distanceToBluePost > distanceToRedPost)
        {
            SetMaterial(_redTeam);
        }
        else
        {
            SetMaterial(_blueTeam);
        }
    }

    void SetMaterial(Material newMaterial)
    {
        _meshRenderer.material = newMaterial;
    }

    public void MoveForwards()
    {
        _rb.velocity = transform.forward * _linearMaxSpeed;
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
        _rb.angularVelocity = transform.up * _angularMaxSpeed;
    }

    public void TurnLeft()
    {
        _rb.angularVelocity = -transform.up * _angularMaxSpeed;
    }

    public void StopMoving()
    {
        _rb.velocity = Vector3.zero;
    }

    public void StopTurning()
    {
        _rb.angularVelocity = Vector3.zero;
    }
}
