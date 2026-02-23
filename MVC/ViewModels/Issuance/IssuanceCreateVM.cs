using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels.Issuance
{
    public class IssuanceCreateVM
    {
        public int ClientId { get; set; }

        [Required]
        public string SerialNumber { get; set; } = string.Empty;

        public List<IssuanceItemVM> Items { get; set; } = new List<IssuanceItemVM>();
    }

    public class IssuanceItemVM
    {
        public int ItemIndex { get; set; }
        public string FieldValue { get; set; } = string.Empty;
        public int? ProductId { get; set; }
        public int? SectionId { get; set; }
    }
}
