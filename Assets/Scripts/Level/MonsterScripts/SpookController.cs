using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpookController : MonsterController
{

    // Start is called before the first frame update
    new void Start()
    {
        deathItem = "LaserBeam";
        spriteFlipper = true;
        base.Start();

        obstaclesForAttacking = new HashSet<Vector3> ();
        obstaclesForRandomMotion = obstaclesForAttacking;
        randomMotionLocations = coordinateSystem.GetObstacleFreeLocations(obstaclesForRandomMotion, randomMotionArea).ToList<Vector3>();

        gameObject.GetComponent<CircleCollider2D>().isTrigger = true;
    }

    new private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Used" + deathItem)
        {
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

    protected override bool isInLOS()
    {
        return true;
    }
}
