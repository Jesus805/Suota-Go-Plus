namespace suota_pgp.Model
{
    /// <summary>
    /// Represents the progress of a SUOTA process.
    /// </summary>
    public class Progress
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
        public Progress() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage"></param>
        public Progress(int percentage)
        {
            Percentage = percentage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        public Progress(string status)
        {
            Status = status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="status"></param>
        public Progress(int percentage, string status)
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
        public Progress(int percentage, string status, bool isComplete)
        {
            Percentage = percentage;
            Status = status;
            IsComplete = isComplete;
        }
    }
}
