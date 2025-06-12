namespace TestCarBot.Models
{
    public class ExtractedData
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? PassportNumber { get; set; }
        public string? CarNumber { get; set; }
        public string? CarModel { get; set; }

        public bool IsPassportDataFound => !string.IsNullOrEmpty(FullName) && !string.IsNullOrEmpty(PassportNumber);
        public bool IsCarDataFound => !string.IsNullOrEmpty(CarNumber);
    }
}