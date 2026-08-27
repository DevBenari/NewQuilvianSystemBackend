namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    public sealed class PharmacyDepotRoutingResult
    {
        public bool IsSuccess { get; private init; }

        public Guid? StorageLocationId { get; private init; }

        public string Code { get; private init; } = string.Empty;

        public string Message { get; private init; } = string.Empty;

        public static PharmacyDepotRoutingResult Success(Guid storageLocationId)
            => new()
            {
                IsSuccess = true,
                StorageLocationId = storageLocationId,
                Code = "PHA_ROUTE_RESOLVED",
                Message = "Depo Farmasi berhasil ditentukan."
            };

        public static PharmacyDepotRoutingResult Failure(string code, string message)
            => new()
            {
                IsSuccess = false,
                Code = code,
                Message = message
            };
    }
}
