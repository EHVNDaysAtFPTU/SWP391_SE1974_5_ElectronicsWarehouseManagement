using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Helpers;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicsWarehouseManagement.WebAPI.Services

{

    public interface IManagerService
    {
        // Get element
        Task<ApiResult<ComponentResp>> GetComponentAsync(int componentId, bool fullInfo);
        Task<ApiResult<TransferRequestResp>> GetTransferAsync(int transferId, bool fullInfo);
        Task<ApiResult<BinResp>> GetBin(int binId, bool fullInfo);
        Task<ApiResult<WarehouseResp>> GetWareHouseAsync(int warehouseId, bool fullInfo);
        // Get list element
        Task<ApiResult<PagedResult<ComponentResp>>> GetComponentListAsync(PagingRequest request);
        Task<ApiResult<PagedResult<TransferRequestResp>>> GetTransferReqListAsync(PagingRequest request);
        Task<ApiResult<PagedResult<BinResp>>> GetBinList(PagingRequest request);
        Task<ApiResult<PagedResult<BinResp>>> GetBinListByWareHouseId(PagingRequest request, int warehouseId, bool fullInfo);
        Task<ApiResult<PagedResult<WarehouseResp>>> GetWareHouseListAsync(PagingRequest request);
        // Crud 
        Task<ApiResult> PostTransferDecisionAsync(int transferId, TransferDecisionType decision, int? approverId);

        // Dashboard
        Task<ApiResult<DashboardSummaryResp>> GetSummaryAsync();
        Task<ApiResult<DashboardChartResp>> GetChartDataAsync(int days);
        Task<byte[]> ExportTransferPdfAsync(int transferId);
        Task<byte[]> ExportInventoryPdfAsync();
        Task<byte[]> ExportStatisticsPdfAsync(int days);

    }

    public class ManagerService : IManagerService
    {
        private readonly EWMDbCtx _dbCtx;

        public ManagerService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<ApiResult<ComponentResp>> GetComponentAsync(int componentId, bool fullInfo)
        {
            var component = await _dbCtx.Components
               .AsNoTracking()
               .Include(i=> i.Categories)
               .Include(i=> i.ComponentBins)
               .Where(i => i.ComponentId == componentId)
               .Select(i => new ComponentResp(i, fullInfo))
               .FirstOrDefaultAsync();

            if (component == null)
                return new ApiResult<ComponentResp>(ApiResultCode.NotFound, "Component not found");

            return new ApiResult<ComponentResp>(component);
        }


        public async Task<ApiResult<PagedResult<ComponentResp>>> GetComponentListAsync(PagingRequest request)
        {
            var query = _dbCtx.Components
                .Include(i => i.Categories)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(c => c.MetadataJson.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "unitPrice":
                        query = query.ApplySort(request.SortDirection, c => c.UnitPrice);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.ComponentId);
                        break;
                }
            }

                int totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<ComponentResp>>(new PagedResult<ComponentResp>
                {
                    data = new List<ComponentResp>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                });
            }

                var data = await query.ApplyPaging(request).Select(i => new ComponentResp(i, false)).ToListAsync();

                var pagedResult = new PagedResult<ComponentResp>
                {
                    data = data,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return new ApiResult<PagedResult<ComponentResp>>(pagedResult);
            }
        


        public async Task<ApiResult<TransferRequestResp>> GetTransferAsync(int requestId, bool fullInfo)
        {
            var query = _dbCtx.TransferRequests.AsNoTracking();

            if (fullInfo)
            {
                query = query
                    .Include(t => t.Approver)
                    .Include(t => t.Creator)
                    .Include(t => t.BinFrom)
                    .Include(t => t.BinTo)
                    .Include(t => t.TransferRequestComponents)
                        .ThenInclude(c => c.Component);
            }

            var transfer = await query
                .Where(t => t.RequestId == requestId)
                .Select(t => new TransferRequestResp(t, fullInfo))
                .FirstOrDefaultAsync();

            if (transfer == null)
                return new ApiResult<TransferRequestResp>(ApiResultCode.NotFound, "Transfer request not found");

            return new ApiResult<TransferRequestResp>(transfer);
        }

        public async Task<ApiResult<PagedResult<TransferRequestResp>>> GetTransferReqListAsync(PagingRequest request)
        {
            IQueryable<TransferRequest> query = _dbCtx.TransferRequests.AsNoTracking().AsQueryable()
                .Include(i => i.Approver)
                .Include(i => i.Creator)
                .Include(i => i.BinFrom)
                .Include(i => i.BinTo);
    
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(t => t.Description.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "creationTime":
                        query = query.ApplySort(request.SortDirection, c => c.CreationTime);
                        break;
                    case "type":
                        query = query.ApplySort(request.SortDirection, c => c.TypeInt);
                        break;
                    case "status":
                        query = query.ApplySort(request.SortDirection, c => c.StatusInt);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.RequestId);
                        break;
                }
            }
            int totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<TransferRequestResp>>(new PagedResult<TransferRequestResp>
                {
                    data = new List<TransferRequestResp>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                });
            }
            var data = await query
                .ApplyPaging(request)
                .Select(t => new TransferRequestResp(t, false))
                .ToListAsync();

            var pagedResult = new PagedResult<TransferRequestResp>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<TransferRequestResp>>(pagedResult);
        }


        private TransferStatus MapDecisionToStatus(TransferDecisionType decision)
        {
            return decision switch
            {
                TransferDecisionType.ApprovedAndWaitForConfirm  => TransferStatus.ApprovedAndWaitForConfirm,
                TransferDecisionType.Rejected => TransferStatus.Rejected,
            };
        }
        public async Task<ApiResult> PostTransferDecisionAsync(int transferId, TransferDecisionType decision, int? approverId)
        {

            var transferReq = await _dbCtx.TransferRequests.Include(i => i.Approver).FirstOrDefaultAsync(i => i.RequestId == transferId);

            if (transferReq == null)
            {
                return new ApiResult(ApiResultCode.NotFound);
            }
            var approver = await _dbCtx.Users.FirstOrDefaultAsync(u => u.UserId == approverId); 
            if (approver == null) 
            { 
                return new ApiResult(ApiResultCode.NotFound, "Approver not found"); 
            } 
            string approverUsername = approver.Username;
            switch (decision)
            {
                case TransferDecisionType.ApprovedAndWaitForConfirm:
                    transferReq.StatusInt = (int)MapDecisionToStatus(decision);
                    transferReq.ApproverId = approverId;
                    break;
                case TransferDecisionType.Rejected:
                    transferReq.StatusInt = (int)MapDecisionToStatus(decision);
                    transferReq.ApproverId = approverId;
                    break;
                default:
                    return new ApiResult(ApiResultCode.InvalidRequest);
            }
            await _dbCtx.SaveChangesAsync();

            return new ApiResult();
        }

        public async Task<ApiResult<BinResp>> GetBin(int binId, bool fullInfo)
        {
            var bin = await _dbCtx.Bins.AsNoTracking()
                .Include(b => b.Warehouse)
                .Include(b => b.ComponentBins)
                    .ThenInclude(cb => cb.Component)
                .FirstOrDefaultAsync(b => b.BinId == binId);

            if (bin == null)
                return new ApiResult<BinResp>(ApiResultCode.NotFound, "Bin not found");

            return new ApiResult<BinResp>(new BinResp(bin, fullInfo));
        }

        public async Task<ApiResult<PagedResult<BinResp>>> GetBinListByWareHouseId(PagingRequest request, int warehouseId, bool fullInfo)
        {
            var query = _dbCtx.Bins
                .Where(b => b.WarehouseId == warehouseId)
                .Include(b => b.Warehouse)
                .Include(b => b.ComponentBins).ThenInclude(c => c.Component).AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(c => c.LocationInWarehouse.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "status":
                        query = query.ApplySort(request.SortDirection, c => c.StatusInt);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.Warehouse.WarehouseName);
                        break;
                }
            }
            int totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<BinResp>>(ApiResultCode.NotFound);
            }

            var data = await query
                .ApplyPaging(request)
                .Select(b => new BinResp(b, fullInfo))
                .ToListAsync();

            var pagedResult = new PagedResult<BinResp>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<BinResp>>(pagedResult);
        }

        public async Task<ApiResult<PagedResult<WarehouseResp>>> GetWareHouseListAsync(PagingRequest request)
        {
            var query = _dbCtx.Warehouses.AsNoTracking();
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(c => c.WarehouseName.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "warehouseName":
                        query = query.ApplySort(request.SortDirection, c => c.WarehouseName);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.WarehouseId);
                        break;
                }
            }

            var totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<WarehouseResp>>(new PagedResult<WarehouseResp>
                {
                    data = new List<WarehouseResp>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                });
            }

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize).ToListAsync();

            var warehouseList = data.Select(i => new WarehouseResp(i, false)).ToList();
            var pagedResult = new PagedResult<WarehouseResp>
            {
                data = warehouseList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            return new ApiResult<PagedResult<WarehouseResp>>(pagedResult);

        }

        public async Task<ApiResult<WarehouseResp>> GetWareHouseAsync(int warehouseId, bool fullInfo)
        {
            var warehouse = await _dbCtx.Warehouses.AsNoTracking()
                .Where(i => i.WarehouseId == warehouseId)
                .Select(i => new WarehouseResp(i, fullInfo))
                .FirstOrDefaultAsync();
            if (warehouse == null)
            {
                return new ApiResult<WarehouseResp>(ApiResultCode.NotFound);
            }
            return new ApiResult<WarehouseResp>(warehouse);
        }

        public async Task<ApiResult<PagedResult<BinResp>>> GetBinList(PagingRequest request)
        {
            var query = _dbCtx.Bins
                .Include(b => b.Warehouse)
                .Include(b => b.ComponentBins).ThenInclude(c => c.Component).AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(c => c.LocationInWarehouse.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "status":
                        query = query.ApplySort(request.SortDirection, c => c.StatusInt);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.LocationInWarehouse);
                        break;
                }
            }
            int totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<BinResp>>(ApiResultCode.NotFound);
            }

            var data = await query
                .ApplyPaging(request)
                .Select(b => new BinResp(b, false))
                .ToListAsync();

            var pagedResult = new PagedResult<BinResp>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<BinResp>>(pagedResult);
        }

        public async Task<ApiResult<DashboardSummaryResp>> GetSummaryAsync()
        {
            var start = DateTime.Today;
            var end = start.AddDays(1);

            var resp = new DashboardSummaryResp
            {
                TotalComponents = await _dbCtx.Components.CountAsync(),

                TotalWarehouses = await _dbCtx.Warehouses.CountAsync(),

                CurrentStock = await _dbCtx.ComponentBins.SumAsync(cb => cb.Quantity),

                LowStockItems = await _dbCtx.ComponentBins
                    .Where(cb => cb.Quantity < 10)
                    .CountAsync(),

                OutOfStockItems = await _dbCtx.ComponentBins
                    .Where(cb => cb.Quantity == 0)
                    .CountAsync(),

                InboundToday = await _dbCtx.TransferRequests
                    .Where(tr => tr.TypeInt == (int)TransferType.Inbound
                        && tr.CreationTime >= start
                        && tr.CreationTime < end)
                    .SelectMany(tr => tr.TransferRequestComponents)
                    .SumAsync(c => c.Quantity),

                OutboundToday = await _dbCtx.TransferRequests
                    .Where(tr => tr.TypeInt == (int)TransferType.Outbound
                        && tr.CreationTime >= start
                        && tr.CreationTime < end)
                    .SelectMany(tr => tr.TransferRequestComponents)
                    .SumAsync(c => c.Quantity)
            };

            return new ApiResult<DashboardSummaryResp>(resp);
        }

        public async Task<ApiResult<DashboardChartResp>> GetChartDataAsync(int days)
        {
            var start = DateTime.Today.AddDays(-days);

            var data = await _dbCtx.TransferRequests
                .Where(tr => tr.CreationTime >= start)
                .SelectMany(tr => tr.TransferRequestComponents,
                    (tr, c) => new
                    {
                        Date = tr.CreationTime.Date,
                        Type = tr.TypeInt,
                        Quantity = c.Quantity
                    })
                .GroupBy(x => x.Date)
                .Select(g => new ImportExportChart
                {
                    Date = g.Key,
                    Import = g.Where(x => x.Type == (int)TransferType.Inbound)
                              .Sum(x => x.Quantity),

                    Export = g.Where(x => x.Type == (int)TransferType.Outbound)
                              .Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return new ApiResult<DashboardChartResp>(
                new DashboardChartResp
                {
                     transferChart = data
                });
        }


        private byte[] GeneratePdf(Action<Document> buildContent)
        {
            if (buildContent == null) throw new ArgumentNullException(nameof(buildContent));
            using var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            buildContent(doc);

            doc.Close();
            return stream.ToArray();
        }
        public async Task<byte[]> ExportTransferPdfAsync(int transferId)
        {
            var transfer = await _dbCtx.TransferRequests
                .Include(x => x.Creator)
                .Include(x => x.Approver)
                .Include(x => x.BinFrom).ThenInclude(x => x.Warehouse)
                .Include(x => x.BinTo).ThenInclude(x => x.Warehouse)
                .Include(x => x.TransferRequestComponents)
                    .ThenInclude(x => x.Component)
                .FirstOrDefaultAsync(x => x.RequestId == transferId);

            if (transfer == null)
                throw new Exception("Transfer request not found");

            using var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            var type = (TransferType)transfer.TypeInt;

            if (type == TransferType.Inbound)
                BuildInboundHeader(doc, transfer);
            else
                BuildOutBoundHeader(doc, transfer);

            BuildComponentTable(doc, transfer);

            doc.Close();

            return stream.ToArray();
        }
        private void BuildInboundHeader(Document doc, TransferRequest transfer)
        {
            doc.Add(new Paragraph("IMPORT RECEIPT")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(18));

            doc.Add(new Paragraph($"Request ID: {transfer.RequestId}"));
            doc.Add(new Paragraph($"Date: {transfer.CreationTime:dd/MM/yyyy}"));

            doc.Add(new Paragraph($"Supplier: {transfer.CustomerInfoJson ?? ""}"));
            doc.Add(new Paragraph($"Address: ..."));

            doc.Add(new Paragraph("\n"));

            var table = new Table(2).UseAllAvailableWidth();

            table.AddCell("Supplier");
            table.AddCell("Receiver");

            table.AddCell(transfer.CustomerInfoJson ?? "");
            table.AddCell(transfer.Creator?.DisplayName ?? "");

            doc.Add(table);

            doc.Add(new Paragraph("\n"));
        }
        private void BuildOutBoundHeader(Document doc, TransferRequest transfer)
        {
            doc.Add(new Paragraph("EXPORT RECEIPT")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(18));

            doc.Add(new Paragraph($"Request ID: {transfer.RequestId}"));
            doc.Add(new Paragraph($"Date: {transfer.CreationTime:dd/MM/yyyy}"));

            doc.Add(new Paragraph($"Customer: {transfer.CustomerInfoJson ?? ""}"));

            doc.Add(new Paragraph("\n"));

            var table = new Table(2).UseAllAvailableWidth();

            table.AddCell("Customer");
            table.AddCell("Invoice Writer");

            table.AddCell(transfer.CustomerInfoJson ?? "");
            table.AddCell(transfer.Creator?.DisplayName ?? "");

            doc.Add(table);

            doc.Add(new Paragraph("\n"));
        }
        private void BuildComponentTable(Document doc, TransferRequest transfer)
        {
            float[] widths = { 1, 4, 2, 2, 2, 2 };

            var table = new Table(UnitValue.CreatePercentArray(widths))
                .UseAllAvailableWidth();

            table.AddHeaderCell("No");
            table.AddHeaderCell("Component");
            table.AddHeaderCell("Unit");
            table.AddHeaderCell("Quantity");
            table.AddHeaderCell("Unit Price");
            table.AddHeaderCell("Total");

            int index = 1;
            double total = 0;

            foreach (var item in transfer.TransferRequestComponents)
            {
                double rowTotal = item.Quantity * item.UnitPrice;
                total += rowTotal;

                table.AddCell(index.ToString());
                table.AddCell(item.Component?.Metadata?.Name ?? "");
                table.AddCell(item.Component?.Unit ?? "");
                table.AddCell(item.Quantity.ToString());
                table.AddCell(item.UnitPrice.ToString("0.00"));
                table.AddCell(rowTotal.ToString("0.00"));

                index++;
            }

            table.AddCell(new Cell(1, 5).Add(new Paragraph("TOTAL")).SetBold());
            table.AddCell(total.ToString("0.00"));

            doc.Add(table);
        }
        private void BuildSummarySection(Document doc, DashboardSummaryResp summary)
        {
            doc.Add(new Paragraph("Inventory Summary")
                .SetBold()
                .SetFontSize(14));

            var table = new Table(2).UseAllAvailableWidth();

            table.AddCell("Total Components");
            table.AddCell(summary.TotalComponents.ToString());

            table.AddCell("Total Warehouses");
            table.AddCell(summary.TotalWarehouses.ToString());

            table.AddCell("Current Total Stock");
            table.AddCell(summary.CurrentStock.ToString());

            table.AddCell("Low Stock Items");
            table.AddCell(summary.LowStockItems.ToString());

            table.AddCell("Out of Stock Items");
            table.AddCell(summary.OutOfStockItems.ToString());

            table.AddCell("Inbound Today");
            table.AddCell(summary.InboundToday.ToString());

            table.AddCell("Outbound Today");
            table.AddCell(summary.OutboundToday.ToString());

            doc.Add(table);
        }

        public async Task<byte[]> ExportInventoryPdfAsync()
        {
            var inventory = await _dbCtx.ComponentBins
                .Include(x => x.Component)
                .Include(x => x.Bin)
                .ToListAsync();

            return GeneratePdf(doc =>
            {
                doc.Add(new Paragraph("INVENTORY REPORT")
                    .SetBold()
                    .SetFontSize(18));

                var table = new Table(3).UseAllAvailableWidth();

                table.AddHeaderCell("Component");
                table.AddHeaderCell("Bin Location");
                table.AddHeaderCell("Quantity");

                foreach (var item in inventory)
                {
                    table.AddCell(item.Component?.Metadata?.Name ?? "N/A");
                    table.AddCell(item.Bin?.LocationInWarehouse ?? "N/A");
                    table.AddCell(item.Quantity.ToString());
                }

                doc.Add(table);
            });
        }

        public async Task<byte[]> ExportStatisticsPdfAsync(int days)
        {
            var summaryResult = await GetSummaryAsync();
            var chartResult = await GetChartDataAsync(days);

            if (summaryResult.ResultCode != ApiResultCode.Success && chartResult.ResultCode != ApiResultCode.Success)
            {
                throw new Exception("Unable to retrieve dashboard data for export.");
            }

            return GeneratePdf(doc =>
            {
                doc.Add(new Paragraph("WAREHOUSE STATISTICS REPORT")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBold()
                    .SetFontSize(18));

                doc.Add(new Paragraph($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}"));
                doc.Add(new Paragraph("\n"));

                if (summaryResult.ResultCode == ApiResultCode.Success)
                {
                    BuildSummarySection(doc, summaryResult.Data);
                }
                else
                {
                    doc.Add(new Paragraph("Summary data currently unavailable."));
                }

                doc.Add(new Paragraph("\n"));

                if (chartResult.ResultCode == ApiResultCode.Success && chartResult.Data?.transferChart?.Any() == true)
                {
                    BuildChartSection(doc, chartResult.Data);
                }
                else
                {
                    doc.Add(new Paragraph("No transfer activity found for the requested period."));
                }
            });
        }

        private void BuildChartSection(Document doc, DashboardChartResp chartData)
        {
            doc.Add(new Paragraph("Daily Transfer Statistics")
                .SetBold()
                .SetFontSize(14));

            float[] widths = { 4, 3, 3 };
            var table = new Table(UnitValue.CreatePercentArray(widths))
                .UseAllAvailableWidth();

            table.AddHeaderCell("Date");
            table.AddHeaderCell("Import (Qty)");
            table.AddHeaderCell("Export (Qty)");

            foreach (var record in chartData.transferChart)
            {
                table.AddCell(record.Date.ToString("dd/MM/yyyy"));
                table.AddCell(record.Import.ToString());
                table.AddCell(record.Export.ToString());
            }

            doc.Add(table);
        }
    }
}