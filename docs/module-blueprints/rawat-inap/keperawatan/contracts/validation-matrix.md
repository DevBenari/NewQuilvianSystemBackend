# Validation Matrix — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | **`0.5.0`** — bagian 6, `draft` |
| `last_changed_in` | **`0.5.0`** — `VAL-KEP-19` s.d. `36`. Sebelumnya `0.1.0`; tidak bergerak pada `0.2.0` s.d. `0.4.0` |
| Status | **`draft`** untuk `0.5.0` |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.1`; `PRD-RWI-FINAL-001` v1.0.0. Bagian 6: `02-backend-architecture.md` `0.4`, decision log `21`, gate `1.6` |
| Tanggal | 2 September 2026 |

Pesan ditulis dalam bahasa yang dipahami pengguna, bukan istilah teknis.

---

## 1. Kelayakan konteks — penjaga `INV-KEP-01`

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-01` | Membuat pengkajian, rencana asuhan, tindakan | Encounter tidak punya episode rawat inap | "Pasien ini tidak sedang dirawat inap. Pengkajian rawat inap hanya untuk pasien yang sudah masuk kamar." | `422` |
| `VAL-KEP-02` | Sama | Episode ada tetapi masih `Draft` | "Pasien belum dikonfirmasi tiba di kamar. Catatan keperawatan dapat dibuat setelah pasien benar-benar masuk." | `422` |
| `VAL-KEP-03` | Sama | Episode `Closed` atau `Cancelled` | "Perawatan pasien ini sudah ditutup. Catatannya hanya dapat dibaca." | `422` |
| `VAL-KEP-04` | Membuat pengkajian tanpa antrean | Encounter bertipe rawat jalan atau medical check-up **tanpa** episode rawat inap | "Pengkajian untuk pasien poliklinik tetap harus lewat antrean." | `400` |

> `VAL-KEP-04` menjaga janji `RWI-DEC-070`: pelonggaran hanya untuk rawat inap dan IGD; rawat
> jalan dan medical check-up tidak boleh berubah sedikit pun.

---

## 2. Kewenangan perawat

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-05` | Menulis dokumentasi | Pengguna bukan perawat penanggung jawab episode dan bukan kepala ruangan | "Anda bukan perawat penanggung jawab pasien ini. Hubungi kepala ruangan bila perlu mencatat." | `403` |
| `VAL-KEP-06` | Menyunting catatan tindakan | Pengguna bukan penulis catatan dan catatannya belum final | "Catatan ini ditulis petugas lain. Anda tidak dapat mengubahnya." | `403` |
| `VAL-KEP-07` | Mengamandemen catatan final | Pengguna bukan penulis dan bukan kepala ruangan | "Catatan yang sudah final hanya dapat diubah penulisnya atau kepala ruangan, dan perubahannya tercatat." | `403` |

> **Episode tanpa perawat penanggung jawab tidak menahan pencatatan.** `RWI-DEC-047` menyatakan
> ketiadaan perawat hanya memunculkan baris pada daftar pantau. Bila `InpNurseAssignment` kosong,
> `VAL-KEP-05` jatuh ke kewenangan unit: perawat yang bertugas di unit layanan episode itu
> diizinkan. Menahan pencatatan karena penugasan belum diisi akan menghentikan pekerjaan nyata
> demi kelengkapan administrasi.

---

## 3. Isi pengkajian

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-08` | Menyelesaikan pengkajian | Isian wajib menurut kebijakan aktif belum terisi | "Pengkajian belum dapat diselesaikan. Bagian berikut masih kosong: {daftar}." | `400` |
| `VAL-KEP-09` | Skor risiko jatuh | Skor terisi tetapi kategorinya tidak | "Kategori risiko jatuh belum dipilih." | `400` |
| `VAL-KEP-10` | Skrining gizi | Hasil skrining berisiko tinggi tetapi rujukan gizi tidak dibuat | **Peringatan, bukan penolakan.** "Hasil skrining menunjukkan risiko gizi. Rujukan ke Gizi disarankan." | `200` |
| `VAL-KEP-11` | Pengkajian awal kedua | Sudah ada pengkajian awal aktif pada episode yang sama | "Pengkajian awal untuk pasien ini sudah ada. Gunakan pengkajian ulang." | `409` |
| `VAL-KEP-12` | Amandemen | Alasan kosong | "Alasan perubahan wajib diisi." | `400` |

> `VAL-KEP-10` sengaja **tidak** menolak. Modul Gizi berstatus `PLANNED`; menolak penyelesaian
> pengkajian karena rujukan tidak dapat dibuat akan menahan pekerjaan perawat karena modul yang
> belum ada.

---

## 4. Tindakan keperawatan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-13` | Mencatat tindakan | Waktu tindakan di masa depan | "Waktu tindakan tidak boleh melewati waktu sekarang." | `400` |
| `VAL-KEP-14` | Mencatat tindakan | Waktu tindakan sebelum pasien masuk kamar | "Waktu tindakan sebelum pasien masuk kamar. Periksa kembali waktunya." | `400` |
| `VAL-KEP-15` | Mencatat tindakan | Kunci idempotency sama dengan yang sudah tersimpan | **Bukan galat.** Mengembalikan catatan yang sudah ada, kode `200` | `200` |
| `VAL-KEP-16` | Menutup butir asuhan sebagai tercapai | Belum ada satu pun evaluasi | "Butir ini belum punya catatan evaluasi, sehingga belum dapat dinyatakan tercapai." | `400` |

---

## 5. Keterlambatan — memantau, bukan menolak

| Aturan | Berlaku pada | Kondisi | Perilaku |
| --- | --- | --- | --- |
| `VAL-KEP-17` | Pemantauan tenggat | `MstClinicalAssessmentPolicy` kosong | **Tidak ada yang dinyatakan terlambat.** `DueAt` tidak terisi; pencatatan tetap berjalan penuh |
| `VAL-KEP-18` | Pemantauan tenggat | Pengkajian lewat tenggat kebijakan aktif | Muncul pada daftar pantau. **Tidak menahan tindakan apa pun** |

> Keterlambatan pengkajian **tidak pernah** menjadi gerbang. `INV-KEP-03` melarangnya, dan PRD
> 16.3 menyatakan dokter tidak perlu menunggu pengkajian selesai.

---

## 6. Perubahan pada `contract_version` `0.5.0` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status `draft`.** Seluruh aturan baru berlaku di server; frontend boleh menampilkan pemeriksaan yang sama lebih awal,
tetapi tidak menggantikannya. Aturan konteks `VAL-KEP-01` s.d. `VAL-KEP-03` dan kewenangan unit `VAL-KEP-05`
(sebagaimana diubah `RWI-DEC-100`, lihat `permission-audit-matrix.md` bagian 3A) berlaku untuk **setiap** jalur tulis
di bawah dan tidak diulang per baris.

> **Catatan pembacaan `VAL-KEP-05`.** Bunyi bagian 2 masih menulis "perawat penanggung jawab". Sejak `0.4.0` kondisinya
> adalah **pengguna tidak ditempatkan di unit layanan episode** (`RWI-DEC-100`). Pesan bagi pengguna: "Anda tidak
> ditempatkan di unit pasien ini."

### 6.1 Konfigurasi klinis berversi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-19a` | Menyimpan versi instrumen | Pita skor bertumpuk, berlubang, atau tidak menutup skor terendah sampai tertinggi yang mungkin | "Rentang kategori tidak boleh bertumpuk atau berlubang. Periksa batas Sedang dan Tinggi." | `400` |
| `VAL-KEP-19b` | Sama | Kode isian ganda; isian `single`/`multi` tanpa pilihan; `binding` ke kolom di luar daftar yang diizinkan | "Definisi formulir tidak sah: {rincian}." | `400` |
| `VAL-KEP-19c` | Membuat atau mengaktifkan instrumen | Rentang usia bertumpuk dengan instrumen aktif sejenis | "Rentang usia bertumpuk dengan instrumen {nama}." | `409` |
| `VAL-KEP-20a` | Mengesahkan versi | Pengesah adalah pengubah terakhir versi itu — `RWI-AC-196` | "Versi ini terakhir diubah oleh Anda. Pengesahan harus dilakukan orang lain." | `403` |
| `VAL-KEP-20b` | Mengubah versi | Versi berstatus `Approved` atau `Retired` | "Versi yang sudah disahkan tidak dapat diubah. Buat versi baru." | `409` |
| `VAL-KEP-20c` | Mengubah atau mengesahkan | `ExpectedDefinitionHash` berbeda dari yang tersimpan | "Definisi sudah diubah orang lain. Muat ulang sebelum melanjutkan." | `409` |

### 6.2 Dokumen Pengkajian Pasien

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-21a` | Menyelesaikan Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Perencanaan Pulang | Lingkungan tanpa `ClinicalConfiguration:AllowDraftVersionsForTesting` dan versi yang dipakai bukan `Approved` — `RWI-DEC-124` | "Instrumen belum disahkan. Dokumen dapat disimpan sebagai konsep dan diselesaikan setelah pengesahan." | `422` |
| `VAL-KEP-21b` | Sama | Isian pada `requiredItemCodes` versi itu kosong | "Isian wajib belum lengkap: {daftar label}." | `422` |
| `VAL-KEP-21c` | Menyimpan konsep | Versi yang dikirim bukan versi yang berlaku bagi usia pasien saat ini | "Formulir sudah diganti versi baru. Muat ulang formulir; isian Anda tetap tersimpan di layar." | `409` |
| `VAL-KEP-21d` | Menyimpan Kajian Umum | `VitalSignId` milik episode lain atau berstatus `Cancelled`/`EnteredInError` | "Tanda vital yang dipilih bukan milik pasien ini." | `400` |
| `VAL-KEP-22a` | Menyelesaikan Monitoring Nyeri | `PainAssessmentState = NotAssessed` | "Pilih keadaan nyeri: tidak nyeri, nyeri, atau tidak dapat dinilai." | `422` |
| `VAL-KEP-22b` | Sama | `HasPain` tanpa `PainScale`; skala di luar rentang instrumen | "Isi skala nyeri sesuai instrumen yang dipakai." | `400` |
| `VAL-KEP-22c` | Mencatat tanda vital untuk encounter berepisode `Admitted` | Isian `HasPain`, `PainScale`, `PainLocation`, atau `PainNote` terisi — `INV-KEP-04` | "Nyeri dicatat pada Monitoring Nyeri, bukan pada tanda vital." | `400` |
| `VAL-KEP-23a` | Menulis Evaluasi Awal | Pengguna tidak memegang `CaseManagementEvaluation : Create`/`Update`, **atau** tidak ditempatkan di unit episode — `RWI-AC-191` | "Evaluasi Awal hanya ditulis MPP yang ditempatkan di unit pasien ini." | `403` |
| `VAL-KEP-23b` | Membuat Evaluasi Awal | Episode sudah punya dokumen `Draft` atau `Completed` | "Pasien ini sudah punya Evaluasi Awal. Lengkapi lewat addendum." | `409` |
| `VAL-KEP-23c` | Menyunting atau menyelesaikan | Pengguna bukan penulis konsep | "Konsep ini ditulis MPP lain." | `403` |

### 6.3 Pengawasan Harian

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-24a` | Entri cairan | `VolumeMl ≤ 0` atau `> 10.000` | "Volume harus lebih dari 0 dan paling banyak 10.000 ml." | `400` |
| `VAL-KEP-24b` | Sama | Sumber tidak sejalan arah, misalnya Urin sebagai cairan masuk | "Sumber {sumber} bukan cairan {arah}." | `400` |
| `VAL-KEP-24c` | Sama | `EntryDateTime` lebih dari 5 menit di masa depan atau sebelum `AdmittedAt` | "Waktu pencatatan tidak boleh di masa depan atau sebelum pasien masuk." | `400` |
| `VAL-KEP-24d` | Sama | Sumber Obat tanpa `MedicationAdministrationId`; atau dosis ditautkan pada sumber selain Obat — `RWI-AC-229` | "Intake obat wajib memilih dosis yang diberikan dari MAR." | `400` |
| `VAL-KEP-24e` | Sama | Dosis bukan `Administered` — `RWI-AC-230` | "Dosis ini belum tercatat diberikan." | `409` |
| `VAL-KEP-24f` | Sama | Dosis sudah punya entri intake aktif — `RWI-AC-229` | "Dosis ini sudah punya entri intake pukul {jam}. Koreksi entri itu bila volumenya salah." | `409` |
| `VAL-KEP-24g` | Sama | Dosis milik episode lain | "Dosis ini bukan milik pasien ini." | `422` |
| `VAL-KEP-24h` | Koreksi atau pembatalan entri terukur (cairan, GDS, observasi) | Alasan kosong; `ExpectedRevisionNumber` basi | "Isi alasan koreksi." / "Data sudah diubah pengguna lain. Muat ulang." | `400` / `409` |
| `VAL-KEP-25a` | GDS bangsal | `GlucoseUnit` kosong — gate `G-25` | "Pilih satuan gula darah: mg/dL atau mmol/L." | `400` |
| `VAL-KEP-25b` | Sama | Nilai di luar `10`–`1000` mg/dL atau `0,6`–`55,5` mmol/L | "Nilai gula darah {nilai} {satuan} tidak mungkin. Periksa angka dan satuan." | `400` |
| `VAL-KEP-25c` | Membatalkan GDS | GDS sudah dipakai pelaksanaan sliding scale | "Gula darah ini sudah dipakai menghitung dosis insulin. Koreksi nilainya, jangan dibatalkan." | `409` |
| `VAL-KEP-26a` | Konfigurasi shift | Shift aktif satu unit tidak menutup 24 jam atau bertumpuk | "Jam shift harus menutup 24 jam tanpa celah dan tanpa tumpang tindih." | `400` |
| `VAL-KEP-26b` | Observasi harian | `DietIntakePercent` di luar `0`–`100`; lingkar perut di luar `20`–`250` cm | "Nilai {isian} di luar batas." | `400` |

### 6.4 Pelaksanaan sliding scale

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| **`VAL-KEP-27`** | Pelaksanaan dan preview | Tidak ada order sliding scale berstatus `Active` untuk butir itu, atau order `Stopped` — `RWI-AC-219`, `RWI-AC-226` | "Tidak ada protokol sliding scale aktif untuk pasien ini. Hubungi dokter." | `409` |
| **`VAL-KEP-28`** | Sama | GDS yang dirujuk bukan `CliBloodGlucoseReading` bangsal aktif — termasuk id hasil laboratorium — `RWI-AC-228` | "Dosis insulin hanya dihitung dari GDS bangsal yang dicatat perawat." | `422` |
| `VAL-KEP-29a` | Sama | Satuan GDS berbeda dari satuan protokol | "Satuan gula darah ({satuan GDS}) berbeda dari satuan protokol ({satuan protokol}). Periksa ulang; sistem tidak mengonversi." | `409` |
| `VAL-KEP-29b` | Pelaksanaan | GDS sudah dipakai pelaksanaan lain yang tercatat | "Gula darah ini sudah dipakai pukul {jam}. Ukur ulang untuk pemberian berikutnya." | `409` |
| `VAL-KEP-29c` | Sama | `ExpectedOrderVersionNumber` bukan versi berlaku | "Dokter sudah menyesuaikan protokol. Periksa dosis baru sebelum memberi." | `409` |
| `VAL-KEP-29d` | Sama | `ActualDoseUnits ≠ ComputedDoseUnits` tanpa `ExceptionReason` | "Dosis berbeda dari hitungan protokol. Isi alasan pengecualian." | `400` |
| `VAL-KEP-29e` | Sama | Header `Idempotency-Key` kosong | "Permintaan tidak dapat diproses. Muat ulang halaman." | `400` |
| `VAL-KEP-29f` | Pencatatan dosis MAR biasa | Butir berjenis dosis `SlidingScale` dicatat lewat `/record` atau `/as-needed` | "Insulin sliding scale dicatat lewat layar sliding scale." | `409` |

### 6.5 MAR

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-30a` | Mencatat `Administered` | Dosis aktual, rute, atau waktu pemberian kosong | "Isi dosis, rute, dan waktu pemberian." | `400` |
| `VAL-KEP-30b` | Mencatat `Held`, `Refused`, `Missed` | Alasan kosong — `AC-MVP-027` | "Isi alasan dosis {status}." | `400` |
| `VAL-KEP-30c` | Mencatat `Administered` | Dosis aktual ≠ dosis resep, atau waktu di luar ±60 menit dari jadwal, tanpa `DeviationNote` | "Dosis atau waktu berbeda dari jadwal. Isi catatan penyimpangan." | `400` |
| `VAL-KEP-30d` | Mencatat | Dosis bukan `Due`, atau sedang `Pending` cek ganda | "Dosis ini sudah dicatat." | `409` |
| `VAL-KEP-30e` | Mencatat | Butir resep `IsStopped = true` dan `ScheduledAt ≥ StoppedAt` | "Resep obat ini sudah dihentikan dokter." | `409` |
| `VAL-KEP-30f` | Mencatat | `AdministeredAt` lebih dari 5 menit di masa depan | "Waktu pemberian tidak boleh di masa depan." | `400` |
| `VAL-KEP-31a` | Cek ganda | Pemeriksa kedua = pencatat — `INV-KEP-05` | "Cek ganda harus dilakukan perawat lain." | `403` |
| `VAL-KEP-31b` | Cek ganda | Dosis tidak `Pending` | "Dosis ini tidak sedang menunggu cek ganda." | `409` |
| `VAL-KEP-31c` | Menolak cek ganda | Catatan kosong | "Isi alasan penolakan." | `400` |
| `VAL-KEP-32a` | Pemberian PRN | Butir bukan `IsAsNeeded` | "Obat ini bukan obat sesuai kebutuhan." | `409` |
| `VAL-KEP-32b` | Sama | Indikasi kosong | "Isi indikasi pemberian." | `400` |
| `VAL-KEP-32c` | Pemberian tanpa jadwal | Kode frekuensi butir **sudah** punya jadwal | "Jadwal obat ini sudah tersedia. Catat pada kolom jam yang sesuai." | `409` |
| `VAL-KEP-33a` | Koreksi MAR | Alasan koreksi kosong; `ExpectedRevisionNumber` basi | "Isi alasan koreksi." / "Data sudah diubah pengguna lain." | `400` / `409` |
| `VAL-KEP-33b` | Koreksi MAR | Dosis tertaut pelaksanaan sliding scale dikoreksi ke status selain `Administered`/`Cancelled` | "Dosis sliding scale hanya dapat dikoreksi menjadi diberikan atau dibatalkan." | `409` |
| `VAL-KEP-34a` | Konfigurasi jadwal | Slot ganda jam yang sama; jumlah slot ≠ kali per hari kode frekuensi yang dikenal | "Kode {kode} membutuhkan {n} jam pemberian." | `400` |
| `VAL-KEP-34b` | Pengaturan MAR | Cakrawala di luar `1`–`72` jam; menit negatif | "Nilai pengaturan di luar batas." | `400` |

### 6.6 Dugaan reaksi obat

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| `VAL-KEP-35a` | Mencatat dugaan reaksi | Dosis bukan `Administered` | "Dugaan reaksi hanya dicatat untuk obat yang sudah diberikan." | `409` |
| `VAL-KEP-35b` | Sama | Deskripsi reaksi kosong | "Jelaskan reaksi yang terlihat." | `400` |

### 6.7 Pemantauan — penanda, bukan penolakan

| Aturan | Kondisi | Perilaku |
| --- | --- | --- |
| `VAL-KEP-36a` | Dosis `Due` melewati `ScheduledAt + MissedAfterMinutes` | Penanda "lewat waktu" di MAR. **Status tidak berubah otomatis** |
| `VAL-KEP-36b` | `MissedAfterMinutes` kosong | Tidak ada penanda lewat waktu |
| `VAL-KEP-36c` | Dosis `Administered` hari itu tanpa entri intake | Pengingat pada Pengawasan Harian — usulan `G-27`. Tidak mewajibkan |
| `VAL-KEP-36d` | `PainReassessmentDueAt` lewat | `ReassessmentOverdue` pada progres; tidak mengubah ✓/!/○ |
| `VAL-KEP-36e` | Butir berfrekuensi tanpa jadwal terkonfigurasi | Pesan di MAR "Jadwal pemberian untuk frekuensi {kode} belum dikonfigurasi"; tidak ada dosis terbentuk |
| `VAL-KEP-36f` | Dosis tertaut entri intake dikoreksi menjadi selain `Administered` | `DoseCorrectionFlaggedAt` diisi; entri tampil "perlu ditinjau" — usulan `G-26` |
