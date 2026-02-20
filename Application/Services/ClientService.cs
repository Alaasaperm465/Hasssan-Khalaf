//using Hassann_Khala.Domain.Interfaces;
using Hassann_Khala.Application.DTOs.Client;
using Hassann_Khala.Application.Interfaces.IServices;
using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;

namespace Hassann_Khala.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepo;
        private readonly IUnitOfWork _uow;

        public ClientService(IClientRepository clientRepo, IUnitOfWork uow)
        {
            _clientRepo = clientRepo;
            _uow = uow;
        }

        public async Task<ClientDto> CreateAsync(CreateClientRequest request)
        {
            // validation
            var existing = await _clientRepo.FindByNameAsync(request.Name.Trim());
            if (existing != null) throw new ArgumentException("Client with the same name already exists");

            var entity = new Client { Name = request.Name.Trim() };
            await _clientRepo.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return new ClientDto { Id = entity.Id, Name = entity.Name };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _clientRepo.GetByIdAsync(id);
            if (entity == null) return false;

            _clientRepo.Remove(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ClientDto>> GetAllAsync()
        {
            var list = await _clientRepo.GetAllAsync();
            return list.Select(c => new ClientDto { Id = c.Id, Name = c.Name });
        }

        public async Task<ClientDto?> GetByIdAsync(int id)
        {
            var entity = await _clientRepo.GetByIdAsync(id);
            if (entity == null) return null;
            return new ClientDto { Id = entity.Id, Name = entity.Name };
        }

        public async Task<ClientDto?> UpdateAsync(int id, UpdateClientRequest request)
        {
            var entity = await _clientRepo.GetByIdAsync(id);
            if (entity == null) return null;

            // check duplicate name
            var existing = await _clientRepo.FindByNameAsync(request.Name.Trim());
            if (existing != null && existing.Id != id) throw new ArgumentException("Client with the same name already exists");

            entity.Name = request.Name.Trim();
            _clientRepo.Update(entity);
            await _uow.SaveChangesAsync();

            return new ClientDto { Id = entity.Id, Name = entity.Name };
        }
    }
}
