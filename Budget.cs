using System;
using System.Collections.Generic;
using System.Linq;


namespace IMoneyBot
{
    internal class Budget
    {
        private List<Spending> PlanningSpending { get; set; }//возможно переименовать на BudgetingTable
        private int MonthBudget { get; set; }
        private int DayBudget { get; set; }
        private int AmountofDays { get; set; }

        public Budget(int monthBudget)
        {
            this.MonthBudget = monthBudget;
            this.AmountofDays = GetAmountOfDays();
            this.DayBudget = GetDayBudget();
            CreatingSpendingPlan();
        }

        private int GetAmountOfDays()//calculate how many day remain to the month's end
        {
            return DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) - DateTime.Today.Day + 1;
        }

        public int GetDayBudget()
        {
            return MonthBudget / AmountofDays;
        }

        private void CreatingSpendingPlan()
        {
            PlanningSpending = new List<Spending>();
            for (int i = 0; i < AmountofDays; i++)
            {
                DateTime date = (i == 0) ? DateTime.Today : DateTime.Today.AddDays(i);
                PlanningSpending.Add(new Spending(date, DayBudget));
            }
        }

        public void AddSpending(int sum)
        {
            TransferSpendingFromYesterdayToToday();

            for (int i = 0; i < AmountofDays; i++)
            {
                Spending oldPlanningSpending = (i == 0) ? PlanningSpending.Where(x => x.Date == DateTime.Today).First() : PlanningSpending.Where(x => x.Date == DateTime.Today.AddDays(i)).First();
                int index = PlanningSpending.IndexOf(oldPlanningSpending);
                PlanningSpending.Remove(oldPlanningSpending);
                if (sum <= oldPlanningSpending.Sum)
                {
                    Spending newPlanningSpending = new Spending(oldPlanningSpending.Date, oldPlanningSpending.Sum - sum);
                    PlanningSpending.Insert(index, newPlanningSpending);
                    break;
                }
                else
                {
                    Spending newPlanningSpending = new Spending(oldPlanningSpending.Date, 0);
                    PlanningSpending.Insert(index, newPlanningSpending);
                    sum -= oldPlanningSpending.Sum;
                }
            }
        }

        private void TransferSpendingFromYesterdayToToday()
        {
            Spending todaySpending = PlanningSpending.Where(x => x.Date == DateTime.Today).First();
            int todaySpendingIndex = PlanningSpending.IndexOf(todaySpending);
            if (todaySpendingIndex != 0)//day isn't first
            {
                Spending yesterdaySpending = PlanningSpending[todaySpendingIndex - 1];
                PlanningSpending.Remove(yesterdaySpending);//remove yesterday spending with money that left
                Spending newYesterdaySpanding = new Spending(yesterdaySpending.Date, 0); //Creation new spending with null summ for yesterday
                PlanningSpending.Insert(todaySpendingIndex - 1, newYesterdaySpanding);//Insertion new yesterday spending (with null sum)

                PlanningSpending.Remove(todaySpending);
                Spending newTodaySpending = new Spending(todaySpending.Date, todaySpending.Sum + yesterdaySpending.Sum);//Adding for today sum yesterday sum that remain 
                PlanningSpending.Insert(todaySpendingIndex, newTodaySpending);
            }
        }

        public int GetTodayBudget()
        {
            Spending spending = PlanningSpending.Where(x => x.Date == DateTime.Today).First();
            int todayBudget = spending.Sum;
            return todayBudget;
        }

        public int GetTomorrowBudget()
        {
            int tomorrowBudget = PlanningSpending.Where(x => x.Date == DateTime.Today.AddDays(1)).First().Sum;
            return tomorrowBudget;
        }
        public List<Spending> ShowPlanningSpendingTable()
        {
            TransferSpendingFromYesterdayToToday();
            return PlanningSpending;
        }
    }
}
