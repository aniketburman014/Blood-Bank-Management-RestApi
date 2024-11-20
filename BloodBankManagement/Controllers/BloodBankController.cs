using Microsoft.AspNetCore.Mvc;

namespace BloodBankManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodBankController : ControllerBase
    {
        //creating a list for storing entries 
        private static List<BloodBank> bloodEntries = new List<BloodBank>();

        // GET: api/bloodbank
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(bloodEntries);
        }

        // GET: api/bloodbank/paginated?page={pageNumber}&size={pageSize}
        [HttpGet("paginated")]
        public IActionResult GetPaginated([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            if (page <= 0 || size <= 0)
                return BadRequest("Page number and size must be greater than 0.");

            var paginatedEntries = bloodEntries
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return Ok(paginatedEntries);
        }

        // GET: api/bloodbank/{id}
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var en = bloodEntries.FirstOrDefault(e => e.Id == id);

            if (en == null)
                return NotFound($"No entry found with ID {id}.");

            return Ok(en);
        }

        // POST: api/bloodbank
        [HttpPost]
        public IActionResult Post([FromBody] BloodBank value)
        {
            

            //checking for null values 
            if (string.IsNullOrWhiteSpace(value.DonorName))
                return BadRequest("DonorName cannot be  empty.");
            if (string.IsNullOrWhiteSpace(value.BloodType))
                return BadRequest("BloodType cannot be  empty.");
            if (string.IsNullOrWhiteSpace(value.ContactInfo))
                return BadRequest("ContactInfo cannot be  empty.");
            if (value.Quantity <= 0)
                return BadRequest("Quantity must be greater than 0.");
            if (value.CollectionDate == default)
                return BadRequest("CollectionDate cannot be null or default.");
            if (value.ExpirationDate == default)
                return BadRequest("ExpirationDate cannot be null or default.");
            if (string.IsNullOrWhiteSpace(value.Status))
                return BadRequest("Status cannot be  empty.");

            //generate guid
            value.Id = Guid.NewGuid();

            bloodEntries.Add(value);
            //returning the response
            return CreatedAtAction(nameof(Get), new { id = value.Id }, value);
        }


        // PUT: api/bloodbank/{id}
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] BloodBank updatedEntry)
        {
            if (updatedEntry == null)
                return BadRequest("Invalid input.");

            var index = bloodEntries.FindIndex(e => e.Id == id);

            if (index == -1)
                return NotFound($"No entry found with ID {id}.");

            //not changing id
            updatedEntry.Id = id;
            bloodEntries[index] = updatedEntry;

            return Ok(new { message = "Blood bank entry updated successfully." });
        }

        // DELETE: api/bloodbank/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var entry = bloodEntries.FirstOrDefault(e => e.Id == id);

            if (entry == null)
                return NotFound($"No entry found with ID {id}.");

            bloodEntries.Remove(entry);
            return Ok(new { message = "Blood bank entry deleted successfully." });
        }

        // GET: api/bloodbank/search
        [HttpGet("search")]
        public IActionResult Search([FromQuery] string? bloodType, [FromQuery] string? status, [FromQuery] string? donorName)
        {
            var results = bloodEntries.AsQueryable();
            // removing the not required blood object from the results
            if (!string.IsNullOrEmpty(bloodType))
                results = results.Where(e => e.BloodType.Equals(bloodType, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(status))
                results = results.Where(e => e.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(donorName))
                results = results.Where(e => e.DonorName.Contains(donorName, StringComparison.OrdinalIgnoreCase));

            var filteredResults = results.ToList();

            if (!filteredResults.Any())
                return NotFound("No entries found matching the search criteria.");

            return Ok(filteredResults);
        }
    }
}