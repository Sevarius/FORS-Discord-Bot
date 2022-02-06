namespace Contract.Interfaces
{
    public interface IJiraLabelsService
    {
        void PutLabelsToTask(string taskKey, params string[] labels);
    }
}