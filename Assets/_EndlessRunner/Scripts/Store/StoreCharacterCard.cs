using UnityEngine;
using UnityEngine.UI;

public class StoreCharacterCard : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private string characterId;

    [Header("Price")]
    [SerializeField] private int price;

    [Header("Default Ownership")]
    [SerializeField] private bool ownedByDefault;

    [Header("Buttons")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button equipButton;

    [Header("State Objects")]
    [SerializeField] private GameObject priceTag;
    [SerializeField] private GameObject buyButtonObject;
    [SerializeField] private GameObject equipButtonObject;

    [Header("Lock")]
    [SerializeField] private GameObject lockIcon;

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyClicked);
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        if (equipButton != null)
        {
            equipButton.onClick.RemoveListener(OnEquipClicked);
            equipButton.onClick.AddListener(OnEquipClicked);
        }
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyClicked);
        }

        if (equipButton != null)
        {
            equipButton.onClick.RemoveListener(OnEquipClicked);
        }
    }

    private void Start()
    {
        RefreshCard();
    }

    public void RefreshCard()
    {
        if (StoreManager.Instance == null)
        {
            return;
        }

        bool owned = StoreManager.Instance.IsOwned(characterId);
        bool equipped = StoreManager.Instance.IsEquipped(characterId);

        if (!owned)
        {
            ShowLockedState();
            return;
        }

        ShowOwnedState(equipped);
    }

    private void ShowLockedState()
    {
        // Price visible
        if (priceTag != null)
        {
            priceTag.SetActive(true);
        }

        // Buy visible
        if (buyButtonObject != null)
        {
            buyButtonObject.SetActive(true);
        }

        // Equip hidden
        if (equipButtonObject != null)
        {
            equipButtonObject.SetActive(false);
        }

        // Lock visible
        if (lockIcon != null)
        {
            lockIcon.SetActive(true);
        }

        if (buyButton != null)
        {
            buyButton.interactable = true;
        }

        if (equipButton != null)
        {
            equipButton.interactable = false;
        }
    }

    private void ShowOwnedState(bool equipped)
    {
        // Price hidden
        if (priceTag != null)
        {
            priceTag.SetActive(false);
        }

        // Buy hidden
        if (buyButtonObject != null)
        {
            buyButtonObject.SetActive(false);
        }

        // Equip visible
        if (equipButtonObject != null)
        {
            equipButtonObject.SetActive(true);
        }

        // Lock hidden
        if (lockIcon != null)
        {
            lockIcon.SetActive(false);
        }

        if (equipButton != null)
        {
            // Equipped character becomes disabled.
            // Previously equipped character becomes active again.
            equipButton.interactable = !equipped;
        }

        if (buyButton != null)
        {
            buyButton.interactable = false;
        }
    }

    private void OnBuyClicked()
    {
        if (StoreManager.Instance == null)
        {
            Debug.LogWarning(
                "StoreCharacterCard: StoreManager.Instance is missing."
            );

            return;
        }

        bool purchaseSuccessful =
            StoreManager.Instance.TryPurchase(characterId);

        if (purchaseSuccessful)
        {
            return;
        }

        // Purchase failed.
        // Since the Buy button is only visible for locked characters,
        // a failed purchase here means the player cannot currently buy it.
        if (StoreNotification.Instance != null)
        {
            StoreNotification.Instance.ShowNotification();
        }
        else
        {
            Debug.LogWarning(
                "StoreCharacterCard: StoreNotification.Instance is missing."
            );
        }
    }

    private void OnEquipClicked()
    {
        if (StoreManager.Instance == null)
        {
            Debug.LogWarning(
                "StoreCharacterCard: StoreManager.Instance is missing."
            );

            return;
        }

        StoreManager.Instance.TryEquip(characterId);
    }

    public string GetCharacterId()
    {
        return characterId;
    }

    public int GetPrice()
    {
        return price;
    }

    public bool IsDefaultOwned()
    {
        return ownedByDefault;
    }
}