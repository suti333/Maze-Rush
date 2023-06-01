using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    [SerializeField]
    private int maxSlots;
    private List<GameObject> slotObjects;
    private PlayerController player;
    private LevelController levelController;
    private ItemsManager itemsManager;
    private int selectedSlot;
    private float scroll;
    [SerializeField]
    private Sprite squareRingSprite;
    [SerializeField]
    private Sprite highlightedSquareRingSprite;
    [SerializeField]
    private Sprite emptySlotSprite;
    private SoundManager soundManager;
    [SerializeField]
    private AudioClip itemPickSound;

    private void Awake()
    {
        selectedSlot = 3;
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        itemsManager = GameObject.Find("Items").GetComponent<ItemsManager>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        slotObjects = new List<GameObject>();

        foreach (Transform slot in transform)
        {
            slotObjects.Add(slot.gameObject);
        }
    }
    
    private void Update()
    {
        if (levelController.IsGameActive)
        {
            scroll = -Input.GetAxisRaw("Mouse ScrollWheel");

            if (scroll >= 0.1f)
            {
                if (selectedSlot < maxSlots) selectedSlot++; // scroll right
                else selectedSlot -= (maxSlots - 1);
            }
            else if (scroll <= -0.1f)
            {
                if (selectedSlot > 1) selectedSlot--; // scroll left
                else selectedSlot += (maxSlots - 1);
            }

            for (int slot = 1; slot <= maxSlots; slot++)
            {
                GameObject slotObject = slotObjects[slot - 1];

                if (slot == selectedSlot)
                {
                    slotObject.transform.GetChild(1).gameObject.GetComponent<Image>().overrideSprite = highlightedSquareRingSprite;
                    slotObject.transform.GetChild(1).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(82f, 82f);
                    slotObject.GetComponent<Canvas>().sortingOrder = 2;
                }
                else
                {
                    slotObject.transform.GetChild(1).gameObject.GetComponent<Image>().overrideSprite = squareRingSprite;
                    slotObject.transform.GetChild(1).gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(75f, 75f);
                    slotObject.GetComponent<Canvas>().sortingOrder = 1;
                }

            }

            if (Input.GetMouseButtonDown(1)) DropItem();
            if (Input.GetMouseButtonDown(0)) UseItem();
        }
    }

    public void AddItem(int slot, String itemName)
    {
        soundManager.PlaySoundWithoutBlocking(itemPickSound, 0.2f);
        GameObject slotObject = slotObjects[slot-1];
        Image image = slotObject.transform.GetChild(0).gameObject.GetComponent<Image>();
        image.overrideSprite = itemsManager.GetItemSprite(itemName);
        slotObject.tag = itemName + "Item";
    }

    private Vector3 GetNextBlock(String isFacing)
    {
        Vector3 playerPosition = player.transform.position;

        // Pending

        return playerPosition;
    }

    public void UseItem()
    {
        if (!player.IsInventorySlotEmpty(selectedSlot))
        {

            if (SelectedItem == "Gun" || SelectedItem == "LaserGun")
            {
                player.FireGun(selectedSlot);
                if (player.GetBulletsCount(selectedSlot) == 0)
                {
                    GameObject slotObject = slotObjects[selectedSlot - 1];

                    slotObject.transform.GetChild(0).gameObject.GetComponent<Image>().overrideSprite = emptySlotSprite;
                    slotObject.tag = "Untagged";

                    player.RemoveFromInventory(selectedSlot);
                }
            }
            else
            {
                GameObject slotObject = slotObjects[selectedSlot - 1];

                GameObject usedItemObject = itemsManager.SpawnItem("Used" + slotObject.tag.Replace("Item", ""), player.gameObject.transform.position, true);

                if (SelectedItem == "Blade")
                {
                    Rigidbody2D usedItemRB2D = usedItemObject.AddComponent<Rigidbody2D>();
                    usedItemRB2D.constraints = RigidbodyConstraints2D.FreezePosition;
                    usedItemRB2D.angularDrag = 0;
                    usedItemRB2D.angularVelocity = 720f;
                }

                slotObject.transform.GetChild(0).gameObject.GetComponent<Image>().overrideSprite = emptySlotSprite;
                slotObject.tag = "Untagged";

                player.RemoveFromInventory(selectedSlot);
            }
        }
    }

    public void DropItem()
    {
        if (!player.IsInventorySlotEmpty(selectedSlot))
        {
            GameObject slotObject = slotObjects[selectedSlot - 1];

            String itemName = slotObject.tag.Replace("Item", "");

            if (SelectedItem == "Gun" || itemName == "LaserGun")
            {
                itemName = itemName + "_" + player.GetBulletsCount(selectedSlot);
            }

            itemsManager.SpawnItem("Dropped" + itemName, player.gameObject.transform.position);

            slotObject.transform.GetChild(0).gameObject.GetComponent<Image>().overrideSprite = emptySlotSprite;
            slotObject.tag = "Untagged";

            player.RemoveFromInventory(selectedSlot);
        }
    }

    public String SelectedItem
    {
        get { return slotObjects[selectedSlot - 1].tag.Replace("Item", "");  }
    }

    public int SelectedSlot
    {
        get { return selectedSlot; }
    }
}
