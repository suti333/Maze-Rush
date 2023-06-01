using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RhinoController : MonsterController
{
    private bool isSlipping;

    new void Start()
    {
        deathItem = "BananaSkin";
        spriteFlipper = true;
        base.Start();
        isSlipping = false;
    }

    new void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Used" + deathItem)
        {
            Destroy(collision.gameObject);
            deathItemLocation = collision.transform.position;
            removeDeathItem = true;
            isSlipping = true;
        }

        if (isSlipping)
        {
            if (collision.tag == "Walls")
            {
                Destroy(gameObject);
            }
        }
        else  base.OnTriggerEnter2D(collision);
    }

    new void FixedUpdate()
    {
        if (levelController.IsGameActive)
        {
            if (isSlipping)
            {
                if (!isPlayingDeathSound.Value)
                {
                    soundManager.PlaySoundWithoutBlocking(monsterDeathSound, isPlayingDeathSound);
                }
                isAttacking = false;
                rigidBody.AddForce(new Vector2(moveHorizontal * runningHorizontalSpeed, 0f), ForceMode2D.Impulse);
                rigidBody.AddForce(new Vector2(0f, moveVertical * runningVerticalSpeed), ForceMode2D.Impulse);
            }

            if (isInRange())
            {
                if (this.isInLOS())
                {
                    isAttacking = true;
                    last_time_LOS = Time.time;
                }
                else isAttacking = false;
            }
            else
            {
                isAttacking = false;
                justLostSight  = false;
            }

            if (isAttacking)
            {
                if (!isPlayingLiveSound.Value)
                {
                    soundManager.PlaySoundWithoutBlocking(monsterLiveSound, isPlayingLiveSound);
                }
            }

            if (!isSlipping && (isAttacking || ((Time.time - last_time_LOS) < blindAttackTiming && justLostSight)))
            {
                collidedWithMonster = false;
                MobAi();
                rigidBody.AddForce(new Vector2(moveHorizontal * runningHorizontalSpeed, 0f), ForceMode2D.Impulse);
                rigidBody.AddForce(new Vector2(0f, moveVertical * runningVerticalSpeed), ForceMode2D.Impulse);
                justLeftAttacking = true;
                justLostSight = true;
            }
            else
            {
                if (!isSlipping)
                {
                    if (justLeftAttacking)
                    {
                        justLeftAttacking = false;
                        anchor = gameObject.transform.position;
                        randomMotionArea = coordinateSystem.GetPointCentredArea(anchor, randomMotionWidth, randomMotionHeight);
                        randomMotionLocations = coordinateSystem.GetObstacleFreeLocations(coordinateSystem.WallsLocations, randomMotionArea).ToList<Vector3>();
                        randomMotionPathCounter = randomMotionPath.Count;
                    }
                    else if (collidedWithMonster)
                    {
                        randomMotionPathCounter = randomMotionPath.Count;
                    }

                    if (randomMotionPathCounter == randomMotionPath.Count)
                    {

                        randomMotionAI();
                    }

                    randomMotionMoveHorizontal = (randomMotionPath[randomMotionPathCounter].x - transform.position.x);
                    randomMotionMoveVertical = (randomMotionPath[randomMotionPathCounter].y - transform.position.y);
                    rigidBody.AddForce(new Vector2(randomMotionMoveHorizontal * randomMotionRunningHorizontalSpeed, 0f), ForceMode2D.Impulse);
                    rigidBody.AddForce(new Vector2(0f, randomMotionMoveVertical * randomMotionRunningVerticalSpeed), ForceMode2D.Impulse);

                    if (Mathf.Abs(transform.position.x - randomMotionPath[randomMotionPathCounter].x) < 0.1f && Mathf.Abs(transform.position.y - randomMotionPath[randomMotionPathCounter].y) < 0.1f)
                    {
                        randomMotionPathCounter += 1;
                    }

                }
            }
        }
    }
}
