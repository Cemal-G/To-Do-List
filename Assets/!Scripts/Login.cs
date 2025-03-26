using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class Login : MonoBehaviour
{
    public TMP_InputField usernameInput; // TMP Input Field voor gebruikersnaam
    public TMP_InputField passwordInput; // TMP Input Field voor wachtwoord
    public TMP_Text messageText; // TMP Text voor berichten
    public TaskManager task;
    public GameObject panel;
    private string loginUrl = "https://informaticaserver.nl/m_daily_planner/login.php";

    public void OnLoginButtonClicked()
    {
        StartCoroutine(LoginUser(usernameInput.text, passwordInput.text));
    }

    IEnumerator LoginUser(string username, string password)
    {
        // Maak een JSON-object met de gebruikersgegevens
        string jsonData = $"{{\"username\": \"{username}\", \"password\": \"{password}\"}}";

        // Stuur een POST-verzoek naar de PHP API
        using (UnityWebRequest webRequest = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                messageText.text = "Login failed: " + webRequest.error;
            }
            else
            {
                // Verwerk de JSON-response
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                // Converteer JSON naar een object
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                if (response.success)
                {
                    messageText.text = "Login successful!";
                    // Sla de user_id op voor later gebruik
                    PlayerPrefs.SetInt("user_id", response.user_id);
                    task.StartFunc(response.user_id);
                    panel.gameObject.SetActive(false);
                    // Hier kun je de speler doorsturen naar het hoofdmenu of de takenlijst
                }
                else
                {
                    messageText.text = "Login failed: " + response.message;
                }
            }
        }
    }

    [System.Serializable]
    private class LoginResponse
    {
        public bool success;
        public int user_id;
        public string message;
    }
}