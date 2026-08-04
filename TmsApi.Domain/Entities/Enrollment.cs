using System;

using TmsApi.Domain.Entities;
public class Enrollment
{
public int Id { get; set; }
public int StudentId { get; set; }
public int CourseId { get; set; }
public bool IsArchived {get;set;} =false;
public int Year { get; set; }
 public string Status { get; set; } = "Pending";
public decimal? Grade { get; set; } // Nullable, as studentmay be currently enrolled
public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;// Navigation properties back to entities
public Student Student { get; set; } = null!;
public Course Course { get; set; } = null!;

}