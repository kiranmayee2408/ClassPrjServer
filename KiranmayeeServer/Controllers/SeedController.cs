using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KiranModel;
using CsvHelper.Configuration;
using System.Globalization;
using KiranmayeeServer.Data;
using CsvHelper;
using Microsoft.AspNetCore.Identity;

namespace KiranmayeeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(WorldcitiesSourceContext context, IHostEnvironment environment, 
        UserManager <WorldCitiesUser> userManager) : ControllerBase
    {
        String _pathName = Path.Combine(environment.ContentRootPath, "Data/worldcities.csv");
        
        [HttpPost("Users")]
        public async Task ImportUsersAsync()
        {
            WorldCitiesUser user = new()
            {
                UserName = "user",
                Email = "user@gmail.com",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            IdentityResult x = await userManager.CreateAsync(user, "Passw0rd!");
            int y = await context.SaveChangesAsync();
        }

        [HttpPost("Countries")]
        public async Task<ActionResult> ImportcountriesAsync()
        {
            // create a lookup dictionary containing all the countries already existing 
            // into the Database (it will be empty on first run).
            Dictionary<string, Country> countriesByName = context.Countries
                .AsNoTracking().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null
            };

            using StreamReader reader = new(_pathName);
            using CsvReader csv = new(reader, config);

            List<WorldcitiesDtos> records = csv.GetRecords<WorldcitiesDtos>().ToList();
            foreach (WorldcitiesDtos record in records)
            {
                if (countriesByName.ContainsKey(record.country))
                {
                    continue;
                }

                Country country = new()
                {
                    Name = record.country,
                    Iso2 = record.iso2,
                    Iso3 = record.iso3
                };
                await context.Countries.AddAsync(country);
                countriesByName.Add(record.country, country);
            }

            await context.SaveChangesAsync();

            return new JsonResult(countriesByName.Count);

        }
        [HttpPost("Cities")]
        public async Task<ActionResult> ImportcitiesAsync()
        {
            Dictionary<string, Country> countries = await context.Countries//.AsNoTracking()
             .ToDictionaryAsync(c => c.Name);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null
            };
            int cityCount = 0;
            using (StreamReader reader = new(_pathName))
            using (CsvReader csv = new(reader, config))
            {
                IEnumerable<WorldcitiesDtos>? records = csv.GetRecords<WorldcitiesDtos>();
                foreach (WorldcitiesDtos record in records)
                {
                    if (!countries.TryGetValue(record.country, out Country? value))
                    {
                        Console.WriteLine($"Not found country for {record.city}");
                        return NotFound(record);
                    }

                    if (!record.population.HasValue || string.IsNullOrEmpty(record.city_ascii))
                    {
                        Console.WriteLine($"Skipping {record.city}");
                        continue;
                    }
                    City city = new()
                    {
                        Name = record.city,
                        Lat = record.lat,
                        Lon = record.lng,
                        Population = (int)record.population.Value,
                        CountryId = value.Id
                    };
                    context.Cities.Add(city);
                    cityCount++;
                }
                await context.SaveChangesAsync();
            }
            return new JsonResult(cityCount);
        }

    }
}
