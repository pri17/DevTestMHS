namespace DevTestMHS.Models
{
    public class MHSMessageContent
    {
        public virtual string Content { get; set; }
        public virtual int SequenceId { get; set; }
        public virtual MHSMessage MessageSequenceId { get; set; }
    }
}