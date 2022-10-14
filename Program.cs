using System;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using File = System.IO.File;


namespace IMoneyBot
{
    internal class Program
    {
        static TelegramBotClient bot = new TelegramBotClient("5249950459:AAGKcMiBPDDilsmtyTddEQleoqyRw_Is0M0");
        private static bool cmdAddSpendingWasSelected;
        private static bool cmdMonthBudgetWasSelected;
        static Budget _budget = new Budget(0);
        static string[] smiles = { "👑", "🍋", "💸", "💵", "💰", "💳", "💶", "😉", "👌", "✅", "📊", "📈" };
        static Random rand = new Random();


        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;

                if (message.Text.ToLower() == "/start")//Button "Start" was pressed
                {
                    if (File.Exists("BudgetingTable.json"))
                    {
                        File.Delete("BudgetingTable.json");
                    }
                    await botClient.SendTextMessageAsync(message.Chat, "Привіт, я телеграм бот, що допоможе тобі спланувати бюджет на місяць. Введи команду /monthbudget для того, щоб задати суму яку ти можеш витратити до кінця цього місяця " + smiles[rand.Next(0, smiles.Length)]);
                }
                else if (message.Text.ToLower() == "/monthbudget") //command "Month Budget" was selected
                {
                    cmdMonthBudgetWasSelected = true;
                    await botClient.SendTextMessageAsync(message.Chat, "Введи суму, яку ти можеш витратити до кінця цього місяця " + smiles[rand.Next(0, smiles.Length)]);
                }
                else if (cmdMonthBudgetWasSelected)
                {
                    try
                    {
                        if (File.Exists("BudgetingTable.json"))
                        {
                            File.Delete("BudgetingTable.json");
                        }
                        _budget = new Budget(int.Parse(message.Text));
                        await botClient.SendTextMessageAsync(message.Chat, $"Твій бюджет на день в цьому місяці складає {_budget.GetDayBudget()} грн " + smiles[rand.Next(0, smiles.Length)] + "\n\n" + "Введи команду /showtable для перегляду таблиці планових витрат на поточний місяць!" + "\n\n" + "Введи команду /addspending для додавання витрати");
                    }
                    catch
                    {
                        Console.WriteLine("Error1");
                    }
                    cmdMonthBudgetWasSelected = false;

                }
                else if (message.Text.ToLower() == "/showtable")//show planning spending table
                {
                    string str = String.Empty;
                    foreach (var element in _budget.ShowPlanningSpendingTable())
                    {

                        str += element.Date.ToShortDateString().Remove(element.Date.ToShortDateString().Length - 5) + " – " + element.Sum.ToString() + " грн\n";
                    }
                    await botClient.SendTextMessageAsync(message.Chat, $"Таблиця планових витрат на поточний місяць " + smiles[rand.Next(0, smiles.Length)] + "\n\n" + str + "\n" + smiles[rand.Next(0, smiles.Length)] + " Введи команду /addspending для додавання витрати");
                }
                else if (message.Text.ToLower() == "/addspending")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Введи суму витрати " + smiles[rand.Next(0, smiles.Length)]);
                    cmdAddSpendingWasSelected = true;
                }
                else if (cmdAddSpendingWasSelected)
                {
                    try
                    {
                        int.TryParse(message.Text, out int sum);
                        _budget.AddSpending(sum);
                        string dayOfWeekToday = CultureInfo.GetCultureInfo("uk-UA").DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek);
                        string outputMessage = string.Empty;
                        
                        outputMessage += "Витрата зафіксована! " + smiles[rand.Next(0, smiles.Length)] + "\n\n" + smiles[rand.Next(0, smiles.Length)] + $" Бюджет на сьогодні {DateTime.Today.Date.ToShortDateString()} ({dayOfWeekToday}): {_budget.GetTodayBudget()} грн\n";
                        if (DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) - DateTime.Today.Day != 0)//today isn't last day of the month
                        {
                                outputMessage += smiles[rand.Next(0, smiles.Length)] + $" На завтра: {_budget.GetTomorrowBudget()} грн" + "\n\n" + "Введи команду /showtable для перегляду таблиці планових витрат на поточний місяць!";
                        }
                        
                        cmdAddSpendingWasSelected = false;
                        await botClient.SendTextMessageAsync(message.Chat, outputMessage);
                    }
                    catch
                    {
                        Console.WriteLine("Error2");
                    }
                }
            }
        }
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }

        static void Main()
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions();
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}