namespace QuilvianSystemBackend.DTOs.Auth
{
    public class AttendanceCheckoutRequest
    {
        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public double? AccuracyMeters { get; set; }
    }
}
