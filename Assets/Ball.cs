// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Ball : MonoBehaviour
// {

//     [SerializeField]
//     private Rigidbody _rb;

//     [SerializeField]
//     private Transform _target;

//     private float _toTarget;
//     private float _force;

//     private float _ballStaticFriction = 0.2f;
//     private float _ballDynamicFriction = 0.3f;
//     private float _groundStaticFriction = 0.6f;
//     private float _groundDynamicFriction = 0.4f;

//     private float _normalForce;
//     private float _startingForce;

//     [SerializeField]
//     private float _time = 2;


//     // Use this for initialization
//     void Start()
//     {
//     }

//     // Update is called once per frame
//     void FixedUpdate()
//     {
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             CalculateDistanceToTarget();
//             CalculateForceNeeded();

//             _rb.AddForce(transform.forward * _force);
//         }
//     }

//     private void CalculateDistanceToTarget()
//     {
//         _toTarget = Vector3.Distance(transform.position, _target.position);
//     }

//     private void CalculateForceNeeded()
//     {
//         CalculateNormalForce();
//         CalculateAcceleration();
//     }

//     private void CalculateNormalForce()
//     {
//         _normalForce = _rb.mass * Physics.gravity.magnitude;
//         Debug.Log("Normal Force: " + _normalForce);
//     }

//     private void CalculateStartingForce()
//     {
//         float staticFriction = StaticFrictionCoeficient();
//         Debug.Log("Static Friction: " + staticFriction);

//         _startingForce = _normalForce * staticFriction;
//         Debug.Log("Starting Force: " + _startingForce);
//     }

//     private float StaticFrictionCoeficient()
//     {
//         return (_ballStaticFriction + _groundStaticFriction) * 0.5f;
//     }

//     private float DynamicFrictionCoeficient()
//     {
//         return (_ballDynamicFriction * _groundDynamicFriction) * 0.5f;
//     }

//     private float CalculateFriction()
//     {
//         return DynamicFrictionCoeficient() * _normalForce;
//     }

//     private float CalculateFrictionDeceleration()
//     {
//         return CalculateFriction() / _rb.mass;
//     }

//     private void CalculateAcceleration()
//     {
//         // return _toTarget / 0.5f * Mathf.Pow(_time, 2);
//         _force = CalculateFinalForce() - CalculateFrictionDeceleration();
//     }

//     private float FinalVelocity()
//     {
//         return _toTarget * _time * 2;
//     }

//     private float CalculateFinalForce()
//     {
//         return FinalVelocity() / _time;
//     }
// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Transform _target;

    private Rigidbody _rb = null;

    enum FrictionType { Static = 0, Dynamic };

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 toDestination = _target.position - transform.position;
            float distance = Mathf.Abs(toDestination.z);
            float frictionalForce = -1.0f * CalculateFrictionalForce(FrictionType.Dynamic);
            float acceleration = ConvertForceToAcceleration(frictionalForce, _rb.mass);
            float speed = CalculateInitialVelocity(0.0f, acceleration, distance);
            _rb.velocity = transform.forward * speed;
        }
    }

    float CalculateFrictionalForce(FrictionType frictionType)
    {
        return CalculateNormalForce() * CalculateFrictionCoefficient(frictionType);
    }

    float CalculateNormalForce()
    {
        return Physics.gravity.y * -1.0f * _rb.mass;
    }

    float CalculateFrictionCoefficient(FrictionType frictionType)
    {
        float frictionCoefficient = 0.0f;

        //determine our friction values
        Collider coll = GetComponent<Collider>();
        float ourFriction = (frictionType == FrictionType.Static) ? coll.material.staticFriction : coll.material.dynamicFriction;
        PhysicMaterialCombine ourCombine = coll.material.frictionCombine;

        //check if we are colliding against an object
        Vector3 ourPosition = transform.position;
        float ourHeight = transform.localScale.y;
        float ourGroundBuffer = 0.25f;

        int layer = LayerMask.NameToLayer("Ball");
        int layerMask = 1 << layer;
        layerMask = ~layerMask;
        RaycastHit hit;

        if (Physics.Raycast(ourPosition, -Vector3.up, out hit, (ourHeight * 0.5f) + ourGroundBuffer, layerMask))
        {
            float hitFriction = (frictionType == FrictionType.Static) ? hit.collider.material.staticFriction : hit.collider.material.dynamicFriction;
            PhysicMaterialCombine hitCombine = hit.collider.material.frictionCombine;
            //Average < Minimum < Multiply < Maximum
            bool isMax = hitCombine == PhysicMaterialCombine.Maximum || ourCombine == PhysicMaterialCombine.Maximum;
            bool isMultiply = hitCombine == PhysicMaterialCombine.Multiply || ourCombine == PhysicMaterialCombine.Multiply;
            bool isMin = hitCombine == PhysicMaterialCombine.Minimum || ourCombine == PhysicMaterialCombine.Minimum;
            bool isAverage = hitCombine == PhysicMaterialCombine.Average || ourCombine == PhysicMaterialCombine.Average;

            if (isMax)
            {
                frictionCoefficient = hitFriction > ourFriction ? hitFriction : ourFriction;
            }
            else if (isMultiply)
            {
                frictionCoefficient = hitFriction * ourFriction;
            }
            else if (isMin)
            {
                frictionCoefficient = hitFriction < ourFriction ? hitFriction : ourFriction;
            }
            else if (isAverage)
            {
                frictionCoefficient = (hitFriction + ourFriction) * 0.5f;
            }
        }

        return frictionCoefficient;
    }

    float ConvertForceToAcceleration(float force, float mass)
    {
        return mass > Mathf.Epsilon ? force / mass : 0.0f;
    }

    float CalculateInitialVelocity(float finalVelocity, float acceleration, float distance)
    {
        //vfˆ2 = viˆ2 + 2ad
        //vfˆ2 - 2ad = viˆ2
        //viˆ2 = vfˆ2 - 2ad
        //sqrt(vi2) = sqrt(vf2 - 2ad)
        return Mathf.Sqrt((finalVelocity * finalVelocity) - (2 * acceleration * distance));
    }
}