using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxHealthItem : Item
{

    public float addAmount = 1;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if(other.gameObject.tag == "Player")
        {
            other.GetComponent<LivingEntity>().AddMaxHealth(addAmount);
            Destroy(gameObject);
        }
    }
}
