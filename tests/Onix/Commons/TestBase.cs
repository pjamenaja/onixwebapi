using System.Collections;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

using Onix.Api.Commons;
using Onix.Api.Factories;
using Onix.Api.Erp.Dao.Models;
using Onix.Api.Erp.Dao.Models.UnitTesting;

namespace Onix.Test.Commons
{
    public class TestBase
    {
        #region DBContext factory
        protected OnixDbContext getInmemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            OnixDbContext db = (OnixDbContext) FactoryDbContext.GetDbContextForTesting("Onix", options);
            return(db);
        }

        protected DbContextUnitTesting getInmemoryDbContextForQueryTesting(string dbName)
        {
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            DbContextUnitTesting db = (DbContextUnitTesting) FactoryDbContext.GetDbContextForTesting("OnixQueryUnitTesting", options);
            return(db);
        }
        #endregion DBContext factory

        protected IDatabaseQuery createQuery(DbContext db, string queryClassName)
        {  
            IDatabaseQuery query = FactoryDbOperation.GetQueryObject(queryClassName, db);  
            return(query);
        }

        protected ArrayList createAndQuery(DbContext db, string queryClassName, CTable dat)
        {  
            ArrayList arr = createAndQuery(db, queryClassName, dat, false);
            return(arr);
        }

        protected ArrayList createAndQuery(DbContext db, string queryClassName, CTable dat, bool chunkFlag)
        {
            IDatabaseQuery query = FactoryDbOperation.GetQueryObject(queryClassName, db);       
            ArrayList arr = query.Query(dat, chunkFlag);

            return(arr);
        }      

        protected ArrayList createAndQuery(DbContext db, string queryClassName, CTable dat, string pageNumberFieldName, int pageSize)
        {
            IDatabaseQuery query = FactoryDbOperation.GetQueryObject(queryClassName, db);
            query.SetPageChunk(pageNumberFieldName, pageSize);
            ArrayList arr = query.Query(dat, true);

            return(arr);
        }              
    }
}