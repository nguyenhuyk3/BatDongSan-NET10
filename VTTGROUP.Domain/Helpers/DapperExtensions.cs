using System.Data;

namespace VTTGROUP.Domain.Helpers
{
    public static class DapperExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<dynamic> items)
        {
            var dataTable = new DataTable();
            if (items == null || !items.Any())
                return dataTable;

            var first = (IDictionary<string, object>)items.First();
            foreach (var key in first.Keys)
            {
                dataTable.Columns.Add(key, first[key]?.GetType() ?? typeof(object));
            }

            foreach (var item in items)
            {
                var dict = (IDictionary<string, object>)item;
                var row = dataTable.NewRow();
                foreach (var key in dict.Keys)
                {
                    row[key] = dict[key] ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
