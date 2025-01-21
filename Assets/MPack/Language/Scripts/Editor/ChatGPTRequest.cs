using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;



namespace MPack
{
    public static class ChatGPTRequest
    {
        private const string API_KEY_FILE_PATH = "Assets/MPack/Language/Scripts/Editor/GPTAPIKey.asset";

        [MenuItem("Tools/MPack/Language - Create GPT API file")]
        public static void CreateAPIKeyFile()
        {
            var key = AssetDatabase.LoadAssetAtPath<StringVariable>(API_KEY_FILE_PATH);
            if (key == null)
            {
                key = ScriptableObject.CreateInstance<StringVariable>();
                key.Value = "API_KEY";
                AssetDatabase.CreateAsset(key, API_KEY_FILE_PATH);
                AssetDatabase.SaveAssets();
            }
            
            Selection.activeObject = key;
        }

        public static string GetAPIKey()
        {
            var key = AssetDatabase.LoadAssetAtPath<StringVariable>(API_KEY_FILE_PATH);

            if (key == null)
                return "";

            return key.Value;
        }

        public static async Task<ResponseJSON> Translate(string toLanguage, string message)
        {
            string endpoint = "https://api.openai.com/v1/chat/completions"; // Adjust endpoint as needed

            Message[] messages = new Message[]
            {
                new Message ( "system", "You are a translation assistant." ),
                new Message ( "user", $"Can you translate the follow text into {toLanguage}? \"{message}\"" ),
            };

            var data = new SendData
            {
                model="gpt-3.5-turbo",
                messages=messages
            };

            // Install Unity Package "com.unity.nuget.newtonsoft-json"
            string response = await GetChatResponse(endpoint, JsonUtility.ToJson(data));
            Debug.Log(response);
            return JsonUtility.FromJson<ResponseJSON>(response);
        }

        static async Task<string> GetChatResponse(string endpoint, string data)
        {
            string key = GetAPIKey();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");

                var content = new StringContent(data, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(endpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
        }

        [Serializable]
        public struct SendData
        {
            public string model;
            public Message[] messages;
        }

        [Serializable]
        public struct ResponseJSON
        {
            public string id;
            public string @object;
            public int created;
            public string model;
            public Choice[] choices;
            public Usage usage;
            public ErrorMessage error;

            [Serializable]
            public struct Choice
            {
                public int index;
                public Message message;
                public string finish_reason;
            }

            [Serializable]
            public struct Usage
            {
                public int prompt_tokens;
                public int completion_tokens;
                public int total_tokens;
            }
        }


        [Serializable]
        public struct Message
        {
            public string role;
            public string content;

            public Message(string role, string content)
            {
                this.role = role;
                this.content = content;
            }
        }

        [Serializable]
        public struct ErrorMessage
        {
            public string message;
            public string type;
            public string param;
            public string code;
        }
    }
}