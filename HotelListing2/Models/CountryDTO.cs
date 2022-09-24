using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing2.Models
{
    //user interact here and database never see this instead database work with the Country class
    
    public class CreateCountryDTO
    {
        [Required]
        [StringLength(maximumLength: 50, ErrorMessage = "Country name is too long")]
        public string Name { get; set; }
        
        [Required]
        [StringLength(maximumLength: 4, ErrorMessage = "Short country name is too long")]
        public string ShortName { get; set; }
    }

        public class UpdateCountryDTO : CreateCountryDTO
        {
        public IList<CreateHotelDTO> Hotels { get; set; }
        }

    public class CountryDTO : CreateCountryDTO
    {
        public int Id { get; set; }
        public IList<CreateHotelDTO> Hotels { get; set; }
    }
}
