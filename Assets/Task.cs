using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TaskManager : MonoBehaviour
{
    public GameObject taskPrefab; // Prefab voor een enkele taak
    public Transform contentParent; // De Content van de ScrollView
    public TMP_InputField taskInputField; // InputField voor nieuwe taken
    public string getTasksUrl = "https://informaticaserver.nl/adamh/z_Other/todo_list_game/get_tasks.php";

    public void StartFunc(int userId)
    {
        gameObject.SetActive(true);
        Debug.Log("User ID: " + userId);
        // Start de coroutine om taken op te halen
        StartCoroutine(GetTasks(userId));
    }

    public void OnAddTaskButtonClicked()
    {
        string taskDescription = taskInputField.text;
        if (!string.IsNullOrEmpty(taskDescription))
        {
            AddTask(taskDescription);
            taskInputField.text = ""; // Maak het invoerveld leeg
        }
    }

    public void AddTask(string taskDescription)
    {
        int userId = PlayerPrefs.GetInt("user_id", -1);
        if (userId != -1)
        {
            StartCoroutine(AddTaskToDatabase(userId, taskDescription));
        }
        else
        {
            Debug.LogError("User ID not found.");
        }
    }

    IEnumerator AddTaskToDatabase(int userId, string taskDescription)
    {
        // Maak een JSON-object met de taakgegevens
        string jsonData = $"{{\"user_id\": {userId}, \"task_description\": \"{taskDescription}\"}}";

        // Stuur een POST-verzoek naar de PHP API
        using (UnityWebRequest webRequest = new UnityWebRequest("https://informaticaserver.nl/adamh/z_Other/todo_list_game/add_task.php", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Verwerk de JSON-response
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                // Herlaad de takenlijst
                StartCoroutine(GetTasks(userId));
            }
        }
    }

    public void CompleteTask(int taskId)
    {
        int userId = PlayerPrefs.GetInt("user_id", -1);
        if (userId != -1)
        {
            StartCoroutine(MarkTaskAsCompleted(taskId));
        }
        else
        {
            Debug.LogError("User ID not found.");
        }
    }

    IEnumerator MarkTaskAsCompleted(int taskId)
    {
        // Maak een JSON-object met de taakgegevens
        string jsonData = $"{{\"task_id\": {taskId}}}";

        // Stuur een POST-verzoek naar de PHP API
        using (UnityWebRequest webRequest = new UnityWebRequest("https://informaticaserver.nl/adamh/z_Other/todo_list_game/complete_task.php", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Verwerk de JSON-response
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                // Herlaad de takenlijst
                int userId = PlayerPrefs.GetInt("user_id", -1);
                StartCoroutine(GetTasks(userId));
            }
        }
    }

    IEnumerator GetTasks(int userId)
    {
        // Stuur een GET-verzoek naar de PHP API
        string url = $"{getTasksUrl}?user_id={userId}";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Verwerk de JSON-response
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Tasks: " + jsonResponse);

                // Converteer JSON naar een lijst van taken
                Task[] tasks = JsonHelper.FromJson<Task>(jsonResponse);

                // Vul de ScrollView met taken
                PopulateScrollView(tasks);
            }
        }
    }

    void PopulateScrollView(Task[] tasks)
    {
        // Verwijder bestaande taken (indien nodig)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Instantieer een prefab voor elke taak
        foreach (Task task in tasks)
        {
            GameObject taskItem = Instantiate(taskPrefab, contentParent);

            // Vul de taakgegevens in
            TMP_Text descriptionText = taskItem.GetComponentInChildren<TMP_Text>();
            Debug.Log(task.task_description);
            if (descriptionText != null)
            {
                descriptionText.text = task.task_description;
            }

            // Pas de kleur van de RawImage aan op basis van de voltooiingsstatus
            RawImage statusImage = taskItem.GetComponentInChildren<RawImage>();
            if (statusImage != null)
            {
                statusImage.color = task.is_completed == 1 ? Color.green : Color.red;
            }

            // Voeg een klik-event toe om de taak als voltooid te markeren
            Button taskButton = taskItem.GetComponent<Button>();
            if (taskButton != null)
            {
                taskButton.onClick.AddListener(() => CompleteTask(task.id));
            }
        }
    }

    [System.Serializable]
    public class Task
    {
        public int id;
        public string task_description;
        public int is_completed;
    }

    // Helper class om JSON te parsen
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}