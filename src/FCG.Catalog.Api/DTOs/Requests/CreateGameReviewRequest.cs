namespace FCG.Catalog.Api.DTOs.Requests
{
    public class CreateGameReviewRequest
    {
        public Guid UserId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}
