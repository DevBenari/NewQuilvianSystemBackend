namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class PositionResponse
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreatePositionRequest
    {
        public Guid DepartmentId { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class UpdatePositionRequest
    {
        public Guid DepartmentId { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PositionOptionResponse
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;
    }
}
