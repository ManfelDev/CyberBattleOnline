using UnityEngine;
using TMPro;
using Unity.Collections;

public class JoinManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputFieldJoinCode;
    [SerializeField] private TMP_InputField inputFieldPlayerName;
    [SerializeField] private int            maxPlayerNameLength = 12;
    [SerializeField] private GameObject     joinUI;
    [SerializeField] private NetworkSetup   networkSetup;

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