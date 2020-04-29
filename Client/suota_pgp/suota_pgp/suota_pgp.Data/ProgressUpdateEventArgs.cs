namespace suota_pgp.Data
{
    public class ProgressUpdateEventArgs
    {
        /// <summary>
        /// Process percentage completed [0-100].
        /// </summary>
        public int? Percentage { get; }

        /// <summary>
        /// Process status string.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// The process is complete.
        /// </summary>
        public bool IsComplete { get; }

        /// <summary>
        /// 
        /// </summary>
        public ProgressUpdateEventArgs() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage"></param>
        public ProgressUpdateEventArgs(int percentage)
        {
            Percentage = percentage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        public ProgressUpdateEventArgs(string status)
        {
            Status = status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="status"></param>
        public ProgressUpdateEventArgs(int percentage, string status)
        {
            Percentage = percentage;
            Status = status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="status"></param>
        /// <param name="isComplete"></param>
        public ProgressUpdateEventArgs(int percentage, string status, bool isComplete)
        {
            Percentage = percentage;
            Status = status;
            IsComplete = isComplete;
        }
    }
}
