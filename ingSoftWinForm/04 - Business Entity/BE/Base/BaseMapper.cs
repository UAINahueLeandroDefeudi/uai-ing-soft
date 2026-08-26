using System.Data;

namespace BE.Base
{
    public abstract class BaseMapper<TEntity>
    {
        public abstract TEntity MapToEntity(DataRow row);

        public IEnumerable<TEntity> MapAll(DataTable dataTable)
        {
            var results = new List<TEntity>();
            foreach (DataRow row in dataTable.Rows)
                results.Add(MapToEntity(row));

            return results;
        }

        public TEntity? MapToEntity(DataTable dataTable)
            => MapAll(dataTable).FirstOrDefault();
    }
}
