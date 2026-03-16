using System;
using System.Linq;
using System.Threading.Tasks;
using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Storekeepertest
{
    [TestFixture]
    public class StorekeeperServiceTests
    {
      

        [Test]
        public async Task CreateBinAsync_ShouldCreateBin_WhenWarehouseExists()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EWMDbCtx>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var ctx = new EWMDbCtx(options))
            {
                // seed warehouse
                var warehouse = new Warehouse { WarehouseName = "WSeed", Description = "d", PhysicalLocation = "L" };
                ctx.Warehouses.Add(warehouse);
                await ctx.SaveChangesAsync();

                var svc = new StorekeeperService(ctx, NullLogger<StorekeeperService>.Instance);
                var req = new CreateBinReq { WarehouseID = warehouse.WarehouseId, LocationInWarehouse = "NewLoc" };

                var res = await svc.CreateBinAsync(req);

                Assert.IsTrue(res.Success);
                Assert.IsNotNull(res.Data);

                using var verifyCtx = new EWMDbCtx(options);
                var bin = await verifyCtx.Bins.Include(b => b.Warehouse).FirstOrDefaultAsync(b => b.BinId == res.Data.ID);
                Assert.IsNotNull(bin);
                Assert.AreEqual(req.LocationInWarehouse, bin.LocationInWarehouse);
                Assert.AreEqual(BinStatus.Empty, bin.Status);
                Assert.AreEqual(warehouse.WarehouseId, bin.WarehouseId);
            }
        }

        [Test]
        public async Task CreateBinAsync_ShouldReturnInvalidRequest_WhenWarehouseDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EWMDbCtx>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var ctx = new EWMDbCtx(options))
            {
                var svc = new StorekeeperService(ctx, NullLogger<StorekeeperService>.Instance);
                var req = new CreateBinReq { WarehouseID = 9999, LocationInWarehouse = "LocX" };

                var res = await svc.CreateBinAsync(req);

                Assert.IsFalse(res.Success);
                Assert.AreEqual(ApiResultCode.InvalidRequest, res.ResultCode);
            }
        }

        // Tests for CreateComponentCategoryAsync
        [Test]
        public async Task CreateComponentCategoryAsync_ShouldCreateCategory_WhenNameIsValid()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EWMDbCtx>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var ctx = new EWMDbCtx(options))
            {
                var svc = new StorekeeperService(ctx, NullLogger<StorekeeperService>.Instance);
                string name = "Passive Components";

                var res = await svc.CreateComponentCategoryAsync(name);

                Assert.IsTrue(res.Success);
                Assert.IsNotNull(res.Data);
                Assert.AreEqual(name, res.Data.Name);

                using var verifyCtx = new EWMDbCtx(options);
                var cat = await verifyCtx.ComponentCategories.FirstOrDefaultAsync(c => c.CategoryName == name);
                Assert.IsNotNull(cat);
            }
        }

        [Test]
        public async Task CreateComponentCategoryAsync_ShouldReturnInvalidRequest_WhenNameIsEmpty()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EWMDbCtx>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var ctx = new EWMDbCtx(options))
            {
                var svc = new StorekeeperService(ctx, NullLogger<StorekeeperService>.Instance);
                string name = "  ";

                var res = await svc.CreateComponentCategoryAsync(name);

                Assert.IsFalse(res.Success);
                Assert.AreEqual(ApiResultCode.InvalidRequest, res.ResultCode);
            }
        }

        [Test]
        public async Task CreateComponentCategoryAsync_ShouldReturnInvalidRequest_WhenNameAlreadyExists()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EWMDbCtx>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var ctx = new EWMDbCtx(options))
            {
                string name = "Active Components";
                ctx.ComponentCategories.Add(new ComponentCategory { CategoryName = name });
                await ctx.SaveChangesAsync();

                var svc = new StorekeeperService(ctx, NullLogger<StorekeeperService>.Instance);

                var res = await svc.CreateComponentCategoryAsync(name);

                Assert.IsFalse(res.Success);
                Assert.AreEqual(ApiResultCode.InvalidRequest, res.ResultCode);
            }
        }
    }
}
