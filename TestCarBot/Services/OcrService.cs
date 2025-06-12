using TestCarBot.Models;

namespace TestCarBot.Services
{
    public class OcrService 
    {
        public Task<ExtractedData> ExtractDataAsync(DocumentTypeEnum docType)
        {
            ExtractedData result = new();

            if (docType == DocumentTypeEnum.Passport)
            {
                result.LastName = "ТКАЧЕНКО";
                result.FirstName = "МАР'ЯНА";
                result.PassportNumber = "000000000";
            }
            else if (docType == DocumentTypeEnum.Vehicle)
            {
                result.CarNumber = "АЕ0282НМ";
                result.CarModel = "Volkswagen";
            }

            Console.WriteLine("\n--- MOCK OCR SERVICE ---");
            Console.WriteLine($"Simulating OCR for document type: {docType}");
            Console.WriteLine("------------------------\n");

            return Task.FromResult(result);
        }
    }
}