using UnityEngine;
using TMPro;
using Unity.Collections;

public class JoinManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField inputFieldJoinCode;
    [SerializeField] private TMP_InputField inputFieldPlayerName;
    [SerializeField] private GameObject     joinUI;
    [SerializeField] private NetworkSetup   networkSetup;

    [Header("Settings")]
    [SerializeField] private int            maxPlayerNameLength = 12;

    public static string             joinCode;
    public static FixedString32Bytes playerName;

    private void Start()
    {
        inputFieldPlayerName.characterLimit = maxPlayerNameLength;
    }

    public void OnJoinButtonClicked()
    {
        joinCode    = inputFieldJoinCode.text;
        playerName  = inputFieldPlayerName.text;

        if (playerName.Length < 1)
        {
            playerName = "Player";
        }
        
        StartCoroutine(networkSetup.StartAsClientCR());

        joinUI.SetActive(false);
    }
}