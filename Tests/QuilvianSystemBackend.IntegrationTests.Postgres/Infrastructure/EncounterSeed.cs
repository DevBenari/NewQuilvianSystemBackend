namespace QuilvianSystemBackend.BillingTests.Infrastructure
{
    /// <summary>
    /// Identitas prasyarat yang dibuat <see cref="BillingTestDatabaseFixture.SeedEncounterAsync"/>
    /// untuk satu test. Dipakai kembali saat teardown agar penghapusan menyasar baris yang persis
    /// sama, bukan menebak lewat pola nama.
    /// </summary>
    public sealed record EncounterSeed(
        Guid EncounterId,
        Guid ActorUserId,
        Guid PatientId,
        Guid ServiceUnitId);
}
