using Hassann_Khala.Application.DTOs.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Hassann_Khala.Application.Interfaces.IServices
{
    public interface ISectionService
    {
        Task<IEnumerable<SectionDTO>> GetAllAsync();
    }

}
