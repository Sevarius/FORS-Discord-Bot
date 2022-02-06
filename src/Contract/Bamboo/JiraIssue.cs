using System;

namespace Contract.Bamboo
{
    public class JiraIssue : IEquatable<JiraIssue>
    {
        public string Key { get; set; }
        
        public string Summary { get; set; }
        
        public string CommitMessage { get; set; }

        public bool Equals(JiraIssue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Key == other.Key && Summary == other.Summary;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Summary);
        }

        public override string ToString()
        {
            return $"{Key} - {Summary}";
        }
    }
}