using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SensoreAPPMVC.Models;

public class Patient : User
{
    public int? ClinicianId { get; set; }
    public bool CompletedRegistration { get; set; }
}