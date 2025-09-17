using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootInteraction : MonoBehaviour
{
    [Header("Object Animation")]
    public Vector3 rotationSpeed;
    
    void Update()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rotationSpeed * Time.deltaTime);
    }

    public virtual void OnEnterCollider() { }
    public virtual void OnStayCollider() { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnEnterCollider();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnStayCollider();
        }
    }
}
