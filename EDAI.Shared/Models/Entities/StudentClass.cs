namespace EDAI.Shared.Models.Entities;

public class StudentClass
{
    public int StudentClassId { get; set; }

    public ICollection<Student>? Students { get; set; } = new List<Student>();

    public string Class { get; set; }

    public string School { get; set; }
}