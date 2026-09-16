namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Jenis catatan pada lembar terpadu (CPPT) — <c>BE-RWI-094</c>, <c>RWI-DEC-140</c>,
    /// <c>RWI-DEC-141</c>, <c>FR-DOK-085</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa jenis catatan dibutuhkan terpisah dari profesi penulisnya.</b> Satu profesi
    /// dapat menulis lebih dari satu bentuk catatan. Perawat menulis SOAP keperawatan <b>dan</b>
    /// catatan keperawatan naratif, dan keduanya dibaca dengan cara yang berbeda: yang pertama
    /// terstruktur per bagian, yang kedua berupa cerita. Tanpa penanda jenis, lini masa CPPT
    /// tidak dapat disaring, dan pembaca harus menerka bentuk setiap entri dari isinya.
    /// </para>
    /// <para>
    /// <b>Bawaannya <see cref="Unspecified"/>, dan itu bukan kemalasan.</b> Seluruh entri yang
    /// sudah ada sebelum kolom ini lahir memang tidak pernah punya jenis. Menebak jenisnya dari
    /// isi catatan adalah <b>pemalsuan data klinis</b>: sistem akan menyatakan "ini catatan
    /// perkembangan dokter" pada baris yang tidak pernah dinyatakan demikian oleh penulisnya.
    /// Karena itu entri lama dibiarkan <c>Unspecified</c> apa adanya, dan layar wajib
    /// menampilkannya sebagai "tidak ditentukan", bukan menyembunyikannya.
    /// </para>
    /// <para>
    /// <b>Bentuk enum ini dikunci di sini, bukan di sub-modul keperawatan.</b> Sub-modul
    /// <c>keperawatan</c> memakai <see cref="NursingSoap"/> dan <see cref="NursingNarrative"/>
    /// lewat <c>FR-KEP-077</c>. Nomornya karena itu tidak boleh bergeser: baris CPPT yang sudah
    /// tersimpan menunjuk angkanya, bukan namanya.
    /// </para>
    /// </remarks>
    public enum CpptNoteKind
    {
        /// <summary>
        /// Jenis tidak ditentukan. Nilai seluruh entri yang lahir sebelum kolom ini ada, dan
        /// <b>tidak pernah</b> diisi tebakan sesudahnya.
        /// </summary>
        Unspecified = 0,

        /// <summary>Catatan perkembangan yang ditulis dokter.</summary>
        PhysicianNote = 1,

        /// <summary>SOAP keperawatan — <c>FR-KEP-077</c>.</summary>
        NursingSoap = 2,

        /// <summary>Catatan keperawatan naratif — <c>FR-KEP-077</c>.</summary>
        NursingNarrative = 3,

        /// <summary>
        /// Catatan profesi lain: farmasi, gizi, fisioterapi, laboratorium, radiologi, dan
        /// seterusnya. Satu nilai untuk seluruhnya, karena bentuk catatannya sama — naratif per
        /// profesi — dan memecahnya per profesi hanya menduplikasi kolom
        /// <c>ProfessionType</c> yang sudah ada.
        /// </summary>
        OtherProfessionNote = 4
    }
}
