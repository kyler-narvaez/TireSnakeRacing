using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CharacterCustomization : NetworkBehaviour
{
    public List<GameObject> characters = new List<GameObject>();
    public List<Material> materials = new List<Material>();

    GameObject currentCharacter;
    CharacterAnimations ca;

    public NetworkVariable<int> characterIndex = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> materialIndex = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        ca = GetComponent<CharacterAnimations>();

        if (IsOwner)
        {
            StartCoroutine(ApplySavedCharacter());
        }

        characterIndex.OnValueChanged += (oldVal, newVal) => SpawnCharacter(newVal);
        materialIndex.OnValueChanged += (oldVal, newVal) => SetShader();

        SpawnCharacter(characterIndex.Value); // apply initial value for everyone, including late joiners
        ca.SetAnimator();
    }

    System.Collections.IEnumerator ApplySavedCharacter()
    {
        while (PlayerDataManager.Instance == null) yield return null;

        for (int i = 0; i < PlayerDataManager.Instance.players.Count; i++)
        {
            if (PlayerDataManager.Instance.players[i].clientId != OwnerClientId) continue;
            characterIndex.Value = PlayerDataManager.Instance.players[i].characterIndex; // triggers SpawnCharacter via OnValueChanged
            materialIndex.Value = PlayerDataManager.Instance.players[i].characterColor;
            break;
        }
    }

    void SpawnCharacter(int index)
    {
        if (currentCharacter != null) Destroy(currentCharacter);
        currentCharacter = Instantiate(characters[index], transform);
        SetShader();
        ca.SetAnimator();
    }

    public void ChangeCharacter()
    {
        if (!IsOwner) return;
        characterIndex.Value = (characterIndex.Value + 1) % characters.Count; // triggers OnValueChanged on every client automatically
        UpdatePlayerData();
    }

    public void ChangeColor()
    {
        if (!IsOwner) return;
        materialIndex.Value = (materialIndex.Value + 1) % materials.Count;
        UpdatePlayerData();
    }

    private void SetShader()
    {
        //Get all renderer components
        Renderer[] renderers = currentCharacter.GetComponentsInChildren<Renderer>();
        //Change all them to material
        foreach (Renderer renderer in renderers)
        {
            renderer.material = materials[materialIndex.Value];
        }
    }

    void UpdatePlayerData()
    {
        for (int i = 0; i < PlayerDataManager.Instance.players.Count; i++)
        {
            if (PlayerDataManager.Instance.players[i].clientId != OwnerClientId) continue;
            PlayerData data = PlayerDataManager.Instance.players[i];
            data.characterIndex = characterIndex.Value;
            data.characterColor = materialIndex.Value;
            PlayerDataManager.Instance.players[i] = data;
            break;
        }
    }
}
