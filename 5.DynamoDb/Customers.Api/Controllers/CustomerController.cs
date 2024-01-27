using Customers.Api.Contracts.Requests;
using Customers.Api.Mapping;
using Customers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Customers.Api.Controllers;

[ApiController()]
[Route("customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = request.ToCustomer();

        await _customerService.CreateAsync(customer, cancellationToken);

        var customerResponse = customer.ToCustomerResponse();

        return CreatedAtAction("Get", new { customerResponse.Id }, customerResponse);
    }

    [HttpGet("{id:guid}")]
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

    [HttpGet("GetByEmail/{email}")]
    public async Task<IActionResult> GetByEmail([FromRoute] string email, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByEmail(email, cancellationToken);

        if (customer is null)
        {
            return NotFound();
        }

        var customerResponse = customer.ToCustomerResponse();
        return Ok(customerResponse);
    }

    [HttpGet()]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(cancellationToken);
        var customersResponse = customers.ToCustomersResponse();
        return Ok(customersResponse);
    }
    
    [HttpPut("{id:guid}")]
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
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _customerService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost("batch")]
    public async Task<IActionResult> Batch([FromBody] List<CustomerBatchRequest> batch, CancellationToken cancellationToken)
    {
        var success = await _customerService.ProcessCustomerBatchAsync(batch, cancellationToken);
        if (!success) 
        {
            return StatusCode(500, "An internal server error occurred");
        }
        return Ok();
    }

    [HttpPost("Order")]
    public async Task<IActionResult> Order([FromBody] CustomerOrderRequest custOrderRequest, CancellationToken cancellationToken) 
    {
        var success = await _customerService.CreateOrderAsync(custOrderRequest, cancellationToken);
        if (!success)
        {
            return StatusCode(500, "An internal server error occurred");
        }
        return Ok();
    }
}
