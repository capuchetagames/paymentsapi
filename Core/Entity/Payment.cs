namespace Core.Entity;

public class Payment : EntityBase
{
    public int UserId { get; set; }
    public int GameId { get; set; }
    
    public decimal Price { get; set; }
    public string Status { get; set; }
    
}