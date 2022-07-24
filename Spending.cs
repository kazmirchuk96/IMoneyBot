using System;

namespace IMoneyBot
{
    internal class Spending
    {
        public DateTime Date { get; set; }
        public int Sum { get; set; }

        public Spending(DateTime date, int sum)
        {
            Date = date;
            Sum = sum;
        }

    }
}