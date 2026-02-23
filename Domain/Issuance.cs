using System.ComponentModel.DataAnnotations;

namespace Hassann_Khala.Domain
{
    public class Issuance
    {
        public int Id { get; set; }

        [Required]
        public string SerialNumber { get; set; } = string.Empty; // unique serial

        public int ClientId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // navigation
        public List<IssuanceItem> Items { get; set; } = new List<IssuanceItem>();
    }
}
