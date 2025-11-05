namespace zoplannerservice.Models;

public class CustomerDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public List<AssignmentDto>? Assignments { get; set; }
}

public class AssignmentDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime? Date { get; set; }
}

public class AssignmentResponseDto
{
    public int Id { get; set; }
    public bool Success { get; set; }
}