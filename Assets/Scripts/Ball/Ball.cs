﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Ball : MonoBehaviour
{
    [SerializeField]
    private bool _debugMode;

    private Vector3 _spawnPosition;

    // [SerializeField]
    // private Transform _target = null;

    [SerializeField]
    private Rigidbody _rb = null;

    // [SerializeField]
    // private float _angularMaxSpeed = 5;

    // private enum FrictionType { Static = 0, Dynamic };

    // private bool _alignedToTarget = false;

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

        _spawnPosition = transform.position;
    }

    // void FixedUpdate()
    // {
    //     CheckAlignmentToTarget();

    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         if (!_alignedToTarget)
    //         {
    //             StartCoroutine(TurnToTarget());
    //         }
    //     }

    //     if (_alignedToTarget)
    //     {
    //         MoveToTarget();
    //     }
    // }

    // IEnumerator TurnToTarget()
    // {
    //     bool targetToLeft = _target.position.x - transform.position.x < 1;

    //     while (!_alignedToTarget)
    //     {
    //         if (targetToLeft)
    //         {
    //             TurnLeft();
    //         }
    //         else
    //         {
    //             TurnRight();
    //         }

    //         CheckAlignmentToTarget();

    //         yield return null;
    //     }

    //     StopTurning();
    // }

    // void CheckAlignmentToTarget()
    // {
    //     RaycastHit hit;
    //     Physics.Raycast(transform.position, transform.forward, out hit);

    //     _alignedToTarget = hit.transform == _target;
    // }

    // void MoveToTarget()
    // {
    //     Vector3 toDestination = _target.position - transform.position;
    //     float distance = Mathf.Abs(toDestination.z);

    //     float frictionalForce = -1.0f * CalculateFrictionalForce(FrictionType.Dynamic);
    //     float acceleration = ConvertForceToAcceleration(frictionalForce, _rb.mass);
    //     float speed = CalculateInitialVelocity(0.0f, acceleration, distance);
    //     _rb.velocity = transform.forward * speed;

    //     Debug.DrawRay(transform.position, transform.forward * distance, Color.blue);
    //     Debug.DrawLine(transform.position, _target.position, Color.green);
    // }

    // float CalculateFrictionalForce(FrictionType frictionType)
    // {
    //     return CalculateNormalForce() * CalculateFrictionCoefficient(frictionType);
    // }

    // float CalculateNormalForce()
    // {
    //     return Physics.gravity.y * -1.0f * _rb.mass;
    // }

    // float CalculateFrictionCoefficient(FrictionType frictionType)
    // {
    //     float frictionCoefficient = 0.0f;

    //     //determine our friction values
    //     Collider coll = GetComponent<Collider>();
    //     float ourFriction = (frictionType == FrictionType.Static) ? coll.material.staticFriction : coll.material.dynamicFriction;
    //     PhysicMaterialCombine ourCombine = coll.material.frictionCombine;

    //     //check if we are colliding against an object
    //     Vector3 ourPosition = transform.position;
    //     float ourHeight = transform.localScale.y;
    //     float ourGroundBuffer = 0.25f;

    //     int layer = LayerMask.NameToLayer("Ball");
    //     int layerMask = 1 << layer;
    //     layerMask = ~layerMask;
    //     RaycastHit hit;

    //     if (Physics.Raycast(ourPosition, -Vector3.up, out hit, (ourHeight * 0.5f) + ourGroundBuffer, layerMask))
    //     {
    //         float hitFriction = (frictionType == FrictionType.Static) ? hit.collider.material.staticFriction : hit.collider.material.dynamicFriction;
    //         PhysicMaterialCombine hitCombine = hit.collider.material.frictionCombine;
    //         //Average < Minimum < Multiply < Maximum
    //         bool isMax = hitCombine == PhysicMaterialCombine.Maximum || ourCombine == PhysicMaterialCombine.Maximum;
    //         bool isMultiply = hitCombine == PhysicMaterialCombine.Multiply || ourCombine == PhysicMaterialCombine.Multiply;
    //         bool isMin = hitCombine == PhysicMaterialCombine.Minimum || ourCombine == PhysicMaterialCombine.Minimum;
    //         bool isAverage = hitCombine == PhysicMaterialCombine.Average || ourCombine == PhysicMaterialCombine.Average;

    //         if (isMax)
    //         {
    //             frictionCoefficient = hitFriction > ourFriction ? hitFriction : ourFriction;
    //         }
    //         else if (isMultiply)
    //         {
    //             frictionCoefficient = hitFriction * ourFriction;
    //         }
    //         else if (isMin)
    //         {
    //             frictionCoefficient = hitFriction < ourFriction ? hitFriction : ourFriction;
    //         }
    //         else if (isAverage)
    //         {
    //             frictionCoefficient = (hitFriction + ourFriction) * 0.5f;
    //         }
    //     }

    //     return frictionCoefficient;
    // }

    // float ConvertForceToAcceleration(float force, float mass)
    // {
    //     return mass > Mathf.Epsilon ? force / mass : 0.0f;
    // }

    // float CalculateInitialVelocity(float finalVelocity, float acceleration, float distance)
    // {
    //     //vfˆ2 = viˆ2 + 2ad
    //     //vfˆ2 - 2ad = viˆ2
    //     //viˆ2 = vfˆ2 - 2ad
    //     //sqrt(vi2) = sqrt(vf2 - 2ad)
    //     return Mathf.Sqrt((finalVelocity * finalVelocity) - (2 * acceleration * distance));
    // }

    // public void TurnLeft()
    // {
    //     _rb.angularVelocity = -transform.up * _angularMaxSpeed * Mathf.Deg2Rad;
    // }

    // public void TurnRight()
    // {
    //     _rb.angularVelocity = transform.up * _angularMaxSpeed * Mathf.Deg2Rad;
    // }

    public void Throw()
    {
        transform.position = transform.forward * 5;
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

    public void Respawn()
    {
        Stop();
        transform.position = _spawnPosition;
    }
}