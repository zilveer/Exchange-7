namespace Logic
{
    public class BusinessOperationResult
    {
        /// <summary>
        /// 0 = No Errors
        /// </summary>
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class BusinessOperationResult<T> : BusinessOperationResult
    {
        public int? Id { get; set; }
        public T Entity { get; set; }
    }
}
