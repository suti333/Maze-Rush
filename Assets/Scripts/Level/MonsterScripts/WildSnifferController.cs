using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using UnityEngine;

public class WildSnifferController : MonsterController
{
    protected override bool isInLOS()
    {
        return true;
    }

    new private void Start()
    {
        deathItem = "Blade";
        spriteFlipper = true;
        base.Start();

    }

    new private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Used" + deathItem)
        {
            Destroy(collision.gameObject);
            deathItemLocation = collision.transform.position;
            removeDeathItem = true;
            Destroy(gameObject);
        }
        else base.OnTriggerEnter2D(collision);
    }

    new private void OnDestroy()
    {
        if (!levelController.IsPlayerDead)
        {
            soundManager.PlaySoundWithoutBlocking(monsterDeathSound, isPlayingDeathSound);
        }

        base.OnDestroy();
    }
}
