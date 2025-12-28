using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryUI;
    private bool isOpen = false;

    void Start()
    {
        // Start with gameplay mode
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryUI.SetActive(isOpen);

        if (isOpen)
        {
            // Inventory OPEN
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // optional: pause game
        }
        else
        {
            // Inventory CLOSED
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }
}
