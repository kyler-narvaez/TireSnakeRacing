using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerNameTag : NetworkBehaviour
{
    public TextMeshProUGUI nameText;
    public Color selfColor = Color.yellow;
    public Color othersColor = Color.white;

    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        if (IsOwner)
        {
            nameText.text = "YOU"; // or pull from PlayerDataManager once name entry exists
            nameText.color = selfColor;
        }
        else
        {
            nameText.text = "NOOB"; // same, pull actual name from PlayerDataManager
            nameText.color = othersColor;
        }
    }

    void LateUpdate()
    {
        if (mainCam == null) return;
        // Billboard — always face the camera regardless of player rotation
        nameText.transform.rotation = Quaternion.LookRotation(nameText.transform.position - mainCam.transform.position);
    }
}