using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PrviLabos.DAL;
using PrviLabos.Model;
using PrviLabos.Models;
using Xunit;

namespace PrviLabos.Tests;

public sealed class ApiCrudTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiCrudTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "api-admin");
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "Admin,Manager");
    }

    [Fact]
    public async Task CustomersApi_ShouldSupportCrudAndValidation()
    {
        var invalid = await _client.PostAsJsonAsync("/api/customers", new CustomerUpsertDto());
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var created = await _client.PostAsJsonAsync("/api/customers", NewCustomer("api.customer@example.test"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var dto = await created.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.NotNull(dto);

        var byId = await _client.GetAsync($"/api/customers/{dto!.Id}");
        Assert.Equal(HttpStatusCode.OK, byId.StatusCode);

        var update = NewCustomer("api.customer.updated@example.test");
        update.FirstName = "Updated";
        var updated = await _client.PutAsJsonAsync($"/api/customers/{dto.Id}", update);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var missing = await _client.GetAsync("/api/customers/999999");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var deleted = await _client.DeleteAsync($"/api/customers/{dto.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task LocationsApi_ShouldSupportCrud()
    {
        var created = await _client.PostAsJsonAsync("/api/locations", NewLocation("Api Branch"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var dto = await created.Content.ReadFromJsonAsync<LocationDto>();
        Assert.NotNull(dto);

        var all = await _client.GetAsync("/api/locations?query=Api");
        Assert.Equal(HttpStatusCode.OK, all.StatusCode);

        var update = NewLocation("Updated Branch");
        var updated = await _client.PutAsJsonAsync($"/api/locations/{dto!.Id}", update);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/locations/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/locations/{dto.Id}")).StatusCode);
    }

    [Fact]
    public async Task VehiclesApi_ShouldSupportCrud()
    {
        var locationId = await CreateLocation();
        var created = await _client.PostAsJsonAsync("/api/vehicles", NewVehicle(locationId));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var dto = await created.Content.ReadFromJsonAsync<VehicleDto>();
        Assert.NotNull(dto);

        var update = NewVehicle(locationId);
        update.Model = "Octavia";
        Assert.Equal(HttpStatusCode.OK, (await _client.PutAsJsonAsync($"/api/vehicles/{dto!.Id}", update)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/vehicles/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/vehicles/{dto.Id}")).StatusCode);
    }

    [Fact]
    public async Task BookingsApi_ShouldSupportCrud()
    {
        var (customerId, vehicleId, pickupLocationId, dropoffLocationId) = await CreateBookingDependencies();

        var created = await _client.PostAsJsonAsync("/api/bookings", NewBooking(customerId, vehicleId, pickupLocationId, dropoffLocationId));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var dto = await created.Content.ReadFromJsonAsync<BookingDto>();
        Assert.NotNull(dto);

        var update = NewBooking(customerId, vehicleId, pickupLocationId, dropoffLocationId);
        update.TotalPrice = 199;
        Assert.Equal(HttpStatusCode.OK, (await _client.PutAsJsonAsync($"/api/bookings/{dto!.Id}", update)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/bookings/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/bookings/{dto.Id}")).StatusCode);
    }

    [Fact]
    public async Task AgentsApi_ShouldSupportCrud()
    {
        var created = await _client.PostAsJsonAsync("/api/agents", NewAgent("API Agent"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var dto = await created.Content.ReadFromJsonAsync<SupportAgentDto>();
        Assert.NotNull(dto);

        var update = NewAgent("Updated Agent");
        Assert.Equal(HttpStatusCode.OK, (await _client.PutAsJsonAsync($"/api/agents/{dto!.Id}", update)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/agents/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/agents/{dto.Id}")).StatusCode);
    }

    [Fact]
    public async Task TicketsApi_ShouldSupportCrud()
    {
        var (customerId, vehicleId, pickupLocationId, dropoffLocationId) = await CreateBookingDependencies();
        var bookingId = await CreateBooking(customerId, vehicleId, pickupLocationId, dropoffLocationId);
        var agentId = await CreateAgent();

        var created = await _client.PostAsJsonAsync("/api/tickets", NewTicket(bookingId, dropoffLocationId, agentId));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var dto = await created.Content.ReadFromJsonAsync<SupportTicketDto>();
        Assert.NotNull(dto);

        var update = NewTicket(bookingId, dropoffLocationId, agentId);
        update.Description = "Updated ticket";
        Assert.Equal(HttpStatusCode.OK, (await _client.PutAsJsonAsync($"/api/tickets/{dto!.Id}", update)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/tickets/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/tickets/{dto.Id}")).StatusCode);
    }

    [Fact]
    public async Task BookingAttachmentsApi_ShouldSupportCrudAndValidation()
    {
        var (customerId, vehicleId, pickupLocationId, dropoffLocationId) = await CreateBookingDependencies();
        var bookingId = await CreateBooking(customerId, vehicleId, pickupLocationId, dropoffLocationId);

        var invalid = await _client.PostAsJsonAsync("/api/booking-attachments", new BookingAttachmentUpsertDto());
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var created = await _client.PostAsJsonAsync("/api/booking-attachments", NewAttachment(bookingId));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var dto = await created.Content.ReadFromJsonAsync<BookingAttachmentDto>();
        Assert.NotNull(dto);
        Assert.Equal(bookingId, dto!.Booking.Id);

        var all = await _client.GetAsync($"/api/booking-attachments?bookingId={bookingId}&query=contract");
        Assert.Equal(HttpStatusCode.OK, all.StatusCode);

        var byId = await _client.GetAsync($"/api/booking-attachments/{dto.Id}");
        Assert.Equal(HttpStatusCode.OK, byId.StatusCode);

        var update = NewAttachment(bookingId);
        update.OriginalFileName = "updated-contract.pdf";
        Assert.Equal(HttpStatusCode.OK, (await _client.PutAsJsonAsync($"/api/booking-attachments/{dto.Id}", update)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/booking-attachments/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/booking-attachments/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/booking-attachments/{dto.Id}")).StatusCode);
    }

    [Theory]
    [InlineData("/api/customers")]
    [InlineData("/api/locations")]
    [InlineData("/api/vehicles")]
    [InlineData("/api/bookings")]
    [InlineData("/api/agents")]
    [InlineData("/api/tickets")]
    [InlineData("/api/booking-attachments")]
    public async Task ApiWriteEndpoints_ShouldRejectAnonymousUsers(string endpoint)
    {
        using var anonymousClient = _factory.CreateClient();

        var post = await anonymousClient.PostAsJsonAsync(endpoint, new { });
        Assert.Equal(HttpStatusCode.Unauthorized, post.StatusCode);

        var put = await anonymousClient.PutAsJsonAsync($"{endpoint}/1", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, put.StatusCode);

        var delete = await anonymousClient.DeleteAsync($"{endpoint}/1");
        Assert.Equal(HttpStatusCode.Unauthorized, delete.StatusCode);
    }

    [Fact]
    public async Task ApiDeleteEndpoints_ShouldRejectManagerWithoutAdminRole()
    {
        using var managerClient = _factory.CreateClient();
        managerClient.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "api-manager");
        managerClient.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, "Manager");

        var response = await managerClient.DeleteAsync("/api/customers/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static CustomerUpsertDto NewCustomer(string email) => new()
    {
        FirstName = "Api",
        LastName = "Customer",
        Email = email,
        PhoneNumber = "+385991234567",
        DriverLicenseNumber = "AB1234567",
        DateOfBirth = new DateOnly(1990, 1, 1)
    };

    private static LocationUpsertDto NewLocation(string name) => new()
    {
        Name = name,
        City = "Zagreb",
        Address = "Test Street 1",
        ContactPhone = "+385991234567",
        OpenAt = new TimeOnly(8, 0),
        CloseAt = new TimeOnly(18, 0),
        ParkingCapacity = 10
    };

    private static VehicleUpsertDto NewVehicle(int locationId) => new()
    {
        PlateNumber = $"ZG{Random.Shared.Next(1000, 9999)}API",
        Brand = "Skoda",
        Model = "Fabia",
        ProductionYear = 2022,
        Category = VehicleCategory.Economy,
        DailyRate = 55,
        CurrentMileage = 12000,
        IsAvailable = true,
        CurrentLocationId = locationId
    };

    private static BookingUpsertDto NewBooking(int customerId, int vehicleId, int pickupLocationId, int dropoffLocationId) => new()
    {
        ReservationCode = $"API-{Guid.NewGuid():N}"[..12],
        CustomerId = customerId,
        VehicleId = vehicleId,
        PickupLocationId = pickupLocationId,
        PlannedDropoffLocationId = dropoffLocationId,
        PickupAt = DateTime.UtcNow.AddDays(1),
        PlannedDropoffAt = DateTime.UtcNow.AddDays(2),
        TotalPrice = 120,
        Status = BookingStatus.Reserved
    };

    private static SupportAgentUpsertDto NewAgent(string fullName) => new()
    {
        FullName = fullName,
        Email = $"{Guid.NewGuid():N}@example.test",
        TeamName = "API",
        ShiftStart = DateTime.UtcNow.AddHours(1),
        ShiftEnd = DateTime.UtcNow.AddHours(9),
        IsOnDuty = true
    };

    private static SupportTicketUpsertDto NewTicket(int bookingId, int locationId, int agentId) => new()
    {
        TicketNumber = $"T-{Guid.NewGuid():N}"[..12],
        BookingId = bookingId,
        RequestedDropoffLocationId = locationId,
        Description = "API ticket",
        Priority = TicketPriority.Medium,
        Status = TicketStatus.Open,
        AssignedAgentIds = new List<int> { agentId }
    };

    private static BookingAttachmentUpsertDto NewAttachment(int bookingId) => new()
    {
        BookingId = bookingId,
        OriginalFileName = "contract.pdf",
        StoredFileName = $"{Guid.NewGuid():N}.pdf",
        FilePath = $"/uploads/bookings/{bookingId}/{Guid.NewGuid():N}.pdf",
        ContentType = "application/pdf",
        FileSize = 1024
    };

    private async Task<int> CreateLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/locations", NewLocation($"Location {Guid.NewGuid():N}"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LocationDto>())!.Id;
    }

    private async Task<int> CreateCustomer()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", NewCustomer($"{Guid.NewGuid():N}@example.test"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!.Id;
    }

    private async Task<int> CreateVehicle(int locationId)
    {
        var response = await _client.PostAsJsonAsync("/api/vehicles", NewVehicle(locationId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VehicleDto>())!.Id;
    }

    private async Task<int> CreateAgent()
    {
        var response = await _client.PostAsJsonAsync("/api/agents", NewAgent("Ticket Agent"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupportAgentDto>())!.Id;
    }

    private async Task<int> CreateBooking(int customerId, int vehicleId, int pickupLocationId, int dropoffLocationId)
    {
        var response = await _client.PostAsJsonAsync("/api/bookings", NewBooking(customerId, vehicleId, pickupLocationId, dropoffLocationId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BookingDto>())!.Id;
    }

    private async Task<(int CustomerId, int VehicleId, int PickupLocationId, int DropoffLocationId)> CreateBookingDependencies()
    {
        var pickupLocationId = await CreateLocation();
        var dropoffLocationId = await CreateLocation();
        var customerId = await CreateCustomer();
        var vehicleId = await CreateVehicle(pickupLocationId);

        return (customerId, vehicleId, pickupLocationId, dropoffLocationId);
    }
}
