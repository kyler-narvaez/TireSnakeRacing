using UnityEngine;

public class GenericPanel : MonoBehaviour
{
    public string panelName;
    public LobbyCameraFollow cameraFollow; // assign the single scene camera here

    public void OnPanelOpen()
    {

        gameObject.SetActive(true);
        if (panelName == "PlayerCustomization" && cameraFollow != null)
        {
            cameraFollow.SetCustomizing(true);
        }
    }

    public void OnPanelClosed()
    {

        gameObject.SetActive(false);
        if (panelName == "PlayerCustomization" && cameraFollow != null)
        {
            cameraFollow.SetCustomizing(false);
        }
    }
}