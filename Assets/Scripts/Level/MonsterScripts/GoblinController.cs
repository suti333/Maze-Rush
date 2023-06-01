using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GoblinController : MonsterController
{
    private Projecter projecter;
    private SoundManager.Bool isPlayingProjectedAxeSound;

    new private void Awake()
    {
        base.Awake();
        projecter = new Projecter();
        isPlayingProjectedAxeSound = new SoundManager.Bool(false);
    }

    new void Start()
    {
        deathItem = "Bullet";
        spriteFlipper = true;
        base.Start();
    }

    new private void Update()
    {
        if (levelController.IsGameActive)
        {
            base.Update();

            if ((isAttacking || justLeftAttacking) && transform.childCount == 0)
            {
                GameObject projectedAxe;

                if (!isPlayingProjectedAxeSound.Value)
                {
                    projectedAxe = projecter.Project("Axe", transform.position, (Player.transform.position - transform.position), Player.GetComponent<PlayerController>().BulletProjectionSpeed - 2.5f, projectileSound);
                }
                else
                {
                    projectedAxe = projecter.Project("Axe", transform.position, (Player.transform.position - transform.position), Player.GetComponent<PlayerController>().BulletProjectionSpeed - 2.5f);
                }

                projectedAxe.transform.parent = transform;
                projectedAxe.GetComponent<Rigidbody2D>().angularVelocity = 720f;
                projectedAxe.GetComponent<ProjectileController>().IsPlayingProjectileSound = isPlayingProjectedAxeSound;
            }
        }
    }

    new private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(collision.name == "Axe"))
        {
            if (collision.name == "Used" + deathItem)
            {
                Destroy(gameObject);
                if (!collision.CompareTag("ProjectedItem")) Destroy(collision.gameObject);
            }
            else base.OnTriggerEnter2D(collision);
        }
    }

    new private void OnDestroy()
    {
        if (!levelController.IsPlayerDead)
        {
            soundManager.PlaySoundWithoutBlocking(monsterDeathSound, isPlayingDeathSound);
        }
    }

}
