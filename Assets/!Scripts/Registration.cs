using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Threading.Tasks;

public class Registration : MonoBehaviour
{
    public TMP_InputField usernameInput; // TMP Input Field voor gebruikersnaam
    public TMP_InputField passwordInput; // TMP Input Field voor wachtwoord
    public TMP_Text messageText; // TMP Text voor berichten
    public GameObject panel;
    public TaskManager task;
    private string registerUrl = "https://informaticaserver.nl/m_daily_planner/register.php";


    public void OnRegisterButtonClicked()
    {
        StartCoroutine(RegisterUser(usernameInput.text, passwordInput.text));
    }

IEnumerator RegisterUser(string username, string password)
{
    // Maak een JSON-object met de gebruikersgegevens
    string jsonData = $"{{\"username\": \"{username}\", \"password\": \"{password}\"}}";

    // Stuur een POST-verzoek naar de PHP API
    using (UnityWebRequest webRequest = new UnityWebRequest(registerUrl, "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + webRequest.error);
            messageText.text = "Registration failed: " + webRequest.error;
        }
        else
        {
            // Verwerk de JSON-response
            string jsonResponse = webRequest.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);

            // Converteer JSON naar een object
            RegistrationResponse response = JsonUtility.FromJson<RegistrationResponse>(jsonResponse);

            if (response.success)
            {
                // Pass the user ID to the TaskManager
                task.StartFunc(response.user_id);

                // Optionally, save the user ID to PlayerPrefs for later use
                PlayerPrefs.SetInt("user_id", response.user_id);
                PlayerPrefs.Save();

                panel.gameObject.SetActive(false);
                messageText.text = "Registration successful!";
            }
            else
            {
                messageText.text = "Registration failed: " + response.message;
            }
        }
    }
}
    [System.Serializable]
    private class RegistrationResponse
    {
        public bool success;
        public string message;
        public int user_id;
    }
}