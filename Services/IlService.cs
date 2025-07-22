using REMS.Backend.Data;
using REMS.Backend.Entities;
using REMS.Backend.DTOs;
using REMS.Backend.Interfaces;
using Microsoft.EntityFrameworkCore;



namespace REMS.Backend.Services
{
    public class IlService : IIlService
    {
        private readonly ApplicationDbContext _context;

        public IlService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IlDto>> GetAllIllerAsync()
        {
            var iller = await _context.Iller.ToListAsync();
            return iller.Select(il => new IlDto { Id = il.Id, Ad = il.Ad });
        }
    }
}
