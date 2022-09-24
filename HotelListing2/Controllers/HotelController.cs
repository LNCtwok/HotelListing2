using AutoMapper;
using HotelListing2.Data;
using HotelListing2.IRepository;
using HotelListing2.Models;
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
    public class HotelController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HotelController> _logger;
        private readonly IMapper _mapper;

        public HotelController(IUnitOfWork unitOfWork, ILogger<HotelController> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotels()
        {
            try
            {
                var hotels = await _unitOfWork.Hotels.GetAll();
                var results = _mapper.Map<IList<HotelDTO>>(hotels);
                return Ok(results);

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Something went wrong {nameof(GetHotels)}");
                return StatusCode(500, "Internal server error please try again later");
            }
        }
       
        [HttpGet("{id:int}", Name = "GetHotel" )]//this is your name when a sibling calls you
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotel(int id)
        {
            try
            {
                var hotel = await _unitOfWork.Hotels.Get(q => q.Id == id, new List<string> {"Country"});
                var result = _mapper.Map<HotelDTO>(hotel);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Something went wrong{nameof(GetHotel)}");
                return StatusCode(500, "Internal server error please try again later");
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDTO hotelDTO)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid post attempt in {nameof(CreateHotel)}");
                return BadRequest(ModelState);
            }
            try
            {
                var hotel = _mapper.Map<Hotel>(hotelDTO);
                await _unitOfWork.Hotels.Insert(hotel);
                await _unitOfWork.Save();

                return CreatedAtRoute("GetHotel",new { id = hotel.Id }, hotel);//returns the created object for the client
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Something went wrong{nameof(CreateHotel)}");
                return StatusCode(500, "Internal server error please try again later");
            }
        }

        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateHotle(int id, [FromBody] UpdateHotelDTO hotelDTO)
        {
            if(!ModelState.IsValid || id < 1)
            {
                _logger.LogError($"Invalid update attempt in {nameof(UpdateHotle)}");
                return BadRequest(ModelState);
            }
            try
            {
                var hotel = await _unitOfWork.Hotels.Get(q => q.Id == id);
                if(hotel == null)
                {
                    _logger.LogError($"Invalid update attempt in {nameof(UpdateHotle)}");
                    return BadRequest("Submitted data is invalid");
                }
                _mapper.Map(hotelDTO, hotel);//source, destination: so take the feilds from the source and if there is any unequaled data update the feilds and that means the missing feilds from the sorce will be put to null
                //because we're not tracking then we don't put a save in here
                _unitOfWork.Hotels.Update(hotel);
                await _unitOfWork.Save();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Something went wrong in the {nameof(UpdateHotle)}");
                return StatusCode(500, "Internal server error please try again later");
            }
        }

        [Authorize(Roles ="Administrator")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            if (id < 1)
            {
                _logger.LogError($"Invalid Delete attempt in {nameof(DeleteHotel)}");
                return BadRequest("Submitted data is invalid");
            }

            try
            {
                var hotel =await _unitOfWork.Hotels.Get(q => q.Id == id);
                if (hotel == null)
                {
                    _logger.LogError($"Invalid delete attempt in {nameof(DeleteHotel)}");
                    return BadRequest("Submitted data is invalid");
                }
                await _unitOfWork.Hotels.Delete(id);
                await _unitOfWork.Save();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Something went wrong in the {nameof(DeleteHotel)}");
                return StatusCode(500, "Something went wrong please try again later");
            }
        }
    }
}
