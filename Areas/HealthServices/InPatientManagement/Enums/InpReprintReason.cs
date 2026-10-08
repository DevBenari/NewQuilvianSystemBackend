namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>Alasan cetak ulang — backend 13.9 (<c>BE-RWI-192</c>, <c>VAL-RWA-40</c>, <c>41</c>).</summary>
    public enum InpReprintReason
    {
        /// <summary>Rusak.</summary>
        Damaged = 1,

        /// <summary>Hilang.</summary>
        Lost = 2,

        /// <summary>Data berubah.</summary>
        DataChanged = 3,

        /// <summary>Lainnya — keterangan wajib.</summary>
        Other = 4
    }
}
