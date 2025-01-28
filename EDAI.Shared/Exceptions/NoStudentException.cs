namespace EDAI.Shared.Exceptions;

public class NoStudentException : Exception
{
    public NoStudentException() : base("No student matched input parameters")
    {
    }
    
    public NoStudentException(string message) : base(message)
    {
    }
}