using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;
List<Customer> customers = new List<Customer> 
{ 
    new Customer 
    { 
        Id = 1, 
        Name = "Alice Johnson", 
        Address = "123 Maple St, Springfield, IL, 62701" 
    },
    new Customer 
    { 
        Id = 2, 
        Name = "Bob Smith", 
        Address = "456 Oak Ave, Springfield, IL, 62702" 
    },
    new Customer 
    { 
        Id = 3, 
        Name = "Charlie Brown", 
        Address = "789 Pine Ln, Springfield, IL, 62703" 
    }
};
List<Employee> employees = new List<Employee> 
{
    new Employee 
    { 
        Id = 1, 
        Name = "Eve Adams", 
        Specialty = "Technical Support" 
    },
    new Employee 
    { 
        Id = 2, 
        Name = "John Doe", 
        Specialty = "Field Technician" 
    }
};
List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket 
    { 
        Id = 1, 
        CustomerId = 1, 
        EmployeeId = 1, 
        Description = "Issue with login", 
        Emergency = true, 
        DateCompleted = null 
    },
    new ServiceTicket 
    { 
        Id = 2, 
        CustomerId = 2, 
        EmployeeId = null, 
        Description = "Server downtime", 
        Emergency = false, 
        DateCompleted = null 
    },
    new ServiceTicket 
    { 
        Id = 3, 
        CustomerId = 3, 
        EmployeeId = 2, 
        Description = "Network issue", 
        Emergency = true, 
        DateCompleted = new DateTime(2024, 8, 25) 
    },
    new ServiceTicket 
    { 
        Id = 4, 
        CustomerId = 1, 
        EmployeeId = 1, 
        Description = "Software installation", 
        Emergency = false, 
        DateCompleted = new DateTime(2024, 8, 26) 
    },
    new ServiceTicket 
    { 
        Id = 5, 
        CustomerId = 2, 
        EmployeeId = null, 
        Description = "Database backup", 
        Emergency = false, 
        DateCompleted = null 
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/servicetickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted
    });
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    Employee employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
  
    return Results.Ok(new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        EmployeeId = serviceTicket.EmployeeId,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted
    });
});

app.MapGet("/employees", () => 
{
    return employees.Select(e => new EmployeeDTO 
    {
        Id = e.Id,
        Name = e.Name,
        Specialty = e.Specialty
    });
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }

    List<ServiceTicket> tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id, 
        Name = employee.Name, 
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    }); 
});

app.MapGet("/customers", () => 
{
    return customers.Select(c => new CustomerDTO 
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address
    });
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    
    List<ServiceTicket> tickets = serviceTickets.Where(st => st.CustomerId == id).ToList();

    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{

    // Get the customer data to check that the customerid for the service ticket is valid
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    // if the client did not provide a valid customer id, this is a bad request
    if (customer == null)
    {
        return Results.BadRequest();
    }

    // creates a new id (SQL will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);

    // Created returns a 201 status code with a link in the headers to where the new resource can be accessed
    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });

});

app.MapDelete("servicetickets/{id}", (int id) =>
{
    ServiceTicket ticket = serviceTickets.FirstOrDefault(t => t.Id == id);
    if (ticket == null)
    {
        return Results.BadRequest();
    }
    serviceTickets.Remove(ticket);
    return Results.NoContent();
});

app.Run();
