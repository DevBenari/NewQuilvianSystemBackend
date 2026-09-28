# Validasi Runtime — Gizi V1

| Field | Nilai |
|---|---|
| Blueprint ID | `gizi` |
| Cakupan | Keputusan V1 `GIZ-DEC-011`, `GIZ-DEC-012`, `GIZ-DEC-014` |
| Tanggal | 25 September 2026 |
| Database uji | `localhost` / `QuilvianNewDevIkbalFr` |
| Verdict | **LULUS** pada lapisan basis data, API, hak akses, dan layar |

## Ringkasan

Struktur Gizi V1 diterapkan ke database uji dan diuji sampai tuntas pada lapisan basis data:
migration, skema, foreign key, constraint, dan histori revisi kebutuhan nutrisi. Seluruh
pengujian transaksional dijalankan di dalam satu transaksi lalu di-rollback, sehingga nol baris
tertinggal.

Alur lewat API ikut divalidasi memakai akun demo bawaan aplikasi: lima langkah alur, sembilan
aturan validasi service, dan penolakan `401` bagi permintaan tanpa token. Penegakan hak akses
diuji terpisah dengan akun non-SuperAdmin dan menghasilkan `403`. Layarnya ditelusuri peramban
sungguhan sampai satu revisi kebutuhan lahir dari layar.

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

## Penegakan hak akses

Yang diuji adalah pertanyaan yang sebenarnya: apakah pengguna yang **sudah login tetapi tidak
berhak** ditolak dengan `403` — bukan `401`, dan bukan diloloskan.

### Akun uji non-SuperAdmin

Dibuat lewat layar admin aplikasi sendiri: `POST /administrator/master-data/kiosk-devices`
lalu `POST /{id}/generate-login`. Akun perangkat kios bertipe bukan SuperAdmin dan tidak
memegang hak `NutritionRequirement` apa pun. Tidak ada kredensial yang disusun di luar
aplikasi, dan tidak ada akun yang sudah ada yang diubah.

### Temuan pendahuluan: otorisasi mati di mesin pengembangan

`appsettings.Development.json` pada mesin ini menyetel `Security:Authorization:Enabled = false`.
Dengan setelan itu `AccessPermissionService` melewati seluruh pemeriksaan hak akses, sehingga
setiap pengguna yang berhasil login diloloskan. Pengujian pertama memang mengembalikan `200`
untuk akun kios, dan itu bukan cacat modul Gizi melainkan saklar pengembangan.

Pengujian diulang dengan `Security__Authorization__Enabled=true` sebagai variabel proses.
Berkas konfigurasi tidak diubah, dan setelan aslinya tidak ikut tersentuh.

### Hasil

`POST /orders/{id}/requirements`, yang menuntut `NutritionRequirement : Update`:

| Pemanggil | Hasil |
|---|---|
| Tanpa token | **`401`** |
| Akun kios, sudah login, tanpa hak `NutritionRequirement` | **`403`** — "Anda tidak memiliki akses ke menu atau fitur ini." |
| Akun SuperAdmin, berhak | **`200`** — "Kebutuhan nutrisi berhasil disimpan." |

Ketiganya dijalankan pada instance yang sama, dalam hitungan detik, dengan badan permintaan
yang sama. Satu-satunya yang berbeda adalah siapa pemanggilnya.

Endpoint lain bagi akun kios juga `403`: `GET /orders/{id}/requirements`,
`GET /masters/nutrition-parameters`, `GET /masters/nutrition-diagnoses`.

## Penelusuran layar

Dijalankan dengan peramban sungguhan (Chromium lewat Playwright) terhadap frontend hasil
`next build` pada `http://localhost:3000`, memakai akun `opr.anestesi`. Skrip mengetik,
memilih dari dropdown, dan menekan tombol seperti petugas — bukan memeriksa render.

| Langkah | Hasil |
|---|---|
| Masuk aplikasi | LULUS |
| 1. Master Diagnosis Gizi | LULUS — Domain Diagnosis (3), ketiga domain IDNT tampil |
| 1b. Tambah diagnosis lewat layar | LULUS — kode baru muncul di daftar sesudah disimpan |
| 2. Master Parameter Nutrisi | LULUS — 5/5 parameter tampil; keterangan "Belum ada rumus terdaftar" muncul |
| 3. Panel Kebutuhan Nutrisi | LULUS — panel tampil pada detail order |
| 3b. Nilai yang berlaku terbaca | LULUS — 5/5 parameter beserta satuannya |
| 4. Input nilai kebutuhan | LULUS — tombol "Revisi Kebutuhan" membuka formulir berisi nilai berlaku sebagai titik awal |
| 5. Revisi kebutuhan disimpan | LULUS — revisi 7 menjadi 8 lewat layar |
| 6. Histori revisi | LULUS — 8 revisi tercatat, label "Berlaku" dan "Riwayat" tampil |

Revisi yang lahir dari layar diperiksa ulang lewat API: `revisionNumber: 8`, `isCurrent: true`,
`changeReason: "Revisi lewat layar saat penelusuran"`, energi `2200`, ditetapkan
"Perawat Instrumen Demo". Jadi yang tampil di layar memang yang tersimpan.

`GIZ017` juga terbukti bekerja di layar: percobaan menyimpan revisi tanpa mengisi alasan
ditolak dengan pesan "Alasan perubahan wajib diisi mulai revisi kedua."

## Keterbatasan yang masih diakui

Tidak ada lagi keterbatasan pada cakupan yang diminta. Dua hal berikut dicatat sebagai
konteks, bukan celah:

* Penegakan `403` diuji dengan mengaktifkan `Security:Authorization:Enabled` lewat variabel
  proses, karena mesin pengembangan ini mematikannya. Di lingkungan yang otorisasinya menyala
  — termasuk produksi, tempat saklar itu diabaikan — perilakunya adalah yang tercatat di atas.
* Penelusuran layar dijalankan peramban terotomasi, bukan tangan manusia. Ia menekan tombol
  yang sama dan membaca layar yang sama, tetapi tidak menilai rasa pemakaian.

## Temuan yang diperbaiki saat validasi

Respons `POST /orders/{id}/records` mengembalikan `diagnosisCode`, `diagnosisName`, dan
`domainCode` **kosong**, sedangkan `GET` atas data yang sama menampilkannya dengan benar.
Sebabnya: baris diagnosis ditambahkan lewat `DbSet`-nya sendiri agar berstatus `Added`, sehingga
navigasi `NutritionDiagnosis` belum terisi saat entity dipetakan. Akibatnya layar menampilkan
diagnosis tanpa keterangan sampai halaman dimuat ulang.

Diperbaiki dengan membaca ulang catatan kunjungan beserta `Include`-nya sesudah disimpan, lalu
diverifikasi: kunjungan berikutnya mengembalikan `NI-1.4 Asupan energi tidak adekuat` dan
`NC-1.1 Kesulitan menelan` lengkap dengan domainnya.


### Footer menutupi tombol "Simpan Kebutuhan"

Penelusuran layar tidak dapat menekan tombol simpan. Diperiksa dengan
`document.elementFromPoint` pada titik tengah tombol, dan yang berada di sana adalah
`FOOTER.iq-footer app-footer position=fixed z-index=1` — bukan tombolnya.

Halaman modul Gizi tidak menyisakan ruang bagi bilah footer yang berposisi tetap, sehingga
pada sebagian posisi gulir baris aksi berada tepat di baliknya dan klik petugas mendarat di
footer. Ini kelas bug yang sama dengan yang pernah ditemukan pada layar permintaan stok.

Diperbaiki dengan menambahkan ruang bawah pada `.page`:
`padding: 1rem 1rem calc(1rem + var(--app-footer-safe-space, 120px))`. Sesudahnya,
pemeriksaan titik yang sama mengembalikan tombolnya sendiri.

### Panel kebutuhan mengirim id pegawai, bukan id profil tenaga

Penyimpanan dari layar ditolak dengan "Ahli gizi yang dipilih tidak ditemukan." Sebabnya
pemilih "Ahli Gizi yang Menetapkan" memuat daftar **pegawai**, sedangkan
`determinedByWorkforceId` menuntut **profil tenaga kerja**. Panel mengirim nilai opsi apa
adanya, sehingga backend menolaknya dengan benar.

Formulir kunjungan yang sudah ada tidak mengalami ini karena membaca
`option.raw.workforceProfileId`. Panel kebutuhan kini melakukan hal yang sama, dengan id
pegawai disimpan terpisah untuk tampilan. Sesudah perbaikan, revisi dari layar tersimpan.

## Yang tidak dikerjakan dan alasannya

| Tidak dikerjakan | Alasan |
|---|---|
| Rumus kalkulasi nutrisi | `GIZ-OQ-007` ditunda pemilik proses. Registry berdiri kosong; tidak ada rumus yang dikarang |
| Batas klinis protein, lemak, karbohidrat, cairan | Belum ditetapkan siapa pun |
| Mengisi master diagnosis gizi | Daftarnya diimpor admin gizi, bukan dikarang sistem |
| Migration tambahan | Tidak dibuat; satu migration V1 sudah mencakup seluruh perubahan |
