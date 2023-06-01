using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RaptorbotController : MonsterController
{
    private bool enableTeleport;

    new void Start()
    {
        deathItem = "Blade";
        spriteFlipper = false;
        enableTeleport = false;
        base.Start();
    }

    new private void Update()
    {
        if (levelController.IsGameActive)
        {
            base.Update();

            if (isAttacking || justLeftAttacking)
            {
                levelController.DisableTeleports(gameObject.GetInstanceID());
                enableTeleport = true;
            }
            else 
            {
                if (enableTeleport) levelController.EnableTeleports(gameObject.GetInstanceID());
            }
        }
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

            if (enableTeleport) levelController.EnableTeleports(gameObject.GetInstanceID());
        }

        base.OnDestroy();
    }
}
