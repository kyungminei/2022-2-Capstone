using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    public ParticleSystem particle;
    public SphereCollider sphere;
    public int Damage;

    Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.AddForce(new Vector3(0, -40, 0),ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag=="Floor")
        {
            sphere.enabled = false;
            particle.gameObject.SetActive(true);
            Destroy(gameObject, 1.0f);
        }
        else if(collision.gameObject.tag=="Wall")
        {
            Destroy(gameObject);
        }
    }
}
