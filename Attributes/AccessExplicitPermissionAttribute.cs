namespace QuilvianSystemBackend.Attributes
{
    /// <summary>
    /// Hak akses <b>penanda</b>: pasangan <c>(resource, action)</c> kanonik yang dibaca service,
    /// tetapi tidak menempel pada satu endpoint pun.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sebagian aturan keselamatan menjawab "orang ini dihitung sebagai apa", bukan "orang ini
    /// boleh memanggil endpoint apa", sehingga tidak ada action controller yang pantas
    /// menampungnya. Contoh pertamanya <c>RadReport : ActAsRadiologist</c> (RAD-DEC-015):
    /// dibaca <c>RadReportService</c> lewat <c>HasAccessAsync</c>, dan sengaja TIDAK digabung
    /// dengan <c>RadReport : Validate</c> karena "boleh mencoba mengesahkan" dan "dihitung
    /// sebagai dokter radiolog" adalah dua kewenangan berbeda.
    /// </para>
    /// <para>
    /// <b>Kenapa ini atribut level assembly, bukan daftar di dalam seeder.</b> Identitas
    /// kanonik hanya boleh punya SATU otoritas penemuan. Bila penanda semacam ini
    /// dideklarasikan di dalam <c>AccessMenuSeeder</c>, maka seeder melihatnya sementara
    /// <c>PermissionRegistryDescriptor.BuildFromAssembly</c> — yang dipakai authorization
    /// verifier — tidak. Registry versi seeder dan versi verifier lalu berbeda secara permanen,
    /// dan selisihnya muncul selamanya sebagai <c>DB_ONLY_ACTIVE</c> pada audit drift.
    /// </para>
    /// <para>
    /// Karena atribut ini melekat pada assembly, kedua jalur penemuan membacanya dari sumber
    /// yang sama: <c>Build(provider)</c> lewat assembly milik controller yang dipindainya, dan
    /// <c>BuildFromAssembly(assembly)</c> lewat assembly yang diberikan kepadanya. Keduanya
    /// bermuara pada <c>BuildCore</c> yang sama.
    /// </para>
    /// <para>
    /// <b>Penanda ini membuat kemampuan DAPAT DIBERIKAN, bukan otomatis diberikan.</b> Ia hanya
    /// menambah baris <c>SysActionAccess</c> supaya dapat dicentang pada layar Akses Role.
    /// Tidak satu pun baris <c>SysAccessPolicy</c> dibuat karenanya — sama seperti seluruh
    /// identitas lain.
    /// </para>
    /// <para>
    /// <c>ResourceName</c> wajib menunjuk resource yang SUDAH terdaftar dari pemindaian
    /// endpoint pada modul yang sama. Penanda yang menunjuk resource tak dikenal ditolak keras
    /// saat registry disusun, karena penanda yang gagal terdaftar menghasilkan <c>403</c>
    /// permanen yang tidak dapat diperbaiki dari layar mana pun.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class AccessExplicitPermissionAttribute : Attribute
    {
        public AccessExplicitPermissionAttribute(
            string moduleCode,
            string resourceName,
            string actionName,
            string displayName,
            string accessType)
        {
            ModuleCode = moduleCode;
            ResourceName = resourceName;
            ActionName = actionName;
            DisplayName = displayName;
            AccessType = accessType;
        }

        /// <summary>Modul tempat resource-nya terdaftar. Wajib sama dengan modul controllernya.</summary>
        public string ModuleCode { get; }

        /// <summary>Resource kanonik. Wajib sudah terdaftar dari pemindaian endpoint.</summary>
        public string ResourceName { get; }

        /// <summary>Action kanonik. Inilah yang dibaca <c>HasAccessAsync</c>.</summary>
        public string ActionName { get; }

        public string DisplayName { get; }

        /// <summary>Menentukan kolom pada layar Akses Role. Lihat <c>AccessTypes</c>.</summary>
        public string AccessType { get; }

        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }
}
