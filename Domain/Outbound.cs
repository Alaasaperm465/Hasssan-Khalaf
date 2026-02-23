namespace Hassann_Khala.Domain
{
    using global::Domain;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Outbound: BaseEntity
    {
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        // optional delegate who authorized the outbound
        public int? DelegateId { get; set; }
        public Delegate? Delegate { get; set; }

        // Navigation
        public IList<OutboundDetail> Details { get; set; } = new List<OutboundDetail>();
    }
}
