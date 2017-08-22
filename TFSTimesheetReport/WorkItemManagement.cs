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
    public class WorkItemManagement
    {
        private TfsTeamProjectCollection m_Collection;
        private WorkItemStore m_WorkItemStore;

        public WorkItemManagement()
        {
            m_Collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(ConfigurationManager.AppSettings["TFSUrl"]));
            m_WorkItemStore = m_Collection.GetService<WorkItemStore>();
        }

        public WorkItemCollection GetWorkItemsByQuery(string projectName, string queryPath)
        {
            var myquery = getQueryDefinitionFromPath((QueryFolder)m_WorkItemStore.Projects[projectName].QueryHierarchy, queryPath);

            return m_WorkItemStore.Query(myquery.QueryText);
        }
        public WorkItem GetWorkItem(int id)
        {
            return m_WorkItemStore.GetWorkItem(id);
        }

        private QueryDefinition getQueryDefinitionFromPath(QueryFolder folder, string path)
        {
            return folder.Select<QueryItem, QueryDefinition>(item =>
            {
                return item.Path == path ?
                    item as QueryDefinition : item is QueryFolder ?
                    getQueryDefinitionFromPath(item as QueryFolder, path) : null;
            })
            .FirstOrDefault(item => item != null);
        }

        public List<WorkItemHistory> GetWorkItemHistory(WorkItem workItem)
        {
            List<WorkItemHistory> _history = new List<WorkItemHistory>();
            string strOldValue, strNewValue;

            for (int i = 1; i < workItem.Revisions.Count; i++)
            {
                foreach (Field field in workItem.Revisions[i].Fields)
                {
                    if (field.Name != "Rev" && field.Name != "Changed By" && field.Name != "Revised Date" && field.Name != "Watermark" && field.Name != "Changed Date" &&
                        field.Name != "Hyperlink Count" && field.Name != "Related Link Count" && field.Name != "Attached File Count" && field.Name != "External Link Count") //Skip some Obvious fields
                    {
                        strOldValue = (workItem.Revisions[i - 1].Fields[field.Name].Value ?? "").ToString();
                        strNewValue = (field.Value ?? "").ToString();
                        if (strOldValue != strNewValue)
                        {
                            _history.Add(
                                new WorkItemHistory
                                {
                                    ChangedBy = workItem.Revisions[i].Fields["Changed By"].Value.ToString(),
                                    ChangedDate = Convert.ToDateTime(workItem.Revisions[i].Fields["Changed Date"].Value),
                                    FieldName = field.Name,
                                    OldValue = strOldValue,
                                    NewValue = strNewValue,
                                });
                        }
                    }
                }
            }

            return _history;
        }
    }

    public class WorkItemHistory
    {
        public string FieldName { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
    }
}
