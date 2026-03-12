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
            _dbCtx.Components.Add(new Component
            {
                ComponentId = 1,
                Unit = "Piece",
                UnitPrice = 100.0,
                MetadataJson = "{}"
            });

            _dbCtx.Users.Add(new User
            {
                UserId = 10,
                Username = "manager_01",
                DisplayName = "Manager One",
                Email = "manager01@test.com",
                PasswordHash = "123",
                StatusInt = 1
            });

            _dbCtx.TransferRequests.Add(new TransferRequest
            {
                RequestId = 100,
                Description = "Test Transfer",
                StatusInt = (int)TransferStatus.Pending,
                CreatorId = 10,
                CreationTime = DateTime.Now,
                TypeInt = 0,
                BinFromId = null,
                BinToId = null
            });

            _dbCtx.SaveChanges();
        }

        // =============================
        // TEST: GetComponentAsync
        // =============================

        [Fact]
        public async Task GetComponentAsync_ValidId_ReturnsSuccessAndCorrectData()
        {
            // Arrange
            int componentId = 1;

            // Act
            var result = await _service.GetComponentAsync(componentId, false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(ApiResultCode.Success, result.ResultCode);
            Assert.NotNull(result.Data);
            Assert.Equal(100.0, result.Data.UnitPrice);
        }

        [Fact]
        public async Task GetComponentAsync_InvalidId_ReturnsNotFound()
        {
            // Arrange
            int componentId = 999;

            // Act
            var result = await _service.GetComponentAsync(componentId, false);

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
            int transferId = 100;
            int approverId = 10;
            var decision = TransferDecisionType.ApprovedAndWaitForConfirm;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                transferId,
                decision,
                approverId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(ApiResultCode.Success, result.ResultCode);

            var updatedRequest = await _dbCtx.TransferRequests.FindAsync(transferId);

            Assert.NotNull(updatedRequest);
            Assert.Equal((int)TransferStatus.ApprovedAndWaitForConfirm, updatedRequest.StatusInt);
            Assert.Equal(approverId, updatedRequest.ApproverId);
        }

        [Fact]
        public async Task PostTransferDecisionAsync_RejectedDecision_UpdatesStatus()
        {
            // Arrange
            int transferId = 100;
            int approverId = 10;
            var decision = TransferDecisionType.Rejected;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                transferId,
                decision,
                approverId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(ApiResultCode.Success, result.ResultCode);

            var updatedRequest = await _dbCtx.TransferRequests.FindAsync(transferId);

            Assert.NotNull(updatedRequest);
            Assert.Equal((int)TransferStatus.Rejected, updatedRequest.StatusInt);
            Assert.Equal(approverId, updatedRequest.ApproverId);
        }

        [Fact]
        public async Task PostTransferDecisionAsync_NonExistingApprover_ReturnsNotFound()
        {
            // Arrange
            int transferId = 100;
            int invalidApproverId = 55;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                transferId,
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
            int invalidTransferId = 999;
            int approverId = 10;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                invalidTransferId,
                TransferDecisionType.ApprovedAndWaitForConfirm,
                approverId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(ApiResultCode.NotFound, result.ResultCode);
        }

        [Fact]
        public async Task PostTransferDecisionAsync_InvalidDecision_ReturnsInvalidRequest()
        {
            // Arrange
            int transferId = 100;
            int approverId = 10;

            var invalidDecision = (TransferDecisionType)999;

            // Act
            var result = await _service.PostTransferDecisionAsync(
                transferId,
                invalidDecision,
                approverId);

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