using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MonsterFactory : MonoBehaviour
{
    [SerializeField]
    private String monsterName;
    [SerializeField]
    private Sprite monsterSprite;
    [SerializeField]
    private PhysicsMaterial2D monsterMaterial;
    [SerializeField]
    private int existingMonstersLimit;
    [SerializeField]
    private int monstersSpawnedOnDestruction;
    [SerializeField]
    public float strength;
    [SerializeField]
    private BoundsInt factoryOccupiedArea;
    private HashSet<Vector3> factoryOccupiedLocations;
    [SerializeField]
    private Vector2Int monsterSpawnAreaDimensions;
    private BoundsInt monsterSpawnArea;
    private String monsterController;
    private int monsterCount;
    [SerializeField]
    private float monsterHorizontalRunningSpeed;
    [SerializeField]
    private float monsterVerticalRunningSpeed;
    [SerializeField]
    private float randommonsterHorizontalRunningSpeed;
    [SerializeField]
    private float randommonsterVerticalRunningSpeed;
    [SerializeField]
    private float monsterBlindAttackTiming;
    private float mass;
    private float linearDrag;
    private float angularDrag;
    private float gravity;
    private RigidbodyInterpolation2D interpolation;
    private Vector2 boxColliderSize;
    private float boxColliderEdgeRadius;
    private List<Vector3> monsterSpawnLocations;
    private HashSet<Vector3> obstaclesLocations;
    private CoordinateSystem coordinateSystem;
    [SerializeField]
    private int attackingRangeWidth;
    [SerializeField]
    private int attackingRangeHeight;
    private LevelController levelController;
    [SerializeField]
    private AudioClip monsterLiveSound;
    [SerializeField]
    private float monsterLiveSoundVolume;
    [SerializeField]
    private AudioClip monsterDeathSound;
    [SerializeField]
    private float monsterDeathSoundVolume;
    [SerializeField]
    private AudioClip projectileSound;

    // Start is called before the first frame update

    private void Awake()
    {
        //gameObject.GetComponent<MonsterFactory>().enabled = false;
        this.enabled = false;
        coordinateSystem = GameObject.Find("CoordinateSystem").GetComponent<CoordinateSystem>();
        factoryOccupiedLocations = coordinateSystem.GetObstacleFreeLocations(new HashSet<Vector3>(), factoryOccupiedArea);

        GameObject player = GameObject.Find("Player");

        mass = player.GetComponent<Rigidbody2D>().mass;
        linearDrag = player.GetComponent<Rigidbody2D>().drag;
        angularDrag = player.GetComponent<Rigidbody2D>().angularDrag;
        gravity = player.GetComponent<Rigidbody2D>().gravityScale;
        interpolation = player.GetComponent<Rigidbody2D>().interpolation;
        boxColliderSize = player.GetComponent<BoxCollider2D>().size;
        boxColliderEdgeRadius = player.GetComponent<BoxCollider2D>().edgeRadius;

        Rigidbody2D factoryRB2D = gameObject.AddComponent<Rigidbody2D>();
        factoryRB2D.sharedMaterial = monsterMaterial;
        factoryRB2D.mass = mass;
        factoryRB2D.drag = linearDrag;
        factoryRB2D.angularDrag = angularDrag;
        factoryRB2D.gravityScale = gravity;
        factoryRB2D.interpolation = interpolation;
        factoryRB2D.constraints = RigidbodyConstraints2D.FreezeAll;

        gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Player";

        monsterCount = 0;
        monsterController = monsterName + "Controller";

        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
    }

    public void Start()
    {
        obstaclesLocations = coordinateSystem.WallsLocations.Union(coordinateSystem.PlayerSafeLocations).ToHashSet<Vector3>();
        obstaclesLocations = obstaclesLocations.Union(coordinateSystem.FactoryOccupiedLocations).ToHashSet<Vector3>();

        monsterSpawnArea = coordinateSystem.GetPointCentredArea(gameObject.transform.position, monsterSpawnAreaDimensions.x, monsterSpawnAreaDimensions.y);

        if (monsterSpawnArea.size != Vector3Int.zero)
        {
            monsterSpawnLocations = coordinateSystem.GetObstacleFreeLocations(obstaclesLocations, monsterSpawnArea).ToList<Vector3>();
        }
        else monsterSpawnLocations = coordinateSystem.WallFreeLocations.ToList<Vector3>();

        if (randommonsterHorizontalRunningSpeed == 0f)
        {
            randommonsterHorizontalRunningSpeed = 2f;
        }
        if (randommonsterVerticalRunningSpeed == 0f)
        {
            randommonsterVerticalRunningSpeed = 2f;
        }


        if (monsterHorizontalRunningSpeed == 0f)
        {
            monsterHorizontalRunningSpeed = 4.5f;
        }
        if (monsterVerticalRunningSpeed == 0f)
        {
            monsterVerticalRunningSpeed = 4.5f;
        }
    }

    void Update()
    {
        if (levelController.IsGameActive)
        {
            monsterCount = GameObject.Find(monsterName + "Monsters").transform.childCount;

            if (monsterCount < existingMonstersLimit)
            {
                SpawnMonster(GetSpawnLocation());
                monsterCount++;
            }


            if (strength <= 0f)
            {
                for (int count = 0; count < monstersSpawnedOnDestruction; count++) SpawnMonster(gameObject.transform.position);
                Destroy(gameObject);
            }
        }
    }

    private Vector3 GetSpawnLocation()
    {

        int i = UnityEngine.Random.Range(0, monsterSpawnLocations.Count);
        Debug.Log("Spawn Location : " + monsterSpawnLocations[i]);
        return monsterSpawnLocations[i];
    }

    private void SpawnMonster(Vector3 location)
    {
        GameObject newMonster = new GameObject(monsterName);
        newMonster.transform.position = location;
        newMonster.tag = monsterName + "Monster";
        newMonster.transform.localScale = Vector3.one;
        newMonster.transform.parent = GameObject.Find(newMonster.tag + "s").transform;

        newMonster.layer = 2;

        Rigidbody2D newMonsterRB2D = newMonster.AddComponent<Rigidbody2D>();
        newMonsterRB2D.sharedMaterial = monsterMaterial;
        newMonsterRB2D.mass = mass;
        newMonsterRB2D.drag = linearDrag;
        newMonsterRB2D.angularDrag = angularDrag;
        newMonsterRB2D.gravityScale = gravity;
        newMonsterRB2D.interpolation = interpolation;
        newMonsterRB2D.freezeRotation = true;

        BoxCollider2D newMonsterBC2D = newMonster.AddComponent<BoxCollider2D>();
        newMonsterBC2D.size = boxColliderSize;
        newMonsterBC2D.edgeRadius = boxColliderEdgeRadius;
        newMonsterBC2D.isTrigger = true;

        CircleCollider2D newMonsterCC2D = newMonster.AddComponent<CircleCollider2D>();
        newMonsterCC2D.radius = 0.49f;

        MonsterController newMonsterMC = newMonster.AddComponent(Type.GetType(monsterController)) as MonsterController;
        newMonsterMC.RunningHorizontalSpeed = monsterHorizontalRunningSpeed;
        newMonsterMC.RunningVerticalSpeed = monsterVerticalRunningSpeed;
        newMonsterMC.RandomMotionRunningHorizontalSpeed = randommonsterHorizontalRunningSpeed;
        newMonsterMC.RandomMotionRunningVerticalSpeed = randommonsterVerticalRunningSpeed;
        newMonsterMC.BlindAttackTiming = monsterBlindAttackTiming;
        newMonsterMC.MonsterLiveSound = monsterLiveSound;
        newMonsterMC.MonsterLiveSoundVolume = monsterLiveSoundVolume;
        newMonsterMC.MonsterDeathSound = monsterDeathSound;
        newMonsterMC.MonsterDeathSoundVolume = monsterDeathSoundVolume;
        if (projectileSound != null)
            newMonsterMC.ProjectileSound = projectileSound;

        GameObject MainCamera = GameObject.Find("Main Camera");
        if (attackingRangeHeight == 0)
        {

            attackingRangeHeight = (int)(MainCamera.GetComponent<Camera>().orthographicSize * 2);

        }
        if (attackingRangeWidth == 0)
        {
            attackingRangeWidth = (attackingRangeHeight * 16) / 9;
        }
        newMonsterMC.AttackingRangeHeight = attackingRangeHeight;
        newMonsterMC.AttackingRangeWidth = attackingRangeWidth;

        SpriteRenderer newMonsterSR = newMonster.AddComponent<SpriteRenderer>();
        newMonsterSR.sprite = monsterSprite;
        newMonsterSR.sortingLayerName = "Player";
        newMonsterSR.sortingOrder = 2;
    }

    public HashSet<Vector3> FactoryOccupiedLocations
    {
        get { return factoryOccupiedLocations; }
    }
}
