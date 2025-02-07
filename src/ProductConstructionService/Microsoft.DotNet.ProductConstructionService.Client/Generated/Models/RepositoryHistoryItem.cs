// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Newtonsoft.Json;

namespace Microsoft.DotNet.ProductConstructionService.Client.Models
{
    public partial class RepositoryHistoryItem
    {
        public RepositoryHistoryItem(DateTimeOffset timestamp, bool success, string repositoryName, string branchName, string errorMessage, string action, string retryUrl)
        {
            Timestamp = timestamp;
            Success = success;
            RepositoryName = repositoryName;
            BranchName = branchName;
            ErrorMessage = errorMessage;
            Action = action;
            RetryUrl = retryUrl;
        }

        [JsonProperty("repositoryName")]
        public string RepositoryName { get; }

        [JsonProperty("branchName")]
        public string BranchName { get; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; }

        [JsonProperty("success")]
        public bool Success { get; }

        [JsonProperty("action")]
        public string Action { get; }

        [JsonProperty("retryUrl")]
        public string RetryUrl { get; }
    }
}
