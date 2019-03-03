using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreWeb.Controllers
{
    [Authorize(Policy = "GeoAccess")]
    [Route("v1/[controller]")]
    public class GeographyController
    {
        [HttpGet("countries")]
        public List<Country.CountryRegion> GetCountries()
        {
            return Country.GetCountries();
        }
    }
}
