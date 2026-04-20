using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tippo.Web.Database;
using Tippo.Web.Models;

namespace Tippo.Web.Controllers
{
    [Route("api/TippController")]
    [ApiController]
    public class TippsController : ControllerBase
    {
        DatabaseManager dbHelperBenutzer;
        public TippsController()
        {
            dbHelperBenutzer = new DatabaseManager();
        }

        [HttpPost]
        [Route("createtipp")]
        public IActionResult CreateTipp([FromBody] Tipp tipp)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
