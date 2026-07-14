namespace PickupAPi.Models
{
    public class HtmlRequestModel
    {
        public string HtmlContent { get; set; }
        public int CoverPageType { get; set; } = 0; // 0 = default, 1 = custom image cover
    }
}