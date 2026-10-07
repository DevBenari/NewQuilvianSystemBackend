namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Status dokumen serah terima klinis transfer antarunit (<c>BE-RWI-183</c>, kamus data 19.3,
    /// state matrix 9.5). Status apa pun tidak pernah menahan transfer, keluar ruangan, atau
    /// penutupan episode (<c>INV-RWF-33</c>).
    /// </summary>
    public enum CliTransferHandoverStatus
    {
        NotSent = 1,
        Sent = 2,
        Accepted = 3,
        Rejected = 4
    }
}
