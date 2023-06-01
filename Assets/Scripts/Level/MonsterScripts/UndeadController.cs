using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UndeadController : MonsterController
{
    private ShortVisionController shortVisionController;
    private bool enabledShortVision;

    new private void Awake()
    {
        base.Awake();
        enabledShortVision = false;
        shortVisionController = GameObject.Find("ShortVision").GetComponent<ShortVisionController>();
        levelController = GameObject.Find("LevelController").GetComponentInParent<LevelController>();
    }
    new void Start()
    {
        deathItem = "Spell";
        spriteFlipper = false;
        shortVisionController = GameObject.Find("ShortVision").GetComponent<ShortVisionController>();
        base.Start();
    }

    new private void Update()
    {
        if (levelController.IsGameActive)
        {
            base.Update();

            if (isAttacking || justLeftAttacking)
            {
                if (!enabledShortVision)
                {
                    shortVisionController.EnableShortVision(gameObject.GetInstanceID());
                    enabledShortVision = true;
                }
            }
            else if ((Time.time - last_time_LOS) < (blindAttackTiming + 0.1f) && enabledShortVision)
            {
                shortVisionController.DisableShortVision(gameObject.GetInstanceID());
                enabledShortVision = false;
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

            if (enabledShortVision)
            {
                shortVisionController.DisableShortVision(gameObject.GetInstanceID());
                enabledShortVision = false;
            }
        }
        
        base.OnDestroy();
    }
}
