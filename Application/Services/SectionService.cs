using Hassann_Khala.Application.DTOs.Section;
using Hassann_Khala.Application.Interfaces.IServices;
using Hassann_Khala.Domain.Interfaces;

//using Hassann_Khala.Domain.Interfaces;
using System.Linq;

namespace Hassann_Khala.Application.Services
{
    public class SectionService : ISectionService
    {
        private readonly ISectionRepository _sectionRepo;

        public SectionService(ISectionRepository sectionRepo)
        {
            _sectionRepo = sectionRepo;
        }

        public async Task<IEnumerable<SectionDTO>> GetAllAsync()
        {
            var sections = await _sectionRepo.GetAllAsync();
            return sections.Select(s => new SectionDTO { Id = s.Id, Name = s.Name });
        }
    }
}
