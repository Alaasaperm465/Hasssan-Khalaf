using System;

namespace Hassann_Khala.Application.DTOs.Reports
{
    public class MovementFilterDto
    {
        public int? ClientId { get; set; }
        public int? SectionId { get; set; }
        public int? ProductId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
