using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.Infrastructure
{
    /// <summary>
    /// Membuktikan bahwa fondasi pengujian benar-benar bekerja.
    ///
    /// Empat uji di bawah adalah bukti acceptance untuk task `BE-00`. Uji ini sengaja memakai
    /// `MstMeasurement`, sebuah master sederhana yang sudah ada, karena tujuannya menguji
    /// fondasinya — bukan menguji aturan bisnis modul mana pun.
    /// </summary>
    public class TestDatabaseTests
    {
        /// <summary>
        /// Membuktikan seluruh pemetaan EF Core dapat dibentuk menjadi tabel sungguhan.
        ///
        /// Ini uji yang paling luas cakupannya walaupun terlihat sederhana: `EnsureCreated`
        /// membaca seluruh konfigurasi entity di aplikasi, sehingga pemetaan yang rusak akan
        /// ketahuan di sini, bukan saat aplikasi dijalankan.
        /// </summary>
        [Fact]
        public void BasisDataUji_DapatDibentukLengkapDenganTabelnya()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            var jumlahBaris = context.Set<MstMeasurement>().Count();

            Assert.Equal(0, jumlahBaris);
        }

        /// <summary>
        /// Uji integrasi yang benar-benar menyentuh basis data.
        ///
        /// Data ditulis lewat satu konteks, lalu dibaca lewat konteks yang berbeda. Memakai
        /// konteks berbeda itu disengaja: bila dibaca dari konteks yang sama, data bisa saja
        /// hanya tertahan di memori dan belum benar-benar tersimpan.
        /// </summary>
        [Fact]
        public void DataYangDisimpan_DapatDibacaKembaliLewatKonteksBaru()
        {
            using var database = TestDatabase.Create();

            var id = Guid.NewGuid();

            using (var penulis = database.CreateContext())
            {
                penulis.Set<MstMeasurement>().Add(new MstMeasurement
                {
                    Id = id,
                    MeasurementCode = "MG",
                    MeasurementName = "Miligram",
                    MeasurementType = "Dose"
                });

                penulis.SaveChanges();
            }

            using var pembaca = database.CreateContext();
            var tersimpan = pembaca.Set<MstMeasurement>().Single(x => x.Id == id);

            Assert.Equal("MG", tersimpan.MeasurementCode);
            Assert.Equal("Miligram", tersimpan.MeasurementName);
        }

        /// <summary>
        /// Membuktikan aturan keunikan benar-benar ditegakkan basis data, bukan hanya tertulis
        /// di konfigurasi.
        ///
        /// Ini yang menentukan apakah fondasi ini layak dipakai task berikutnya. Task `BE-01`
        /// menuntut pembuktian bahwa index unik `(DocumentKind, DocumentId)` menolak baris
        /// kembar. Bila basis data uji tidak menegakkan keunikan, pembuktian itu mustahil dan
        /// ujinya akan lulus padahal seharusnya gagal.
        /// </summary>
        [Fact]
        public void IndexUnik_MenolakKodeYangKembar()
        {
            using var database = TestDatabase.Create();
            using var context = database.CreateContext();

            context.Set<MstMeasurement>().Add(new MstMeasurement
            {
                MeasurementCode = "ML",
                MeasurementName = "Mililiter",
                MeasurementType = "Volume"
            });
            context.SaveChanges();

            context.Set<MstMeasurement>().Add(new MstMeasurement
            {
                MeasurementCode = "ML",
                MeasurementName = "Mililiter Duplikat",
                MeasurementType = "Volume"
            });

            Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        }

        /// <summary>
        /// Membuktikan satu uji tidak melihat data milik uji lain.
        ///
        /// Tanpa sifat ini, uji akan lulus atau gagal bergantung urutan jalannya — masalah yang
        /// sangat sulit ditelusuri dan membuat orang berhenti memercayai hasil uji.
        /// </summary>
        [Fact]
        public void SetiapBasisDataUji_BerdiriSendiriDanTidakSalingMelihat()
        {
            using var pertama = TestDatabase.Create();
            using var kedua = TestDatabase.Create();

            using (var context = pertama.CreateContext())
            {
                context.Set<MstMeasurement>().Add(new MstMeasurement
                {
                    MeasurementCode = "KG",
                    MeasurementName = "Kilogram",
                    MeasurementType = "Weight"
                });
                context.SaveChanges();
            }

            using var contextKedua = kedua.CreateContext();

            Assert.Equal(0, contextKedua.Set<MstMeasurement>().Count());
        }
    }
}
