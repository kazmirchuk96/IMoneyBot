using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace IMoneyBot
{
    internal class Budget
    {
        private List<Spending> BudgetingTable { get; set; }
        private int MonthBudget { get; set; }
        private int DayBudget { get; set; }
        private int AmountofDays { get; set; }

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
            for (int i = 0; i < AmountofDays; i++)
            {
                DateTime date = (i == 0) ? DateTime.Today : DateTime.Today.AddDays(i);
                BudgetingTable.Add(new Spending(date, DayBudget));
            }
            WritingSpendingTableToFile(BudgetingTable);
        }

        public void AddSpending(int sum)
        {
            TransferSpendingFromYesterdayToToday();
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
            WritingSpendingTableToFile(BudgetingTable);
        }

        private void TransferSpendingFromYesterdayToToday()
        {
            Spending todaySpending = BudgetingTable.Where(x => x.Date == DateTime.Today).First();
            int todaySpendingIndex = BudgetingTable.IndexOf(todaySpending);
            if (todaySpendingIndex != 0)//day isn't first
            {
                Spending yesterdaySpending = BudgetingTable[todaySpendingIndex - 1];
                BudgetingTable.Remove(yesterdaySpending);//remove yesterday spending with money that left
                Spending newYesterdaySpanding = new Spending(yesterdaySpending.Date, 0); //Creation new spending with null summ for yesterday
                BudgetingTable.Insert(todaySpendingIndex - 1, newYesterdaySpanding);//Insertion new yesterday spending (with null sum)

                BudgetingTable.Remove(todaySpending);
                Spending newTodaySpending = new Spending(todaySpending.Date, todaySpending.Sum + yesterdaySpending.Sum);//Adding for today sum yesterday sum that remain 
                BudgetingTable.Insert(todaySpendingIndex, newTodaySpending);
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
            TransferSpendingFromYesterdayToToday();
            return BudgetingTable;
        }

        private void WritingSpendingTableToFile(List<Spending> listSpendings)
        {
            var sw = new StreamWriter(@"SpendingTable.txt", false, System.Text.Encoding.Default);
            foreach (var item in listSpendings)
            {
                sw.WriteLine(item.Date+"-"+item.Sum);
            }
            sw.Close();
        }
        /*private int ReadingBudgetingTableFromFile()
        {
            if (File.Exists(@"SpendingTable.txt"))
            {
                var sr = new StreamReader(@"SpendingTable.txt");
                string line;
                while (sr.Re)
                string str = sr.ReadLine();//считываем время, через которое нужно выключить компьютер из файла
                sr.Close();


                return int.Parse(str);
            }

            using (StreamReader reader = new StreamReader(@"SpendingTable.txt"))
            {
                string line;
                while ((line =  != null)
                {
                    Console.WriteLine(line);
                }
            }
            return 0;//файл не существует
        }*/
    }
}
