using System.Text.Json;
using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ElectronicsWarehouseManagement.Tests
{
    public class ManagerServiceTests : IDisposable
    {
        private readonly EWMDbCtx _dbCtx;
        private readonly ManagerService _service;

        private int _componentId;
        private int _managerId;
        private int _transferId;

        public ManagerServiceTests()
        {
            // Setup In-Memory Database
            var options = new DbContextOptionsBuilder<EWMDbCtx>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbCtx = new EWMDbCtx(options);
            _service = new ManagerService(_dbCtx);

            SeedData();
        }

        private void SeedData()
        {
            // Category
            var category1 = new ComponentCategory
            {
                CategoryName = "Resistor"
            };

            var category2 = new ComponentCategory
            {
                CategoryName = "Capacitor"
            };

            _dbCtx.ComponentCategories.AddRange(category1, category2);
            _dbCtx.SaveChanges();
            //Component
            var component = new Component
            {
                Unit = "Piece",
                UnitPrice = 100,
                MetadataJson = JsonSerializer.Serialize(new ComponentMetadata
                {
                    Name = "Test",
                    Description = "Test component",
                    Manufacturer = "TestCorp",
                    ManufacturingDate = DateTime.UtcNow
                }),
            };




            var component2 = new Component
            {
                Unit = "Prc",
                UnitPrice = 50.0,
                MetadataJson = JsonSerializer.Serialize(new ComponentMetadata
                {
                    Name = "Test 2",
                    Description = "Test component 2",
                    Manufacturer = "TestComp2",
                    ManufacturingDate = DateTime.Parse("2026-03-12"),
                    ImageUrl ="",
                    DatasheetUrl = ""
                }),
                 Categories = new List<ComponentCategory> { category1, category2 }
            };
            _dbCtx.Components.Add(component);
            _dbCtx.Components.Add(component2);

            //User
            var user = new User
            {
                Username = "manager_01",
                DisplayName = "Manager One",
                Email = "manager01@test.com",
                PasswordHash = "123",
                StatusInt = 1
            };
            _dbCtx.Users.Add(user);
            _dbCtx.SaveChanges();



            _componentId = component.ComponentId;
            _managerId = user.UserId;

            
            //Transfer
            var transfer = new TransferRequest
            {
                Description = "Test Transfer",
                StatusInt = (int)TransferStatus.Pending,
                CreatorId = _managerId,
                CreationTime = DateTime.Now,
                TypeInt = (int)TransferType.Inbound,
                BinFromId = null,
                BinToId = null,
                CustomerInfoJson = "{}"
            };
            _dbCtx.TransferRequests.Add(transfer);
            _dbCtx.SaveChanges();

            _transferId = transfer.RequestId;
        }

        // =============================
        // TEST: GetComponentAsync
        // =============================

        //[Fact]
        //public async Task GetComponentAsync_ValidId_ReturnSuccessAndCorrectData2()
        //{
        //    var componentId = 2;
        //    var manufracture = "TestComp2";
        //    var category1 = "Resistor";
        //    var result = await _service.GetComponentAsync(componentId, true);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(category1, result.Data.Categories.First().Name);
        //}

        [Fact]
        public async Task GetComponentAsync_ValidId_ReturnsSuccessAndCorrectData()
        {
            var manufracture = "TestCorp";
            // Act
            var result = await _service.GetComponentAsync(_componentId, false);

            // Assert
            Assert.NotNull(result.Data);
            Assert.True(result.Success);
            Assert.Equal(ApiResultCode.Success, result.ResultCode);
            Assert.NotNull(result.Data);
            Assert.Equal(manufracture, result.Data.Metadata.Manufacturer);
        }

        [Fact]
        public async Task GetComponentAsync_InvalidId_ReturnsNotFound()
        {
            // Arrange
            int invalidId = 9999;

            // Act
            var result = await _service.GetComponentAsync(invalidId, false);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(ApiResultCode.NotFound, result.ResultCode);
        }

        // =============================
        // TEST: PostTransferDecisionAsync
        // =============================

        [Fact]
        public async Task PostTransferDecisionAsync_ApproveDecision_UpdatesStatusAndApprover()
        {
            // Arrange
            var decision = TransferDecisionType.ApprovedAndWaitForConfirm;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                _transferId,
                decision,
                _managerId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(ApiResultCode.Success, result.ResultCode);

            var updatedRequest = await _dbCtx.TransferRequests.FindAsync(_transferId);

            Assert.NotNull(updatedRequest);
            Assert.Equal((int)TransferStatus.ApprovedAndWaitForConfirm, updatedRequest.StatusInt);
            Assert.Equal(_managerId, updatedRequest.ApproverId);
        }

        [Fact]
        public async Task PostTransferDecisionAsync_RejectedDecision_UpdatesStatus()
        {
            // Arrange
            var decision = TransferDecisionType.Rejected;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                _transferId,
                decision,
                _managerId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(ApiResultCode.Success, result.ResultCode);

            var updatedRequest = await _dbCtx.TransferRequests.FindAsync(_transferId);

            Assert.NotNull(updatedRequest);
            Assert.Equal((int)TransferStatus.Rejected, updatedRequest.StatusInt);
            Assert.Equal(_managerId, updatedRequest.ApproverId);
        }

        [Fact]
        public async Task PostTransferDecisionAsync_NonExistingApprover_ReturnsNotFound()
        {
            // Arrange
            int invalidApproverId = 5555;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                _transferId,
                TransferDecisionType.Rejected,
                invalidApproverId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(ApiResultCode.NotFound, result.ResultCode);
        }

        [Fact]
        public async Task PostTransferDecisionAsync_NonExistingTransferRequest_ReturnsNotFound()
        {
            // Arrange
            int invalidTransferId = 9999;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                invalidTransferId,
                TransferDecisionType.ApprovedAndWaitForConfirm,
                _managerId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(ApiResultCode.NotFound, result.ResultCode);
        }

        [Fact]
        public async Task PostTransferDecisionAsync_InvalidDecision_ReturnsInvalidRequest()
        {
            // Arrange
            var invalidDecision = (TransferDecisionType)999;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                _transferId,
                invalidDecision,
                _managerId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(ApiResultCode.InvalidRequest, result.ResultCode);
        }

        public void Dispose()
        {
            _dbCtx.Database.EnsureDeleted();
            _dbCtx.Dispose();
        }
    }
}