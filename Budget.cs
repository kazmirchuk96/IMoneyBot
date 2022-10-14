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
            for (int i = 0; i < AmountofDays; i++)
            {
                Spending oldPlanningSpending = (i == 0) ? BudgetingTable.Where(x => x.Date == DateTime.Today).First() : BudgetingTable.Where(x => x.Date == DateTime.Today.AddDays(i)).First();
                int index = BudgetingTable.IndexOf(oldPlanningSpending);
                BudgetingTable.Remove(oldPlanningSpending);
                if (sum <= oldPlanningSpending.Sum)
                {
                    Spending newPlanningSpending = new Spending(oldPlanningSpending.Date, oldPlanningSpending.Sum - sum);
                    BudgetingTable.Insert(index, newPlanningSpending);
                    break;
                }
                else
                {
                    Spending newPlanningSpending = new Spending(oldPlanningSpending.Date, 0);
                    BudgetingTable.Insert(index, newPlanningSpending);
                    sum -= oldPlanningSpending.Sum;
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
                /*Spending yesterdaySpending = BudgetingTable[todaySpendingIndex - 1];
                BudgetingTable.Remove(yesterdaySpending);//remove yesterday spending with money that left
                Spending newYesterdaySpanding = new Spending(yesterdaySpending.Date, 0); //Creation new spending with null summ for yesterday
                BudgetingTable.Insert(todaySpendingIndex - 1, newYesterdaySpanding);//Insertion new yesterday spending (with null sum)*/
                int sumRemainPreviousDays = 0;
                for (int i = 0; i < todaySpendingIndex; i++)
                {
                    sumRemainPreviousDays += BudgetingTable[i].Sum;
                    BudgetingTable[i].Sum = 0;
                }

                BudgetingTable[todaySpendingIndex].Sum += sumRemainPreviousDays;
                //BudgetingTable.Remove(todaySpending);
                //Spending newTodaySpending = new Spending(todaySpending.Date, todaySpending.Sum + sumRemainPreviousDays);//Adding for today sum yesterday sum that remain 
                //BudgetingTable.Insert(todaySpendingIndex, newTodaySpending);
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
