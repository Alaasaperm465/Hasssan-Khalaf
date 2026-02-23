namespace Hassann_Khala.Domain
{
    public class IssuanceItem
    {
        public int Id { get; set; }
        public int IssuanceId { get; set; }

        public int ItemIndex { get; set; } // row index

        public string FieldValue { get; set; } = string.Empty; // stored value for the field

        // optional extra columns
        public int? ProductId { get; set; }
        public int? SectionId { get; set; }
    }
}
