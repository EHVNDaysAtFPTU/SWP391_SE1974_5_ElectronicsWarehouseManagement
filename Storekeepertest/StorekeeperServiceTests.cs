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
        public async Task CreateWarehouseAsync_ShouldCreateWarehouseWithDefaultBin()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EWMDbCtx>()
                .UseInMemoryDatabase(dbName)
                .Options;

            // Arrange
            using (var ctx = new EWMDbCtx(options))
            {
                var svc = new StorekeeperService(ctx, NullLogger<StorekeeperService>.Instance);
                var req = new CreateWarehouseReq
                {
                    Name = "WH1",
                    Description = "Test warehouse",
                    PhysicalLocation = "Loc1",
                    ImageUrl = "img.png"
                };

                // Act
                var res = await svc.CreateWarehouseAsync(req);

                // Assert
                Assert.IsTrue(res.Success, "CreateWarehouseAsync should succeed for valid request");
                Assert.IsNotNull(res.Data, "Response data should not be null");

                // Verify persisted data in a new context using same in-memory database
                using var verifyCtx = new EWMDbCtx(options);
                var wh = await verifyCtx.Warehouses.Include(w => w.Bins).FirstOrDefaultAsync(w => w.WarehouseId == res.Data.ID);
                Assert.IsNotNull(wh, "Warehouse should be persisted in the database");
                Assert.AreEqual(1, wh.Bins.Count, "A default bin should be created for the new warehouse");
                var bin = wh.Bins.First();
                Assert.AreEqual("Default Bin", bin.LocationInWarehouse);
                Assert.AreEqual(BinStatus.Empty, bin.Status);
            }
        }

        [Test]
        public async Task UpdateBinStatusAsync_ShouldReturnInvalidRequest_WhenSettingEmptyOnBinWithComponents()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<EWMDbCtx>()
                .UseInMemoryDatabase(dbName)
                .Options;

            // Arrange: seed a warehouse, bin and component with a ComponentBin (non-empty bin)
            using (var seedCtx = new EWMDbCtx(options))
            {
                var component = new Component { Unit = "pcs", UnitPrice = 1.0 };
                seedCtx.Components.Add(component);

                var warehouse = new Warehouse { WarehouseName = "W1", Description = "d", PhysicalLocation = "L" };
                var bin = new Bin { LocationInWarehouse = "B1", Status = BinStatus.Available };
                warehouse.Bins.Add(bin);

                // attach a ComponentBin to make the bin non-empty
                var cb = new ComponentBin { Component = component, Quantity = 5 };
                bin.ComponentBins.Add(cb);

                seedCtx.Warehouses.Add(warehouse);
                await seedCtx.SaveChangesAsync();

                var svc = new StorekeeperService(seedCtx, NullLogger<StorekeeperService>.Instance);

                // Act: try to set the bin status to Empty
                var result = await svc.UpdateBinStatusAsync(bin.BinId, BinStatus.Empty);

                // Assert
                Assert.IsFalse(result.Success, "Updating bin to Empty when it contains components should fail");
                Assert.AreEqual(ApiResultCode.InvalidRequest, result.ResultCode);
            }
        }
    }
}
