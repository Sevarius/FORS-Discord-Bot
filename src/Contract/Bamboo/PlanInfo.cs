using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Contract.Bamboo
{
    public class PlanInfo
    {
        public string ProjectName { get; set; }
        
        public string PlanName { get; set; }
        
        public long BuildNumber { get; set; }
        
        public string BuildResultKey { get; set; }
        
        public DateTime BuildStartedTime { get; set; }
        
        public DateTime BuildCompletedTime { get; set; }
        
        public long SuccessfulTestCount { get; set; }
        
        public long FailedTestCount { get; set; }
        
        public long SkippedTestCount { get; set; }
        
        public bool Finished { get; set; }
        
        [JsonIgnore]
        public List<JiraIssue> JiraIssues { get; set; }
    }
}