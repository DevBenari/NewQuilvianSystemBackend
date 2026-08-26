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
| **Status** | **Sebagian terbukti — kriteria 1, 2, 3 lulus di dua database; migrasi mundur belum diuji, kriteria 4 belum dapat dibuktikan** |

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

### 4.1 Pemeriksaan statis sebelum eksekusi

| Pemeriksaan | Hasil |
| --- | --- |
| Snapshot sebelum perubahan bersih dari `IsSlaBreached` milik triage | Terbukti — satu-satunya kemunculan ada pada `TrxHrServiceRequest` |
| Diff snapshot setelah perubahan | Tepat 10 baris, 3 sisipan, seluruhnya di dalam entitas `TrxEmergencyTriage` |
| Designer dibanding snapshot | Berbeda hanya pada 4 titik header, pola identik dengan `MakeTriageMaxWaitingMinutesNullable.Designer.cs` |
| Urutan properti dan index mengikuti konvensi EF | `IsRetriage` < `IsSlaBreached` < `MaxWaitingMinutesSnapshot`; `Sequence` < `SlaBreachedAt` < `StartedAt` |
| Nama index terhadap batas identifier PostgreSQL | Nama penuh 66 karakter, dipotong ke 62 + `~` menjadi 63 |
| Konsistensi nama index antara `Up` dan `Down` | Sama persis |

### 4.2 Bukti eksekusi

**Build lulus.** Assembly Debug tertanggal 19 Agustus 2026 memuat `IsSlaBreached`,
`SlaBreachedAt`, `MarkSlaBreachesAsync`, dan `EmergencyTriageSlaMonitorOptions`.

**Migration tulisan tangan terbukti setara dengan keluaran tooling.** Sebuah migration
verifikasi dibuat memakai `dotnet ef migrations add`, dan hasilnya **`Up` dan `Down` kosong
tanpa satu operasi pun**. Itu membuktikan snapshot beserta Designer yang ditulis tangan cocok
sepenuhnya dengan model; bila ada selisih sekecil apa pun, EF akan menuliskan koreksinya di
situ. Migration verifikasi kemudian dibuang.

**Diterapkan dan diverifikasi di dua database.**

| Database | Keadaan |
| --- | --- |
| PostgreSQL lokal (container Docker, `QuilvianLocal`) — **lingkungan ini sudah dibongkar setelahnya**, lihat bagian 5 | 83 migrasi diterapkan dari nol; kedua kolom dan index terbentuk |
| `QuilvianNewDevTim01` | `20260818084734_AddTriageSlaBreachMarker` tercatat pada `__EFMigrationsHistory`; nol migrasi tertunda |

Bentuk kolom pada **kedua** database, dibaca langsung dari `information_schema`:

| Kolom | Tipe | Nullable | Default |
| --- | --- | --- | --- |
| `IsSlaBreached` | `boolean` | NOT NULL | `false` |
| `SlaBreachedAt` | `timestamp with time zone` | boleh kosong | — |

Index yang terbentuk, dibaca dari `pg_indexes`:

```
IX_TrxEmergencyTriage_EmergencyVisitId_ResponseDueAt_IsSlaBrea~
  -> btree ("EmergencyVisitId", "ResponseDueAt", "IsSlaBreached")
```

Pemotongan nama ke 63 karakter yang dihitung manual pada bagian 6.3 terbukti tepat.

### 4.3 Yang **belum** terbukti

| Belum dijalankan | Alasan |
| --- | --- |
| **Uji migration mundur** | Belum dijalankan. Ini satu-satunya sisa pekerjaan teknis pada task ini |
| **Kriteria 4 — baris lama terisi salah** | `TrxEmergencyTriage` bernilai **nol baris** pada kedua database, sehingga tidak ada "baris lama" yang dapat diperiksa |
| **Test otomatis** | Solution tidak memiliki test project |

Kriteria 4 dijamin secara semantik oleh PostgreSQL: `ADD COLUMN ... NOT NULL DEFAULT false`
mengisi baris yang sudah ada. Tetapi jaminan semantik bukan bukti pengujian, dan aturan pada
`requirement-traceability.md` bagian 5 melarang menghitungnya lulus. Pembuktiannya menunggu
data nyata pada `QuilvianNewDevTim01`.

### 4.4 Acceptance criteria

| # | Kriteria | Status |
| --- | --- | --- |
| 1 | `IsSlaBreached` boolean wajib, nilai bawaan salah | **Terbukti** di dua database |
| 2 | `SlaBreachedAt` boleh kosong | **Terbukti** di dua database |
| 3 | Index gabungan terbentuk | **Terbukti** di dua database |
| 4 | Baris lama terisi salah tanpa perhitungan ulang | **Belum terbukti** — nol baris |
| 5 | Migration dapat maju dan mundur | Maju **terbukti**; mundur **belum diuji** |

Task ini berstatus `In Progress`, bukan `Done`.

## 5. Catatan koreksi tentang target database

Revisi laporan sebelumnya menyatakan pengujian tertahan karena `DefaultConnection` menunjuk
database dev bersama tim. **Pernyataan itu keliru** dan dicabut di sini.

Kenyataannya project sudah menyediakan mekanisme override lokal, yaitu `appsettings.Local.json`
yang diabaikan Git. Yang benar-benar kurang saat itu hanyalah server PostgreSQL lokal yang
berjalan. Setelah container Docker dijalankan, migrasi diterapkan ke database lokal tanpa
menyentuh milik tim.

Mekanisme override lokal tersebut kemudian **dihapus atas permintaan owner**, dan lingkungan
lokalnya dibongkar: berkas `appsettings.Local.json` dihapus, blok pemuatannya dikeluarkan dari
`Program.cs`, dan container PostgreSQL lokal dimatikan beserta datanya.

**Keputusan yang berlaku sejak saat itu:** modul ini bekerja langsung terhadap
`appsettings.Development.json`, yaitu `QuilvianNewDevTim01`. Tidak ada lagi lapisan lokal yang
memisahkan pekerjaan pengembangan dari database bersama.

Pada `QuilvianNewDevTim01`, migrasi ini **sudah diterapkan** dan nol migrasi tertunda, sehingga
tidak diperlukan penulisan tambahan ke database bersama.

### 5.1 Temuan sampingan yang perlu perhatian

Perbandingan riwayat migrasi menemukan satu migrasi yang ada di server tetapi **tidak ada di
repo**: `20260610151122_addColumnMstKioskDevice`.

Ini bukan akibat pekerjaan pada task ini, melainkan kemungkinan berasal dari branch lain yang
pernah diterapkan ke database tersebut. Dampaknya belum terasa, tetapi berpotensi bentrok saat
rollback atau saat `database update` dijalankan dari branch berbeda. Owner: Backend/API.

## 6. Penyimpangan yang perlu disahkan

### 6.1 Migration ditulis tangan, bukan dihasilkan tooling

Ini penyimpangan proses, bukan penyimpangan desain. Penyebabnya: `dotnet ef migrations add`
menuntut build yang sukses, sedangkan build project ini berjalan lebih dari satu jam tanpa
menghasilkan assembly dan owner memilih menjalankannya sendiri.

Ketiga file yang biasanya dihasilkan tooling — migration, designer, dan pembaruan snapshot —
ditulis mengikuti pola file yang sudah ada, lalu diverifikasi seperti pada bagian 4.1.

**Pembuktiannya sudah dijalankan dan lulus.** Migration verifikasi yang dihasilkan tooling
menghasilkan `Up` dan `Down` kosong, sehingga ketiga file tulisan tangan terbukti setara dengan
keluaran tooling. Rinciannya pada bagian 4.2. Penyimpangan ini karena itu **selesai** dan tidak
lagi menyisakan risiko teknis; yang tersisa hanya pencatatannya di sini.

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

## 7. Langkah berikutnya

Dua langkah pertama pada revisi sebelumnya — build dan pembuktian anti-drift — **sudah selesai
dan lulus**. Yang tersisa:

1. **Uji migration mundur.** Satu-satunya kriteria teknis yang belum dijalankan:

   ```bash
   dotnet ef database update 20260814073820_MakeTriageMaxWaitingMinutesNullable
   ```

   Kedua kolom harus hilang, lalu `dotnet ef database update` mengembalikannya.

   **Tertahan.** Lingkungan lokal sudah dibongkar, sehingga satu-satunya sasaran yang tersisa
   adalah `QuilvianNewDevTim01` — database bersama tim. Menjatuhkan lalu membuat ulang kolom di
   sana berarti mengubah skema yang sedang dipakai orang lain, dan itu memerlukan izin eksplisit
   Backend/API owner sesuai Risk/blocker `BE-IGD-005` pada roadmap. Alternatifnya, siapkan
   kembali lingkungan lokal sementara khusus untuk uji ini.

2. **Buktikan kriteria 4** dengan data nyata. Selama `TrxEmergencyTriage` masih nol baris —
   dan pada `QuilvianNewDevTim01` memang nol — kriteria ini tidak dapat dibuktikan.

   Pembuktiannya baru mungkin setelah ada penilaian triage sungguhan di database tersebut,
   entah dari pemakaian normal atau dari restore data. Sampai saat itu, kriteria ini tetap
   ditulis **belum diuji**, bukan dianggap lulus.

3. **Bawa temuan bagian 5.1 ke Backend/API owner** — migrasi `addColumnMstKioskDevice` yang ada
   di server tetapi tidak ada di repo.

`BE-IGD-006` kini **boleh dilanjutkan**, karena keberadaan kedua kolom di database sudah
terbukti pada bagian 4.2.

---

## 8. Risiko tersisa

| No | Risiko | Keadaan |
| ---: | --- | --- |
| 1 | Migration tulisan tangan meleset dari model | **Tertutup** — migration verifikasi kosong membuktikan tidak ada selisih |
| 2 | Migration diterapkan ke DB dev tanpa pemberitahuan | **Tidak berlaku lagi** — migrasi sudah tercatat di `QuilvianNewDevTim01` dan nol migrasi tertunda |
| 3 | `BE-IGD-006` dimulai sebelum kolomnya ada | **Tertutup** — kolom terbukti ada di dua database |
| 4 | Kriteria 4 dianggap lulus padahal tidak diuji | **Terbuka** — ditulis apa adanya pada bagian 4.3 dan 4.4 |
| 5 | Migrasi mundur belum pernah diuji | **Terbuka** — langkah 1 pada bagian 7 |
| 6 | Riwayat migrasi repo dan server berbeda satu migrasi | **Terbuka** — bagian 5.1, menunggu Backend/API owner |
| 7 | Ada yang menghitung ulang breach untuk data lama | **Terkendali** — aturan bisnis nomor 4 pada bagian 2.3 |
