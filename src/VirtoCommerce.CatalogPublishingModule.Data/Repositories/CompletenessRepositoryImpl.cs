using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogPublishingModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public class CompletenessRepositoryImpl : DbContextRepositoryBase<CatalogPublishingDbContext>, ICompletenessRepository
    {
        public CompletenessRepositoryImpl(CatalogPublishingDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<CompletenessEntryEntity> Entries => DbContext.Set<CompletenessEntryEntity>();
        public IQueryable<CompletenessDetailEntity> Details => DbContext.Set<CompletenessDetailEntity>();
        public IQueryable<CompletenessChannelEntity> Channels => DbContext.Set<CompletenessChannelEntity>().Include(x => x.Languages).Include(x => x.Currencies);

        public async Task<CompletenessChannelEntity[]> GetChannelsByIdsAsync(string[] ids)
        {
            return await Channels.Where(x => ids.Contains(x.Id)).ToArrayAsync();
        }

        public async Task<CompletenessEntryEntity[]> GetEntriesByIdsAsync(string[] ids)
        {
            return await Entries.Include(x => x.Channel).Include(x => x.Details).Where(x => ids.Contains(x.Id)).ToArrayAsync();
        }

        public async Task DeleteChannelsAsync(string[] ids)
        {
            await ExecuteStoreQueryAsync("DELETE FROM CompletenessChannel WHERE Id IN ({0})", ids);
        }


        protected virtual async Task<int> ExecuteStoreQueryAsync(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var command = CreateCommand(commandTemplate, parameterValues);
            return await DbContext.Database.ExecuteSqlRawAsync(command.Text, command.Parameters.ToArray());
        }

        protected virtual Command CreateCommand(string commandTemplate, IEnumerable<string> parameterValues)
        {
            var parameters = parameterValues.Select((v, i) => new SqlParameter($"@p{i}", v)).ToArray();
            var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));

            return new Command
            {
                Text = string.Format(commandTemplate, parameterNames),
                Parameters = parameters.OfType<object>().ToList(),
            };
        }

        protected SqlParameter[] AddArrayParameters<T>(Command cmd, string paramNameRoot, IEnumerable<T> values)
        {
            /* An array cannot be simply added as a parameter to a SqlCommand so we need to loop through things and add it manually. 
             * Each item in the array will end up being it's own SqlParameter so the return value for this must be used as part of the
             * IN statement in the CommandText.
             */
            var parameters = new List<SqlParameter>();
            var parameterNames = new List<string>();
            var paramNbr = 1;
            foreach (var value in values)
            {
                var paramName = $"{paramNameRoot}{paramNbr++}";
                parameterNames.Add(paramName);
                var p = new SqlParameter(paramName, value);
                cmd.Parameters.Add(p);
                parameters.Add(p);
            }
            cmd.Text = cmd.Text.Replace(paramNameRoot, string.Join(",", parameterNames));

            return parameters.ToArray();
        }

        protected class Command
        {
            public string Text { get; set; }
            public IList<object> Parameters { get; set; } = new List<object>();
        }
    }
}
