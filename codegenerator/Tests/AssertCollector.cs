namespace Tests
{
    public class AssertCollector
    {
        private List<string> failures = new List<string>();

        public bool Assert(bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
            return condition;
        }

        public void ReportFailures()
        {
            if (failures.Count > 0)
            {
                throw new AssertionException(string.Join(Environment.NewLine, failures));
            }
        }
    }
}

