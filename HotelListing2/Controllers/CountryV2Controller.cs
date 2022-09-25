using AutoMapper;
using HotelListing2.Data;
using HotelListing2.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing2.Controllers
{
    [ApiVersion("2.0",Deprecated = true)]//it's deprecated because we have a newer version it's not the most preffered anymore it's a supported or deprecated
    // [Route("api/{v:apiversion}/country")]
    [Route("api/country")]
    [ApiController]
    public class CountryV2Controller : ControllerBase
    {
        private DatabaseContext _context;

        public CountryV2Controller(DatabaseContext context)
        {
          _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountries()
        { 
            return Ok(_context.Countries);
        }

    }
}
