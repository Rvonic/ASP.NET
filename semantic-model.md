# Semantic Model

Sažetak semantičkog DB modela za projekt PrviLabos.

## Entiteti i tablice

### Customer / Customers
Glavna svojstva:
- Id
- FirstName
- LastName
- Email
- PhoneNumber
- DriverLicenseNumber
- DateOfBirth
- RegisteredAt

### Location / Locations
Glavna svojstva:
- Id
- Name
- City
- Address
- ContactPhone
- OpenAt
- CloseAt
- ParkingCapacity

### Vehicle / Vehicles
Glavna svojstva:
- Id
- PlateNumber
- Brand
- Model
- ProductionYear
- Category
- DailyRate
- CurrentMileage
- IsAvailable
- CurrentLocationId

### Booking / Bookings
Glavna svojstva:
- Id
- ReservationCode
- CustomerId
- VehicleId
- PickupLocationId
- PlannedDropoffLocationId
- PickupAt
- PlannedDropoffAt
- ActualDropoffAt
- TotalPrice
- Status

### SupportAgent / Agents
Glavna svojstva:
- Id
- FullName
- Email
- TeamName
- ShiftStart
- ShiftEnd
- IsOnDuty

### SupportTicket / Tickets
Glavna svojstva:
- Id
- TicketNumber
- BookingId
- RequestedDropoffLocationId
- Description
- CreatedAt
- ResolvedAt
- Priority
- Status

### Enumovi
- VehicleCategory
- BookingStatus
- TicketPriority
- TicketStatus

## Veze među tablicama

- Customer 1:N Booking preko Booking.CustomerId
- Vehicle 1:N Booking preko Booking.VehicleId
- Location 1:N Vehicle preko Vehicle.CurrentLocationId; Vehicle 0..1:1 Location
- Location 1:N Booking preko Booking.PickupLocationId
- Location 1:N Booking preko Booking.PlannedDropoffLocationId
- Booking 1:N SupportTicket preko SupportTicket.BookingId
- Location 1:N SupportTicket preko SupportTicket.RequestedDropoffLocationId
- SupportAgent N:N SupportTicket preko spojne tablice SupportAgentSupportTicket

## Kratak opis modela

- Customer predstavlja klijenta koji može imati više rezervacija.
- Booking povezuje klijenta, vozilo i dvije lokacije te prati status rezervacije.
- Vehicle pripada jednoj lokaciji i može biti korišten u više rezervacija.
- SupportTicket prati zahtjeve vezane uz rezervaciju i lokaciju.
- SupportAgent može biti dodijeljen na više prijava, a prijava može imati više agenata.
