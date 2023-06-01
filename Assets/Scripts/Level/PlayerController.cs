using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private Inventory inventory;
    private Projecter projecter;
    private SpriteRenderer spriteRenderer;
    private HotbarController hotbarController;
    [SerializeField]
    private int inventoySize;
    [SerializeField]
    private Vector2Int playerSafeArea;
    [SerializeField]
    private float horizontalSpeed;
    [SerializeField]
    private float verticalSpeed;
    [SerializeField]
    private float bulletProjectionSpeed;
    private float moveHorizontal;
    private float moveVertical;
    private int lastUsedFacingIndex;
    [SerializeField]
    private Sprite LeftSprite;
    [SerializeField]
    private Sprite RightSprite;
    [SerializeField]
    private Sprite DownSprite;
    [SerializeField]
    private Sprite UpSprite;
    private Dictionary<int, Sprite> playerSpritesDict;
    private LevelController levelController;
    private ItemsManager itemsManager;
    protected SoundManager soundmanager;
    [SerializeField]
    private AudioClip firingSound;
    [SerializeField]
    private AudioClip playerDeathSound;


    private void Awake()
    {
        tag = "Player";
        rigidBody = gameObject.GetComponent<Rigidbody2D>();

        lastUsedFacingIndex = -2;

        playerSpritesDict = new Dictionary<int, Sprite>()
        {
            {-1, LeftSprite},
            {1, RightSprite},
            {2, UpSprite},
            {-2, DownSprite}
        };

        if (playerSafeArea.x == 0) playerSafeArea.x = 12;
        if (playerSafeArea.y == 0) playerSafeArea.y = 20;

        if (horizontalSpeed == 0.0f) horizontalSpeed = 2.5f;
        if (verticalSpeed == 0.0f) verticalSpeed = 2.5f;

        inventory = new Inventory(inventoySize);
        projecter = new Projecter();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        hotbarController = GameObject.Find("Hotbar").GetComponent<HotbarController>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        itemsManager = GameObject.Find("Items").GetComponent<ItemsManager>();
        soundmanager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

    private void Start()
    {
        UpdateSprite(-2);
    }

    // Update is called once per frame
    private void Update()
    {
        if (levelController.IsGameActive)
        {
            moveHorizontal = Input.GetAxisRaw("Horizontal");
            moveVertical = Input.GetAxisRaw("Vertical");
            bool isRunning = false;

            if (moveHorizontal > 0.1f)
            {
                if (lastUsedFacingIndex == 1) isRunning = true;
                lastUsedFacingIndex = 1;
            }
            else if (moveHorizontal < -0.1f)
            {
                if (lastUsedFacingIndex == -1) isRunning = true;
                lastUsedFacingIndex = -1;
            }

            if (moveVertical > 0.1f)
            {
                if (lastUsedFacingIndex == 2) isRunning = true;
                lastUsedFacingIndex = 2;
            }
            else if (moveVertical < -0.1f)
            {
                if (lastUsedFacingIndex == -2) isRunning = true;
                lastUsedFacingIndex = -2;
            }

            UpdateSprite(lastUsedFacingIndex);
        }
    }

    private void FixedUpdate()
    {
        if (levelController.IsGameActive)
        {
            if (moveHorizontal > 0.1f || moveHorizontal < -0.1f)
            {
                rigidBody.AddForce(new Vector2(moveHorizontal * horizontalSpeed, 0f), ForceMode2D.Impulse);
            }

            if (moveVertical > 0.1f || moveVertical < -0.1f)
            {
                rigidBody.AddForce(new Vector2(0f, moveVertical * verticalSpeed), ForceMode2D.Impulse);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Contains("Item") && !collision.name.Contains("Used") && !collision.name.Contains("Dropped") && !collision.CompareTag("ProjectedItem"))
        {
            if (!IsInventoryFull)
            {
                AddToInventory(collision.name);
                Destroy(collision.gameObject);
                itemsManager.RemoveItem(collision.name, collision.transform.position);
            }
        }
        else if (collision.tag.Contains("Monster") || (collision.CompareTag("ProjectedItem") && !collision.name.Contains("Used")))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Contains("Item") && collision.name.Contains("Dropped"))
        {
            collision.name = collision.name.Replace("Dropped", "");
        }
    }

    private void UpdateSprite(int direction)
    {
        spriteRenderer.sprite = playerSpritesDict[direction];
        spriteRenderer.sprite = playerSpritesDict[direction];

        if (direction == 1) spriteRenderer.flipX = true;
        else spriteRenderer.flipX = false;
    }

    private void Project(String projectileName, float projectionSpeed)
    {
        Vector3 direction = Vector3.zero;

        if (FaceDirection == -2)
        {
            direction.x = 0f;
            direction.y = -1f;
        }
        else if (FaceDirection == 2)
        {
            direction.x = 0f;
            direction.y = 1f;
        }
        else if (FaceDirection == -1)
        {
            direction.x = -1f;
            direction.y = 0f;
        }
        else if (FaceDirection == 1)
        {
            direction.x = 1f;
            direction.y = 0f;
        }

        projecter.Project("Used" + projectileName, transform.position, direction, projectionSpeed);
    }

    public void FireGun(int slot)
    {
        String projectileName = "";

        if (hotbarController.SelectedItem == "Gun") projectileName = "Bullet";
        else if (hotbarController.SelectedItem == "LaserGun") projectileName = "LaserBeam";

        Project(projectileName, bulletProjectionSpeed);
        soundmanager.PlaySoundWithoutBlocking(firingSound, 0.3f);
        inventory.DecreaseBulletsCount(slot);
    }

    private void OnDestroy()
    {
        levelController.IsPlayerDead = true;
        soundmanager.PlaySoundWithoutBlocking(playerDeathSound, 0.3f);
    }

    public float HorizontalSpeed
    {
        get { return horizontalSpeed; }
    }

    public float VerticalSpeed
    {
        get { return verticalSpeed; }
    }

    public float BulletProjectionSpeed
    {
        get { return bulletProjectionSpeed; }
    }

    public int FaceDirection
    {
        get { return lastUsedFacingIndex; }
    }

    public bool IsInventoryFull
    {
        get { return inventory.IsFull; }
    }

    public Vector2Int PlayerSafeArea
    {
        get { return playerSafeArea; }
    }

    public bool IsInventorySlotEmpty(int slot)
    {
        return inventory.IsSlotEmpty(slot);
    }

    public void AddToInventory(String itemName)
    {
        if (!IsInventoryFull)
        {
            int slot = inventory.GetEmptySlot();

            if (itemName.Contains("Gun"))
            {
                int bullets = int.Parse(itemName.Split('_')[1]);
                itemName = itemName.Split('_')[0];
                inventory.AddItem(slot, itemName, bullets);
            }
            else inventory.AddItem(slot, itemName);

            hotbarController.AddItem(slot, itemName);
        }
    }

    public void RemoveFromInventory(int slot)
    {
        inventory.RemoveItem(slot);
    }

    public int GetInventoryItemCount(String itemName)
    {
        return inventory.GetItemCount(itemName);
    }

    public int GetBulletsCount(int slot)
    {
        return inventory.GetBulletsCount(slot);
    }

}

public class Inventory
{
    private List<String> itemsList;
    private Dictionary<String, int> itemsCount;
    private Dictionary<int, int> bulletsCount;
    private int maxSize;

    private bool isFull;

    public Inventory(int maxSize)
    {
        isFull = false;
        itemsList = new List<String>() { "empty", "empty", "empty", "empty", "empty" };
        bulletsCount = new Dictionary<int, int>();
        this.maxSize = maxSize;
        itemsCount = new Dictionary<String, int>();
    }
    public bool IsFull
    {
        get { return isFull; }
    }

    public bool IsSlotEmpty(int slot)
    {
        return itemsList[slot - 1] == "empty";
    }

    public void RemoveItem(int slot)
    {
        if (itemsList[slot - 1] == "Gun" || itemsList[slot - 1] == "LaserGun")
        {
            bulletsCount.Remove(slot - 1);
        }

        itemsCount[itemsList[slot - 1]]--;
        itemsList[slot - 1] = "empty";

        if (itemsList.Contains("empty")) isFull = false;
    }

    public void AddItem(int slot, String itemName, int bullets = 0)
    {
        if (itemName == "Gun" || itemName == "LaserGun") bulletsCount.Add(slot - 1, bullets);

        itemsList[slot - 1] = itemName;

        if (itemsCount.ContainsKey(itemName)) itemsCount[itemName]++;
        else itemsCount[itemName] = 1;

        if (!itemsList.Contains("empty")) isFull = true;
    }

    public int GetEmptySlot()
    {
        int slot;
        for (slot = 0; slot < maxSize; slot++)
        {
            if (itemsList[slot] == "empty") break;
        }

        return slot + 1;
    }

    public int GetItemCount(String itemName)
    {
        if (itemsCount.ContainsKey(itemName))
        {
            return itemsCount[itemName];
        }
        else return 0;
    }

    public int GetBulletsCount(int slot)
    {
        return bulletsCount[slot - 1];
    }

    public void DecreaseBulletsCount(int slot)
    {
        bulletsCount[slot - 1]--;
    }
}
