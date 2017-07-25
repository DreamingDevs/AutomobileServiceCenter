using ASC.Models.BaseTypes;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace ASC.Models.Queries
{
    public class Queries
    {
        public static string GetDashboardQuery(DateTime? requestedDate,
            List<string> status = null,
            string email = "",
            string serviceEngineerEmail = "")
        {
            var finalQuery = string.Empty;
            var statusQueries = new List<string>();
            // Add Requested Date Clause
            if (requestedDate.HasValue)
            {
                finalQuery = TableQuery.GenerateFilterConditionForDate("RequestedDate", Constants.GreaterThanOrEqual, requestedDate.Value);
            }

            // Add Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(email))
            {
                finalQuery = !string.IsNullOrWhiteSpace(finalQuery) ?
                    TableQuery.CombineFilters(finalQuery, TableOperators.And, TableQuery.GenerateFilterCondition("PartitionKey", Constants.Equal, email)) :
                    TableQuery.GenerateFilterCondition("PartitionKey", Constants.Equal, email);
            }

            // Add  Service Engineer Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                finalQuery = !string.IsNullOrWhiteSpace(finalQuery) ?
                    TableQuery.CombineFilters(finalQuery, TableOperators.And, TableQuery.GenerateFilterCondition("ServiceEngineer", Constants.Equal, serviceEngineerEmail)) :
                    TableQuery.GenerateFilterCondition("ServiceEngineer", Constants.Equal, serviceEngineerEmail);
            }

            // Add Status clause if status is passed a parameter.
            // Individual status clauses are appended with OR Condition
            if (status != null)
            {
                foreach (var state in status)
                {
                    statusQueries.Add(TableQuery.GenerateFilterCondition("Status", Constants.Equal, state));
                }
                var statusQuery = string.Join(string.Format("{0} ", TableOperators.Or), statusQueries);

                finalQuery = !string.IsNullOrWhiteSpace(finalQuery) ?
                    string.Format("{0} {1} ({2})", finalQuery, TableOperators.And, statusQuery) :
                    string.Format("({0})", statusQuery);
            }

            return finalQuery;
        }

        public static string GetDashboardAuditQuery(string email = "")
        {
            var finalQuery = string.Empty;

            // Add Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(email))
            {
                finalQuery = TableQuery.GenerateFilterCondition("ServiceEngineer", Constants.Equal, email);
            }

            return finalQuery;
        }

        public static string GetDashboardServiceEngineersQuery(List<string> status)
        {
            var finalQuery = string.Empty;
            var statusQueries = new List<string>();

            // Add Status clause if status is passed a parameter.
            foreach (var state in status)
            {
                statusQueries.Add(TableQuery.GenerateFilterCondition("Status", Constants.Equal, state));
            }
            finalQuery = string.Join(string.Format("{0} ", TableOperators.Or), statusQueries);

            return finalQuery;
        }

        public static string GetServiceRequestDetailsQuery(string id)
        {
            return TableQuery.GenerateFilterCondition("RowKey", Constants.Equal, id);
        }

        public static string GetServiceRequestAuditDetailsQuery(string id)
        {
            return TableQuery.GenerateFilterCondition("PartitionKey", Constants.Equal, id);
        }
    }
}
