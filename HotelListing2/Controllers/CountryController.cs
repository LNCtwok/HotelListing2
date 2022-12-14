using AutoMapper;
using HotelListing2.Data;
using HotelListing2.IRepository;
using HotelListing2.Models;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CountryController> _logger;//it has to be injected to the context of the class that it has been injected into
        private readonly IMapper _mapper;

        public CountryController(IUnitOfWork unitOfWork, ILogger<CountryController> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        //[ResponseCache(Duration = 300)]//300 seconds means 5 minutes
        //[ResponseCache(CacheProfileName= "120SecondDuration")] there is no need for this after using Marvin.Cache.Headers which is setting the global setting in the serviceExtension
       /*  //local settings
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge=60)]
        [HttpCacheValidation(MustRevalidate = false)]
       */
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountries([FromQuery] RequestParams requestParams)
        { //throw new Exception(); // it looks like a break but with error message
           //if I have  a global error handling then ther is no need for the try catch
                var countries = await _unitOfWork.Countries.GetAll(requestParams);
                var results = _mapper.Map<IList<CountryDTO>>(countries);
                return Ok(results);
        }

        [HttpGet("{id:int}", Name = "GetCountry")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountry(int id)
        {
           var country = await _unitOfWork.Countries.Get(q => q.Id==id, new List<string> {"Hotels"});
                var result = _mapper.Map<CountryDTO>(country);
                return Ok(result);
         
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCountry([FromBody] CreateCountryDTO countryDTO)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid post attempt in {nameof(CreateCountry)}");
                return BadRequest(ModelState);
            }
               var country = _mapper.Map<Country>(countryDTO);
                await _unitOfWork.Countries.Insert(country);
                await _unitOfWork.Save();

                return CreatedAtRoute("GetCountry", new { id = country.Id }, country);//routeName,routeValue,value
              
        }
        
        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryDTO countryDTO)
        {
            if(!ModelState.IsValid || id < 0)
            {
                _logger.LogError($"Invalid update attempt in {nameof(UpdateCountry)}");
                return BadRequest(ModelState);
            }
             var country = await _unitOfWork.Countries.Get(q => q.Id == id);
                if(country == null)
                {
                    _logger.LogError($"Invalid update attempt in the {nameof(UpdateCountry)}");
                    return BadRequest("Submitted data is invalid");
                }
                 _mapper.Map(countryDTO, country);
                _unitOfWork.Countries.Update(country);
                
                await _unitOfWork.Save();

                return NoContent();
        }
        
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            if (id < 1)
            {
                _logger.LogError($"Invalid Delete attempt in {nameof(DeleteCountry)}");
                return BadRequest("Submitted data is invalid");
            }

             var country =await _unitOfWork.Countries.Get(q => q.Id == id);
                if (country == null)
                {
                    _logger.LogError($"Invalid delete attempt in {nameof(DeleteCountry)}");
                    return BadRequest("Submitted data is invalid");
                }
                await _unitOfWork.Countries.Delete(id);
                await _unitOfWork.Save();

                return NoContent();
        }
    }
}
