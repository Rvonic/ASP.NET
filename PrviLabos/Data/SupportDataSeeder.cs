using Microsoft.EntityFrameworkCore;
using PrviLabos.Model;

namespace PrviLabos.DAL;

public static class SupportDataSeeder
{
    public static async Task SeedAsync(PrviLabosDbContext context)
    {
        if (await context.Customers.AnyAsync())
        {
            return;
        }

        var seed = CreateSeededContext();

        var locations = seed.Locations.Select(location => new Location
        {
            Name = location.Name,
            City = location.City,
            Address = location.Address,
            ContactPhone = location.ContactPhone,
            OpenAt = location.OpenAt,
            CloseAt = location.CloseAt,
            ParkingCapacity = location.ParkingCapacity
        }).ToList();

        var customers = seed.Customers.Select(customer => new Customer
        {
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            DriverLicenseNumber = customer.DriverLicenseNumber,
            DateOfBirth = customer.DateOfBirth,
            RegisteredAt = customer.RegisteredAt
        }).ToList();

        var agents = seed.Agents.Select(agent => new SupportAgent
        {
            FullName = agent.FullName,
            Email = agent.Email,
            TeamName = agent.TeamName,
            ShiftStart = agent.ShiftStart,
            ShiftEnd = agent.ShiftEnd,
            IsOnDuty = agent.IsOnDuty
        }).ToList();

        var vehicles = seed.Vehicles.Select(vehicle => new Vehicle
        {
            PlateNumber = vehicle.PlateNumber,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            ProductionYear = vehicle.ProductionYear,
            Category = vehicle.Category,
            DailyRate = vehicle.DailyRate,
            CurrentMileage = vehicle.CurrentMileage,
            IsAvailable = vehicle.IsAvailable,
            CurrentLocation = locations.Single(location => location.Name == vehicle.CurrentLocation!.Name)
        }).ToList();

        var bookings = seed.Bookings.Select(booking => new Booking
        {
            ReservationCode = booking.ReservationCode,
            Customer = customers[booking.CustomerId - 1],
            Vehicle = vehicles[booking.VehicleId - 1],
            PickupLocation = locations[booking.PickupLocationId - 1],
            PlannedDropoffLocation = locations[booking.PlannedDropoffLocationId - 1],
            PickupAt = booking.PickupAt,
            PlannedDropoffAt = booking.PlannedDropoffAt,
            ActualDropoffAt = booking.ActualDropoffAt,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status
        }).ToList();

        var tickets = seed.Tickets.Select(ticket => new SupportTicket
        {
            TicketNumber = ticket.TicketNumber,
            Booking = bookings[ticket.BookingId - 1],
            RequestedDropoffLocation = locations[ticket.RequestedDropoffLocationId - 1],
            Description = ticket.Description,
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Priority = ticket.Priority,
            Status = ticket.Status
        }).ToList();

        bookings[0].SupportTickets.Add(tickets[0]);
        bookings[1].SupportTickets.Add(tickets[1]);
        bookings[2].SupportTickets.Add(tickets[2]);

        tickets[0].AssignedAgents.Add(agents[0]);
        tickets[0].AssignedAgents.Add(agents[1]);
        tickets[1].AssignedAgents.Add(agents[1]);
        tickets[2].AssignedAgents.Add(agents[0]);
        tickets[2].AssignedAgents.Add(agents[2]);

        foreach (var location in locations)
        {
            location.Vehicles = vehicles.Where(vehicle => vehicle.CurrentLocation == location).ToList();
            location.SupportTickets = tickets.Where(ticket => ticket.RequestedDropoffLocation == location).ToList();
        }

        foreach (var customer in customers)
        {
            customer.Bookings = bookings.Where(booking => booking.Customer == customer).ToList();
        }

        foreach (var vehicle in vehicles)
        {
            vehicle.Bookings = bookings.Where(booking => booking.Vehicle == vehicle).ToList();
        }

        foreach (var agent in agents)
        {
            agent.AssignedTickets = tickets.Where(ticket => ticket.AssignedAgents.Contains(agent)).ToList();
        }

        context.Locations.AddRange(locations);
        context.Customers.AddRange(customers);
        context.Agents.AddRange(agents);
        context.Vehicles.AddRange(vehicles);
        context.Bookings.AddRange(bookings);
        context.Tickets.AddRange(tickets);

        await context.SaveChangesAsync();
    }

    public static SupportDataContext CreateSeededContext()
    {
        var zagreb = new Location
        {
            Id = 1,
            Name = "Zagreb Airport",
            City = "Zagreb",
            Address = "Rudolfa Fizira 1",
            ContactPhone = "+385-1-1000-100",
            OpenAt = new TimeOnly(0, 0),
            CloseAt = new TimeOnly(23, 59),
            ParkingCapacity = 200
        };

        var split = new Location
        {
            Id = 2,
            Name = "Split Center",
            City = "Split",
            Address = "Obala Lazareta 3",
            ContactPhone = "+385-21-200-300",
            OpenAt = new TimeOnly(6, 0),
            CloseAt = new TimeOnly(22, 0),
            ParkingCapacity = 110
        };

        var rijeka = new Location
        {
            Id = 3,
            Name = "Rijeka Port",
            City = "Rijeka",
            Address = "Riva 8",
            ContactPhone = "+385-51-777-600",
            OpenAt = new TimeOnly(7, 0),
            CloseAt = new TimeOnly(21, 0),
            ParkingCapacity = 90
        };

        var customers = new List<Customer>
        {
            new()
            {
                Id = 1,
                FirstName = "Ana",
                LastName = "Klaric",
                Email = "ana.klaric@mail.com",
                PhoneNumber = "+385-91-111-222",
                DriverLicenseNumber = "HR-AN-12345",
                DateOfBirth = new DateTime(1997, 3, 12),
                RegisteredAt = new DateTime(2024, 6, 1)
            },
            new()
            {
                Id = 2,
                FirstName = "Ivan",
                LastName = "Maric",
                Email = "ivan.maric@mail.com",
                PhoneNumber = "+385-98-333-444",
                DriverLicenseNumber = "HR-IV-54321",
                DateOfBirth = new DateTime(1991, 11, 2),
                RegisteredAt = new DateTime(2023, 12, 20)
            },
            new()
            {
                Id = 3,
                FirstName = "Luka",
                LastName = "Novak",
                Email = "luka.novak@mail.com",
                PhoneNumber = "+385-95-555-666",
                DriverLicenseNumber = "HR-LU-88888",
                DateOfBirth = new DateTime(1988, 8, 8),
                RegisteredAt = new DateTime(2022, 9, 5)
            }
        };

        var vehicles = new List<Vehicle>
        {
            new()
            {
                Id = 1,
                PlateNumber = "ZG-101-AA",
                Brand = "Skoda",
                Model = "Octavia",
                ProductionYear = 2022,
                Category = VehicleCategory.Sedan,
                DailyRate = 58,
                CurrentMileage = 39200,
                IsAvailable = false,
                CurrentLocationId = zagreb.Id,
                CurrentLocation = zagreb
            },
            new()
            {
                Id = 2,
                PlateNumber = "ST-202-BB",
                Brand = "Volkswagen",
                Model = "T-Cross",
                ProductionYear = 2023,
                Category = VehicleCategory.Suv,
                DailyRate = 67,
                CurrentMileage = 21000,
                IsAvailable = false,
                CurrentLocationId = split.Id,
                CurrentLocation = split
            },
            new()
            {
                Id = 3,
                PlateNumber = "RI-303-CC",
                Brand = "Renault",
                Model = "Clio",
                ProductionYear = 2021,
                Category = VehicleCategory.Economy,
                DailyRate = 45,
                CurrentMileage = 61200,
                IsAvailable = false,
                CurrentLocationId = zagreb.Id,
                CurrentLocation = zagreb
            },
            new()
            {
                Id = 4,
                PlateNumber = "ZG-404-DD",
                Brand = "Mercedes",
                Model = "Vito",
                ProductionYear = 2024,
                Category = VehicleCategory.Van,
                DailyRate = 99,
                CurrentMileage = 13000,
                IsAvailable = true,
                CurrentLocationId = rijeka.Id,
                CurrentLocation = rijeka
            }
        };

        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1,
                ReservationCode = "BO-1001",
                CustomerId = customers[0].Id,
                Customer = customers[0],
                VehicleId = vehicles[0].Id,
                Vehicle = vehicles[0],
                PickupLocationId = zagreb.Id,
                PickupLocation = zagreb,
                PlannedDropoffLocationId = split.Id,
                PlannedDropoffLocation = split,
                PickupAt = DateTime.UtcNow.AddDays(-4),
                PlannedDropoffAt = DateTime.UtcNow.AddHours(-7),
                ActualDropoffAt = null,
                TotalPrice = 232,
                Status = BookingStatus.Active
            },
            new()
            {
                Id = 2,
                ReservationCode = "BO-1002",
                CustomerId = customers[1].Id,
                Customer = customers[1],
                VehicleId = vehicles[1].Id,
                Vehicle = vehicles[1],
                PickupLocationId = split.Id,
                PickupLocation = split,
                PlannedDropoffLocationId = rijeka.Id,
                PlannedDropoffLocation = rijeka,
                PickupAt = DateTime.UtcNow.AddDays(-3),
                PlannedDropoffAt = DateTime.UtcNow.AddHours(-1),
                ActualDropoffAt = null,
                TotalPrice = 201,
                Status = BookingStatus.Active
            },
            new()
            {
                Id = 3,
                ReservationCode = "BO-1003",
                CustomerId = customers[2].Id,
                Customer = customers[2],
                VehicleId = vehicles[2].Id,
                Vehicle = vehicles[2],
                PickupLocationId = zagreb.Id,
                PickupLocation = zagreb,
                PlannedDropoffLocationId = zagreb.Id,
                PlannedDropoffLocation = zagreb,
                PickupAt = DateTime.UtcNow.AddDays(-2),
                PlannedDropoffAt = DateTime.UtcNow.AddHours(5),
                ActualDropoffAt = null,
                TotalPrice = 135,
                Status = BookingStatus.Active
            }
        };

        var agents = new List<SupportAgent>
        {
            new()
            {
                Id = 1,
                FullName = "Petra Basic",
                Email = "petra.basic@dropoff.support",
                TeamName = "Escalations",
                ShiftStart = DateTime.UtcNow.Date.AddHours(7),
                ShiftEnd = DateTime.UtcNow.Date.AddHours(15),
                IsOnDuty = true
            },
            new()
            {
                Id = 2,
                FullName = "Marko Jelavic",
                Email = "marko.jelavic@dropoff.support",
                TeamName = "Operations",
                ShiftStart = DateTime.UtcNow.Date.AddHours(14),
                ShiftEnd = DateTime.UtcNow.Date.AddHours(22),
                IsOnDuty = true
            },
            new()
            {
                Id = 3,
                FullName = "Sara Vidic",
                Email = "sara.vidic@dropoff.support",
                TeamName = "Fleet",
                ShiftStart = DateTime.UtcNow.Date.AddHours(6),
                ShiftEnd = DateTime.UtcNow.Date.AddHours(14),
                IsOnDuty = false
            }
        };

        var tickets = new List<SupportTicket>
        {
            new()
            {
                Id = 1,
                TicketNumber = "TK-9001",
                BookingId = bookings[0].Id,
                Booking = bookings[0],
                RequestedDropoffLocationId = rijeka.Id,
                RequestedDropoffLocation = rijeka,
                Description = "Klijent trazi promjenu drop-off lokacije zbog promjene rute.",
                CreatedAt = DateTime.UtcNow.AddHours(-10),
                Priority = TicketPriority.Critical,
                Status = TicketStatus.Escalated
            },
            new()
            {
                Id = 2,
                TicketNumber = "TK-9002",
                BookingId = bookings[1].Id,
                Booking = bookings[1],
                RequestedDropoffLocationId = zagreb.Id,
                RequestedDropoffLocation = zagreb,
                Description = "Kasnjenje dolaska klijenta na drop-off i potreban novi termin.",
                CreatedAt = DateTime.UtcNow.AddHours(-4),
                Priority = TicketPriority.High,
                Status = TicketStatus.InProgress
            },
            new()
            {
                Id = 3,
                TicketNumber = "TK-9003",
                BookingId = bookings[2].Id,
                Booking = bookings[2],
                RequestedDropoffLocationId = split.Id,
                RequestedDropoffLocation = split,
                Description = "Upit za zamjensko vozilo tijekom finalizacije povrata.",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                Priority = TicketPriority.Medium,
                Status = TicketStatus.Open
            }
        };

        LinkRelations(customers, locations: new[] { zagreb, split, rijeka }, vehicles, bookings, agents, tickets);

        return new SupportDataContext
        {
            Customers = customers,
            Locations = [zagreb, split, rijeka],
            Vehicles = vehicles,
            Bookings = bookings,
            Agents = agents,
            Tickets = tickets
        };
    }

    private static void LinkRelations(
        List<Customer> customers,
        IEnumerable<Location> locations,
        List<Vehicle> vehicles,
        List<Booking> bookings,
        List<SupportAgent> agents,
        List<SupportTicket> tickets)
    {
        foreach (var location in locations)
        {
            location.Vehicles = vehicles.Where(v => v.CurrentLocation?.Id == location.Id).ToList();
            location.SupportTickets = tickets.Where(t => t.RequestedDropoffLocation.Id == location.Id).ToList();
        }

        foreach (var booking in bookings)
        {
            booking.Customer.Bookings.Add(booking);
            booking.Vehicle.Bookings.Add(booking);
        }

        bookings[0].SupportTickets.Add(tickets[0]);
        bookings[1].SupportTickets.Add(tickets[1]);
        bookings[2].SupportTickets.Add(tickets[2]);

        tickets[0].AssignedAgents.Add(agents[0]);
        tickets[0].AssignedAgents.Add(agents[1]);
        tickets[1].AssignedAgents.Add(agents[1]);
        tickets[2].AssignedAgents.Add(agents[0]);
        tickets[2].AssignedAgents.Add(agents[2]);

        foreach (var agent in agents)
        {
            agent.AssignedTickets = tickets.Where(t => t.AssignedAgents.Any(a => a.Id == agent.Id)).ToList();
        }
    }
}
