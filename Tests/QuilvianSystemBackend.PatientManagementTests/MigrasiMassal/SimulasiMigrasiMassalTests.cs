using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal;

/// <summary>
/// Simulasi <c>dryRun=true</c> dan klasifikasi B3.2 — `PAT-MIG2-AC-09`, `12`–`16`, `21`, `23`, `26`–`28`,
/// `30`, `37`, `38`, `40`, `43`, `48`, `50`; `PAT-DEC-036`, `045`, `046`, `050`, `052`.
/// </summary>
public class SimulasiMigrasiMassalTests
{
    [Fact]
    public async Task Simulasi_TanpaMutasiDatabaseMaupunFilesystem()
    {
        using var h = new MigrasiMassalHarness();
        var database = h.SnapshotDatabase();
        var filesystem = h.SnapshotFilesystem();

        var hasil = await h.JalankanAsync(true);

        Assert.Equal(RsmmcBulkPatientMigrationOutcome.Completed, hasil.Outcome);
        Assert.Equal(database, h.SnapshotDatabase());
        Assert.Equal(filesystem, h.SnapshotFilesystem());
        Assert.Empty(h.Perintah.PerintahTulis);
        Assert.Equal(0, h.Transaksi.Dibuka);
        Assert.Empty(h.Kendali.KunciDiminta);
        Assert.Equal(0, h.Kendali.JumlahInsertCrosswalk);

        // Penulis QR tidak pernah meminta folder penyimpanan; pemeriksa tidak membuat folder namespace.
        Assert.Equal(0, h.JumlahResolveStorage);
        Assert.Empty(h.PayloadDirender);
        Assert.False(Directory.Exists(h.FolderNamespaceBulk));
    }

    [Fact]
    public async Task Simulasi_SeluruhPasienManifestDalamUrutanManifest_LegacyPidSebagaiTeks()
    {
        using var h = new MigrasiMassalHarness();

        var hasil = await h.JalankanAsync(true, 5);

        Assert.Equal(
            RsmmcBulkCanaryManifest.CanaryAV1.LegacyPids.Select(x => x.ToString(CultureInfo.InvariantCulture)),
            hasil.Data!.Items.Select(x => x.LegacyPid));
        Assert.Equal("-667992", hasil.Data.Items[0].LegacyPid);
        Assert.Equal("0", hasil.Data.Items[19].LegacyPid);
        Assert.DoesNotContain(hasil.Data.Items, x => MigrasiMassalHarness.PidDiLuarManifest.Select(p => p.ToString(CultureInfo.InvariantCulture)).Contains(x.LegacyPid));
    }

    [Fact]
    public async Task Simulasi_RingkasanSidikJariDanJumlah()
    {
        using var h = new MigrasiMassalHarness();

        var data = (await h.JalankanAsync(true, 5)).Data!;

        Assert.Equal(MigrasiMassalHarness.BatchKanonik, data.BatchId);
        Assert.Equal("CANARY_A", data.SelectionMode);
        Assert.True(data.DryRun);
        Assert.Equal(5, data.Limit);
        Assert.Equal("Staging", data.EnvironmentName);
        Assert.Equal("RSMMC-CANARY-A", data.ManifestId);
        Assert.Equal(1, data.ManifestVersion);
        Assert.Equal("c43207eb73f1f244db13ff930bca1c22e2e4ae779018e5cd48e26f628a5a79ad", data.ManifestSha256);
        Assert.Equal(25, data.ManifestSize);
        Assert.Equal(25, data.TotalInManifest);
        Assert.Equal(0, data.AlreadyMigrated);
        Assert.Equal(25, data.EligibleBeforeRun);
        Assert.Equal(5, data.Selected);
        Assert.Equal(25, data.Ready);
        Assert.Equal(0, data.Migrated);
        Assert.Equal(0, data.Conflict);
        Assert.Equal(0, data.Failed);
        Assert.Equal(0, data.NotProcessed);
        Assert.Equal(25, data.Remaining);
        Assert.False(data.Stopped);
        Assert.Null(data.StopReason);
    }

    [Fact]
    public async Task Simulasi_BarisMemuatRencanaDanBuktiQrHistoris_TanpaNilaiIdentitas()
    {
        using var h = new MigrasiMassalHarness();

        var items = (await h.JalankanAsync(true)).Data!.Items;
        var pertama = items[0];
        var mrn = MigrasiMassalHarness.MrnUntuk(0);
        var isiHistoris = Encoding.UTF8.GetBytes("QR-HISTORIS:" + mrn);

        Assert.Equal("NOREG_WITHOUT_RM", pertama.SourceGroup);
        Assert.Equal(RsmmcBulkCrosswalkState.Missing, pertama.CrosswalkStateBefore);
        Assert.Null(pertama.PatientId);
        Assert.Equal(MigrasiMassalHarness.KodeUntuk(0), pertama.PatientCode);
        Assert.Equal(mrn, pertama.MedicalRecordNumber);
        Assert.Equal(MigrasiMassalHarness.PathQrBulkPublik(mrn), pertama.PlannedQrCodePath);
        Assert.Equal(MigrasiMassalHarness.NamaUntuk(0), pertama.FullName);
        Assert.True(pertama.HasIdentityNumber);
        Assert.True(pertama.HasPhoneNumber);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Ready, pertama.Status);

        // QR historis Pilot di path bawaan hanya informasi, bukan konflik (PAT-DEC-052).
        Assert.False(pertama.PlannedQrExists);
        Assert.True(pertama.HistoricalQrAtDefaultPath);
        Assert.Equal(isiHistoris.LongLength, pertama.HistoricalQrLength);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(isiHistoris)).ToLowerInvariant(), pertama.HistoricalQrSha256);

        Assert.False(items[2].HistoricalQrAtDefaultPath);
        Assert.Null(items[2].HistoricalQrLength);
        Assert.Null(items[2].HistoricalQrSha256);

        // Hanya tanda ada atau tidak; nilai identitas dan telepon tidak pernah ada di respons.
        Assert.False(items[3].HasIdentityNumber);
        Assert.False(items[4].HasPhoneNumber);
        Assert.DoesNotContain(typeof(RsmmcBulkPatientMigrationItemResponse).GetProperties(), p => p.Name is "IdentityNumber" or "PhoneNumber" or "Address" or "BirthDate");
    }

    [Fact]
    public async Task Simulasi_BerkasDiPathBulk_KonflikTanpaMenyentuhBerkas_DanTidakBerhenti()
    {
        using var h = new MigrasiMassalHarness();
        var mrn = MigrasiMassalHarness.MrnUntuk(2);
        MigrasiMassalHarness.TulisFile(h.PathQrBulkFisik(mrn), "BERKAS-TAK-DIKENAL");

        var data = (await h.JalankanAsync(true)).Data!;
        var item = data.Items[2];

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Conflict, item.Status);
        Assert.Equal(RsmmcBulkPatientConflictType.QrArtifactAlreadyExists, item.ConflictType);
        Assert.True(item.PlannedQrExists);
        Assert.Equal("BERKAS-TAK-DIKENAL", File.ReadAllText(h.PathQrBulkFisik(mrn)));
        Assert.All(data.Items.Skip(3), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.Ready, x.Status));
        Assert.False(data.Stopped);
        Assert.Equal(24, data.Ready);
        Assert.Equal(1, data.Conflict);
    }

    [Theory]
    [InlineData("crosswalk-tanpa-target", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.CrosswalkExistsWithoutTarget)]
    [InlineData("crosswalk-yatim", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.OrphanedCrosswalk)]
    [InlineData("target-beda-kode", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.TargetMappingMismatch)]
    [InlineData("target-beda-mrn", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.TargetMappingMismatch)]
    [InlineData("target-ganda", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.DuplicateTargetMapping)]
    [InlineData("sudah-dimigrasi", RsmmcBulkPatientMigrationStatus.AlreadyMigrated, null)]
    [InlineData("qr-path-lama", RsmmcBulkPatientMigrationStatus.MigratedQrInconsistent, null)]
    [InlineData("qr-berkas-hilang", RsmmcBulkPatientMigrationStatus.MigratedQrInconsistent, null)]
    [InlineData("preview-bukan-pending", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewStateChanged)]
    [InlineData("preview-status-null", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewStateChanged)]
    [InlineData("perencana-mrn-beda", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PlannerMismatch)]
    [InlineData("perencana-format-salah", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PlannerMismatch)]
    [InlineData("perencana-bukan-finalized", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PlannerMismatch)]
    [InlineData("kode-dipakai", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PatientCodeAlreadyUsed)]
    [InlineData("kode-dipakai-pasien-terhapus", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PatientCodeAlreadyUsed)]
    [InlineData("mrn-dipakai", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.MedicalRecordNumberAlreadyUsed)]
    [InlineData("mrn-dipakai-pasien-terhapus", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.MedicalRecordNumberAlreadyUsed)]
    [InlineData("nik-dipakai", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.IdentityNumberAlreadyUsed)]
    [InlineData("nik-dipakai-pasien-terhapus", RsmmcBulkPatientMigrationStatus.Ready, null)]
    [InlineData("telepon-dipakai", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PhoneAlreadyUsed)]
    [InlineData("telepon-dipakai-pasien-terhapus", RsmmcBulkPatientMigrationStatus.Ready, null)]
    [InlineData("nama-kosong", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("nama-terlalu-panjang", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("telepon-terlalu-panjang", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("enum-null", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("patient-type-nol", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("patient-status-nol", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("religion-bukan-anggota", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("gender-bukan-anggota", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("gender-null", RsmmcBulkPatientMigrationStatus.Ready, null)]
    [InlineData("boolean-null", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("is-member-true", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    [InlineData("is-active-false", RsmmcBulkPatientMigrationStatus.Conflict, RsmmcBulkPatientConflictType.PreviewValueInvalid)]
    public async Task Simulasi_KlasifikasiB32_SatuPasien_TanpaMutasiDanTanpaBerhenti(string skenario, string status, string? konflik)
    {
        const int urutan = 2;
        using var h = new MigrasiMassalHarness();
        SiapkanKeadaan(h, skenario, urutan);
        var database = h.SnapshotDatabase();
        var filesystem = h.SnapshotFilesystem();

        var data = (await h.JalankanAsync(true)).Data!;
        var item = data.Items[urutan];

        Assert.Equal(status, item.Status);
        Assert.Equal(konflik, item.ConflictType);
        Assert.Null(item.FailureStage);
        Assert.False(data.Stopped);
        Assert.Equal(25, data.Items.Count);
        Assert.All(data.Items.Where((_, i) => i != urutan), x => Assert.Equal(RsmmcBulkPatientMigrationStatus.Ready, x.Status));

        // Pesan tidak pernah memuat nilai pribadi preview.
        Assert.DoesNotContain(MigrasiMassalHarness.NamaUntuk(urutan), item.Message ?? string.Empty);
        Assert.DoesNotContain(MigrasiMassalHarness.NikUntuk(urutan), item.Message ?? string.Empty);
        Assert.DoesNotContain(MigrasiMassalHarness.TeleponUntuk(urutan), item.Message ?? string.Empty);

        Assert.Equal(database, h.SnapshotDatabase());
        Assert.Equal(filesystem, h.SnapshotFilesystem());
    }

    [Fact]
    public async Task Simulasi_PasienSudahDimigrasi_TidakTermasukLayakDanTidakDipilih()
    {
        using var h = new MigrasiMassalHarness();
        SiapkanKeadaan(h, "sudah-dimigrasi", 0);
        SiapkanKeadaan(h, "qr-path-lama", 1);

        var data = (await h.JalankanAsync(true, 25)).Data!;

        // Ringkasan hanya memakai field B2.2: QR tidak konsisten tetap status baris, tetapi pasiennya
        // sudah dimigrasi sehingga ikut alreadyMigrated.
        Assert.Equal(RsmmcBulkPatientMigrationStatus.AlreadyMigrated, data.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.MigratedQrInconsistent, data.Items[1].Status);
        Assert.Equal(2, data.AlreadyMigrated);
        Assert.Equal(23, data.EligibleBeforeRun);
        Assert.Equal(data.TotalInManifest - data.AlreadyMigrated, data.EligibleBeforeRun);
        Assert.Equal(23, data.Selected);
        Assert.Equal(23, data.Ready);
        Assert.Equal(23, data.Remaining);
        Assert.Equal(RsmmcBulkCrosswalkState.ExistsWithTarget, data.Items[0].CrosswalkStateBefore);
        Assert.NotNull(data.Items[0].PatientId);
    }

    [Fact]
    public async Task Simulasi_LegacyPidTertundaDiManifest_DeferredLegacyPid()
    {
        using var h = new MigrasiMassalHarness(MigrasiMassalHarness.ManifestUji(1, 280143, 888));

        var data = (await h.JalankanAsync(true)).Data!;

        Assert.Equal(RsmmcBulkPatientMigrationStatus.Ready, data.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientConflictType.DeferredLegacyPid, data.Items[1].ConflictType);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Ready, data.Items[2].Status);
    }

    [Fact]
    public async Task Simulasi_LegacyPidDiLuarInt32_FailedLockKeyOutOfRange_NilaiInt64Utuh()
    {
        using var h = new MigrasiMassalHarness(MigrasiMassalHarness.ManifestUji(1, 3000000000, -3000000000, 888));

        var data = (await h.JalankanAsync(true)).Data!;

        Assert.Equal(["1", "3000000000", "-3000000000", "888"], data.Items.Select(x => x.LegacyPid));
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Ready, data.Items[0].Status);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Failed, data.Items[1].Status);
        Assert.Equal(RsmmcBulkPatientFailureStage.LockKeyOutOfRange, data.Items[1].FailureStage);
        Assert.Equal(RsmmcBulkPatientFailureStage.LockKeyOutOfRange, data.Items[2].FailureStage);
        Assert.Equal(RsmmcBulkPatientMigrationStatus.Ready, data.Items[3].Status);
        Assert.Equal(2, data.Failed);
        Assert.False(data.Stopped);
    }

    /// <summary>Menyiapkan satu keadaan B3.2 untuk pasien manifest ke-<paramref name="urutan"/>.</summary>
    internal static void SiapkanKeadaan(MigrasiMassalHarness h, string skenario, int urutan)
    {
        var pid = h.PidManifest(urutan);
        var kode = MigrasiMassalHarness.KodeUntuk(urutan);
        var mrn = MigrasiMassalHarness.MrnUntuk(urutan);
        var pathBulk = MigrasiMassalHarness.PathQrBulkPublik(mrn);

        Guid PasienTermigrasi(string patientCode, string medicalRecordNumber, string? qrCodePath) =>
            h.TambahPasien(patientCode, medicalRecordNumber, qrCodePath: qrCodePath);

        void Petakan(Guid target) =>
            h.TambahCrosswalk(pid, target, "TARGET_CREATED", "MIGRATED_BULK_WORKER", MigrasiMassalHarness.BatchKanonik);

        switch (skenario)
        {
            case "crosswalk-tanpa-target":
                h.TambahCrosswalk(pid, null, "UNPROCESSED", "RAW", null);
                break;
            case "crosswalk-yatim":
                h.TambahCrosswalk(pid, Guid.NewGuid(), "TARGET_CREATED", "MIGRATED_BULK_WORKER", MigrasiMassalHarness.BatchKanonik);
                break;
            case "target-beda-kode":
                Petakan(PasienTermigrasi("PAT-LAIN-00009", mrn, pathBulk));
                break;
            case "target-beda-mrn":
                Petakan(PasienTermigrasi(kode, "00-99-99-09", pathBulk));
                break;
            case "target-ganda":
                var bersama = PasienTermigrasi(kode, mrn, pathBulk);
                Petakan(bersama);
                h.TambahCrosswalk(700004, bersama, "TARGET_CREATED", "RAW", null);
                break;
            case "sudah-dimigrasi":
                Petakan(PasienTermigrasi(kode, mrn, pathBulk));
                MigrasiMassalHarness.TulisFile(h.PathQrBulkFisik(mrn), "QR-BARU:" + mrn);
                break;
            case "qr-path-lama":
                Petakan(PasienTermigrasi(kode, mrn, $"/uploads/patient-qrcodes/{mrn}/qrcode.png"));
                MigrasiMassalHarness.TulisFile(h.PathQrBulkFisik(mrn), "QR-BARU:" + mrn);
                break;
            case "qr-berkas-hilang":
                Petakan(PasienTermigrasi(kode, mrn, pathBulk));
                break;
            case "preview-bukan-pending":
                h.UbahPreview(pid, "validation_status", "REJECTED");
                break;
            case "preview-status-null":
                h.UbahPreview(pid, "validation_status", null);
                break;
            case "perencana-mrn-beda":
                h.UbahPerencana(pid, "final_medical_record_number", "00-99-99-08");
                break;
            case "perencana-format-salah":
                h.UbahPerencana(pid, "final_medical_record_number", "00120002");
                h.UbahPreview(pid, "proposed_medical_record_number", "00120002");
                break;
            case "perencana-bukan-finalized":
                h.UbahPerencana(pid, "plan_status", "PENDING");
                break;
            case "kode-dipakai":
                h.TambahPasien(kode, "00-99-99-07");
                break;
            case "kode-dipakai-pasien-terhapus":
                h.TambahPasien(kode, "00-99-99-07", terhapus: true);
                break;
            case "mrn-dipakai":
                h.TambahPasien("PAT-LAIN-00007", mrn);
                break;
            case "mrn-dipakai-pasien-terhapus":
                h.TambahPasien("PAT-LAIN-00007", mrn, terhapus: true);
                break;
            case "nik-dipakai":
                h.TambahPasien("PAT-LAIN-00006", "00-99-99-06", identityNumber: MigrasiMassalHarness.NikUntuk(urutan));
                break;
            case "nik-dipakai-pasien-terhapus":
                h.TambahPasien("PAT-LAIN-00006", "00-99-99-06", identityNumber: MigrasiMassalHarness.NikUntuk(urutan), terhapus: true);
                break;
            case "telepon-dipakai":
                h.TambahPasien("PAT-LAIN-00005", "00-99-99-05", phoneNumber: MigrasiMassalHarness.TeleponUntuk(urutan));
                break;
            case "telepon-dipakai-pasien-terhapus":
                h.TambahPasien("PAT-LAIN-00005", "00-99-99-05", phoneNumber: MigrasiMassalHarness.TeleponUntuk(urutan), terhapus: true);
                break;
            case "nama-kosong":
                h.UbahPreview(pid, "full_name", "   ");
                break;
            case "nama-terlalu-panjang":
                h.UbahPreview(pid, "full_name", new string('N', RsmmcBulkPatientMapper.FullNameMaxLength + 1));
                break;
            case "telepon-terlalu-panjang":
                h.UbahPreview(pid, "phone_number", new string('8', RsmmcBulkPatientMapper.PhoneNumberMaxLength + 1));
                break;
            case "enum-null":
                h.UbahPreview(pid, "marital_status", null);
                break;
            case "patient-type-nol":
                h.UbahPreview(pid, "patient_type", 0);
                break;
            case "patient-status-nol":
                h.UbahPreview(pid, "patient_status", 0);
                break;
            case "religion-bukan-anggota":
                h.UbahPreview(pid, "religion", 42);
                break;
            case "gender-bukan-anggota":
                h.UbahPreview(pid, "gender", 7);
                break;
            case "gender-null":
                h.UbahPreview(pid, "gender", null);
                break;
            case "boolean-null":
                h.UbahPreview(pid, "is_newborn", null);
                break;
            case "is-member-true":
                h.UbahPreview(pid, "is_member", 1);
                break;
            case "is-active-false":
                h.UbahPreview(pid, "is_active", 0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(skenario), skenario, null);
        }
    }
}
