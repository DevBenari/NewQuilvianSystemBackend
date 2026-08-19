# Laporan Perubahan Backend — `BE-IGD-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-005` |
| Judul | Kunjungan menyimpan penanda pelampauan batas waktu |
| Slice | S2 — Pasien menunggu terlalu lama tertandai |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `IGD-GAP-007`; data dictionary bagian 1 dan 6.2; `02-backend-architecture.md` bagian 5 dan 6 |
| Contract version | Tidak ada kontrak yang berubah — task ini murni penambahan struktur data |
| Dependency | `BE-IGD-001` — sudah dikerjakan |
| Commit backend saat dikerjakan | `21c609f2853574532f74dd2b1489b8d2e502abd1` |
| Tanggal | 18 Agustus 2026 |
| Jenis perubahan | Dua kolom, satu index, satu migration |
| **Status** | **Belum selesai — belum pernah dikompilasi dan belum diterapkan ke database mana pun** |

---

## 1. Masalah yang diperbaiki

### 1.1 Keterlambatan tidak punya tempat tinggal

Setiap penilaian triage sudah menyimpan `ResponseDueAt`, yaitu batas waktu kapan pasien
seharusnya sudah ditangani. Yang belum ada adalah tempat untuk mencatat **kenyataan bahwa
batas itu benar-benar terlampaui**.

Selisih keduanya penting. `ResponseDueAt` adalah janji; ia dihitung sekali saat penilaian
dibuat dan tidak pernah berubah. Sedangkan "pasien ini terlambat ditangani" adalah peristiwa
yang terjadi kemudian, punya waktu kejadiannya sendiri, dan perlu bertahan sebagai riwayat
walaupun pasiennya akhirnya ditangani.

Tanpa kolom penyimpan, satu-satunya cara mengetahui siapa yang terlambat adalah menghitung
ulang `ResponseDueAt < sekarang` setiap kali layar dibuka. Cara itu punya tiga kelemahan:

| Kelemahan | Akibatnya |
| --- | --- |
| Keterlambatan hanya terlihat bila ada yang membuka layar | Kalau tidak ada yang melihat, tidak ada yang tercatat |
| Waktu pelampauan tidak pernah tersimpan | Audit tidak dapat menjawab "sejak kapan pasien ini terlambat" |
| Riwayat hilang begitu pasien ditangani | Laporan mutu kehilangan seluruh kejadian keterlambatan masa lalu |

### 1.2 Yang task ini kerjakan, dan yang tidak

Task ini **hanya menyiapkan tempatnya**. Ia tidak mengisi kolom itu, tidak memeriksa
keterlambatan, dan tidak menampilkan apa pun. Berdiri sendiri, perubahan ini tidak mengubah
perilaku aplikasi sama sekali — setiap baris baru maupun lama bernilai `false`.

Pengisiannya adalah pekerjaan `BE-IGD-006` (proses pemantau), dan penyajiannya `BE-IGD-007`
(daftar breach). Pemisahan ini disengaja: menambah kolom dapat dilakukan tanpa mematikan
layanan, sedangkan menyalakan pemantau tidak.

---

## 2. Proses bisnis

### 2.1 Tujuan

Menyediakan penyimpanan permanen bagi fakta "target respons untuk penilaian ini terlampaui",
beserta waktu kejadiannya.

### 2.2 Pelaku

Tidak ada pelaku manusia. Kedua kolom diisi proses latar belakang, bukan oleh petugas. Tidak
ada layar, endpoint, maupun hak akses yang berubah pada task ini.

### 2.3 Aturan bisnis yang melekat pada bentuk datanya

| No | Aturan | Cara ditegakkan |
| --- | --- | --- |
| 1 | Sebuah penilaian selalu punya jawaban atas "apakah terlambat" | `IsSlaBreached` wajib terisi, tidak boleh kosong |
| 2 | Keadaan awal setiap penilaian adalah "belum terlambat" | Nilai bawaan `false`, ditegakkan di database |
| 3 | Waktu pelampauan hanya ada bila pelampauan memang terjadi | `SlaBreachedAt` boleh kosong |
| 4 | Riwayat lama tidak boleh dinilai ulang secara surut | Migration mengisi `false` apa adanya, tanpa menghitung `ResponseDueAt` lama |

Aturan keempat adalah yang paling mudah dilanggar tanpa sadar. Menghitung ulang breach untuk
data lama terdengar seperti kelengkapan, padahal itu mengarang riwayat: `ResponseDueAt` lama
tidak selalu terisi, dan `BE-IGD-002` baru saja mengubah arti kolom targetnya. Menandai breach
untuk data yang dibuat sebelum aturan itu ada berarti menilai masa lalu dengan aturan baru.

### 2.4 Hasil akhir

Tabel `TrxEmergencyTriage` memiliki dua kolom baru dan satu index gabungan. Seluruh baris
lama bernilai `false` dengan waktu kosong. Tidak ada satu pun perilaku aplikasi yang berubah.

---

## 3. File yang diubah

| File | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs` | Dua properti baru beserta dokumentasinya |
| `Repositories/Configurations/HealthService/EmergencyInstallationManagement/TrxEmergencyTriageConfiguration.cs` | Nilai bawaan `false` dan index gabungan |
| `Migrations/20260818084734_AddTriageSlaBreachMarker.cs` | **Baru** — dua `AddColumn`, satu `CreateIndex`, beserta `Down` |
| `Migrations/20260818084734_AddTriageSlaBreachMarker.Designer.cs` | **Baru** — snapshot model target |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tiga sisipan, sepuluh baris, seluruhnya di dalam entitas `TrxEmergencyTriage` |

### 3.1 Bentuk kolom

```csharp
public bool IsSlaBreached { get; set; } = false;

public DateTime? SlaBreachedAt { get; set; }
```

Bentuknya mengikuti preseden `TrxHrServiceRequest.IsSlaBreached` pada modul Human Resource,
sesuai arahan Reuse di roadmap. Nilai bawaan ditegakkan di database lewat
`HasDefaultValue(false)`, bukan hanya di kode C#, supaya baris yang dibuat lewat jalur lain
tetap memperoleh nilai yang benar.

### 3.2 Index gabungan

```csharp
builder.HasIndex(x => new { x.EmergencyVisitId, x.ResponseDueAt, x.IsSlaBreached });
```

Urutan kolomnya bukan selera. `BE-IGD-006` akan memindai "penilaian yang `ResponseDueAt`-nya
sudah lewat dan belum ditandai", dan `BE-IGD-007` akan menyaring per unit. Urutan ini
melayani keduanya.

### 3.3 Yang sengaja tidak diubah

| Tidak diubah | Alasan |
| --- | --- |
| DTO triage mana pun | Kedua kolom belum perlu keluar lewat API sampai `BE-IGD-007` |
| `EmergencyTriageService`, `EmergencyTriageController` | Task ini tidak menyentuh perilaku |
| Perhitungan `ResponseDueAt` | Milik `BE-IGD-002`, sudah selesai dan tidak boleh diusik |
| Data lama | Aturan bisnis nomor 4 di atas |

---

## 4. Verifikasi

### 4.1 Yang sudah dijalankan

| Pemeriksaan | Hasil |
| --- | --- |
| Snapshot model sebelum perubahan bersih dari `IsSlaBreached` milik triage | Terbukti — satu-satunya kemunculan ada pada `TrxHrServiceRequest` baris 14193 |
| Diff snapshot setelah perubahan | Tepat 10 baris, 3 sisipan, seluruhnya di dalam entitas `TrxEmergencyTriage` |
| Designer dibanding snapshot | Berbeda hanya pada 4 titik header, pola identik dengan `MakeTriageMaxWaitingMinutesNullable.Designer.cs` |
| Urutan properti dan index mengikuti konvensi EF | `IsRetriage` < `IsSlaBreached` < `MaxWaitingMinutesSnapshot`; `Sequence` < `SlaBreachedAt` < `StartedAt`; index 3-kolom diurutkan `ResponseDueAt` sebelum `TriageStatus` |
| Nama index terhadap batas identifier PostgreSQL | Nama penuh 66 karakter, dipotong ke 62 + `~` menjadi 63, mengikuti pola yang dipakai 8 index lain di project ini |
| Konsistensi nama index antara `Up` dan `Down` | Sama persis |

### 4.2 Yang **belum** dijalankan, beserta alasannya

| Belum dijalankan | Alasan |
| --- | --- |
| **`dotnet build`** | Diminta owner untuk dijalankan sendiri. Build sebelumnya dihentikan setelah satu jam tanpa menghasilkan assembly |
| **`dotnet ef migrations add`** | Migration ditulis tangan justru karena perintah ini menuntut build lebih dulu |
| **Uji migration maju** | Butuh database; lihat bagian 5 |
| **Uji migration mundur** | Sama |
| **Verifikasi bentuk kolom dan index di database** | Sama |

### 4.3 Acceptance criteria

| # | Kriteria | Status |
| --- | --- | --- |
| 1 | `IsSlaBreached` boolean wajib, nilai bawaan salah | Ada di kode dan migration — **belum terbukti** |
| 2 | `SlaBreachedAt` boleh kosong | Ada di kode dan migration — **belum terbukti** |
| 3 | Index `(EmergencyVisitId, ResponseDueAt, IsSlaBreached)` terbentuk | **Belum** — baru dideklarasikan, belum ada di database |
| 4 | Baris lama terisi salah tanpa perhitungan ulang | Dijamin oleh `defaultValue: false`, **belum terbukti** |
| 5 | Migration dapat maju dan mundur tanpa mematikan layanan | **Belum diuji** |

Tidak ada satu pun kriteria yang boleh dihitung lulus. Sesuai `requirement-traceability.md`
bagian 5 poin 2, task ini berstatus `In Progress`, bukan `Done`.

---

## 5. Gate database yang menahan pengujian

`DefaultConnection` pada `appsettings.Development.json` menunjuk ke
`160.22.250.77:5432/QuilvianNewDevTim02` — **database dev bersama tim, bukan lokal**. Tidak
ada PostgreSQL yang berjalan di `localhost:5432`.

Roadmap `BE-IGD-005` menyatakan pada Risk/blocker: *"Migration tidak boleh diterapkan ke basis
data mana pun selain lokal tanpa izin eksplisit."* Karena itu `dotnet ef database update`
**tidak dijalankan**, dan uji maju-mundur belum dapat dilakukan.

Tiga jalan keluar, urut dari yang paling sesuai roadmap:

1. Sediakan PostgreSQL lokal, arahkan connection string lewat User Secrets atau environment
   variable — bukan dengan mengedit `appsettings` yang ter-commit
2. Minta izin eksplisit Backend/API owner untuk menerapkan ke DB dev tim, dengan pemberitahuan
   lebih dulu kepada anggota tim yang memakainya
3. Tunda pengujian dan biarkan task ini `In Progress` sampai salah satu di atas tersedia

---

## 6. Penyimpangan yang perlu disahkan

### 6.1 Migration ditulis tangan, bukan dihasilkan tooling

Ini penyimpangan proses, bukan penyimpangan desain. Penyebabnya: `dotnet ef migrations add`
menuntut build yang sukses, sedangkan build project ini berjalan lebih dari satu jam tanpa
menghasilkan assembly dan owner memilih menjalankannya sendiri.

Ketiga file yang biasanya dihasilkan tooling — migration, designer, dan pembaruan snapshot —
ditulis mengikuti pola file yang sudah ada, lalu diverifikasi seperti pada bagian 4.1.

**Cara membuktikan hasilnya setara dengan keluaran tooling** ada di bagian 7 langkah 2. Selama
pembuktian itu belum dijalankan, anggap ketiga file berstatus belum tervalidasi.

### 6.2 Tipe waktu berbeda dari DDL pada data dictionary

Data dictionary bagian 6.1 menuliskan `"SlaBreachedAt" timestamp`, sedangkan yang saya pakai
adalah `timestamp with time zone`.

Alasannya konsistensi: **seluruh** kolom waktu pada tabel ini — `StartedAt`, `CompletedAt`,
`ResponseDueAt`, `ReviewedAt`, dan kolom audit — bertipe `timestamp with time zone` di model
yang sebenarnya. DDL pada data dictionary adalah penyederhanaan penulisan, bukan spesifikasi
yang berbeda. Memakai `timestamp` polos justru akan membuat satu kolom menyimpang sendirian.

Yang perlu dirapikan: DDL pada data dictionary bagian 6.1 dan 6.2, agar cocok dengan
kenyataan. Owner: Backend/API.

### 6.3 Nama index terpotong

Nama pada data dictionary, `IX_TrxEmergencyTriage_EmergencyVisitId_ResponseDueAt_IsSlaBreached`,
panjangnya 66 karakter sedangkan batas identifier PostgreSQL 63. Nama sebenarnya menjadi
`IX_TrxEmergencyTriage_EmergencyVisitId_ResponseDueAt_IsSlaBrea~`.

Ini bukan pilihan, melainkan akibat aturan yang sudah berlaku — delapan index lain di project
ini terpotong dengan cara yang sama. Dicatat di sini supaya tidak dikira salah ketik saat ada
yang membandingkan dokumen dengan database.

---

## 7. Langkah berikutnya, berurutan

1. **Owner menjalankan `dotnet build ./QuilvianSystemBackend.sln --configuration Release`.**
   Bila gagal, penyebabnya hampir pasti ada pada lima file di bagian 3 dan menjadi tanggung
   jawab pembuat laporan ini untuk diperbaiki.

2. **Buktikan migration tulisan tangan setara dengan keluaran tooling.** Setelah build hijau:

   ```bash
   dotnet ef migrations add VerifikasiDrift --no-build
   ```

   Migration yang dihasilkan harus **kosong** — `Up` dan `Down` tanpa satu pun operasi. Itu
   membuktikan snapshot sudah cocok dengan model. Setelah terbukti, hapus lagi:

   ```bash
   dotnet ef migrations remove --no-build
   ```

   Bila migration verifikasi ternyata **tidak** kosong, isinya menunjukkan persis bagian mana
   dari tulisan tangan yang meleset, dan itu yang harus diperbaiki.

3. **Selesaikan gate database** pada bagian 5, lalu jalankan uji maju-mundur.

4. **Perbarui laporan ini** — bagian 4.2, 4.3, dan 6.1 — dengan hasil yang benar-benar
   dijalankan, lalu naikkan status di `requirement-traceability.md`.

Sampai langkah 1 dan 2 selesai, `BE-IGD-006` **belum boleh dimulai**. Hosted service pemantau
akan menulis ke dua kolom yang keberadaannya di database belum terbukti.

---

## 8. Risiko tersisa

| No | Risiko | Dampak | Penanganan |
| ---: | --- | --- | --- |
| 1 | Migration tulisan tangan meleset dari model | EF menolak, atau diff migration berikutnya salah | Langkah 2 pada bagian 7 mendeteksinya sebelum menyentuh database |
| 2 | Migration diterapkan ke DB dev tim tanpa pemberitahuan | Anggota tim lain menemukan skema berubah di tengah pekerjaan | Gate pada bagian 5 |
| 3 | `BE-IGD-006` dimulai sebelum kolomnya benar-benar ada | Pemantau gagal saat runtime, bukan saat build | Urutan pada bagian 7 |
| 4 | Ada yang menghitung ulang breach untuk data lama | Riwayat palsu; laporan mutu mencampur dua aturan | Aturan bisnis nomor 4 pada bagian 2.3 |
