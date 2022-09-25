using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing2.Models
{
    public class RequestParams
    {
        const int maxPageSize = 50;
        //default page number is 1
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;

        public int PageSize //the ' value ' keyword is whatever value will be set for PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }
}
