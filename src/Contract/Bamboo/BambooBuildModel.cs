namespace Contract.Bamboo
{
    public class BambooBuildModel
    {
        public string Uuid { get; set; }
        
        public string TimeStamp { get; set; }
        
        public string Notification { get; set; }
        
        public WebhookInfo Webhook { get; set; }
        
        public BuildInfo Build { get; set; }
    }

    public class WebhookInfo
    {
        public string TemplateId { get; set; }
        
        public string TemplateName { get; set; }
    }

    public class BuildInfo
    {
        public string BuildResultKey { get; set; }
        
        public string Status { get; set; }
        
        public string BuildPlanName { get; set; }
        
        public string StartedAt { get; set; }
        
        public string FinishedAt { get; set; }
        
        public string TriggerReason { get; set; }
        
        public string TriggerSentence { get; set; }
    }
}