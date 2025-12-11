using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAttachmentsRepository
    {
        Task<IEnumerable<Attachments>> GetAllAsync();
        Task<IEnumerable<Attachments>> GetByUserIdAsync(int userId);
        Task<Attachments?> GetByIdAsync(int id);
        Task<int> CreateAsync(Attachments attachment);
        Task<bool> UpdateAsync(Attachments attachment);
        Task<bool> DeleteAsync(int id);
    }
}
