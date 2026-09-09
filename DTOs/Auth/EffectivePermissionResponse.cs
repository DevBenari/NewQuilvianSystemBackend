namespace QuilvianSystemBackend.DTOs.Auth
{
    /// <summary>
    /// Satu pasangan kewenangan, memakai penamaan yang sama dengan
    /// <c>[AccessPermission(resource, action)]</c> pada action controller.
    /// </summary>
    /// <remarks>
    /// Sengaja <c>record</c>: kesetaraan strukturalnya dipakai untuk membuang duplikat ketika
    /// satu pengguna memegang jabatan di lebih dari satu unit dan kebijakannya bertumpang tindih.
    /// </remarks>
    public sealed record EffectivePermission(string Resource, string Action);

    /// <summary>
    /// Daftar kewenangan efektif milik pengguna yang sedang masuk.
    /// </summary>
    public sealed record EffectivePermissionSet(
        bool IsSuperAdmin,
        IReadOnlyList<EffectivePermission> Permissions)
    {
        public int TotalPermission => Permissions.Count;
    }
}
