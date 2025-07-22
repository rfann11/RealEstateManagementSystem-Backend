using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using REMS.Backend.DTOs;
using REMS.Backend.Interfaces;

namespace REMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IllerController : ControllerBase
    {
        private readonly IIlService _ilservice;

        public IllerController(IIlService ilservice) {
            _ilservice = ilservice;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IlDto>>> GetIller()
        {
            var iller = await _ilservice.GetAllIllerAsync();
            return Ok(iller);
        }
    }
}
