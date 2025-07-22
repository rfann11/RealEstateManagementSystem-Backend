using REMS.Backend.DTOs;

namespace REMS.Backend.Interfaces
{
    public interface IIlService
    {
        Task<IEnumerable<IlDto>> GetAllIllerAsync();
    }
}
