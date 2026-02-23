using global::Domain;

namespace Hassann_Khala.Domain
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Section: BaseEntity
    {
        public string Name { get; set; } = null!;


    }
}
