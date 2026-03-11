using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Xunit;

namespace ElectronicsWarehouseManagement.Tests
{
    public class ManagerServiceTests : IDisposable
    {
        private readonly EWMDbCtx _dbCtx;
        private readonly ManagerService _service;

        public ManagerServiceTests()
        {
            // Thiết lập Database ảo (In-Memory)
            var options = new DbContextOptionsBuilder<EWMDbCtx>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbCtx = new EWMDbCtx(options);
            _service = new ManagerService(_dbCtx);

            // Seed dữ liệu mẫu cho các test case
            SeedData();
        }

        private void SeedData()
        {
            _dbCtx.Components.Add(new Repositories.Entities.Component { ComponentId = 1, Unit = "Piece", UnitPrice = 100.0, MetadataJson = "{}" });
            _dbCtx.Users.Add(new User { UserId = 10, Username = "manager_01", DisplayName = "Manager One", Email = "manager01@test.com", PasswordHash = "hash123", StatusInt = 1 });
            _dbCtx.TransferRequests.Add(new TransferRequest
            {
                RequestId = 100,
                Description = "Test Transfer",
                StatusInt = (int)TransferStatus.Pending,
                CreatorId = 10,
                CreationTime = DateTime.Now,
                TypeInt = 0
            });
            _dbCtx.SaveChanges();
        }

        // --- Chức năng 1: GetComponentAsync ---

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
            Assert.Equal(ApiResultCode.NotFound, result.ResultCode);
            Assert.Contains("not found", result.Message.ToLower());
        }

        // --- Chức năng 2: PostTransferDecisionAsync ---

        [Fact]
        public async Task PostTransferDecisionAsync_ApproveDecision_UpdatesStatusAndApprover()
        {
            // Arrange
            int transferId = 100;
            int approverId = 10;
            var decision = TransferDecisionType.ApprovedAndWaitForConfirm;

            // Act
            var result = await _service.PostTransferDecisionAsync(transferId, decision, approverId);

            // Assert
            Assert.True(result.Success);

            // Kiểm tra DB đã thực sự thay đổi chưa
            var updatedRequest = await _dbCtx.TransferRequests.FindAsync(transferId);
            Assert.Equal((int)TransferStatus.ApprovedAndWaitForConfirm, updatedRequest.StatusInt);
            Assert.Equal(approverId, updatedRequest.ApproverId);
        }

        [Fact]
        public async Task PostTransferDecisionAsync_NonExistingApprover_ReturnsNotFound()
        {
            // Arrange
            int transferId = 100;
            int invalidApproverId = 55; // ID không có trong SeedData

            // Act
            var result = await _service.PostTransferDecisionAsync(transferId, TransferDecisionType.Rejected, invalidApproverId);

            // Assert
            Assert.Equal(ApiResultCode.NotFound, result.ResultCode);
            Assert.Equal("Approver not found", result.Message);
        }

        public void Dispose()
        {
            _dbCtx.Database.EnsureDeleted();
            _dbCtx.Dispose();
        }
    }
}