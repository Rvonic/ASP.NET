# Sitemap

Sažetak dostupnih URL-ova u aplikaciji. Svi pogledi koriste zajednički layout iz `Views/Shared/_Layout.cshtml`.

## HomeController

| URL | Action | View |
| --- | --- | --- |
| `/` | `Index` | `Views/Home/Index.cshtml` |
| `/naslovnica` | `Index` | `Views/Home/Index.cshtml` |
| `/Home/Index` | `Index` | `Views/Home/Index.cshtml` |
| `/Home/Error` | `Error` | `Views/Shared/Error.cshtml` |

## VehiclesController

| URL | Action | View |
| --- | --- | --- |
| `/vozila` | `Index` | `Views/Vehicles/Index.cshtml` |
| `/vozila/detalji/{id:int}` | `Details` | `Views/Vehicles/Details.cshtml` |
| `/Vehicles/Index` | `Index` | `Views/Vehicles/Index.cshtml` |
| `/Vehicles/Details/{id}` | `Details` | `Views/Vehicles/Details.cshtml` |

## CustomersController

| URL | Action | View |
| --- | --- | --- |
| `/kupci` | `Index` | `Views/Customers/Index.cshtml` |
| `/kupci/detalji/{id:int}` | `Details` | `Views/Customers/Details.cshtml` |
| `/Customers/Index` | `Index` | `Views/Customers/Index.cshtml` |
| `/Customers/Details/{id}` | `Details` | `Views/Customers/Details.cshtml` |

## LocationsController

| URL | Action | View |
| --- | --- | --- |
| `/lokacije` | `Index` | `Views/Locations/Index.cshtml` |
| `/lokacije/detalji/{id:int}` | `Details` | `Views/Locations/Details.cshtml` |
| `/Locations/Index` | `Index` | `Views/Locations/Index.cshtml` |
| `/Locations/Details/{id}` | `Details` | `Views/Locations/Details.cshtml` |

## BookingsController

| URL | Action | View |
| --- | --- | --- |
| `/rezervacije` | `Index` | `Views/Bookings/Index.cshtml` |
| `/rezervacije/detalji/{id:int}` | `Details` | `Views/Bookings/Details.cshtml` |
| `/Bookings/Index` | `Index` | `Views/Bookings/Index.cshtml` |
| `/Bookings/Details/{id}` | `Details` | `Views/Bookings/Details.cshtml` |

## TicketsController

| URL | Action | View |
| --- | --- | --- |
| `/prijave` | `Index` | `Views/Tickets/Index.cshtml` |
| `/prijave/detalji/{id:int}` | `Details` | `Views/Tickets/Details.cshtml` |
| `/Tickets/Index` | `Index` | `Views/Tickets/Index.cshtml` |
| `/Tickets/Details/{id}` | `Details` | `Views/Tickets/Details.cshtml` |

## AgentsController

| URL | Action | View |
| --- | --- | --- |
| `/agenti` | `Index` | `Views/Agents/Index.cshtml` |
| `/agenti/{id:int}` | `Details` | `Views/Agents/Details.cshtml` |
| `/Agents/Index` | `Index` | `Views/Agents/Index.cshtml` |
| `/Agents/Details/{id}` | `Details` | `Views/Agents/Details.cshtml` |

## Napomene

- `Bookings`, `Customers`, `Tickets`, `Vehicles`, `Locations` i `Agents` svi imaju `Index` i `Details` view.
- `HomeController` dodatno ima `Error` akciju koja koristi shared error view.
- `Tickets/Details` koristi povezane entitete kroz `AssignedAgents`, `Booking` i `RequestedDropoffLocation`.
