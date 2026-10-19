using UnityEngine;
using Unity.Netcode;

public class MainMenuUI : MonoBehaviour
{
    public GameObject lobbyPlayer;
    public void OnHostClicked()
    {
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = lobbyPlayer; //change spawn to lobby, not race
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnJoinClicked()
    {
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = lobbyPlayer;
        NetworkManager.Singleton.StartClient();
    }

    public void OnSettingsClicked()
    {
        // open your settings panel — purely local UI, no networking involved
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
