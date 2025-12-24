using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Weapons
    private Weapons[] playerWeapons = new Weapons[4];
    private int selectedWeaponIndex = 0;
    private float mouseScroll;
    [SerializeField] private float weapon0Cooldown = 1f, weapon0Damage = 25f;
    [SerializeField] private float weapon1Cooldown = 2.5f, weapon1Damage = 75f;
    [SerializeField] private float weapon2Cooldown = 1.5f, weapon2Damage = 50f;
    [SerializeField] private float weapon3Cooldown = 0.5f, weapon3Damage = 100f;

    // Attack & Cooldown
    private bool canAttack = true;
    private float attackCoolDownTimer = 0f;

    void Start()
    {
        // Populate the weapons array
        playerWeapons[0] = new Weapons(weapon0Cooldown, weapon0Damage);
        playerWeapons[1] = new Weapons(weapon1Cooldown, weapon1Damage);
        playerWeapons[2] = new Weapons(weapon2Cooldown, weapon2Damage);
        playerWeapons[3] = new Weapons(weapon3Cooldown, weapon3Damage);
    }

    void Update()
    {
        // Switching weapons
        mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScroll < 0)
        {
            selectedWeaponIndex--;
        }
        else if (mouseScroll > 0)
        {
            selectedWeaponIndex++;
        }
        selectedWeaponIndex = (selectedWeaponIndex > 3) ? 0 : selectedWeaponIndex;
        selectedWeaponIndex = (selectedWeaponIndex < 0) ? 3 : selectedWeaponIndex;
        selectedWeaponIndex = (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Keypad1)) ? 0 : selectedWeaponIndex;
        selectedWeaponIndex = (Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Keypad2)) ? 1 : selectedWeaponIndex;
        selectedWeaponIndex = (Input.GetKey(KeyCode.Alpha3) || Input.GetKey(KeyCode.Keypad3)) ? 2 : selectedWeaponIndex;
        selectedWeaponIndex = (Input.GetKey(KeyCode.Alpha4) || Input.GetKey(KeyCode.Keypad4)) ? 3 : selectedWeaponIndex;

        // Attack cooldown
        if (!canAttack)
        {
            attackCoolDownTimer += Time.deltaTime;
        }
        if (attackCoolDownTimer >= playerWeapons[selectedWeaponIndex].coolDownTime)
        {
            attackCoolDownTimer = 0f;
            canAttack = true;
        }
        Debug.Log(selectedWeaponIndex);

        // Attack action
        if (canAttack && Input.GetMouseButtonDown(0))
        {
            canAttack = false;
        }
    }
}
