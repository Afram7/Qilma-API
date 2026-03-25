namespace Qilma_API.DTOs;

// DTO for returning guest information
public class GuestDTO
{
    required public int GuestId { get; set; }
}

// DTO for returning the result of a guest creation attempt
public class CreateGuestResult
{
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
    public GuestDTO? Guest { get; set; }
}