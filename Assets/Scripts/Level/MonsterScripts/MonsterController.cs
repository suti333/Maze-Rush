using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MonsterController : MonoBehaviour
{
    protected int[] dx = { 1, 0, -1, 0 };
    protected int[] dy = { 0, 1, 0, -1 };
    protected GameObject Player;
    private SpriteRenderer spriteRenderer;
    protected bool spriteFlipper;
    private String savedTag;
    protected String deathItem;
    protected Rigidbody2D rigidBody;
    protected bool isAttacking;
    protected bool collidedWithMonster;
    protected CoordinateSystem coordinateSystem;
    protected HashSet<Vector3> obstaclesForRandomMotion;
    protected HashSet<Vector3> obstaclesForAttacking;
    protected float randomMotionMoveHorizontal;
    protected float randomMotionMoveVertical;
    protected float moveHorizontal;
    protected float moveVertical;
    protected float randomMotionRunningHorizontalSpeed;
    protected float randomMotionRunningVerticalSpeed;
    protected float runningHorizontalSpeed;
    protected float runningVerticalSpeed;
    protected List<Vector2> randomMotionPath;
    protected int randomMotionPathCounter;
    protected List<Vector3> randomMotionLocations;
    protected BoundsInt randomMotionArea;
    protected int randomMotionHeight;
    protected int randomMotionWidth;
    protected Vector3 anchor;
    protected float last_time_LOS = 0f;
    protected bool justLostSight;
    protected bool justLeftAttacking;
    protected float blindAttackTiming;
    protected int attackingRangeWidth;
    protected int attackingRangeHeight;
    protected BoundsInt attackingRange;
    protected Dictionary<Vector2, bool> Explored = new Dictionary<Vector2, bool>();
    protected Vector2Int mob, target;
    protected List<Vector2> path = new List<Vector2>();
    protected IDictionary<Vector2, Vector2> nodeParents = new Dictionary<Vector2, Vector2>();
    protected LevelController levelController;
    protected ItemsManager itemsManager;
    protected bool removeDeathItem;
    protected Vector3 deathItemLocation;
    protected AudioClip monsterLiveSound;
    protected AudioClip monsterDeathSound;
    protected AudioClip projectileSound;
    protected SoundManager soundManager;
    protected SoundManager.Bool isPlayingLiveSound;
    protected float monsterLiveSoundVolume;
    protected SoundManager.Bool isPlayingDeathSound;
    protected float monsterDeathSoundVolume;

    protected virtual bool isInLOS()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Player.transform.position - transform.position);
        
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Player")
            {

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    protected bool isInRange()
    {
        if ((Player.transform.position.x<(attackingRange.position.x+attackingRange.size.x) && Player.transform.position.x>attackingRange.position.x) && (Player.transform.position.y < (attackingRange.position.y + attackingRange.size.y) && Player.transform.position.y > attackingRange.position.y))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected void randomMotionAI()
    {
        Vector3 source = new Vector3(transform.position.x, transform.position.y, 0f);
        Vector3 destination;
        int enterTime1 = DateTime.UtcNow.Millisecond;
        while (randomMotionPath.Count == randomMotionPathCounter)
        {
            if (DateTime.UtcNow.Millisecond - enterTime1 > 1000) Debug.Log("Line : 106");
            int enterTime = DateTime.UtcNow.Millisecond;
            destination = randomMotionLocations[UnityEngine.Random.Range(0, randomMotionLocations.Count)];
            while (source == destination)
            {
                if (DateTime.UtcNow.Millisecond - enterTime > 1000) Debug.Log("Line : 111");
                destination = randomMotionLocations[UnityEngine.Random.Range(0, randomMotionLocations.Count)];
            }
            source.x = Mathf.Floor(source.x) + 0.5f;
            source.y = Mathf.Floor(source.y) + 0.5f;
            destination.x = Mathf.Floor(destination.x) + 0.5f;
            destination.y = Mathf.Floor(destination.y) + 0.5f;
            randomMotionPath = coordinateSystem.GetPathTo(new Vector2(source.x, source.y), new Vector2(destination.x, destination.y), obstaclesForRandomMotion, randomMotionArea);
            randomMotionPathCounter = 0;
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {   
        if (collision.tag == "Walls" && name != "Spook")
        {
            Vector3 node = transform.position;
            node.x = Mathf.Floor(node.x);
            node.x += 0.5f;
            node.y = Mathf.Floor(node.y);
            node.y += 0.5f;
            transform.position = node;
        }

        if (collision.tag.Contains("Monster") && !isAttacking && !justLeftAttacking)
        {
            collidedWithMonster = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Contains("Monster")) collidedWithMonster = false;
    }

    protected void MobAi()
    {
        moveHorizontal = moveVertical = 0f;
        path.Clear();
        nodeParents.Clear();
        Explored.Clear();

        if (GameObject.Find("Player") != null)
        {
            Vector2 source, destination;
            source = new Vector2();
            destination = new Vector2();

            source.x = Mathf.Floor(transform.position.x) + 0.5f;
            source.y = Mathf.Floor(transform.position.y) + 0.5f;

            destination.x = Mathf.Floor(Player.transform.position.x) + 0.5f;
            destination.y = Mathf.Floor(Player.transform.position.y) + 0.5f;

            path = coordinateSystem.GetPathTo(source, destination, obstaclesForAttacking, attackingRange);

            if (path.Count > 0)
            {
                moveHorizontal = (path[0].x - source.x);
                moveVertical = (path[0].y - source.y);
            }
        }
    }

    protected void Awake()
    {
        Player = GameObject.Find("Player");
        coordinateSystem = GameObject.Find("CoordinateSystem").GetComponent<CoordinateSystem>();
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        itemsManager = GameObject.Find("Items").GetComponent<ItemsManager>();

        if (randomMotionHeight == 0) randomMotionHeight = 12;
        if (randomMotionWidth == 0) randomMotionWidth = 10;
        
        anchor = transform.position;
        
        randomMotionArea = coordinateSystem.GetPointCentredArea(anchor, randomMotionWidth, randomMotionHeight);
        
        randomMotionPath = new List<Vector2>();
        randomMotionPathCounter = 0;
        
        isAttacking = false;
        justLeftAttacking = false;
        justLostSight = false;

        collidedWithMonster = false;

        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        isPlayingLiveSound = new SoundManager.Bool(false);
        isPlayingDeathSound = new SoundManager.Bool(false);
    }

    protected void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        if (name != "Spook")
        {
            obstaclesForAttacking = coordinateSystem.WallsLocations.Union(coordinateSystem.FactoryOccupiedLocations).ToHashSet<Vector3>();
            obstaclesForRandomMotion = obstaclesForAttacking;
            randomMotionLocations = coordinateSystem.GetObstacleFreeLocations(obstaclesForRandomMotion, randomMotionArea).ToList<Vector3>();
        }

        savedTag = tag;
        gameObject.transform.localScale = new Vector3(0.9f, 0.9f, 1);
    }

    protected void Update()
    {
        if (levelController.IsGameActive)
        {
            attackingRange = coordinateSystem.GetPointCentredArea(transform.position, attackingRangeWidth, attackingRangeHeight);

            if (!isAttacking || !justLeftAttacking)
            {
                if (randomMotionMoveHorizontal > 0f)
                {
                    spriteRenderer.flipX = spriteFlipper;
                }
                if (randomMotionMoveHorizontal < 0f)
                {
                    spriteRenderer.flipX = !spriteFlipper;
                }
            }
            else
            {
                if (moveHorizontal > 0f)
                {
                    spriteRenderer.flipX = spriteFlipper;
                }
                if (moveHorizontal < 0f)
                {
                    spriteRenderer.flipX = !spriteFlipper;
                }
            }

            if (coordinateSystem.FactoryDestroyed && name != "Spook")
            {
                obstaclesForRandomMotion = coordinateSystem.WallsLocations.Union(coordinateSystem.FactoryOccupiedLocations).ToHashSet();
                randomMotionLocations = coordinateSystem.GetObstacleFreeLocations(obstaclesForRandomMotion, randomMotionArea).ToList<Vector3>();

                obstaclesForAttacking = obstaclesForRandomMotion;
            }
        }
    }

    protected void FixedUpdate()
    {
        if (levelController.IsGameActive)
        {
            if (isInRange())
            {
                if (this.isInLOS())
                {
                    isAttacking = true;
                    if (name == "Raptorbot") Debug.Log("Attacking started (MonsterController)");
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
                    if (monsterLiveSoundVolume != 0f) soundManager.PlaySoundWithoutBlocking(monsterLiveSound, isPlayingLiveSound, monsterLiveSoundVolume);
                    else soundManager.PlaySoundWithoutBlocking(monsterLiveSound, isPlayingLiveSound);
                }
            }

            if (isAttacking || ((Time.time - last_time_LOS) < blindAttackTiming && justLostSight))
            {
                collidedWithMonster = false;
                MobAi();
                rigidBody.AddForce(new Vector2(moveHorizontal * runningHorizontalSpeed, 0f), ForceMode2D.Impulse);
                rigidBody.AddForce(new Vector2(0f, moveVertical * runningVerticalSpeed), ForceMode2D.Impulse);
                justLostSight = true;
                justLeftAttacking = true;
            }
            else
            {
                if (justLeftAttacking)
                {
                    justLeftAttacking = false;
                    anchor = gameObject.transform.position;
                    randomMotionArea = coordinateSystem.GetPointCentredArea(anchor, randomMotionWidth, randomMotionHeight);
                    randomMotionLocations = coordinateSystem.GetObstacleFreeLocations(obstaclesForRandomMotion, randomMotionArea).ToList<Vector3>();
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

    protected void OnDestroy()
    {
        if (removeDeathItem && !levelController.IsPlayerDead) itemsManager.RemoveItem(deathItem, deathItemLocation);
    }

    public float RunningHorizontalSpeed
    {
        set { runningHorizontalSpeed = value; }
    }
    
    public float RunningVerticalSpeed
    {
        set { runningVerticalSpeed = value; }
    }

    public float RandomMotionRunningHorizontalSpeed
    {
        set { randomMotionRunningHorizontalSpeed = value; }
    }

    public float RandomMotionRunningVerticalSpeed
    {
        set {randomMotionRunningVerticalSpeed = value; }
    }

    public String DeathItem
    {
        get { return deathItem; }
    }

    public float BlindAttackTiming
    {
        set { blindAttackTiming = value; }
    }
    public int AttackingRangeWidth
    {
        set { attackingRangeWidth = value; }
    }
    public int AttackingRangeHeight
    {
        set { attackingRangeHeight = value; }
    }

    public AudioClip MonsterLiveSound
    {
        set { monsterLiveSound = value; }
    }

    public AudioClip MonsterDeathSound
    {
        set { monsterDeathSound = value; }
    }

    public float MonsterLiveSoundVolume
    {
        set { monsterLiveSoundVolume = value; }
    }

    public float MonsterDeathSoundVolume
    {
        set { monsterDeathSoundVolume = value; }
    }

    public AudioClip ProjectileSound
    {
        set { projectileSound = value; }
    }
}
