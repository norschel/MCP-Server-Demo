using System.ComponentModel;
using ModelContextProtocol.Server;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace LocalMCPServer;

[McpServerToolType]
public static class AzureDevOpsTool
{
    [McpServerTool, Description("Reads a work item from Azure DevOps using a PAT.")]
    public static async Task<string> GetWorkItemByIdAsync(
        [Description("Azure DevOps organization name, e.g. my_org_name")] string orgName,
        //[Description("Azure DevOps project name")] string project,
        [Description("Work Item ID, - Title or -Description")] string workItemSearchString,
        [Description("Retrieve full history")] bool fullHistory = false)
    {
        try
        {
            var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
            var orgUrl = $"https://dev.azure.com/{orgName}";

            // Authenticate using PAT
            var creds = new VssBasicCredential(string.Empty, pat);
            using var connection = new VssConnection(new Uri(orgUrl), creds);
            var witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            // Trim and check if the search string is a valid integer (ID)
            var trimmedSearch = workItemSearchString?.Trim();
            var resultsList = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(trimmedSearch) && int.TryParse(trimmedSearch, out int workItemId))
            {
                // Directly get the work item by ID
                try
                {
                    var workItem = await witClient.GetWorkItemAsync(workItemId);
                    var title = workItem.Fields["System.Title"]?.ToString();
                    var state = workItem.Fields["System.State"]?.ToString();
                    var type = workItem.Fields["System.WorkItemType"]?.ToString();

                    if (fullHistory)
                    {
                        var revisions = await witClient.GetRevisionsAsync(workItemId);
                        var historyList = new System.Text.StringBuilder();
                        foreach (var rev in revisions)
                        {
                            var changedDate = rev.Fields.ContainsKey("System.ChangedDate") ? rev.Fields["System.ChangedDate"]?.ToString() : "";
                            var changedBy = rev.Fields.ContainsKey("System.ChangedBy") ? rev.Fields["System.ChangedBy"]?.ToString() : "";
                            var history = rev.Fields.ContainsKey("System.History") ? rev.Fields["System.History"]?.ToString() : "";
                            if (!string.IsNullOrWhiteSpace(history))
                            {
                                historyList.AppendLine($"[{changedDate}] {changedBy}: {history}");
                            }
                        }
                        var historyString = historyList.Length > 0 ? historyList.ToString() : "No history available.";
                        resultsList.AppendLine($"Work Item {workItem.Id}: '{title}' (Type: {type}, State: {state})\nHistory:\n{historyString}");
                    }
                    else
                    {
                        resultsList.AppendLine($"Work Item {workItem.Id}: '{title}' (Type: {type}, State: {state})");
                    }
                }
                catch (Exception ex)
                {
                    resultsList.AppendLine($"Error retrieving work item by ID: {ex.Message}");
                }
            }
            else
            {
                // Search by title or description
                var wiql = new Wiql()
                {
                    Query = $"SELECT [System.Id], [System.Title], [System.Description] FROM WorkItems WHERE [System.Title] CONTAINS '{trimmedSearch}' OR [System.Description] CONTAINS '{trimmedSearch}'"
                };
                var result = await witClient.QueryByWiqlAsync(wiql);

                if (result.WorkItems.Count() == 0)
                {
                    resultsList.AppendLine($"No work item found with title or description containing '{workItemSearchString}'");
                }
                else
                {
                    foreach (var workItemRef in result.WorkItems)
                    {
                        var workItem = await witClient.GetWorkItemAsync(workItemRef.Id);
                        var title = workItem.Fields["System.Title"]?.ToString();
                        var state = workItem.Fields["System.State"]?.ToString();
                        var type = workItem.Fields["System.WorkItemType"]?.ToString();

                        if (fullHistory)
                        {
                            var revisions = await witClient.GetRevisionsAsync(workItemRef.Id);
                            var historyList = new System.Text.StringBuilder();
                            foreach (var rev in revisions)
                            {
                                var changedDate = rev.Fields.ContainsKey("System.ChangedDate") ? rev.Fields["System.ChangedDate"]?.ToString() : "";
                                var changedBy = rev.Fields.ContainsKey("System.ChangedBy") ? rev.Fields["System.ChangedBy"]?.ToString() : "";
                                var history = rev.Fields.ContainsKey("System.History") ? rev.Fields["System.History"]?.ToString() : "";
                                if (!string.IsNullOrWhiteSpace(history))
                                {
                                    historyList.AppendLine($"[{changedDate}] {changedBy}: {history}");
                                }
                            }
                            var historyString = historyList.Length > 0 ? historyList.ToString() : "No history available.";
                            resultsList.AppendLine($"Work Item {workItem.Id}: '{title}' (Type: {type}, State: {state})\nHistory:\n{historyString}");
                        }
                        else
                        {
                            resultsList.AppendLine($"Work Item {workItem.Id}: '{title}' (Type: {type}, State: {state})");
                        }
                    }
                }
            }
            var results = resultsList.ToString();
            return string.IsNullOrWhiteSpace(results) ? "No work item found." : results;
        }
        catch (Exception ex)
        {
            return $"Error retrieving work item: {ex.Message}";
        }

        //return "No work item found.";
    }
}
