using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;


namespace IMoneyBot
{
    internal class Budget
    {
        private List<Spending> BudgetingTable { get; set; }
        private int MonthBudget { get; set; }
        private int DayBudget { get; set; }
        private int AmountofDays { get; set; }

        private string fileName = "BudgetingTable.json";

        public Budget(int monthBudget)
        {
            MonthBudget = monthBudget;
            AmountofDays = GetAmountOfDays();
            DayBudget = GetDayBudget();
            CreatingSpendingTable();
        }

        private int GetAmountOfDays()//calculate how many day remain to the month's end
        {
            return DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) - DateTime.Today.Day + 1;
        }

        public int GetDayBudget()
        {
            return MonthBudget / AmountofDays;
        }

        private void CreatingSpendingTable()
        {
            BudgetingTable = new List<Spending>();
            if (File.Exists(fileName))
            {
                //Reading spending table from JSON file
                string table = File.ReadAllText(fileName);
                BudgetingTable = JsonConvert.DeserializeObject<List<Spending>>(table);
            }
            else
            {
                for (int i = 0; i < AmountofDays; i++)
                {
                    DateTime date = (i == 0) ? DateTime.Today : DateTime.Today.AddDays(i);
                    BudgetingTable.Add(new Spending(date, DayBudget));
                    //Writing spending table to JSON file
                    File.WriteAllText(fileName, JsonConvert.SerializeObject(BudgetingTable));
                }
            }
        }

        public void AddSpending(int sum)
        {
            TransferRemainingPreviousDaysSum();



            /*for (int i = 0; i < AmountofDays; i++)
            {

                Spending oldPlanningSpending = (i == 0)
                    ? BudgetingTable.Where(x => x.Date == DateTime.Today).First()
                    : BudgetingTable.Where(x => x.Date == DateTime.Today.AddDays(i)).First();
                int index = BudgetingTable.IndexOf(oldPlanningSpending);
                if (sum <= oldPlanningSpending.Sum)
                {
                    BudgetingTable[index].Sum -= sum;
                    break;
                }
                else
                {
                    BudgetingTable[index].Sum = 0;

                    sum -= oldPlanningSpending.Sum;
                }
            }*/

          
            if (sum <= BudgetingTable.First(x => x.Date == DateTime.Today).Sum)
            {
                BudgetingTable.First(x => x.Date == DateTime.Today).Sum -= sum;
            }
            else
            {
                int debt = sum - BudgetingTable.First(x => x.Date == DateTime.Today).Sum;//загальний борг
                int dayDebt = debt / (AmountofDays-1);//сума яку треба відняти від планової суми витрат кожного дня
                BudgetingTable.First(x => x.Date == DateTime.Today).Sum = 0;

                for (int i = 1; i < AmountofDays; i++)
                {
                    BudgetingTable.First(x => x.Date == DateTime.Today.AddDays(i)).Sum-=dayDebt;

                }
            }

            //Writing spending table to JSON file
            File.WriteAllText(fileName, JsonConvert.SerializeObject(BudgetingTable));
        }

        private void TransferRemainingPreviousDaysSum()
        {
            Spending todaySpending = BudgetingTable.Where(x => x.Date == DateTime.Today).First();
            int todaySpendingIndex = BudgetingTable.IndexOf(todaySpending);
            if (todaySpendingIndex != 0)//day isn't first
            {
                int sumRemainPreviousDays = 0;
                for (int i = 0; i < todaySpendingIndex; i++)
                {
                    sumRemainPreviousDays += BudgetingTable[i].Sum;
                    BudgetingTable[i].Sum = 0;
                }
                BudgetingTable[todaySpendingIndex].Sum += sumRemainPreviousDays;
            }
        }

        public int GetTodayBudget()
        {
            Spending spending = BudgetingTable.Where(x => x.Date == DateTime.Today).First();
            int todayBudget = spending.Sum;
            return todayBudget;
        }

        public int GetTomorrowBudget()
        {
            int tomorrowBudget = BudgetingTable.Where(x => x.Date == DateTime.Today.AddDays(1)).First().Sum;
            return tomorrowBudget;
        }
        public List<Spending> ShowPlanningSpendingTable()
        {
            TransferRemainingPreviousDaysSum();
            //Writing spending table to JSON file
            File.WriteAllText(fileName, JsonConvert.SerializeObject(BudgetingTable));
            return BudgetingTable;
        }
    }
}
