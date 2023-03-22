using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Controllers
{
    [Route("customers")]
    public class CustomerController : Controller
    {
        private readonly IRepository<Customer> customerRepository;
        public CustomerController(IRepository<Customer> customerRepository)
        {
            this.customerRepository = customerRepository;
        }
        [HttpGet("{id:long}")]
        public async Task <IActionResult> GetCustomerAsync([FromRoute] long id)
        {
            //ResponseType type = ResponseType.Success;
            var user = await customerRepository.GetAsync(id);
            if (user == null)
            {
                return NotFound("Customer not found!");
            }
            return  Ok(user);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateCustomerAsync([FromBody] Customer customer)
        {
            var user = await customerRepository.GetAsync(customer.Id);
            if (user != null)
            {                
                return StatusCode(409, "Customer already exists!");
            }
            await customerRepository.AddAsync(customer);
            return Ok(customer.Id);
        }
    }
}