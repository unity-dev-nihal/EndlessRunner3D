using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance { get; private set; }

    [Serializable]
    public class CharacterData
    {
        [Header("Character")]
        public string characterId;
        public string displayName;

        [Header("Price")]
        [Min(0)]
        public int price;

        [Header("Default State")]
        public bool ownedByDefault;
    }

    [Header("Characters")]
    [SerializeField] private List<CharacterData> characters =
        new List<CharacterData>();

    [Header("Character Cards")]
    [SerializeField] private StoreCharacterCard[] characterCards;

    [Header("Store UI")]
    [SerializeField] private TMP_Text coinText;

    [Header("Character Scroll")]
    [SerializeField] private ScrollRect characterScrollRect;

    [SerializeField] private GameObject storePanel;

    [Header("Formatting")]
    [SerializeField] private bool useCompactCoinFormat = true;

    [Header("PlayerPrefs")]
    [SerializeField] private string totalCoinsKey =
        "GottaGo_TotalCoins";

    [SerializeField] private string equippedCharacterKey =
        "GottaGo_EquippedCharacter";

    [SerializeField] private string ownershipKeyPrefix =
        "GottaGo_CharacterOwned_";

    private const string DefaultCharacterId = "Rogue";

    public event Action OnStoreUpdated;

    private bool wasStorePanelActive;

    public int TotalCoins
    {
        get
        {
            return PlayerPrefs.GetInt(
                totalCoinsKey,
                0
            );
        }
    }

    public string EquippedCharacterId
    {
        get
        {
            string equippedId = PlayerPrefs.GetString(
                equippedCharacterKey,
                DefaultCharacterId
            );

            if (string.IsNullOrEmpty(equippedId))
            {
                equippedId = DefaultCharacterId;
            }

            return equippedId;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializeCharacters();
        RefreshStore();

        if (storePanel != null)
        {
            wasStorePanelActive =
                storePanel.activeInHierarchy;

            if (wasStorePanelActive)
            {
                ResetCharacterScrollPosition();
            }
        }
    }

    private void Update()
    {
        if (storePanel == null)
        {
            return;
        }

        bool isStorePanelActive =
            storePanel.activeInHierarchy;

        if (isStorePanelActive &&
            !wasStorePanelActive)
        {
            ResetCharacterScrollPosition();
        }

        wasStorePanelActive =
            isStorePanelActive;
    }

    private void InitializeCharacters()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];

            if (character == null ||
                string.IsNullOrEmpty(character.characterId))
            {
                continue;
            }

            string ownershipKey =
                GetOwnershipKey(character.characterId);

            if (!PlayerPrefs.HasKey(ownershipKey))
            {
                PlayerPrefs.SetInt(
                    ownershipKey,
                    character.ownedByDefault ? 1 : 0
                );
            }
        }

        if (!PlayerPrefs.HasKey(equippedCharacterKey))
        {
            PlayerPrefs.SetString(
                equippedCharacterKey,
                DefaultCharacterId
            );
        }

        PlayerPrefs.Save();
    }

    public void RefreshStore()
    {
        RefreshCoinText();

        RefreshAllCharacterCards();

        OnStoreUpdated?.Invoke();
    }

    private void RefreshAllCharacterCards()
    {
        if (characterCards == null)
        {
            return;
        }

        for (int i = 0; i < characterCards.Length; i++)
        {
            StoreCharacterCard card = characterCards[i];

            if (card == null)
            {
                continue;
            }

            card.RefreshCard();
        }
    }

    // =========================================================
    // STORE OPEN
    // =========================================================

    public void OpenStore()
    {
        if (storePanel == null)
        {
            Debug.LogWarning(
                "StoreManager: Store Panel is not assigned."
            );

            return;
        }

        storePanel.SetActive(true);

        RefreshStore();

        StartCoroutine(
            ResetScrollAfterStoreOpens()
        );

        wasStorePanelActive = true;
    }

    private IEnumerator ResetScrollAfterStoreOpens()
    {
        yield return null;

        ResetCharacterScrollPosition();

        yield return null;

        ResetCharacterScrollPosition();
    }

    public void ResetCharacterScrollPosition()
    {
        if (characterScrollRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        characterScrollRect.StopMovement();

        characterScrollRect.horizontalNormalizedPosition = 0f;

        Canvas.ForceUpdateCanvases();

        characterScrollRect.horizontalNormalizedPosition = 0f;
    }

    // =========================================================
    // COINS
    // =========================================================

    public void RefreshCoinText()
    {
        if (coinText == null)
        {
            return;
        }

        coinText.text = FormatCoins(TotalCoins);
    }

    public bool IsOwned(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return false;
        }

        string ownershipKey =
            GetOwnershipKey(characterId);

        return PlayerPrefs.GetInt(
            ownershipKey,
            characterId == DefaultCharacterId ? 1 : 0
        ) == 1;
    }

    public bool IsEquipped(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return false;
        }

        return string.Equals(
            EquippedCharacterId,
            characterId,
            StringComparison.OrdinalIgnoreCase
        );
    }

    public bool CanAfford(int price)
    {
        return TotalCoins >= price;
    }

    // =========================================================
    // PURCHASE
    // =========================================================

    public bool TryPurchase(string characterId)
    {
        CharacterData character =
            GetCharacter(characterId);

        if (character == null)
        {
            Debug.LogWarning(
                $"StoreManager: Character '{characterId}' was not found."
            );

            return false;
        }

        if (IsOwned(characterId))
        {
            return false;
        }

        if (!CanAfford(character.price))
        {
            Debug.Log(
                $"StoreManager: Not enough coins to purchase {character.displayName}."
            );

            return false;
        }

        int remainingCoins =
            TotalCoins - character.price;

        PlayerPrefs.SetInt(
            totalCoinsKey,
            remainingCoins
        );

        PlayerPrefs.SetInt(
            GetOwnershipKey(characterId),
            1
        );

        PlayerPrefs.Save();

        RefreshStore();

        return true;
    }

    // =========================================================
    // EQUIP
    // =========================================================

    public bool TryEquip(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return false;
        }

        if (!IsOwned(characterId))
        {
            Debug.LogWarning(
                $"StoreManager: Cannot equip '{characterId}' because it is not owned."
            );

            return false;
        }

        PlayerPrefs.SetString(
            equippedCharacterKey,
            characterId
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"StoreManager: Equipped character changed to '{characterId}'."
        );

        RefreshAllCharacterCards();

        RefreshCoinText();

        OnStoreUpdated?.Invoke();

        return true;
    }

    // =========================================================
    // CHARACTER DATA
    // =========================================================

    public CharacterData GetCharacter(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return null;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];

            if (character == null)
            {
                continue;
            }

            if (string.Equals(
                character.characterId,
                characterId,
                StringComparison.OrdinalIgnoreCase))
            {
                return character;
            }
        }

        return null;
    }

    // =========================================================
    // COIN FORMATTING
    // =========================================================

    public string FormatCoins(int amount)
    {
        if (!useCompactCoinFormat)
        {
            return amount.ToString("N0");
        }

        if (amount < 10000)
        {
            return amount.ToString("N0");
        }

        if (amount < 1000000)
        {
            float value = amount / 1000f;

            if (amount % 1000 == 0)
            {
                return $"{value:0}K";
            }

            return $"{value:0.#}K";
        }

        if (amount < 1000000000)
        {
            float value = amount / 1000000f;

            if (amount % 1000000 == 0)
            {
                return $"{value:0}M";
            }

            return $"{value:0.#}M";
        }

        float billionValue =
            amount / 1000000000f;

        if (amount % 1000000000 == 0)
        {
            return $"{billionValue:0}B";
        }

        return $"{billionValue:0.#}B";
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int newTotal =
            TotalCoins + amount;

        PlayerPrefs.SetInt(
            totalCoinsKey,
            newTotal
        );

        PlayerPrefs.Save();

        RefreshStore();
    }

    // =========================================================
    // RESET STORE
    // =========================================================

    public void ResetStoreData()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];

            if (character == null ||
                string.IsNullOrEmpty(character.characterId))
            {
                continue;
            }

            PlayerPrefs.DeleteKey(
                GetOwnershipKey(character.characterId)
            );
        }

        PlayerPrefs.SetString(
            equippedCharacterKey,
            DefaultCharacterId
        );

        PlayerPrefs.Save();

        RefreshStore();
    }

    private string GetOwnershipKey(string characterId)
    {
        return ownershipKeyPrefix + characterId;
    }
}