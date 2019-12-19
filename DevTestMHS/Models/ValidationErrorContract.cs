namespace DevTestMHS.Models
{
    public class ValidationErrorContract
    {
        public string SegmentOrGroup { get; set; }
        public string FieldNumber { get; set; }
        public string Field { get; set; }
        public string ErrorType { get; set; }
        public string ErrorText { get; set; }
    }
}