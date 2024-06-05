using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Collections;

public class JoinManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputFieldJoinCode;
    [SerializeField] private TMP_InputField inputFieldPlayerName;

    public static string             joinCode;
    public static FixedString32Bytes playerName;

    public void OnJoinButtonClicked()
    {
        joinCode    = inputFieldJoinCode.text;
        playerName  = inputFieldPlayerName.text;
        
        SceneManager.LoadScene(1);
    }
}