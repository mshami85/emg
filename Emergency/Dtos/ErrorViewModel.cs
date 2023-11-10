namespace Emergency.Dtos
{
    public class ErrorViewModel
    {
        public ErrorViewModel(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception { get; set; }
    }
}
