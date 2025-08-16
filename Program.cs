using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payroll_with_Overtime___Tiers
{
    public class Employee
    {
        public string Name { get; }
        public decimal HourlyRate { get; }
        public decimal[] DailyHours { get; } = new decimal[7];

        private static readonly (decimal ceiling, decimal rate)[] TaxBrackets =
        {
        (600m, 0.10m),
        (1200m, 0.15m),
        (2000m, 0.20m),
        (decimal.MaxValue, 0.25m)
    };

        public Employee(string name, decimal hourlyRate, decimal[] dailyHours)
        {
            if (dailyHours.Length != 7)
                throw new ArgumentException("Must provide exactly 7 days of hours");

            Name = name;
            HourlyRate = hourlyRate;
            DailyHours = dailyHours;
        }

        public decimal CalculateRegularHours()
        {
            decimal total = 0;
            foreach (var hours in DailyHours)
            {
                total += Math.Min(hours, 8m); 
            }
            return total;
        }

        public decimal CalculateDailyOvertime()
        {
            decimal total = 0;
            foreach (var hours in DailyHours)
            {
                total += Math.Max(hours - 8m, 0);
            }
            return total;
        }

        public decimal CalculateWeeklyOvertime()
        {
            decimal totalHours = DailyHours.Sum();
            return Math.Max(totalHours - 40m, 0) - CalculateDailyOvertime();
        }

        public decimal GrossPay()
        {
            decimal regularHours = CalculateRegularHours();
            decimal dailyOT = CalculateDailyOvertime();
            decimal weeklyOT = CalculateWeeklyOvertime();

            return (regularHours * HourlyRate) +
                   (dailyOT * HourlyRate * 1.5m) +
                   (weeklyOT * HourlyRate * 1.75m);
        }

        public decimal CalculateTax()
        {
            decimal gross = GrossPay();
            decimal tax = 0m;
            decimal remaining = gross;

            foreach (var bracket in TaxBrackets.OrderBy(b => b.ceiling))
            {
                if (remaining <= 0) break;

                decimal taxableInBracket = Math.Min(remaining, bracket.ceiling);
                tax += taxableInBracket * bracket.rate;
                remaining -= bracket.ceiling;
            }

            return tax;
        }

        public decimal NetPay()
        {
            return GrossPay() - CalculateTax();
        }
    }

    public class PayrollSystem
    {
        private List<Employee> employees = new List<Employee>();

        public void AddEmployee(Employee employee)
        {
            employees.Add(employee);
        }

        public void ProcessPayroll()
        {
            ValidateEntries();
            var sortedEmployees = employees.OrderByDescending(e => e.NetPay()).ToList();

            Console.WriteLine("WEEKLY PAYROLL REPORT");

            foreach (var emp in sortedEmployees)
            {
                PrintPaySlip(emp);
            }
        }

        private void ValidateEntries()
        {
            foreach (var emp in employees)
            {
                if (emp.HourlyRate <= 0)
                    throw new ArgumentException($"Invalid hourly rate for {emp.Name}");

                foreach (var hours in emp.DailyHours)
                {
                    if (hours < 0 || hours > 24)
                        throw new ArgumentException($"Invalid hours entry for {emp.Name}");
                }
            }
        }

        private void PrintPaySlip(Employee emp)
        {
            Console.WriteLine($"\nPAY SLIP FOR: {emp.Name}");
            Console.WriteLine($"Hourly Rate: {emp.HourlyRate:C}");
            Console.WriteLine("Daily Hours: " + string.Join(", ", emp.DailyHours));

            Console.WriteLine($"Regular Hours: {emp.CalculateRegularHours()} @ {emp.HourlyRate:C}");
            Console.WriteLine($"Daily OT Hours: {emp.CalculateDailyOvertime()} @ {emp.HourlyRate * 1.5m:C}");
            Console.WriteLine($"Weekly OT Hours: {emp.CalculateWeeklyOvertime()} @ {emp.HourlyRate * 1.75m:C}");

            Console.WriteLine($"Gross Pay: {emp.GrossPay():C}");
            Console.WriteLine($"Tax Withheld: {emp.CalculateTax():C}");
            Console.WriteLine($"NET PAY: {emp.NetPay():C}\n");
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            var payroll = new PayrollSystem();

            payroll.AddEmployee(new Employee("John Terence", 20.00m, new decimal[] { 8, 8, 8, 8, 8, 0, 0 }));
            payroll.AddEmployee(new Employee("Janine Reyes", 25.00m, new decimal[] { 9, 8, 10, 7, 8, 6, 0 }));
            payroll.AddEmployee(new Employee("Michael John", 18.50m, new decimal[] { 8, 8, 8, 12, 10, 0, 0 }));

            try
            {
                payroll.ProcessPayroll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing payroll: {ex.Message}");
            }
        }
    }
}
