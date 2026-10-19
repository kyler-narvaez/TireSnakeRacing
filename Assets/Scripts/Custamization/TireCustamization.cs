using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class TireCustamization : NetworkBehaviour
{
    public List<TireOption> tires = new List<TireOption>();
    public List<Material> tireColors = new List<Material>();

    private MeshFilter currentMesh;
    private MeshRenderer mr;

    public NetworkVariable<int> tireIndex = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> colorIndex = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public override void OnNetworkSpawn()
    {
        currentMesh = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        if (IsOwner)
        {
            StartCoroutine(ApplySavedTire());
        }

        tireIndex.OnValueChanged += (oldVal, newVal) => SetTire();
        colorIndex.OnValueChanged += (oldVal, newVal) => SetColor();

        SetTire();
        SetColor();
    }

    System.Collections.IEnumerator ApplySavedTire()
    {
        while (PlayerDataManager.Instance == null) yield return null;

        for (int i = 0; i < PlayerDataManager.Instance.players.Count; i++)
        {
            var d = PlayerDataManager.Instance.players[i];
        }

        for (int i = 0; i < PlayerDataManager.Instance.players.Count; i++)
        {
            if (PlayerDataManager.Instance.players[i].clientId != OwnerClientId) continue;
            tireIndex.Value = PlayerDataManager.Instance.players[i].tireIndex; // triggers SpawnCharacter via OnValueChanged
            colorIndex.Value = PlayerDataManager.Instance.players[i].tireColor;
            break;
        }
    }

    void SetTire()
    {
        currentMesh.mesh = tires[tireIndex.Value].mesh;
        transform.localScale = tires[tireIndex.Value].localScale;
        transform.localPosition = tires[tireIndex.Value].localPositionOffset;
        SetColor();
    }

    public void ChangeTire()
    {
        if (!IsOwner) return;
        tireIndex.Value = (tireIndex.Value + 1) % tires.Count;
        UpdatePlayerData();
    }

    void SetColor()
    {
        mr.material = tireColors[colorIndex.Value];
    }

    public void ChangeColor()
    {
        if (!IsOwner) return;
        colorIndex.Value = (colorIndex.Value + 1) % tireColors.Count;
        UpdatePlayerData();
    }

    void UpdatePlayerData()
    {
        for (int i = 0; i < PlayerDataManager.Instance.players.Count; i++)
        {
            if (PlayerDataManager.Instance.players[i].clientId != OwnerClientId) continue;
            PlayerData data = PlayerDataManager.Instance.players[i];
            data.tireIndex = tireIndex.Value;
            data.tireColor = colorIndex.Value;
            PlayerDataManager.Instance.players[i] = data;
            break;
        }
    }

    //FIXME - Changed the order of where Red material was still made it red
    private void Update()
    {
        print("Current ColorIndex: " + colorIndex.Value); //Correct value at all times
        SetColor(); //Makes it what it is suppsed to be
        //Timing issue?
    }

}

[System.Serializable]
public class TireOption
{
    public Mesh mesh;
    public Vector3 localScale = Vector3.one;
    public Vector3 localPositionOffset = Vector3.zero;
}
