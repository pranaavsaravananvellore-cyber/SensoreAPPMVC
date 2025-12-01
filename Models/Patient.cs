using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SensoreAPPMVC.Models;

public class Patient : User
{
    public int? ClinitionId { get; set; }
    public bool CompletedRegistration { get; set; }
}