using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class AttachmentsService
    {
        private readonly IAttachmentsRepository _repo;


        public AttachmentsService(IAttachmentsRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Attachments>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<IEnumerable<Attachments>> GetByUserIdAsync(int userId) => await _repo.GetByUserIdAsync(userId);
        public async Task<Attachments?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);
        public async Task<int> CreateAsync(Attachments attachment) => await _repo.CreateAsync(attachment);
        public async Task<bool> UpdateAsync(Attachments attachment) => await _repo.UpdateAsync(attachment);
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}
