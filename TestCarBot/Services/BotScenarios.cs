namespace TestCarBot.Services
{
    public static class BotScenarios
    {
        public const string ConfirmDataYes = "✅ Так, все вірно";
        public const string ConfirmDataNo = "❌ Ні, дані невірні";
        public const string ConfirmPriceYes = "✅ Приймаю";
        public const string ConfirmPriceNo = "❌ Відмовляюсь";

        public const string Welcome = "Привітай користувача і коротко поясни, що ти бот для страхування авто.";
        public const string RequestPassport = "Попроси користувача надіслати фото його паспорта.";
        public const string PassportReceived = "Повідом користувача, що його паспорт отримано, і тепер попроси надіслати фото документа на авто (техпаспорт).";
        public const string DocumentsReceived = "Повідом, що обидва документи отримано і ти починаєш їх обробку.";
        public const string DataRejected = "Повідом, що шкода, що дані невірні. Попроси надіслати фото документів ще раз, починаючи з паспорта.";
        public const string DataAccepted = "Повідом, що дані підтверджено. Назви фіксовану ціну страхування (100 USD) і запитай, чи користувач згоден.";
        public const string PriceRejected = "Повідом, що ціна 100 USD є фіксованою та іншої немає. Вибачись і скажи, що якщо він передумає, то може почати знову з команди /start.";
        public const string AlreadyCompleted = "Повідом, що процес вже завершено. Якщо потрібен новий поліс, нехай почне з команди /start.";
        public const string AwaitingStart = "Повідом, що для початку роботи потрібно ввести команду /start.";
        public const string GeneratePolicyText = "Згенеруй текст для страхового поліса. Дані: Номер поліса: {policyNumber}, Паспорт: {passport}, Авто: {car}, Сума: 100 USD, Дата видачі: {date}.";
        public const string PurchaseConfirmed = "Повідом, що поліс успішно згенеровано, і він зараз буде надісланий у вигляді PDF-файлу.";
        public const string FinalWords = "Подякуй користувачу за покупку. Скажи, що якщо знадобиться допомога або новий поліс, він знає, де тебе знайти. Побажай гарного дня.";
        public const string GenericError = "Повідом, що сталася несподівана помилка і попроси спробувати пізніше.";
        public const string ConfirmData = "Покажи користувачу розпізнані дані. Прізвище: {last_name}, Ім'я: {first_name}, Паспорт: {passport}, Авто: {car_model}, Номер: {car_number}. Запитай, чи все правильно.";
    }
}