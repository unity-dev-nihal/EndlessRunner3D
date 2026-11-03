using System;
using UnityEngine;

public class RunnerCharacterSelector : MonoBehaviour
{
    [Serializable]
    private class CharacterEntry
    {
        public string characterId;
        public GameObject characterObject;
    }

    [Header("Characters")]
    [SerializeField] private CharacterEntry[] characters;

    [Header("PlayerPrefs")]
    [SerializeField] private string equippedCharacterKey =
        "GottaGo_EquippedCharacter";

    [Header("Default Character")]
    [SerializeField] private string defaultCharacterId = "Rogue";

    private void Awake()
    {
        ApplyEquippedCharacter();
    }

    private void Start()
    {
        ApplyEquippedCharacter();
    }

    public void ApplyEquippedCharacter()
    {
        string equippedCharacterId =
            PlayerPrefs.GetString(
                equippedCharacterKey,
                defaultCharacterId
            );

        if (string.IsNullOrEmpty(equippedCharacterId))
        {
            equippedCharacterId =
                defaultCharacterId;
        }

        bool characterFound = false;

        for (int i = 0; i < characters.Length; i++)
        {
            CharacterEntry entry = characters[i];

            if (entry == null ||
                entry.characterObject == null)
            {
                continue;
            }

            bool shouldBeActive =
                string.Equals(
                    entry.characterId,
                    equippedCharacterId,
                    StringComparison.OrdinalIgnoreCase
                );

            entry.characterObject.SetActive(
                shouldBeActive
            );

            if (shouldBeActive)
            {
                characterFound = true;
            }
        }

        if (!characterFound)
        {
            Debug.LogWarning(
                "RunnerCharacterSelector: Equipped character '" +
                equippedCharacterId +
                "' was not found. Falling back to '" +
                defaultCharacterId +
                "'."
            );

            ActivateDefaultCharacter();
        }

        Debug.Log(
            "RunnerCharacterSelector: Active character = " +
            equippedCharacterId
        );
    }

    private void ActivateDefaultCharacter()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            CharacterEntry entry = characters[i];

            if (entry == null ||
                entry.characterObject == null)
            {
                continue;
            }

            bool isDefault =
                string.Equals(
                    entry.characterId,
                    defaultCharacterId,
                    StringComparison.OrdinalIgnoreCase
                );

            entry.characterObject.SetActive(
                isDefault
            );
        }
    }

    public void RefreshCharacter()
    {
        ApplyEquippedCharacter();
    }
}