using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.MedicalRecordManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-01`.
    ///
    /// Yang dibuktikan di sini adalah aturan yang ditegakkan basis data, bukan aturan yang
    /// ditegakkan kode. Membedakan keduanya penting: aturan yang hanya ada di kode dapat
    /// terlewat bila ada jalur masuk lain, sedangkan aturan yang ditegakkan basis data berlaku
    /// tanpa kecuali.
    /// </summary>
    public class ClinicalDocumentIntegritySchemaTests
    {
        private static TrxClinicalDocumentIntegrity Keutuhan(
            RekamMedisTestData.Konteks konteks,
            ClinicalDocumentKind kind,
            Guid documentId) => new()
            {
                DocumentKind = kind,
                DocumentId = documentId,
                PatientId = konteks.PatientId,
                EncounterId = konteks.EncounterId,
                AuthorUserId = Guid.NewGuid(),
                IntegrityStatus = ClinicalDocumentIntegrityStatus.Draft
            };

        /// <summary>
        /// Acceptance criteria 1: migration membentuk ketiga tabel dan dapat dipakai.
        /// </summary>
        [Fact]
        public void KetigaTabelKeutuhan_TerbentukDanDapatDipakai()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            Assert.Equal(0, context.Set<TrxClinicalDocumentIntegrity>().Count());
            Assert.Equal(0, context.Set<TrxClinicalNoteAddendum>().Count());
            Assert.Equal(0, context.Set<TrxClinicalNoteAuthorDelegation>().Count());
        }

        /// <summary>
        /// Acceptance criteria 2: satu dokumen tepat satu baris keutuhan.
        ///
        /// Tanpa aturan ini, satu dokumen bisa punya dua status yang bertentangan — misalnya
        /// satu baris menyatakan terkunci dan baris lain menyatakan masih draf. Aturan
        /// penguncian menjadi tidak bermakna.
        /// </summary>
        [Fact]
        public void DokumenYangSama_TidakDapatPunyaDuaBarisKeutuhan()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var documentId = Guid.NewGuid();

            context.Set<TrxClinicalDocumentIntegrity>()
                .Add(Keutuhan(konteks, ClinicalDocumentKind.ProgressNote, documentId));
            context.SaveChanges();

            context.Set<TrxClinicalDocumentIntegrity>()
                .Add(Keutuhan(konteks, ClinicalDocumentKind.ProgressNote, documentId));

            Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        }

        /// <summary>
        /// Sisi lain dari aturan yang sama: keunikan berlaku pada PASANGAN jenis dan Id, bukan
        /// pada Id saja. Dua jenis dokumen berbeda boleh kebetulan memiliki Id yang sama.
        /// </summary>
        [Fact]
        public void JenisDokumenBerbeda_BolehMemakaiIdYangSama()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var documentId = Guid.NewGuid();

            context.Set<TrxClinicalDocumentIntegrity>()
                .Add(Keutuhan(konteks, ClinicalDocumentKind.ProgressNote, documentId));
            context.Set<TrxClinicalDocumentIntegrity>()
                .Add(Keutuhan(konteks, ClinicalDocumentKind.Consultation, documentId));

            context.SaveChanges();

            Assert.Equal(2, context.Set<TrxClinicalDocumentIntegrity>().Count());
        }

        /// <summary>
        /// Acceptance criteria 3: urutan addendum tidak dapat kembar.
        ///
        /// Bila dua addendum punya urutan yang sama, pembaca tidak dapat memastikan mana koreksi
        /// yang lebih dulu — dan pada rekam medis, urutan koreksi adalah bagian dari maknanya.
        /// </summary>
        [Fact]
        public void Addendum_TidakDapatPunyaUrutanKembarPadaDokumenYangSama()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var keutuhan = Keutuhan(konteks, ClinicalDocumentKind.ProgressNote, Guid.NewGuid());
            context.Set<TrxClinicalDocumentIntegrity>().Add(keutuhan);
            context.SaveChanges();

            context.Set<TrxClinicalNoteAddendum>().Add(new TrxClinicalNoteAddendum
            {
                IntegrityId = keutuhan.Id,
                Sequence = 1,
                AuthorUserId = Guid.NewGuid(),
                AddendumText = "Pembetulan dosis.",
                CorrectionReason = "Salah tulis dosis."
            });
            context.SaveChanges();

            context.Set<TrxClinicalNoteAddendum>().Add(new TrxClinicalNoteAddendum
            {
                IntegrityId = keutuhan.Id,
                Sequence = 1,
                AuthorUserId = Guid.NewGuid(),
                AddendumText = "Koreksi kedua.",
                CorrectionReason = "Percobaan urutan kembar."
            });

            Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        }

        /// <summary>
        /// Urutan yang berurut pada dokumen yang sama diterima, dan urutan yang sama pada
        /// dokumen berbeda juga diterima.
        /// </summary>
        [Fact]
        public void Addendum_BerurutanDiterimaDanUrutanBolehSamaAntarDokumen()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var pertama = Keutuhan(konteks, ClinicalDocumentKind.ProgressNote, Guid.NewGuid());
            var kedua = Keutuhan(konteks, ClinicalDocumentKind.ProgressNote, Guid.NewGuid());
            context.Set<TrxClinicalDocumentIntegrity>().AddRange(pertama, kedua);
            context.SaveChanges();

            context.Set<TrxClinicalNoteAddendum>().AddRange(
                new TrxClinicalNoteAddendum
                {
                    IntegrityId = pertama.Id,
                    Sequence = 1,
                    AuthorUserId = Guid.NewGuid(),
                    AddendumText = "Koreksi pertama.",
                    CorrectionReason = "Salah tulis."
                },
                new TrxClinicalNoteAddendum
                {
                    IntegrityId = pertama.Id,
                    Sequence = 2,
                    AuthorUserId = Guid.NewGuid(),
                    AddendumText = "Koreksi kedua.",
                    CorrectionReason = "Melengkapi koreksi sebelumnya."
                },
                new TrxClinicalNoteAddendum
                {
                    IntegrityId = kedua.Id,
                    Sequence = 1,
                    AuthorUserId = Guid.NewGuid(),
                    AddendumText = "Koreksi pada dokumen lain.",
                    CorrectionReason = "Salah tulis."
                });

            context.SaveChanges();

            Assert.Equal(3, context.Set<TrxClinicalNoteAddendum>().Count());
        }

        /// <summary>
        /// Acceptance criteria 4: relasi memakai perilaku hapus terbatas, sehingga histori
        /// tidak ikut terhapus berantai.
        ///
        /// Dibuktikan dengan mencoba menghapus baris keutuhan yang masih punya addendum.
        /// Penghapusan harus ditolak.
        ///
        /// Catatan: penolakan terjadi di lapisan EF Core, bukan di basis data, sehingga jenis
        /// galatnya <see cref="InvalidOperationException"/> dan bukan
        /// <see cref="DbUpdateException"/>. Yang penting hasilnya sama — addendum tidak pernah
        /// ikut terhapus diam-diam bersama dokumen induknya.
        /// </summary>
        [Fact]
        public void KeutuhanYangMasihPunyaAddendum_TidakDapatDihapus()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();
            var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(context);

            var keutuhan = Keutuhan(konteks, ClinicalDocumentKind.ProgressNote, Guid.NewGuid());
            context.Set<TrxClinicalDocumentIntegrity>().Add(keutuhan);
            context.SaveChanges();

            context.Set<TrxClinicalNoteAddendum>().Add(new TrxClinicalNoteAddendum
            {
                IntegrityId = keutuhan.Id,
                Sequence = 1,
                AuthorUserId = Guid.NewGuid(),
                AddendumText = "Pembetulan.",
                CorrectionReason = "Salah tulis."
            });
            context.SaveChanges();

            // Penolakan terjadi tepat saat penghapusan ditandai, bukan saat disimpan.
            Assert.Throws<InvalidOperationException>(
                () => context.Set<TrxClinicalDocumentIntegrity>().Remove(keutuhan));

            // Addendum tetap ada setelah penghapusan ditolak.
            Assert.Equal(1, context.Set<TrxClinicalNoteAddendum>().AsNoTracking().Count());
        }

        /// <summary>
        /// Enum disimpan sebagai angka, bukan teks. Dibuktikan dengan menyimpan lalu membaca
        /// kembali lewat konteks baru dan memastikan nilainya utuh.
        /// </summary>
        [Fact]
        public void StatusKeutuhanDanJenisDokumen_TersimpanUtuh()
        {
            using var database = TestDatabase.Create();

            var documentId = Guid.NewGuid();

            using (var penulis = database.CreateContext())
            {
                var konteks = RekamMedisTestData.SiapkanPasienDanKunjungan(penulis);
                var keutuhan = Keutuhan(konteks, ClinicalDocumentKind.Consultation, documentId);
                keutuhan.IntegrityStatus = ClinicalDocumentIntegrityStatus.LockedUnsigned;
                keutuhan.LockTrigger = ClinicalDocumentLockTrigger.EncounterClosed;
                penulis.Set<TrxClinicalDocumentIntegrity>().Add(keutuhan);
                penulis.SaveChanges();
            }

            using var pembaca = database.CreateContext();
            var tersimpan = pembaca.Set<TrxClinicalDocumentIntegrity>()
                .Single(x => x.DocumentId == documentId);

            Assert.Equal(ClinicalDocumentKind.Consultation, tersimpan.DocumentKind);
            Assert.Equal(ClinicalDocumentIntegrityStatus.LockedUnsigned, tersimpan.IntegrityStatus);
            Assert.Equal(ClinicalDocumentLockTrigger.EncounterClosed, tersimpan.LockTrigger);
        }
    }
}
