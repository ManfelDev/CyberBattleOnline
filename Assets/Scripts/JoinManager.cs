using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class JoinManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    public static string joinCode;

    public void OnJoinButtonClicked()
    {
        joinCode = inputField.text;
        SceneManager.LoadScene(1);
    }
}
