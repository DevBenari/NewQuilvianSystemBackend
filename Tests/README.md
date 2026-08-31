# Pengujian Otomatis Backend Quilvian

| Field | Nilai |
|---|---|
| Task asal | `BE-00` pada `docs/module-blueprints/rekam-medis/roadmap/backend-roadmap.md` |
| Dibuat | 24 Agustus 2026 |
| Kerangka uji | xUnit |
| Basis data uji | SQLite di dalam memori |
| Keputusan terkait | Pilihan basis data uji ditetapkan pada sesi `BE-00`; alternatifnya tercatat pada bagian "Batasan yang diketahui" |

---

## Cara menjalankan

Dari folder `NewQuilvianSystemBackend`:

```bash
dotnet test tests/QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj
```

Menjalankan seluruh uji beserta project utamanya sekaligus:

```bash
dotnet test QuilvianSystemBackend.sln
```

Menjalankan satu uji saja, misalnya saat sedang menelusuri kegagalan:

```bash
dotnet test tests/QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj \
  --filter "FullyQualifiedName~IndexUnik_MenolakKodeYangKembar"
```

Keluaran yang menandakan berhasil:

```text
Passed!  - Failed: 0, Passed: 4, Skipped: 0, Total: 4
```

---

## Yang perlu diketahui sebelum menulis uji baru

### Basis data uji tidak pernah menyentuh basis data sungguhan

Setiap uji membuat basis data SQLite baru yang hidup **di dalam memori**, lalu membuangnya
setelah selesai. Tidak ada berkas yang tertinggal di disk, dan tidak ada koneksi ke basis data
mana pun yang tercatat di `appsettings`.

Ini bukan sekadar kemudahan. Koneksi `Development` pada project ini menunjuk server bersama,
sehingga uji yang membuat lalu menghapus data di sana akan mengganggu pekerjaan orang lain.
**Jangan pernah mengarahkan uji ke koneksi itu.**

### Setiap uji berdiri sendiri

Dua uji yang berjalan bersamaan tidak akan saling melihat datanya. Karena itu tidak perlu ada
pembersihan manual, dan tidak perlu khawatir soal urutan jalannya uji.

### Contoh menulis uji baru

```csharp
[Fact]
public void ContohUji()
{
    using var database = TestDatabase.Create();

    using (var penulis = database.CreateContext())
    {
        penulis.Set<MstMeasurement>().Add(new MstMeasurement
        {
            MeasurementCode = "MG",
            MeasurementName = "Miligram",
            MeasurementType = "Dose"
        });
        penulis.SaveChanges();
    }

    using var pembaca = database.CreateContext();
    Assert.Equal(1, pembaca.Set<MstMeasurement>().Count());
}
```

Perhatikan pola menulis dan membaca lewat **konteks yang berbeda**. Bila keduanya memakai
konteks yang sama, uji bisa lulus padahal datanya belum benar-benar tersimpan — ia hanya masih
tertahan di memori konteks tersebut.

---

## Batasan yang diketahui

Batasan berikut dinyatakan terbuka agar tidak ada yang mengira cakupan ujinya lebih luas
daripada kenyataannya.

| Batasan | Akibatnya | Kapan perlu ditangani |
|---|---|---|
| SQLite bukan PostgreSQL | Hal yang khas PostgreSQL tidak teruji apa adanya: pembagian tabel per periode, tipe `jsonb`, dan sebagian perilaku index | Saat task `BE-10` dikerjakan, karena tabel jejak akses dirancang terbagi per tahun |
| Migration tidak diuji | Tabel dibentuk lewat `EnsureCreated`, bukan dengan menjalankan migration satu per satu. Migration yang rusak **tidak** akan ketahuan dari uji ini | Bila migration mulai memuat perpindahan data, misalnya `BE-08` |
| Belum ada uji lewat HTTP | Uji menyentuh basis data langsung, bukan lewat endpoint. Aturan hak akses dan bentuk respons belum teruji | Saat endpoint pertama modul rekam medis dibuat |
| Belum berjalan di CI | `.github/workflows/validate-agent-codex-backend.yml` hanya menjalankan `restore` dan `build` | Sebaiknya segera, agar uji benar-benar menjadi jaring pengaman |

Baris terakhir yang paling penting. Uji yang hanya dijalankan manual di komputer developer akan
terlupakan. Menambahkan satu langkah `dotnet test` ke CI mengubah perilaku CI, sehingga
perubahannya perlu persetujuan pemilik CI lebih dulu.

Cara menambahkannya, bila sudah disetujui:

```yaml
      - name: Test
        run: dotnet test ./QuilvianSystemBackend.sln --configuration Release --no-build
```

Langkah itu diletakkan setelah langkah `Build` yang sudah ada.

---

## Susunan berkas

```text
tests/
├── README.md                                  # dokumen ini
└── QuilvianSystemBackend.Tests/
    ├── QuilvianSystemBackend.Tests.csproj
    └── Infrastructure/
        ├── TestDatabase.cs                    # penyedia basis data uji
        └── TestDatabaseTests.cs               # bukti fondasi ini bekerja
```

Project test **tidak** ikut terkompilasi ke dalam aplikasi. Pengecualiannya tertulis di
`QuilvianSystemBackend.csproj` pada `ItemGroup` bertanda `tests\**`. Pengecualian itu wajib ada
karena project utama memakai SDK Web yang secara otomatis mengikutsertakan seluruh berkas `.cs`
di bawah folder root.
