namespace EDAI.Shared.Models.DTO;

public class GenerateScoreRequestDTO
{
    public IEnumerable<int> DocumentIds { get; set; }

    public string ConnectionId { get; set; }
}