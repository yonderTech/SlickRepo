using Microsoft.AspNetCore.Mvc;
using WebTest.Module;
using Dto = WebTest.Dtos;

namespace WebTest.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : Controller
    {
        private UserModule Module { get; set; }

        public UserController(UserModule module)
        {
            Module = module;
        }

        [HttpGet]
        [Route("Users")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await Module.GetAll());
        }

        [HttpGet]
        [Route("Users/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Module.GetById(id));
        }


        [HttpPost]
        [Route("Users")]
        public async Task<IActionResult> AddItem([FromBody] Dto.User user)
        {
            return Ok(await Module.Add(user));
        }


        [HttpPut]
        [Route("Users")]
        public async Task<IActionResult> UpdateItem([FromBody] Dto.User user)
        {
            return Ok(await Module.Update(user));
        }

        [HttpDelete]
        [Route("Users/{id}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            await Module.Delete(id);
            return Ok();
        }
    }
}
