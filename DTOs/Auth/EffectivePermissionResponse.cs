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
    /// <param name="UnrestrictedAccess">
    /// Benar hanya bila <c>HasAccessAsync</c> menjawab benar untuk pasangan APA PUN bagi pengguna ini: SuperAdmin
    /// tanpa <c>Security:Authorization:EnforceClinicalPolicyForSuperAdmin</c>. Daftar <see cref="Permissions"/> hanya
    /// memuat pasangan yang sudah terdaftar di registry, sedangkan penjaga menjawab benar juga untuk pasangan yang
    /// belum terdaftar (mis. backend belum dimulai ulang setelah endpoint baru ditambahkan); bendera ini membuat
    /// frontend mengikuti jawaban penjaga yang sebenarnya, bukan menebak dari nama peran. Bukan sama dengan
    /// <see cref="IsSuperAdmin"/>: dengan penegakan kebijakan klinis SuperAdmin TIDAK tak terbatas.
    /// </param>
    public sealed record EffectivePermissionSet(
        bool IsSuperAdmin,
        IReadOnlyList<EffectivePermission> Permissions,
        bool UnrestrictedAccess = false)
    {
        public int TotalPermission => Permissions.Count;
    }
}
