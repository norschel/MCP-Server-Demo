using System.ComponentModel;
using ModelContextProtocol.Server;

namespace LocalMCPServer
{
    [McpServerToolType]
    public static class MyTesTool
    {
        [McpServerTool, Description("Returns a random Chuck Norris joke or quote fetched from a public API.")]
        public static async Task<string> GetChuckNorrisJokeAsync()
        {
            const string apiUrl = "https://api.chucknorris.io/jokes/random";
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetFromJsonAsync<ChuckNorrisJokeResponse>(apiUrl);
                if (response?.value != null)
                    return response.value;
                return "No joke received from API.";
            }
            catch (Exception ex)
            {
                return $"Error fetching joke: {ex.Message}";
            }
        }

        private class ChuckNorrisJokeResponse
        {
            public string value { get; set; } = string.Empty;
        }

        [McpServerTool, Description("Retrieves work items from azure devops")]
        public static string GetWorkItemByID([Description("Work Item Id")] string workItemId)
        {
            // Simulate some work
            Thread.Sleep(1000);
            return $"Work item by id {workItemId}";
        }

        public static string GetWorkItemByTitle([Description("Work Item Title")] string workItemTitle)
        {
            // Simulate some work
            Thread.Sleep(1000);
            return $"Get Work item by title {workItemTitle}";
        }


    }
}