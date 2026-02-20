using Hassann_Khala.Application.DTOs.Client;

namespace Hassann_Khala.Application.Interfaces.IServices
{
    public interface IClientService
    {
        Task<IEnumerable<ClientDto>> GetAllAsync();
        Task<ClientDto?> GetByIdAsync(int id);
        Task<ClientDto> CreateAsync(CreateClientRequest request);
        Task<ClientDto?> UpdateAsync(int id, UpdateClientRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
