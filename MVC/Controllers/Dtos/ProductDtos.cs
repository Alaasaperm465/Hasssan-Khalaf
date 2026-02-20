namespace Frozen_Warehouse.API.Controllers.Dtos
{
    public class CreateProductDto { public string Name { get; set; } = null!; }
    public class ProductResponseDto { public int Id { get; set; } public string Name { get; set; } = null!; public bool IsActive { get; set; } }
    public class ProductOverlapDto { public int SourceProductId { get; set; } public int TargetProductId { get; set; } public int SectionId { get; set; } public int Cartons { get; set; } public int Pallets { get; set; } public int? ClientId { get; set; } public string? ClientName { get; set; } }
    public class ProductTransferDto { public int ClientId { get; set; } public int ProductId { get; set; } public int FromSectionId { get; set; } public int ToSectionId { get; set; } public int Cartons { get; set; } public int Pallets { get; set; } }
}
