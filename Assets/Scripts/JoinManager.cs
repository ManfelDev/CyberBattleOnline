using UnityEngine;
using TMPro;
using Unity.Collections;

public class JoinManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputFieldJoinCode;
    [SerializeField] private TMP_InputField inputFieldPlayerName;
    [SerializeField] private GameObject     joinUI;
    [SerializeField] private GameObject     gameUI;
    [SerializeField] private NetworkSetup   networkSetup;

    public static string             joinCode;
    public static FixedString32Bytes playerName;

    public void OnJoinButtonClicked()
    {
        joinCode    = inputFieldJoinCode.text;
        playerName  = inputFieldPlayerName.text;
        
        StartCoroutine(networkSetup.StartAsClientCR());

        joinUI.SetActive(false);
        gameUI.SetActive(true);
    }
}