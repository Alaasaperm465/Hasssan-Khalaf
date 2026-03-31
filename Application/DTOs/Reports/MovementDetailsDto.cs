using System.Collections.Generic;
using System.Linq;

namespace Hassann_Khala.Application.DTOs.Reports
{
    public class MovementDetailsDto
    {
        public MovementFilterDto Filter { get; set; } = new();
        public List<MovementGroupDto> Groups { get; set; } = new();

        public decimal TotalIncoming => Groups.Sum(g => g.TotalIncoming);
        public decimal TotalOutgoing => Groups.Sum(g => g.TotalOutgoing);
        public decimal Net => TotalIncoming - TotalOutgoing;
    }
}
