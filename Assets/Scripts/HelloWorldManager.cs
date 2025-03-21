using System.Collections; // Required for IEnumerator
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        VisualElement rootVisualElement;
        Button hostButton;
        Button clientButton;
        Button serverButton;
        Label statusLabel;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            rootVisualElement = uiDocument.rootVisualElement;

            // Initialize buttons and statusLabel
            hostButton = CreateButton("HostButton", "Host");
            clientButton = CreateButton("ClientButton", "Client");
            serverButton = CreateButton("ServerButton", "Server");
            statusLabel = CreateLabel("StatusLabel", "Not Connected");

            // Add UI elements to root visual element
            rootVisualElement.Add(hostButton);
            rootVisualElement.Add(clientButton);
            rootVisualElement.Add(serverButton);
            rootVisualElement.Add(statusLabel);

            // Subscribe to button click events
            hostButton.clicked += OnHostButtonClicked;
            clientButton.clicked += OnClientButtonClicked;
            serverButton.clicked += OnServerButtonClicked;
        }

        void Update()
        {
            UpdateUI();
        }

        void OnDisable()
        {
            hostButton.clicked -= OnHostButtonClicked;
            clientButton.clicked -= OnClientButtonClicked;
            serverButton.clicked -= OnServerButtonClicked;
        }

        void OnHostButtonClicked()
        {
            NetworkManager.Singleton.StartHost();
            SetStatusText("Starting Host..."); // Update status text when host button is clicked
            StartCoroutine(AssignCorrectPrefabForHost()); // Assign Frog prefab for host
        }

        void OnClientButtonClicked()
        {
            NetworkManager.Singleton.StartClient();
            SetStatusText("Starting Client..."); // Update status text when client button is clicked
            StartCoroutine(AssignCorrectPrefabForClient()); // Assign Evil prefab for client
        }

        void OnServerButtonClicked()
        {
            NetworkManager.Singleton.StartServer();
            SetStatusText("Starting Server..."); // Update status text when server button is clicked
        }

        private Button CreateButton(string name, string text)
        {
            var button = new Button();
            button.name = name;
            button.text = text;
            button.style.width = 240;
            button.style.backgroundColor = Color.white;
            button.style.color = Color.black;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            return button;
        }

        private Label CreateLabel(string name, string content)
        {
            var label = new Label();
            label.name = name;
            label.text = content;
            label.style.color = Color.black;
            label.style.fontSize = 18;
            return label;
        }

        void UpdateUI()
        {
            if (NetworkManager.Singleton == null)
            {
                SetStartButtons(false);
                SetStatusText("NetworkManager not found");
                return;
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                SetStartButtons(true);
                SetStatusText("Not connected");
            }
            else
            {
                SetStartButtons(false);
                UpdateStatusLabels();
            }
        }

        void SetStartButtons(bool state)
        {
            hostButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            clientButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            serverButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetStatusText(string text)
        {
            // Update statusLabel's text to display the status
            statusLabel.text = text;
        }

        void UpdateStatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
            string transport = "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
            string modeText = "Mode: " + mode;
            SetStatusText($"{transport}\n{modeText}");
        }

        // === NEW CODE: Assign the correct prefab when starting host or client ===

        IEnumerator AssignCorrectPrefabForHost()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton.IsServer);

            PrefabManager prefabManager = FindObjectOfType<PrefabManager>();
            if (prefabManager != null)
            {
                SetStatusText("Prefab set: frog");
                prefabManager.ReplaceWithCorrectPrefabServerRpc(NetworkManager.Singleton.LocalClientId, "frog");
            }
            else
            {
                SetStatusText("PrefabManager not found in the scene!");
            }
        }

        IEnumerator AssignCorrectPrefabForClient()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton.IsConnectedClient);

            PrefabManager prefabManager = FindObjectOfType<PrefabManager>();
            if (prefabManager != null)
            {
                SetStatusText("Prefab set: evil");
                prefabManager.ReplaceWithCorrectPrefabServerRpc(NetworkManager.Singleton.LocalClientId, "evil");
            }
            else
            {
                SetStatusText("PrefabManager not found in the scene!");
            }
        }
    }
}


