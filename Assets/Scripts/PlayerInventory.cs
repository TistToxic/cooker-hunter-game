using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public List<itemType> inventoryList;
    public int playerReach;
    [SerializeField] Camera cam;
    [SerializeField] GameObject pressToPickup_gameobject;
    [SerializeField] Image[] inventorySlotImage = new Image[9];
    [SerializeField] Image[] inventoryBackgroundImage = new Image[9];
    [SerializeField] Sprite prazdnySlotImage;
    [SerializeField] GameObject throwObject_gameobject;
    [SerializeField] KeyCode throwItemKey;
    [SerializeField] KeyCode pickUpItemKey;

    public int selectedItem = 0;
    public bool animationIsPlaying = false;

    [Space(10)]

    [Header("Zbrane gameobjects")]
    [SerializeField] GameObject hammer_item;
    [SerializeField] GameObject machete_item;
    [SerializeField] GameObject club_item;
    [SerializeField] GameObject mace_item;

    [Header("weapon prefabs")]
    [SerializeField] GameObject hammer_prefab;
    [SerializeField] GameObject machete_prefab;
    [SerializeField] GameObject club_prefab;
    [SerializeField] GameObject mace_prefab;

    private Dictionary<itemType, GameObject> itemSetActive = new Dictionary<itemType, GameObject>()
    {
    };
    private Dictionary<itemType, GameObject> itemInstantiate = new Dictionary<itemType, GameObject>()
    {
    };
    void Start()
    {
        itemSetActive.Add(itemType.Hammer, hammer_item);
        itemSetActive.Add(itemType.Machete, machete_item);
        itemSetActive.Add(itemType.Mace, mace_item);
        itemSetActive.Add(itemType.Club, club_item);

        itemInstantiate.Add(itemType.Hammer, hammer_prefab);
        itemInstantiate.Add(itemType.Machete, machete_prefab);
        itemInstantiate.Add(itemType.Mace, mace_prefab);
        itemInstantiate.Add(itemType.Club, club_prefab);

        NewItemSelected();
    }

    void Update()
    {
        Animator animator = itemSetActive[inventoryList[selectedItem]].GetComponent<Animator>();
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        string stateName = stateInfo.shortNameHash.ToString();
        string idleStateHash = Animator.StringToHash("Idle").ToString();

        if (stateName == idleStateHash)
        {
            animationIsPlaying = false;
        }
        else
        {
            animationIsPlaying = true;
        }

        if (Input.GetKeyDown(throwItemKey) && inventoryList.Count > 1 && !animationIsPlaying)
        {
            Instantiate(itemInstantiate[inventoryList[selectedItem]], position: throwObject_gameobject.transform.position, new Quaternion());
            inventoryList.RemoveAt(selectedItem);

            if (selectedItem != 0)
            {
                selectedItem -= 1;
            }
            NewItemSelected();
        }

        if (Input.GetButton("Fire1") && !animationIsPlaying)
        {
            itemSetActive[inventoryList[selectedItem]].GetComponent<Animator>().Play("Attack");
        }

        for (int i = 0; i < 8; i++)
        {
            if (i < inventoryList.Count)
            {
                inventorySlotImage[i].sprite = itemSetActive[inventoryList[i]].GetComponent<Item>().itemScriptableObject.item_sprite;
            }
            else
            {
                inventorySlotImage[i].sprite = prazdnySlotImage;
            }
        }

        int a = 0;
        foreach(Image image in inventoryBackgroundImage)
        {
            if (a == selectedItem)
            {
                image.color = new Color32(145, 255, 126, 255);
            }
            else
            {
                image.color = new Color32(219, 219, 219, 255);
            }
            a++;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, playerReach) && Input.GetKey(pickUpItemKey))
        {
            IPickable item = hitInfo.collider.GetComponent<IPickable>();
            if (item != null)
            {
                pressToPickup_gameobject.SetActive(true);
                inventoryList.Add(hitInfo.collider.GetComponent<WeaponPickable>().weaponScriprableObject.item_type);
                item.PickItem();
            }
            else
            {
                pressToPickup_gameobject.SetActive(false);
            }
        }
        else
        {
            pressToPickup_gameobject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryList.Count > 0 && !animationIsPlaying)
        {
            selectedItem = 0;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryList.Count > 1 && !animationIsPlaying)
        {
            selectedItem = 1;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryList.Count > 2 && !animationIsPlaying)
        {
            selectedItem = 2;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && inventoryList.Count > 3 && !animationIsPlaying)
        {
            selectedItem = 3;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && inventoryList.Count > 4 && !animationIsPlaying)
        {
            selectedItem = 4;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6) && inventoryList.Count > 5 && !animationIsPlaying)
        {
            selectedItem = 5;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7) && inventoryList.Count > 6 && !animationIsPlaying)
        {
            selectedItem = 6;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8) && inventoryList.Count > 7 && !animationIsPlaying)
        {
            selectedItem = 7;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9) && inventoryList.Count > 8 && !animationIsPlaying)
        {
            selectedItem = 8;
            NewItemSelected();
        }
    }


    private void NewItemSelected()
    {
        hammer_item.SetActive(false);
        machete_item.SetActive(false);
        mace_item.SetActive(false);
        club_item.SetActive(false);

        animationIsPlaying = false;
        GameObject selectedItemGameobject = itemSetActive[inventoryList[selectedItem]];
        selectedItemGameobject.SetActive(true);
    }
}

public interface IPickable
{
    void PickItem();
}
