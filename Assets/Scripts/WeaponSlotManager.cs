using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotManager : MonoBehaviour
{
    public WeaponData[] weaponSlots = new WeaponData[4];
    public int currentSlot = 0;

    public Transform weaponHoldPoint; // hand position
    private GameObject currentWeapon;

    public Image[] slotUIImages; // 4 UI icons

    void Start()
    {
        EquipWeapon(0);
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            PreviousSlot();

        if (Input.GetKeyDown(KeyCode.E))
            NextSlot();
    }

    void NextSlot()
    {
        currentSlot = (currentSlot + 1) % weaponSlots.Length;
        EquipWeapon(currentSlot);
        UpdateUI();
    }

    void PreviousSlot()
    {
        currentSlot--;
        if (currentSlot < 0) currentSlot = weaponSlots.Length - 1;
        EquipWeapon(currentSlot);
        UpdateUI();
    }

    void EquipWeapon(int slot)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon);

        if (weaponSlots[slot] == null) return;

        currentWeapon = Instantiate(
            weaponSlots[slot].weaponPrefab,
            weaponHoldPoint
        );

        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
    }

    void UpdateUI()
    {
        for (int i = 0; i < slotUIImages.Length; i++)
        {
            if (weaponSlots[i] != null)
                slotUIImages[i].sprite = weaponSlots[i].icon;

            slotUIImages[i].color = (i == currentSlot)
                ? Color.yellow
                : Color.white;
        }
    }
}
