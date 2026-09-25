# Validasi Runtime — Gizi V1

| Field | Nilai |
|---|---|
| Blueprint ID | `gizi` |
| Cakupan | Keputusan V1 `GIZ-DEC-011`, `GIZ-DEC-012`, `GIZ-DEC-014` |
| Tanggal | 25 September 2026 |
| Database uji | `localhost` / `QuilvianNewDevIkbalFr` |
| Verdict | **LULUS pada lapisan basis data dan lapisan API.** Satu keterbatasan tersisa: penegakan `403` per-permission |

## Ringkasan

Struktur Gizi V1 diterapkan ke database uji dan diuji sampai tuntas pada lapisan basis data:
migration, skema, foreign key, constraint, dan histori revisi kebutuhan nutrisi. Seluruh
pengujian transaksional dijalankan di dalam satu transaksi lalu di-rollback, sehingga nol baris
tertinggal.

Alur lewat API kemudian ikut divalidasi memakai akun demo bawaan aplikasi: lima langkah alur,
sembilan aturan validasi service, dan penolakan `401` bagi permintaan tanpa token. Satu
keterbatasan tersisa, yaitu penegakan `403` bagi pengguna yang login tanpa hak terkait.

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

## Validasi lewat API

Dijalankan 25 September 2026 terhadap aplikasi yang berjalan di `http://localhost:5000`,
memakai akun demo `opr.anestesi` yang kata sandinya diselaraskan oleh seeder bawaan aplikasi
(`OperatingRoomDemoSeeder.EnsureDemoUsersAsync`, lewat `UserManager.ResetPasswordAsync`). Tidak
ada kredensial yang disusun sendiri dan tidak ada akun non-demo yang tersentuh.

### Permukaan API

Delapan jalur baru terdaftar pada dokumen OpenAPI aplikasi yang sedang berjalan, yang sekaligus
membuktikan controller terdaftar dan seluruh dependency injection-nya resolve:

```text
/masters/nutrition-diagnosis-domains      /orders/{orderId}/requirements
/masters/nutrition-diagnoses              /orders/{orderId}/requirements/current
/masters/nutrition-parameters             /orders/{orderId}/requirements/calculate
/masters/nutrition-formulas               /orders/{orderId}/records/{recordId}/diagnoses
```

### Alur lima langkah

| Langkah | Permintaan | Hasil |
|---|---|---|
| Master dapat dibaca | `GET /masters/nutrition-parameters`, `/nutrition-diagnosis-domains` | `200`; 5 parameter dan 3 domain terbaca |
| Master dapat diisi | `POST /masters/nutrition-diagnoses` ×2 | `200`; `NI-1.4` dan `NC-1.1` tersimpan |
| 1. Diagnosis gizi pasien | `POST /orders/{id}/records` dengan 2 diagnosis | `200`; keduanya tersimpan, satu primer, IMT terhitung 20,51 |
| 2. Simpan kebutuhan nutrisi | `POST /orders/{id}/requirements` | `200`; revisi 1 berlaku, 5 parameter |
| 3. Ubah kebutuhan nutrisi | `POST /orders/{id}/requirements` | `200`; revisi 2 berlaku, energi 1800 → 2100 |
| 4. Histori tersimpan | `GET /orders/{id}/requirements` | `200`; revisi 2 `isCurrent=true`, revisi 1 `isCurrent=false` **tetap terbaca** beserta alasan perubahannya |
| 5. Yang berlaku | `GET /orders/{id}/requirements/current` | `200`; revisi 2 |

### Aturan validasi service

Seluruhnya dipicu lewat API dan seluruhnya ditolak dengan kode yang benar.

| Kode | Perlakuan | Hasil |
|---|---|---|
| `GIZ015` | Revisi tanpa parameter `FLUID` | `422 GIZ015` — "Belum terisi: Cairan." |
| `GIZ014` | Parameter `ENERGY` dikirim dua kali | `422 GIZ014` |
| `GIZ016` | Nilai final 2000 berbeda dari kalkulasi 1800, tanpa alasan | `422 GIZ016` |
| `GIZ017` | Revisi kedua tanpa alasan perubahan | `422 GIZ017` |
| `GIZ019` | Rumus yang tidak terdaftar | `422 GIZ019` |
| `GIZ006` | Energi 12000 kkal, di atas batas master | `422 GIZ006` |
| `GIZ018` | Dua diagnosis primer pada satu kunjungan | `422 GIZ018` |
| `GIZ013` | Idempotency key sama, isi **sama** | `200`, revisi tetap 2 — tidak melahirkan revisi baru |
| `GIZ013` | Idempotency key sama, isi **berbeda** | `409 GIZ013` |

**Keutuhan data sesudah penolakan.** Diperiksa sesudah tujuh permintaan ditolak berturut-turut:
riwayat tetap berisi persis 2 revisi. Tidak ada revisi separuh jadi yang tertinggal.

### Perilaku tanpa rumus (`GIZ-OQ-007` deferred)

`POST /orders/{id}/requirements/calculate` menjawab `200` dengan `calculated: false` dan pesan
"Belum ada rumus terdaftar, sehingga nilai kebutuhan diisi ahli gizi." Seluruh `calculatedValue`
kosong. Ini yang diharapkan: ketiadaan rumus diterangkan, bukan disembunyikan, dan bukan diisi
angka bawaan yang akan tampak seperti hasil hitungan.

### Otorisasi

| Pemeriksaan | Hasil |
|---|---|
| 10 kombinasi metode+jalur dipanggil **tanpa token** | seluruhnya `401` |
| Pendaftaran permission | `NutritionRequirement` muncul pada registry akses dengan aksi `Read` dan `Update`, `canAssign: true`; `NutritionMaster` juga terdaftar |

## Keterbatasan yang masih diakui

**Penegakan per-permission (`403`) belum terbukti.** Yang terbukti adalah penolakan `401` bagi
permintaan tanpa token, dan bahwa permission-nya terdaftar serta dapat diberikan lewat layar
hak akses. Yang belum diuji adalah pengguna yang **sudah login tetapi tidak memiliki**
`NutritionRequirement : Update` — ia seharusnya menerima `403`.

Sebabnya: kedua akun demo yang tersedia bertipe SuperAdmin, dan membuat akun berhak terbatas
menuntut pembuatan akun baru yang berada di luar wewenang pengujian ini. Menutupnya memerlukan
satu akun uji tanpa hak `NutritionRequirement`, lalu memanggil ulang kesembilan endpoint di atas.

**Layar frontend belum ditelusuri seorang pengguna.** `next build` lolos dan kedua rute master
terdaftar, tetapi klik demi klik pada layar belum dijalankan.

## Temuan yang diperbaiki saat validasi

Respons `POST /orders/{id}/records` mengembalikan `diagnosisCode`, `diagnosisName`, dan
`domainCode` **kosong**, sedangkan `GET` atas data yang sama menampilkannya dengan benar.
Sebabnya: baris diagnosis ditambahkan lewat `DbSet`-nya sendiri agar berstatus `Added`, sehingga
navigasi `NutritionDiagnosis` belum terisi saat entity dipetakan. Akibatnya layar menampilkan
diagnosis tanpa keterangan sampai halaman dimuat ulang.

Diperbaiki dengan membaca ulang catatan kunjungan beserta `Include`-nya sesudah disimpan, lalu
diverifikasi: kunjungan berikutnya mengembalikan `NI-1.4 Asupan energi tidak adekuat` dan
`NC-1.1 Kesulitan menelan` lengkap dengan domainnya.

## Yang tidak dikerjakan dan alasannya

| Tidak dikerjakan | Alasan |
|---|---|
| Rumus kalkulasi nutrisi | `GIZ-OQ-007` ditunda pemilik proses. Registry berdiri kosong; tidak ada rumus yang dikarang |
| Batas klinis protein, lemak, karbohidrat, cairan | Belum ditetapkan siapa pun |
| Mengisi master diagnosis gizi | Daftarnya diimpor admin gizi, bukan dikarang sistem |
| Migration tambahan | Tidak dibuat; satu migration V1 sudah mencakup seluruh perubahan |
