using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tippo.Web.Database;
using Tippo.Web.Models;

namespace Tippo.Web.Controllers
{
    [Route("api/SpielController")]
    [ApiController]
    public class SpieleController : ControllerBase
    {
        DatabaseManager dbHelperBenutzer;
        public SpieleController()
        {
            dbHelperBenutzer = new DatabaseManager();
        }
        [HttpPost]
        [Route("creategame")]
        public IActionResult CreateGame([FromBody] Spiel spiel)
        {
            try
            {
                dbHelperBenutzer.CreateGame(spiel);
                return Ok("Ja");
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }
        }
    }
}
