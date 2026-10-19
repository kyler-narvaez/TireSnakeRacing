using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

public class LobbyUI : MonoBehaviour
{
    NetworkObject localPlayer;
    public TMP_InputField lapInputField;
    public GameObject player;

    void Start()
    {
        StartCoroutine(FindLocalPlayer());
        StartCoroutine(WaitForGameSettings());
    }

    System.Collections.IEnumerator WaitForGameSettings()
    {
        while (GameSettings.Instance == null)
        {
            yield return null;
        }
        lapInputField.text = GameSettings.Instance.numberOfLaps.Value.ToString();
    }

    System.Collections.IEnumerator FindLocalPlayer()
    {
        while (localPlayer == null)
        {
            localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            yield return null;
        }
    }

    public void OnChangeCharacterClicked()
    {
        if (localPlayer == null) return;
        CharacterCustomization cc = localPlayer.GetComponentInChildren<CharacterCustomization>();
        cc.ChangeCharacter();
    }

    public void OnChangeCharacterColorClicked()
    {
        if (localPlayer == null) return;
        CharacterCustomization cc = localPlayer.GetComponentInChildren<CharacterCustomization>();
        cc.ChangeColor();
    }

    public void OnChangeTireClicked()
    {
        if (localPlayer == null) return;
        TireCustamization tc = localPlayer.GetComponentInChildren<TireCustamization>();
        tc.ChangeTire();
    }

    public void OnChangeTireColorClicked()
    {
        if (localPlayer == null) return;
        TireCustamization tc = localPlayer.GetComponentInChildren<TireCustamization>();
        tc.ChangeColor();
    }

    public void OnChangeLapNumber(string input)
    {
        if (int.TryParse(input, out int parsedLaps) && parsedLaps > 0)
        {
            GameSettings.Instance.numberOfLaps.Value = parsedLaps;
        }
        else
        {
            // invalid input — leave numberOfLaps unchanged
            lapInputField.text = GameSettings.Instance.numberOfLaps.Value.ToString();
        }
    }

    public void OnSetRandomRoute(bool isRandom)
    {
        GameSettings.Instance.isRandom.Value = isRandom;
    }

    public void OnSetSameSequence(bool sameSequence)
    {
        GameSettings.Instance.isSameSequence.Value = sameSequence;
    }

    public void OnStartClicked()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        GameSettings.Instance.SwapPlayerPrefabClientRpc(); // tells every Client to update their own local config
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = GameSettings.Instance.player; // Host's own local update

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += RespawnPlayersForRace;
        NetworkManager.Singleton.SceneManager.LoadScene("RaceTrack", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    void RespawnPlayersForRace(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= RespawnPlayersForRace; // one-shot

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn(true); // destroys the old lobby player object
            }

            GameObject newPlayer = Instantiate(GameSettings.Instance.player);
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
        }
    }
}
