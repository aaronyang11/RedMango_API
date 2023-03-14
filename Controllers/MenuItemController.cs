using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RedMango_API.Data;
using RedMango_API.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RedMango_API.Controllers
{
    [Route("api/MenuItem")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private ApiResponse _response;

        public MenuItemController(ApplicationDbContext db)
        {
            _db = db;
            _response = new ApiResponse();
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            _response.Result = _db.MenuItems;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            if(id == 0)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            MenuItem menuItem = _db.MenuItems.FirstOrDefault(u => u.Id == id);
            if (menuItem == null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            _response.Result = menuItem;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }
    }
}

