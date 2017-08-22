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
    public class Timesheet
    {
        private const string COMPLETED_WORK_FIELD = "Completed Work";
        private const string REMAINING_WORK_FIELD = "Remaining Work";

        private WorkItemManagement m_WorkItemManagement;

        public Timesheet()
        {
            m_WorkItemManagement = new WorkItemManagement();
        }

        public string ProjectName { get { return ConfigurationManager.AppSettings["ProjectName"]; } }

        public string QueryPath { get { return ConfigurationManager.AppSettings["QueryPath"]; } }

        public List<DailyTimesheet> GetTimesheet()
        {
            return getTimesheet(a => true);
        }

        public List<DailyTimesheet> GetTimesheet(string userName)
        {
            return getTimesheet(u => u.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase));
        }

        public List<DailyTimesheet> GetTimesheet(int workItemId)
        {
            var _workItems = new List<TimesheetWorkItem> { getApontamentosWorkItem(m_WorkItemManagement.GetWorkItem(workItemId)) };
            return getTimesheet(_workItems, u => true);
        }

        private List<DailyTimesheet> getTimesheet(Func<TimesheetItem, bool> filter)
        {
            return getTimesheet(getApontamentos(), filter);
        }

        private List<DailyTimesheet> getTimesheet(List<TimesheetWorkItem> workItems, Func<TimesheetItem, bool> filter)
        {
            return workItems
                .SelectMany(a => a.Timesheet
                                        .Where(filter)
                                        .Select(b => new
                                        {
                                            WorkItemId = a.WorkItemId,
                                            UserName = b.UserName,
                                            Data = b.Date,
                                            HorasApontada = b.Hours,
                                            ValorBaixado = b.Value
                                        }))
                .GroupBy(a => new { Data = a.Data, UserName = a.UserName })
                .Select(a => new DailyTimesheet
                {
                    Date = a.Key.Data,
                    UserName = a.Key.UserName,
                    Hours = a.Sum(b => b.HorasApontada),
                    Value = a.Sum(b => b.ValorBaixado),
                    WorkItems = a.Select(b => b.WorkItemId).ToList()
                }).ToList();
        }

        private List<TimesheetWorkItem> getApontamentos()
        {
            var _workItems = new List<TimesheetWorkItem>();

            var queryResults = m_WorkItemManagement.GetWorkItemsByQuery(ProjectName, QueryPath);

            foreach (WorkItem item in queryResults)
            {
                _workItems.Add(getApontamentosWorkItem(item));
            }

            return _workItems;
        }

        private TimesheetWorkItem getApontamentosWorkItem(WorkItem workItem)
        {
            var _wiHistory = m_WorkItemManagement.GetWorkItemHistory(workItem);

            var _timesheet = _wiHistory.Where(a => a.FieldName == COMPLETED_WORK_FIELD || a.FieldName == REMAINING_WORK_FIELD)
                                            .GroupBy(a => new
                                            {
                                                Data = a.ChangedDate.Date,
                                                UserName = a.ChangedBy
                                            })
                                            .Select(a => new TimesheetItem
                                            {
                                                Date = a.Key.Data,
                                                UserName = a.Key.UserName,
                                                Hours = a.Where(b => b.FieldName == COMPLETED_WORK_FIELD).Sum(b => b.NewValue.ToDecimal() - b.OldValue.ToDecimal()),
                                                Value = a.Where(b => b.FieldName == REMAINING_WORK_FIELD).Sum(b => b.OldValue.ToDecimal() - b.NewValue.ToDecimal())
                                            });

            return new TimesheetWorkItem
            {
                WorkItemId = workItem.Id,
                Title = workItem.Title,
                Timesheet = _timesheet.ToList()
            };
        }
    }

    public class TimesheetWorkItem
    {
        public int WorkItemId { get; set; }

        public string Title { get; set; }

        public List<TimesheetItem> Timesheet { get; set; }
    }

    public class DailyTimesheet : TimesheetItem
    {
        public List<int> WorkItems { get; set; }
    }

    public class TimesheetItem
    {
        public string UserName { get; set; }

        public DateTime Date { get; set; }

        public decimal Hours { get; set; }

        public decimal Value { get; set; }
    }

}