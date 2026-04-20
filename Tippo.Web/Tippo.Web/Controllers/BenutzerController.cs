using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tippo.Web.Database;
using Tippo.Web.Models;

namespace WebServiceTippo.Controllers
{
    [Route("api/BenutzerController")]
    [ApiController]
    public class BenutzerController : ControllerBase
    {
        DatabaseManager dbHelperBenutzer;
        public BenutzerController()
        {
            dbHelperBenutzer = new DatabaseManager();
        }

        [HttpGet]
        [Route("test")]
        public IActionResult GetAll() => Ok(new string[] { "Prod1", "Prod2" });

        [HttpGet("{id}")]
        [Route("test2")]
        public IActionResult GetById(int id) => Ok($"Prod{id}");

        [HttpPost]
        [Route("createaccount")]
        public IActionResult CreateAccount([FromBody] Benutzer dtoBenutzer)
        {
            try
            {
                Benutzer benutzer = new Benutzer
                {
                    benutzername = dtoBenutzer.benutzername,
                    email = dtoBenutzer.email,
                    passwort_hash = dtoBenutzer.passwort_hash,
                    erstellungsdatum = dtoBenutzer.erstellungsdatum,
                    rollenid = dtoBenutzer.rollenid
                };
                dbHelperBenutzer.InsertBenutzer(benutzer);
                return Ok("JA");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
