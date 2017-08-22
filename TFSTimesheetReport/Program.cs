using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSTimesheetReport
{
    class Program
    {

        public static string User { get { return ConfigurationManager.AppSettings["User"]; } }

        static void Main(string[] args)
        {
            try
            {
                if (args.Any(a => a.Equals("/w", StringComparison.CurrentCultureIgnoreCase) || a.Equals("/workitem", StringComparison.CurrentCultureIgnoreCase)))
                    workItemReport(int.Parse(args[1]));
                else if (args.Any(a => a.Equals("/team", StringComparison.CurrentCultureIgnoreCase)))
                    teamReport();
                else
                    userReport();
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine("Error!", ConsoleColor.Red);
                ConsoleHelper.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }

        private static void teamReport()
        {
            var _timesheet = new Timesheet();

            var _teamTimesheet = _timesheet.GetTimesheet();

            writeTeamReport(_teamTimesheet);
        }

        private static void userReport()
        {
            var _timesheet = new Timesheet();

            var _userTimesheet = _timesheet.GetTimesheet(User);
            writeUserReport(User, _userTimesheet);
        }


        private static void workItemReport(int id)
        {
            var _timesheet = new Timesheet();

            var _userTimesheet = _timesheet.GetTimesheet(id);
            writeTeamReport(_userTimesheet);
        }

        private static void writeTeamReport(List<DailyTimesheet> teamTimesheet)
        {
            var _gpr = teamTimesheet.GroupBy(a => a.UserName).OrderBy(a => a.Key);

            foreach (var item in _gpr)
            {
                writeUserReport(item.Key, item.ToList());
            }

            Console.WriteLine("----------");
            writeTotal(teamTimesheet);
        }

        private static void writeUserReport(string userName, List<DailyTimesheet> userTimesheet)
        {
            Console.WriteLine("");
            ConsoleHelper.WriteLine($"{userName}", ConsoleColor.Cyan);
            Console.WriteLine("");

            writeDailyReport(userTimesheet);
            writeTotal(userTimesheet);
        }

        private static void writeDailyReport(List<DailyTimesheet> timesheet)
        {
            var _currentDate = timesheet.Min(a => a.Date);
            var _lastDate = timesheet.Max(a => a.Date);

            while (_currentDate <= _lastDate)
            {
                var _item = timesheet.FirstOrDefault(a => a.Date == _currentDate);

                Console.Write($"{_currentDate.ToShortDateString()} {_currentDate.DayOfWeek,-9} - ");

                if (_item == null)
                {
                    ConsoleHelper.WriteLine($"Empty", getColorByDayOfWeekWithoutValue(_currentDate.DayOfWeek));
                }
                else
                {
                    ConsoleHelper.WriteLine($"Hours: {_item.Hours,3}  Value: {_item.Value,3}  WorkItems: {string.Join(",", _item.WorkItems)}", getColorByRegister(_item));
                }
                _currentDate = _currentDate.AddDays(1);
            }
        }

        private static void writeTotal(List<DailyTimesheet> apontamentos)
        {
            Console.WriteLine("");
            Console.WriteLine("Total");
            Console.WriteLine($" Hours: {apontamentos.Sum(a => a.Hours)} Value: { apontamentos.Sum(a => a.Value)}");
        }

        private static ConsoleColor getColorByDayOfWeekWithoutValue(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    return ConsoleColor.White;
                default:
                    return ConsoleColor.Yellow;
            }
        }

        private static ConsoleColor getColorByRegister(TimesheetItem registro)
        {
            if (registro.Hours < 0) return ConsoleColor.Red;

            if (registro.Value < 0) return ConsoleColor.DarkYellow;

            return ConsoleColor.Gray;
        }
    }
}

