# Validasi Runtime — Gizi V1

| Field | Nilai |
|---|---|
| Blueprint ID | `gizi` |
| Cakupan | Keputusan V1 `GIZ-DEC-011`, `GIZ-DEC-012`, `GIZ-DEC-014` |
| Tanggal | 25 September 2026 |
| Database uji | `localhost` / `QuilvianNewDevIkbalFr` |
| Verdict | **LULUS pada lapisan basis data. Lapisan API BELUM divalidasi** |

## Ringkasan

Struktur Gizi V1 diterapkan ke database uji dan diuji sampai tuntas pada lapisan basis data:
migration, skema, foreign key, constraint, dan histori revisi kebutuhan nutrisi. Seluruh
pengujian transaksional dijalankan di dalam satu transaksi lalu di-rollback, sehingga nol baris
tertinggal.

Yang **belum** divalidasi adalah alur lewat API. Sebabnya tercatat di bagian keterbatasan, dan
itu bukan temuan cacat melainkan akses yang belum tersedia.

## Yang diverifikasi

### Migration dan skema

| Pemeriksaan | Hasil |
|---|---|
| Tabel `Gzi` sebelum migration | 10 |
| Tabel `Gzi` sesudah migration | **17** |
| Kolom yang dicabut dari `GziNutritionCareRecord` | `NutritionDiagnosisId`, `DietPrescription`, `EnergyRequirementKcal` — **0 tersisa** |
| Baris yang hilang akibat pencabutan | **0** — seluruh tabel `Gzi` diperiksa 0 baris sebelum migration dijalankan |
| Tabel milik modul lain yang tersentuh | **nihil**; skrip hanya menyebut tabel berawalan `Gzi` |

### Isi master yang diisi migration

| Master | Isi |
|---|---|
| `GziNutritionDiagnosisDomain` | `NI` Nutrition Intake, `NC` Nutrition Clinical, `NB` Nutrition Behavioral-Environmental |
| `GziNutritionParameter` | `ENERGY` kkal/hari batas 1–10000; `PROTEIN`, `FAT`, `CARBOHYDRATE` gram/hari; `FLUID` ml/hari — keempatnya **tanpa batas** |
| `GziNutritionDiagnosis` | **kosong**, sesuai keputusan: daftarnya diimpor admin gizi |
| `GziNutritionFormula` | **kosong**, sesuai `GIZ-OQ-007` yang ditunda |

Batas empat parameter terakhir sengaja kosong. Kosong berarti belum ditetapkan siapa pun, bukan
tak terbatas secara klinis.

### Alur lima langkah

Dijalankan langsung terhadap basis data, di dalam transaksi, lalu di-rollback.

| Langkah | Hasil |
|---|---|
| 1. Buat diagnosis gizi pasien | 2 diagnosis tersimpan pada satu kunjungan, satu di antaranya primer |
| 2. Simpan kebutuhan nutrisi | Revisi 1 tersimpan dengan 5 parameter; energi 1800 kkal/hari |
| 3. Ubah kebutuhan nutrisi | Revisi 1 berhenti berlaku, revisi 2 dibuka; energi 2100 kkal/hari beserta alasannya |
| 4. Histori perubahan tersimpan | Revisi 1 `IsCurrent=false` tetap terbaca lengkap; revisi 2 `IsCurrent=true` |
| 5. Master dapat dipakai | 2 baris `GziNutritionDiagnosis` dan 5 baris `GziNutritionParameter` benar-benar dirujuk |

Keluaran langkah 4 apa adanya:

```text
revisi 1 | berlaku=false | energi=1800.000 | alasan=(penetapan awal)
revisi 2 | berlaku=true  | energi=2100.000 | alasan=Berat badan pasien turun 4 kg
```

Inilah bukti bahwa perubahan **tidak menimpa**: angka yang dipakai merawat pasien sebelumnya
masih dapat dibaca sesudah direvisi.

### Constraint yang terbukti menolak

Keduanya diuji dengan sengaja melanggar, dan keduanya ditolak basis data — bukan hanya oleh
service.

| Aturan | Perlakuan | Hasil |
|---|---|---|
| `GIZ018` | Menjadikan diagnosis kedua ikut primer | **DITOLAK** — `duplicate key ... "IX_GziNutritionCareRecordDiagnosis_CareRecordId_Primary"` |
| `GIZ020` | Membuka revisi kedua tanpa menutup revisi pertama | **DITOLAK** — `duplicate key ... "IX_GziNutritionRequirement_NutritionOrderId_Current"` |

Penegakan di basis data penting karena dua permintaan yang tiba bersamaan tidak dapat dicegah
oleh pemeriksaan di service saja.

### Kebersihan sesudah pengujian

Diperiksa sesudah rollback:

```text
GziNutritionOrder=0  CareRecord=0  Requirement=0  RequirementItem=0
CareRecordDiagnosis=0  Diagnosis=0
seed: Domain=3  Parameter=5
```

Nol baris transaksi tertinggal; yang tersisa hanya isi master dari migration.

### Build dan drift

| Pemeriksaan | Hasil |
|---|---|
| Build backend `-p:SkipMigrationMetadata=true` | 0 error |
| Build backend + snapshot ikut dikompilasi | 0 error |
| `dotnet ef migrations has-pending-model-changes` | **`No changes have been made to the model since the last migration.`** |
| `next build` frontend | Compiled successfully; rute master diagnosis dan parameter terdaftar |

Snapshot ditulis tangan, dan bahwa ia benar-benar ikut dikompilasi dibuktikan dengan menyisipkan
baris yang sengaja salah lalu memastikan compiler menolaknya.

## Keterbatasan yang diakui

**Alur lewat API belum divalidasi, karena akses akun uji belum tersedia.**

Kata sandi akun pada database uji berbeda dari `SeedSuperAdmin:Password` di
`appsettings.Development.json`, dan ketiga akun yang ada menolak login. Upaya membuat akun uji
baru lewat seeder gagal karena `UserCode` SuperAdmin bersifat unik sehingga akun SuperAdmin kedua
tidak dapat dibuat. Tidak ada kredensial yang diubah, dan tidak ada akun yang tersisa.

### Yang karenanya belum terbukti berjalan

Aturan berikut ada di kode dan lolos kompilasi, tetapi belum diuji saat berjalan:

| Kode | Aturan |
|---|---|
| `GIZ015` | Seluruh parameter aktif wajib punya nilai final pada satu revisi |
| `GIZ016` | Alasan wajib bila nilai final berbeda dari nilai kalkulasi |
| `GIZ017` | Alasan revisi wajib mulai revisi kedua |
| `GIZ014` | Parameter yang dikirim harus aktif pada master |
| `GIZ019` | Rumus yang dirujuk harus terdaftar dan aktif |
| `GIZ013` | Idempotency key dipakai dengan isi permintaan berbeda |
| — | Penegakan hak akses `NutritionMaster` dan `NutritionRequirement` |

Yang **sudah** terbukti adalah bahwa dua aturan terpenting — `GIZ018` dan `GIZ020` — tetap
ditegakkan basis data walaupun service dilewati sama sekali.

### Cara menutup keterbatasan ini

Sediakan kredensial akun uji yang berlaku pada `QuilvianNewDevIkbalFr`, lalu jalankan alur lima
langkah di atas lewat endpoint `POST /orders/{id}/requirements`,
`PUT /orders/{id}/records/{recordId}/diagnoses`, dan `GET /orders/{id}/requirements`.

## Yang tidak dikerjakan dan alasannya

| Tidak dikerjakan | Alasan |
|---|---|
| Rumus kalkulasi nutrisi | `GIZ-OQ-007` ditunda pemilik proses. Registry berdiri kosong; tidak ada rumus yang dikarang |
| Batas klinis protein, lemak, karbohidrat, cairan | Belum ditetapkan siapa pun |
| Mengisi master diagnosis gizi | Daftarnya diimpor admin gizi, bukan dikarang sistem |
| Migration tambahan | Tidak dibuat; satu migration V1 sudah mencakup seluruh perubahan |
