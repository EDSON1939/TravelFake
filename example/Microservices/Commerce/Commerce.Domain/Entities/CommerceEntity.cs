namespace Commerce.Domain.Entities;

public class CommerceEntity
{
    public long        CommerceId   { get; set; }
    public string      Name         { get; set; } = string.Empty;
    public string      Nit          { get; set; } = string.Empty;
    public bool        IsActive     { get; set; }
    public DateTime    CreatedAt    { get; set; }
    public DateTime?   UpdatedAt    { get; set; }
}