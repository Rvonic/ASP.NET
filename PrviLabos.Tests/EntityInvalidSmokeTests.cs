using Xunit;
using PrviLabos.Model;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PrviLabos.Tests;

public class EntityInvalidSmokeTests
{
    // Use TestEntityFactory helper for creating valid entities and running validations.

    [Fact]
    public void Customer_invalid_email_fails()
    {
        var c = Helpers.TestEntityFactory.ValidCustomer();
        c.Email = "not-an-email";
        var ok = Helpers.TestEntityFactory.IsValid(c, out var results);
        Assert.False(ok);
        Assert.Contains(results, r => r.ErrorMessage != null);
    }

    [Fact]
    public void Customer_missing_required_fields_fail()
    {
        var c = Helpers.TestEntityFactory.ValidCustomer();
        c.FirstName = "";
        c.LastName = "";
        c.Email = "";
        c.PhoneNumber = "";
        c.DriverLicenseNumber = "";
        var ok = Helpers.TestEntityFactory.IsValid(c, out var results);
        Assert.False(ok);
    }
      [Fact]
    public void Customer_missing_first_name_fail()
    {
        var c = Helpers.TestEntityFactory.ValidCustomer();
        c.FirstName = "";
        
        var ok = Helpers.TestEntityFactory.IsValid(c, out var results);
        Assert.False(ok);
    }

      [Fact]
    public void Customer_missing_lastname_fail()
    {
        var c = Helpers.TestEntityFactory.ValidCustomer();
       
        c.LastName = "";
       
        var ok = Helpers.TestEntityFactory.IsValid(c, out var results);
        Assert.False(ok);
    }
      [Fact]
    public void Customer_long_phone_fail()
    {
        var c = Helpers.TestEntityFactory.ValidCustomer();
      
        c.PhoneNumber = "12345678901234567890";
      
        var ok = Helpers.TestEntityFactory.IsValid(c, out var results);
        Assert.False(ok);
    }

    [Fact]
     
    public void Customer_short_phone_fail()
    {
        var c = Helpers.TestEntityFactory.ValidCustomer();
       
        c.PhoneNumber = "123";
        
        var ok = Helpers.TestEntityFactory.IsValid(c, out var results);
        Assert.False(ok);
    }
      [Fact]
    public void Customer_long_driver_license_fail()
    {
        var c = Helpers.TestEntityFactory.ValidCustomer();
     
        c.DriverLicenseNumber = "DL-12345678901234567890";
        var ok = Helpers.TestEntityFactory.IsValid(c, out var results);
        Assert.False(ok);
    }
      [Fact]
    public void Customer_short_driver_license_fail()
    {
        var c = Helpers.TestEntityFactory.ValidCustomer();
        c.DriverLicenseNumber = "DL-123";
        var ok = Helpers.TestEntityFactory.IsValid(c, out var results);
        Assert.False(ok);
    }
      [Fact]
   
    public void Vehicle_invalid_production_year_and_rate_fail()
    {
        var v = Helpers.TestEntityFactory.ValidVehicle();
        v.PlateNumber = "";
        v.ProductionYear = 3000;
        v.DailyRate = 0m;
        var ok = Helpers.TestEntityFactory.IsValid(v, out var results);
        Assert.False(ok);
        Assert.Contains(results, r => r.MemberNames != null);
    }

    [Fact]
   
    public void Vehicle_invalid_production_year__fail()
    {
        var v = Helpers.TestEntityFactory.ValidVehicle();

        v.ProductionYear = 1800;
      
        var ok = Helpers.TestEntityFactory.IsValid(v, out var results);
        Assert.False(ok);
        Assert.Contains(results, r => r.MemberNames != null);
    }

    [Fact]
    public void Location_invalid_capacity_and_hours_fail()
    {
        var l = Helpers.TestEntityFactory.ValidLocation();
        l.ParkingCapacity = -1;
        l.OpenAt = new TimeOnly(18, 0);
        l.CloseAt = new TimeOnly(8, 0);
        var ok = Helpers.TestEntityFactory.IsValid(l, out var results);
        Assert.False(ok);
    }

    [Fact]
    public void Booking_invalid_dates_and_missing_relations_fail()
    {
        var b = Helpers.TestEntityFactory.ValidBooking();
        b.PickupAt = System.DateTime.Now.AddDays(3);
        b.PlannedDropoffAt = System.DateTime.Now.AddDays(2);
        b.Customer = null!;
        b.Vehicle = null!;
        b.PickupLocation = null!;
        var ok = Helpers.TestEntityFactory.IsValid(b, out var results);
        Assert.False(ok);
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("PlannedDropoffAt"));
    }

    [Fact]
    public void SupportAgent_invalid_shift_and_name_fail()
    {
        var s = Helpers.TestEntityFactory.ValidSupportAgent();
        s.FullName = "";
        s.ShiftStart = System.DateTime.Now.AddHours(8);
        s.ShiftEnd = System.DateTime.Now.AddHours(6);
        var ok = Helpers.TestEntityFactory.IsValid(s, out var results);
        Assert.False(ok);
    }

    [Fact]
    public void SupportTicket_missing_fields_fail()
    {
        var t = Helpers.TestEntityFactory.ValidSupportTicket();
        t.Booking = null!;
        t.RequestedDropoffLocation = null!;
        t.Description = "";
        var ok = Helpers.TestEntityFactory.IsValid(t, out var results);
        Assert.False(ok);
    }
}
