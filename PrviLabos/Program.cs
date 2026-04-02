using PrviLabos.Data;
using PrviLabos.Services;

var context = SupportDataSeeder.CreateSeededContext();
var service = new DropoffSupportService(context);

Console.WriteLine("=== DROP-OFF RENT-A-CAR SUPPORT (BEZ UI) ===");
Console.WriteLine();

var lateDropoffs = service.GetLateDropoffs(DateTime.UtcNow);
Console.WriteLine("Kasnjenja drop-offa:");
foreach (var booking in lateDropoffs)
{
    Console.WriteLine($"- {booking.ReservationCode} | Klijent: {booking.Customer.FirstName} {booking.Customer.LastName} | Plan: {booking.PlannedDropoffAt:u} | Lokacija: {booking.PlannedDropoffLocation.Name}");
}

Console.WriteLine();
Console.WriteLine("Aktivni eskalirani ticketi (High/Critical):");
foreach (var ticket in service.GetActiveEscalations())
{
    Console.WriteLine($"- {ticket.TicketNumber} | {ticket.Priority} | Status: {ticket.Status} | Booking: {ticket.Booking.ReservationCode}");
}

Console.WriteLine();
Console.WriteLine("Najtrazenije drop-off lokacije:");
foreach (var item in service.GetMostRequestedDropoffLocations())
{
    Console.WriteLine($"- {item.location}: {item.requests} zahtjeva");
}

Console.WriteLine();
Console.WriteLine("Top 3 korisnika po potrosnji:");
foreach (var customer in service.GetTopCustomersBySpending(3))
{
    Console.WriteLine($"- {customer.customer}: {customer.totalSpent} EUR");
}

Console.WriteLine();
Console.WriteLine("Pokretanje async obrade otvorenih ticketa...");
var resolvedCount = await service.ProcessOpenTicketsAsync();
Console.WriteLine($"Obrada zavrsena. Rijeseno ticketa: {resolvedCount}");

Console.WriteLine();
Console.WriteLine("Statusi nakon async obrade:");
foreach (var ticket in context.Tickets)
{
    Console.WriteLine($"- {ticket.TicketNumber}: {ticket.Status} | ResolvedAt: {ticket.ResolvedAt:u}");
}
