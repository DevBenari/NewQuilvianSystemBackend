namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class DepartmentResponse
    {
        public Guid Id { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateDepartmentRequest
    {
        public string DepartmentName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class UpdateDepartmentRequest
    {
        public string DepartmentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DepartmentOptionResponse
    {
        public Guid Id { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;
    }
}
