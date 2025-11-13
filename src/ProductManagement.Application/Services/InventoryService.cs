using Microsoft.Extensions.Logging;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enum;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.Repositories;

namespace ProductManagement.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IInventoryItemRepository _inventoryRepo;
    private readonly IStockTransferRepository _transferRepo;
    private readonly IInventoryAuditRepository _auditRepo;
    private readonly IProductRepository _productRepo;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IWarehouseRepository warehouseRepo,
        IInventoryItemRepository inventoryRepo,
        IStockTransferRepository transferRepo,
        IInventoryAuditRepository auditRepo,
        IProductRepository productRepo,
        IMessagePublisher messagePublisher,
        ILogger<InventoryService> logger)
    {
        _warehouseRepo = warehouseRepo;
        _inventoryRepo = inventoryRepo;
        _transferRepo = transferRepo;
        _auditRepo = auditRepo;
        _productRepo = productRepo;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    // ============================================
    // Warehouse Management
    // ============================================
    
    public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto dto)
    {
        var warehouse = new Warehouse(
            dto.Code,
            dto.Name,
            dto.Address,
            dto.City,
            dto.State,
            dto.ZipCode,
            dto.ContactPerson,
            dto.Phone,
            dto.Priority
        );

        await _warehouseRepo.AddAsync(warehouse);
        await PublishDomainEvents(warehouse);

        _logger.LogInformation("Warehouse created: {Code} - {Name}", dto.Code, dto.Name);

        return MapToDto(warehouse);
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
    {
        var warehouses = await _warehouseRepo.GetAllAsync();
        return warehouses.Select(MapToDto);
    }

    public async Task<WarehouseDto?> GetWarehouseByIdAsync(Guid id)
    {
        var warehouse = await _warehouseRepo.GetByIdAsync(id);
        return warehouse == null ? null : MapToDto(warehouse);
    }

    public async Task<WarehouseDto?> GetWarehouseByCodeAsync(string code)
    {
        var warehouse = await _warehouseRepo.GetByCodeAsync(code);
        return warehouse == null ? null : MapToDto(warehouse);
    }

    // ============================================
    // Inventory Item Management
    // ============================================
    
    public async Task<InventoryItemDto> CreateInventoryItemAsync(CreateInventoryItemDto dto)
    {
        // Validate warehouse exists
        var warehouse = await _warehouseRepo.GetByIdAsync(dto.WarehouseId);
        if (warehouse == null)
            throw new WarehouseNotFoundException(dto.WarehouseId);

        // Validate product exists
        var product = await _productRepo.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new ProductNotFoundException(dto.ProductId);

        var inventoryItem = new InventoryItem(
            dto.WarehouseId,
            dto.ProductId,
            dto.InitialQuantity,
            dto.ReorderPoint,
            dto.ReorderQuantity
        );

        await _inventoryRepo.AddAsync(inventoryItem);
        await PublishDomainEvents(inventoryItem);

        _logger.LogInformation(
            "Inventory item created: Product {ProductId} in Warehouse {WarehouseId}, Qty: {Quantity}",
            dto.ProductId, dto.WarehouseId, dto.InitialQuantity);

        return MapToDto(inventoryItem, warehouse.Code, product.Name);
    }

    public async Task<IEnumerable<InventoryItemDto>> GetInventoryByProductAsync(Guid productId)
    {
        var items = await _inventoryRepo.GetByProductIdAsync(productId);
        var result = new List<InventoryItemDto>();

        foreach (var item in items)
        {
            var warehouse = await _warehouseRepo.GetByIdAsync(item.WarehouseId);
            var product = await _productRepo.GetByIdAsync(item.ProductId);
            result.Add(MapToDto(item, warehouse?.Code ?? "Unknown", product?.Name ?? "Unknown"));
        }

        return result;
    }

    public async Task<IEnumerable<InventoryItemDto>> GetInventoryByWarehouseAsync(Guid warehouseId)
    {
        var items = await _inventoryRepo.GetByWarehouseIdAsync(warehouseId);
        var warehouse = await _warehouseRepo.GetByIdAsync(warehouseId);
        var result = new List<InventoryItemDto>();

        foreach (var item in items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId);
            result.Add(MapToDto(item, warehouse?.Code ?? "Unknown", product?.Name ?? "Unknown"));
        }

        return result;
    }

    public async Task<int> GetTotalAvailableStockAsync(Guid productId)
    {
        return await _inventoryRepo.GetTotalAvailableStockAsync(productId);
    }

    // ============================================
    // Stock Operations
    // ============================================
    
    public async Task AdjustStockAsync(Guid inventoryItemId, AdjustStockDto dto)
    {
        var item = await _inventoryRepo.GetByIdAsync(inventoryItemId);
        if (item == null)
            throw new InventoryItemNotFoundException(inventoryItemId);

        item.AdjustStock(dto.Adjustment, dto.Reason, dto.UserId);
        
        await _inventoryRepo.UpdateAsync(item);
        await PublishDomainEvents(item);

        _logger.LogInformation(
            "Stock adjusted: Item {ItemId}, Adjustment: {Adjustment}, Reason: {Reason}",
            inventoryItemId, dto.Adjustment, dto.Reason);
    }

    public async Task RestockAsync(Guid inventoryItemId, int quantity)
    {
        var item = await _inventoryRepo.GetByIdAsync(inventoryItemId);
        if (item == null)
            throw new InventoryItemNotFoundException(inventoryItemId);

        item.Restock(quantity);
        
        await _inventoryRepo.UpdateAsync(item);
        await PublishDomainEvents(item);

        _logger.LogInformation("Stock restocked: Item {ItemId}, Quantity: {Quantity}", 
            inventoryItemId, quantity);
    }

    public async Task ReserveStockAsync(Guid orderId, List<OrderItemAllocation> allocations)
    {
        foreach (var allocation in allocations)
        {
            var item = await _inventoryRepo.GetByWarehouseAndProductAsync(
                allocation.WarehouseId, allocation.ProductId);

            if (item == null)
                throw new InventoryItemNotFoundException(allocation.ProductId, allocation.WarehouseId);

            item.Reserve(allocation.Quantity, orderId);
            
            await _inventoryRepo.UpdateAsync(item);
            await PublishDomainEvents(item);
        }

        _logger.LogInformation("Stock reserved for order {OrderId}", orderId);
    }

    public async Task ReleaseReservationAsync(Guid orderId)
    {
        var reservations = await _inventoryRepo.GetReservationsByOrderAsync(orderId);

        foreach (var (item, quantity) in reservations)
        {
            item.ReleaseReservation(quantity, orderId);
            await _inventoryRepo.UpdateAsync(item);
            await PublishDomainEvents(item);
        }

        _logger.LogInformation("Reservation released for order {OrderId}", orderId);
    }

    public async Task FulfillReservationAsync(Guid orderId)
    {
        var reservations = await _inventoryRepo.GetReservationsByOrderAsync(orderId);

        foreach (var (item, quantity) in reservations)
        {
            item.FulfillReservation(quantity, orderId);
            await _inventoryRepo.UpdateAsync(item);
            await PublishDomainEvents(item);
        }

        _logger.LogInformation("Reservation fulfilled for order {OrderId}", orderId);
    }

    // ============================================
    // Smart Stock Allocation Algorithm
    // ============================================
    
    public async Task<List<OrderItemAllocation>> AllocateStockForOrderAsync(
        List<OrderItemRequest> items,
        string customerCity)
    {
        var allocations = new List<OrderItemAllocation>();

        foreach (var item in items)
        {
            var productInventory = await _inventoryRepo.GetByProductIdAsync(item.ProductId);
            var activeWarehouses = await _warehouseRepo.GetActiveWarehousesAsync();

            // Filter warehouses with available stock
            var availableWarehouses = productInventory
                .Where(inv => inv.QuantityAvailable >= item.Quantity)
                .Join(activeWarehouses,
                    inv => inv.WarehouseId,
                    wh => wh.Id,
                    (inv, wh) => new { Inventory = inv, Warehouse = wh })
                .ToList();

            if (!availableWarehouses.Any())
            {
                // Try split allocation from multiple warehouses
                var splitAllocation = await TrySplitAllocation(item, productInventory, activeWarehouses);
                if (splitAllocation.Any())
                {
                    allocations.AddRange(splitAllocation);
                    continue;
                }

                throw new InsufficientStockException(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    productInventory.Sum(i => i.QuantityAvailable));
            }

            // Allocation Strategy: 
            // 1. Same city (lowest shipping cost)
            // 2. Highest priority warehouse
            // 3. Most stock (to consolidate)

            var selectedWarehouse = availableWarehouses
                .OrderByDescending(w => w.Warehouse.City.Equals(customerCity, StringComparison.OrdinalIgnoreCase))
                .ThenBy(w => w.Warehouse.Priority)
                .ThenByDescending(w => w.Inventory.QuantityAvailable)
                .First();

            allocations.Add(new OrderItemAllocation(
                item.ProductId,
                selectedWarehouse.Warehouse.Id,
                selectedWarehouse.Warehouse.Code,
                item.Quantity
            ));

            _logger.LogInformation(
                "Allocated {Quantity}x {ProductName} from {WarehouseCode}",
                item.Quantity, item.ProductName, selectedWarehouse.Warehouse.Code);
        }

        return allocations;
    }

    private async Task<List<OrderItemAllocation>> TrySplitAllocation(
        OrderItemRequest item,
        IEnumerable<InventoryItem> productInventory,
        IEnumerable<Warehouse> activeWarehouses)
    {
        var allocations = new List<OrderItemAllocation>();
        var remainingQuantity = item.Quantity;

        var sortedInventory = productInventory
            .Where(inv => inv.QuantityAvailable > 0)
            .Join(activeWarehouses,
                inv => inv.WarehouseId,
                wh => wh.Id,
                (inv, wh) => new { Inventory = inv, Warehouse = wh })
            .OrderByDescending(x => x.Inventory.QuantityAvailable)
            .ToList();

        foreach (var source in sortedInventory)
        {
            if (remainingQuantity == 0) break;

            var allocateQty = Math.Min(remainingQuantity, source.Inventory.QuantityAvailable);
            
            allocations.Add(new OrderItemAllocation(
                item.ProductId,
                source.Warehouse.Id,
                source.Warehouse.Code,
                allocateQty
            ));

            remainingQuantity -= allocateQty;
        }

        if (remainingQuantity > 0)
        {
            return new List<OrderItemAllocation>(); // Cannot fulfill
        }

        _logger.LogInformation(
            "Split allocation for {ProductName}: {Count} warehouses",
            item.ProductName, allocations.Count);

        return allocations;
    }

    // ============================================
    // Stock Transfer
    // ============================================
    
    public async Task<StockTransferDto> RequestTransferAsync(RequestStockTransferDto dto)
    {
        // Validate from warehouse has stock
        var fromInventory = await _inventoryRepo.GetByWarehouseAndProductAsync(
            dto.FromWarehouseId, dto.ProductId);

        if (fromInventory == null || fromInventory.QuantityAvailable < dto.Quantity)
            throw new InsufficientStockException(
                dto.ProductId, "Product", dto.Quantity, fromInventory?.QuantityAvailable ?? 0);

        var transfer = new StockTransfer(
            dto.ProductId,
            dto.FromWarehouseId,
            dto.ToWarehouseId,
            dto.Quantity,
            dto.RequestedBy,
            dto.Notes
        );

        await _transferRepo.AddAsync(transfer);
        await PublishDomainEvents(transfer);

        _logger.LogInformation(
            "Transfer requested: {Quantity} units from {From} to {To}",
            dto.Quantity, dto.FromWarehouseId, dto.ToWarehouseId);

        return MapToDto(transfer);
    }

    public async Task ApproveTransferAsync(Guid transferId, string approvedBy)
    {
        var transfer = await _transferRepo.GetByIdAsync(transferId);
        if (transfer == null)
            throw new StockTransferNotFoundException(transferId);

        transfer.Approve(approvedBy);
        
        await _transferRepo.UpdateAsync(transfer);
        await PublishDomainEvents(transfer);

        _logger.LogInformation("Transfer {TransferId} approved by {ApprovedBy}", 
            transferId, approvedBy);
    }

    public async Task CompleteTransferAsync(Guid transferId)
    {
        var transfer = await _transferRepo.GetByIdAsync(transferId);
        if (transfer == null)
            throw new StockTransferNotFoundException(transferId);

        // Deduct from source warehouse
        var fromInventory = await _inventoryRepo.GetByWarehouseAndProductAsync(
            transfer.FromWarehouseId, transfer.ProductId);

        if (fromInventory != null)
        {
            fromInventory.AdjustStock(-transfer.Quantity, $"Transfer to warehouse", "SYSTEM");
            await _inventoryRepo.UpdateAsync(fromInventory);
            await PublishDomainEvents(fromInventory);
        }

        // Add to destination warehouse
        var toInventory = await _inventoryRepo.GetByWarehouseAndProductAsync(
            transfer.ToWarehouseId, transfer.ProductId);

        if (toInventory == null)
        {
            // Create inventory item if not exists
            toInventory = new InventoryItem(
                transfer.ToWarehouseId,
                transfer.ProductId,
                transfer.Quantity);
            await _inventoryRepo.AddAsync(toInventory);
        }
        else
        {
            toInventory.Restock(transfer.Quantity);
            await _inventoryRepo.UpdateAsync(toInventory);
        }
        
        await PublishDomainEvents(toInventory);

        // Mark transfer as completed
        transfer.Complete();
        await _transferRepo.UpdateAsync(transfer);
        await PublishDomainEvents(transfer);

        _logger.LogInformation("Transfer {TransferId} completed", transferId);
    }

    public async Task CancelTransferAsync(Guid transferId, string reason)
    {
        var transfer = await _transferRepo.GetByIdAsync(transferId);
        if (transfer == null)
            throw new StockTransferNotFoundException(transferId);

        transfer.Cancel(reason);
        
        await _transferRepo.UpdateAsync(transfer);
        await PublishDomainEvents(transfer);

        _logger.LogInformation("Transfer {TransferId} cancelled: {Reason}", transferId, reason);
    }

    public async Task<IEnumerable<StockTransferDto>> GetPendingTransfersAsync()
    {
        var transfers = await _transferRepo.GetByStatusAsync(StockTransferStatus.Pending);
        return transfers.Select(MapToDto);
    }

    // ============================================
    // Audit
    // ============================================
    
    public async Task<InventoryAuditDto> PerformAuditAsync(PerformAuditDto dto)
    {
        var inventoryItem = await _inventoryRepo.GetByWarehouseAndProductAsync(
            dto.WarehouseId, dto.ProductId);

        var expectedQuantity = inventoryItem?.QuantityOnHand ?? 0;

        var audit = new InventoryAudit(
            dto.WarehouseId,
            dto.ProductId,
            expectedQuantity,
            dto.ActualQuantity,
            dto.AuditedBy,
            dto.Notes
        );

        await _auditRepo.AddAsync(audit);

        // Adjust stock if variance found
        if (audit.Variance != 0 && inventoryItem != null)
        {
            inventoryItem.AdjustStock(
                audit.Variance,
                $"Audit adjustment. Variance: {audit.Variance}",
                dto.AuditedBy);
            
            await _inventoryRepo.UpdateAsync(inventoryItem);
            await PublishDomainEvents(inventoryItem);
        }

        _logger.LogInformation(
            "Audit performed: Warehouse {WarehouseId}, Product {ProductId}, Variance: {Variance}",
            dto.WarehouseId, dto.ProductId, audit.Variance);

        return MapToDto(audit);
    }

    public async Task<IEnumerable<InventoryAuditDto>> GetAuditHistoryAsync(
        Guid warehouseId, 
        Guid productId)
    {
        var audits = await _auditRepo.GetByWarehouseAndProductAsync(warehouseId, productId);
        return audits.Select(MapToDto);
    }

    // ============================================
    // Analytics
    // ============================================
    
    public async Task<InventoryHealthReportDto> GetInventoryHealthReportAsync()
    {
        var allItems = await _inventoryRepo.GetAllAsync();
        
        var totalItems = allItems.Count();
        var lowStockItems = allItems.Count(i => i.QuantityAvailable <= i.ReorderPoint);
        var outOfStockItems = allItems.Count(i => i.QuantityAvailable == 0);
        var totalValue = 0m; // Would need product prices

        return new InventoryHealthReportDto(
            totalItems,
            lowStockItems,
            outOfStockItems,
            totalValue,
            DateTime.UtcNow
        );
    }

    public async Task<IEnumerable<LowStockItemDto>> GetLowStockItemsAsync()
    {
        var lowStockItems = await _inventoryRepo.GetLowStockItemsAsync();
        var result = new List<LowStockItemDto>();

        foreach (var item in lowStockItems)
        {
            var warehouse = await _warehouseRepo.GetByIdAsync(item.WarehouseId);
            var product = await _productRepo.GetByIdAsync(item.ProductId);

            result.Add(new LowStockItemDto(
                item.Id,
                item.ProductId,
                product?.Name ?? "Unknown",
                item.WarehouseId,
                warehouse?.Code ?? "Unknown",
                item.QuantityAvailable,
                item.ReorderPoint,
                item.ReorderQuantity
            ));
        }

        return result;
    }

    // ============================================
    // Helper Methods
    // ============================================
    
    private async Task PublishDomainEvents(Warehouse warehouse)
    {
        foreach (var domainEvent in warehouse.DomainEvents)
        {
            await _messagePublisher.PublishAsync(domainEvent, "inventory-events");
        }
        warehouse.ClearDomainEvents();
    }

    private async Task PublishDomainEvents(InventoryItem item)
    {
        foreach (var domainEvent in item.DomainEvents)
        {
            await _messagePublisher.PublishAsync(domainEvent, "inventory-events");
        }
        item.ClearDomainEvents();
    }

    private async Task PublishDomainEvents(StockTransfer transfer)
    {
        foreach (var domainEvent in transfer.DomainEvents)
        {
            await _messagePublisher.PublishAsync(domainEvent, "inventory-events");
        }
        transfer.ClearDomainEvents();
    }

    private static WarehouseDto MapToDto(Warehouse warehouse) => new(
        warehouse.Id,
        warehouse.Code,
        warehouse.Name,
        warehouse.Address,
        warehouse.City,
        warehouse.State,
        warehouse.ZipCode,
        warehouse.ContactPerson,
        warehouse.Phone,
        warehouse.IsActive,
        warehouse.Priority,
        warehouse.CreatedAt
    );

    private static InventoryItemDto MapToDto(InventoryItem item, string warehouseCode, string productName) => new(
        item.Id,
        item.WarehouseId,
        warehouseCode,
        item.ProductId,
        productName,
        item.QuantityOnHand,
        item.QuantityReserved,
        item.QuantityAvailable,
        item.ReorderPoint,
        item.ReorderQuantity,
        item.LastRestockedAt
    );

    private static StockTransferDto MapToDto(StockTransfer transfer) => new(
        transfer.Id,
        transfer.ProductId,
        transfer.FromWarehouseId,
        transfer.ToWarehouseId,
        transfer.Quantity,
        transfer.Status.ToString(),
        transfer.Notes,
        transfer.RequestedBy,
        transfer.ApprovedBy,
        transfer.RequestedAt,
        transfer.ApprovedAt,
        transfer.CompletedAt,
        transfer.CancelledAt,
        transfer.CancellationReason
    );

    private static InventoryAuditDto MapToDto(InventoryAudit audit) => new(
        audit.Id,
        audit.WarehouseId,
        audit.ProductId,
        audit.ExpectedQuantity,
        audit.ActualQuantity,
        audit.Variance,
        audit.AuditedBy,
        audit.Notes,
        audit.AuditedAt
    );
}
public record OrderItemRequest(Guid ProductId, string ProductName, int Quantity);
public record OrderItemAllocation(Guid ProductId, Guid WarehouseId, string WarehouseCode, int Quantity);

