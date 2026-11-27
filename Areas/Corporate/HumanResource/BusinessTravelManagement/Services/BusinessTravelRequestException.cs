using System;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    /// <summary>
    /// Penolakan aturan SPPD yang membawa kode status HTTP kontrak <c>v28</c> bagian 34.5
    /// (<c>403</c> bukan pemohon, <c>409</c> status tidak sah, atau kode dari Workflow V2).
    /// </summary>
    public sealed class BusinessTravelRequestException(int statusCode, string message) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }
}
