namespace suota_pgp.Model
{
    public class Progress
    {
        /// <summary>
        /// Operation percentage [0-100].
        /// </summary>
        public int? Percentage { get; }
        /// <summary>
        /// Operation status string.
        /// </summary>
        public string Status { get; }
        /// <summary>
        /// The operation is complete.
        /// </summary>
        public bool IsComplete { get; }

        public Progress() { }

        public Progress(int percentage)
        {
            Percentage = percentage;
        }

        public Progress(string status)
        {
            Status = status;
        }

        public Progress(int percentage, string status)
        {
            Percentage = percentage;
            Status = status;
        }

        public Progress(int percentage, string status, bool isComplete)
        {
            Percentage = percentage;
            Status = status;
            IsComplete = isComplete;
        }
    }
}
