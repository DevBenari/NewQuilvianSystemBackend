namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants
{
    /// <summary>
    /// Nilai <c>TravelStatus</c> SPPD yang dipakai alur persetujuan kontrak <c>v28</c>
    /// (state-transition-matrix bagian 32; kamus data 29.3). Status lanjutan sesudah disetujui
    /// tetap mengikuti source yang ada.
    /// </summary>
    public static class BusinessTravelRequestStatuses
    {
        public const string Draft = "Draft";

        /// <summary>Menunggu persetujuan; untuk SPPD lama dapat tanpa workflow (<c>HRD-DEC-227</c>).</summary>
        public const string Submitted = "Submitted";

        /// <summary>Dikembalikan penyetuju untuk direvisi; dapat diubah dan diajukan ulang.</summary>
        public const string NeedRevision = "NeedRevision";

        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";
        public const string Completed = "Completed";
    }
}
