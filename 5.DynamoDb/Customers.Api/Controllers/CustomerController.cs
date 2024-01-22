using Customers.Api.Contracts.Requests;
using Customers.Api.Mapping;
using Customers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Customers.Api.Controllers;

[ApiController]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost("customers")]
    public async Task<IActionResult> Create([FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = request.ToCustomer();

        await _customerService.CreateAsync(customer, cancellationToken);

        var customerResponse = customer.ToCustomerResponse();

        return CreatedAtAction("Get", new { customerResponse.Id }, customerResponse);
    }

    [HttpGet("customers/{id:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetAsync(id, cancellationToken);

        if (customer is null)
        {
            return NotFound();
        }

        var customerResponse = customer.ToCustomerResponse();
        return Ok(customerResponse);
    }
    
    [HttpGet("customers")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(cancellationToken);
        var customersResponse = customers.ToCustomersResponse();
        return Ok(customersResponse);
    }
    
    [HttpPut("customers/{id:guid}")]
    public async Task<IActionResult> Update([FromMultiSource] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        DateTime requestStarted = DateTime.UtcNow;
        var existingCustomer = await _customerService.GetAsync(request.Id, cancellationToken);

        if (existingCustomer is null)
        {
            return NotFound();
        }

        var customer = request.ToCustomer();
        await _customerService.UpdateAsync(customer, requestStarted, cancellationToken);

        var customerResponse = customer.ToCustomerResponse();
        return Ok(customerResponse);
    }
    
    [HttpDelete("customers/{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _customerService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}
