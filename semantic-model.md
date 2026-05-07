# Semantic Model

Sažetak modela i relacija za EF Core bazu u projektu `PrviLabos`.

## Entiteti i tablice

### Customer -> `Customers`
- `Id`
- `FirstName`
- `LastName`
- `Email`
- `PhoneNumber`
- `DriverLicenseNumber`
- `DateOfBirth`
- `RegisteredAt`

### Location -> `Locations`
- `Id`
- `Name`
- `City`
- `Address`
- `ContactPhone`
- `OpenAt`
- `CloseAt`
- `ParkingCapacity`

### Vehicle -> `Vehicles`
- `Id`
- `PlateNumber`
- `Brand`
- `Model`
- `ProductionYear`
- `Category`
- `DailyRate`
- `CurrentMileage`
- `IsAvailable`
- `CurrentLocationId`

### Booking -> `Bookings`
- `Id`
- `ReservationCode`
- `CustomerId`
- `VehicleId`
- `PickupLocationId`
- `PlannedDropoffLocationId`
- `PickupAt`
- `PlannedDropoffAt`
- `ActualDropoffAt`
- `TotalPrice`
- `Status`

### SupportAgent -> `Agents`
- `Id`
- `FullName`
- `Email`
- `TeamName`
- `ShiftStart`
- `ShiftEnd`
- `IsOnDuty`

### SupportTicket -> `Tickets`
- `Id`
- `TicketNumber`
- `BookingId`
- `RequestedDropoffLocationId`
- `Description`
- `CreatedAt`
- `ResolvedAt`
- `Priority`
- `Status`

### Vrijednosni tipovi (enumovi)
- `VehicleCategory`
- `BookingStatus`
- `TicketPriority`
- `TicketStatus`

## Veze među tablicama

- `Customer` 1:N `Booking` preko `Booking.CustomerId`
- `Vehicle` 1:N `Booking` preko `Booking.VehicleId`
- `Location` 1:N `Vehicle` preko `Vehicle.CurrentLocationId`
- `Location` 1:N `Booking` kao pickup lokacija preko `Booking.PickupLocationId`
- `Location` 1:N `Booking` kao planned dropoff lokacija preko `Booking.PlannedDropoffLocationId`
- `Booking` 1:N `SupportTicket` preko `SupportTicket.BookingId`
- `Location` 1:N `SupportTicket` kao requested dropoff lokacija preko `SupportTicket.RequestedDropoffLocationId`
- `SupportAgent` N:N `SupportTicket` preko implicitne spojne tablice `SupportAgentSupportTicket`

## Kratki pregled

- Glavne poslovne cjeline su korisnici, vozila, lokacije, rezervacije i podrška.
- `Booking` povezuje korisnika, vozilo i dvije lokacije.
- `SupportTicket` je vezan uz rezervaciju i lokaciju, a može biti dodijeljen više agenata.
