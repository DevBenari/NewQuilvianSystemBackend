# Laporan Perubahan Backend — `BE-RWI-037`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-037` |
| Judul | Catatan dokter untuk pasien tanpa antrean tidak lagi menggagalkan sistem |
| Slice | `DOK-MVP-0` — perbaikan jalur tanpa antrean |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-037` |
| Trace | `DOK-TRC-DEF-01`; `FR-DOK-037`; `02-backend-architecture.md` §3.2; `contracts/integration-contract.md` §1.1 |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM`, skor 5: repository 0, berkas diperiksa 1, berkas diubah 1 (+2 berkas uji), logika bisnis 1, kontrak API 0, database 0, keamanan/auth 1, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, project uji, dan dokumen tracked sub-modul `dokter-rawat-inap` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `c8e83854af240186b5091da412fadde3810afcb1` pada branch `MHamzah` |
| Tanggal | 3 September 2026 |
| Status | **Selesai.** Kelima acceptance criteria terbukti; nol perubahan bentuk data |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Submodule | Tidak ada submodule tersendiri; berkas berada langsung di bawah modul |
| Pemilik / prefix registry | `ClinicalManagement / Cli`, `ACTIVE` |
| Applicability | `TOUCHED LEGACY` — `TrxDoctorConsultation` dan controller-nya adalah kode lama; perbaikan dibatasi pada cacat yang disebut task |
| QBE berlaku | `QBE-API-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-PERM-001` |
| QBE tidak berlaku | `QBE-NAM-001` dan `QBE-ENT-001` — task ini tidak membuat entity baru dan tidak menamai ulang entity lama |
| Archetype | Transaksi, aggregate ber-lifecycle. Tidak ada endpoint baru, tidak ada perubahan bentuk endpoint |
| Database authority | `NONE`. Nol kolom, nol tabel, nol migration pada task ini |
| Frontend | Diperiksa read-only untuk memastikan bentuk balasan tidak berubah; tidak ada berkas frontend yang disentuh |

---

## 1. Masalah yang diperbaiki

Pembuatan catatan dokter mengenal dua keadaan: pasien yang membawa nomor antrean, dan pasien yang
tidak. Pasien IGD dan pasien rawat inap termasuk yang kedua — mereka memang tidak pernah
mengambil nomor antrean poliklinik.

Kode pengambilannya sudah benar: baris antrean diambil **hanya bila** permintaan menyebutkan
nomornya, sehingga nilainya boleh kosong. Yang keliru adalah beberapa baris sesudahnya. Kode
langsung menulis keadaan antrean, waktu mulai konsultasi, dan waktu selesai ke dalam baris yang
boleh kosong itu **tanpa memeriksanya lebih dulu**.

Akibatnya sederhana dan berat: setiap permintaan tanpa nomor antrean berakhir sebagai kegagalan
sistem. Dari sisi dokter, layar hanya menampilkan pesan gagal, dan catatannya tidak tersimpan sama
sekali.

**Contoh nyata.** Seorang dokter jaga IGD memeriksa pasien kecelakaan pukul 02.10 dini hari.
Pasien itu tidak mengambil nomor antrean — memang tidak boleh, ia datang lewat pintu gawat
darurat. Dokter menulis keluhan dan hasil pemeriksaan, lalu menekan Simpan. Permintaan itu gagal.
Catatan pemeriksaannya hilang, dan dokter terpaksa menuliskannya di kertas.

Hal yang sama akan menimpa **setiap** pasien rawat inap begitu jalurnya dibuka, karena jalur tanpa
antrean adalah satu-satunya jalur mereka.

---

## 2. Proses bisnis

**Tujuan.** Dokter dapat menyimpan catatan pemeriksaan untuk pasien yang tidak memiliki nomor
antrean, tanpa permintaannya berujung kegagalan sistem.

**Pelaku.** Dokter pemeriksa, atau petugas yang mencatat atas sepengetahuan dokter.

**Pemicu.** Dokter menyelesaikan pemeriksaan lalu menyimpan catatannya.

**Langkah yang berurutan.**

1. Permintaan pembuatan catatan diperiksa lebih dulu. Bila permintaan **membawa** nomor antrean,
   yang diperiksa adalah antreannya: ada, membutuhkan dokter, dokternya sudah ditentukan, dan
   keadaannya termasuk yang boleh diperiksa dokter.
2. Bila permintaan **tidak** membawa nomor antrean, yang diperiksa adalah kunjungannya: kunjungan
   itu wajib merupakan kunjungan IGD, dan dokter pemeriksanya wajib disebut. Aturan ini tidak
   diubah task ini.
3. Catatan dokter dibentuk beserta nomornya, salinan tanda vital, dan isi pemeriksaan.
4. **Bila ada antrean**, keadaan antrean berpindah dan waktu mulai konsultasinya diisi.
   **Bila tidak ada antrean, langkah ini dilewati seluruhnya** — inilah perbaikannya.
5. Keadaan kunjungan berpindah menjadi sedang diperiksa, atau selesai diperiksa bila catatan
   langsung diselesaikan. Langkah ini berlaku pada **kedua** cabang.
6. Seluruhnya disimpan di dalam satu transaksi. Bila ada satu langkah gagal, tidak ada satu pun
   yang tersimpan setengah jadi.

**Aturan yang berlaku.**

- Baris antrean **tidak pernah dibuat** hanya karena catatan dokter dibuat. Jumlah baris antrean
  sebelum dan sesudah permintaan tanpa antrean wajib identik.
- Perpindahan keadaan kunjungan bukan mutasi antrean, sehingga ia tetap berjalan pada kedua
  cabang.

**Status yang dihasilkan.** Catatan berstatus sedang berjalan, atau selesai bila permintaan
memintanya diselesaikan langsung.

**Jalur tidak normal.**

| Keadaan | Hasilnya |
| --- | --- |
| Kunjungan rawat jalan dikirim tanpa nomor antrean | Ditolak `400` dengan kalimat lama: "Konsultasi tanpa antrean hanya untuk pasien IGD. Untuk pasien poli, buat konsultasi dari baris antreannya." |
| Tanpa antrean, dokter pemeriksa tidak disebut | Ditolak `400`, "Dokter pemeriksa wajib diisi untuk konsultasi tanpa antrean." |
| Kunjungan sudah punya catatan dokter | Ditolak `400`, "Konsultasi dokter untuk encounter ini sudah ada." |

**Hasil akhirnya.** Catatan tersimpan, dan pemeriksaan pasien IGD maupun pasien rawat inap tidak
lagi kehilangan dokumentasinya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs`
- `Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs`
- `Areas/HealthServices/RegistrationManagement/Models/TrxQueue.cs` dan configuration-nya
- `Areas/HealthServices/RegistrationManagement/Enums/EncounterStatus.cs`, `QueueStatus.cs`
- `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgVisit.cs`
- `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/DoctorConsultationValidationTests.cs`
- `docs/module-blueprints/rawat-inap/dokter-rawat-inap/02-backend-architecture.md` §3.2
- `docs/module-blueprints/rawat-inap/dokter-rawat-inap/contracts/integration-contract.md` §1.1

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | Seluruh mutasi baris antrean pada pembuatan catatan dibungkus penjagaan "hanya bila antreannya ada". Perpindahan keadaan kunjungan dipindahkan keluar dari penjagaan itu supaya berlaku pada kedua cabang |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/DoctorConsultationInpatientPathTests.cs` | **Baru.** Uji jalur tanpa antrean, hitungan baris antrean, regresi poliklinik, dan regresi kunjungan rawat jalan tanpa antrean |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Infrastructure/RawatInapTestData.cs` | **Baru.** Penyiapan data kunjungan, perawatan rawat inap, dokter master, dan penugasan DPJP untuk uji sub-modul ini |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/DoctorConsultationCompletionTests.cs` | Penyesuaian pembuatan controller mengikuti dependency baru yang datang dari `BE-RWI-043` |
| `QuilvianSystemBackend.csproj` | **Perbaikan infrastruktur build di luar scope task**, dikerjakan lebih dulu karena tanpanya tidak ada satu pun validasi yang dapat dijalankan — lihat bagian 3.4 |

### 3.4 Perbaikan infrastruktur build yang mendahului seluruh pekerjaan

Pada commit `c8e83854` — sebelum satu baris pun disunting task ini — `dotnet build` terhadap
project utama **gagal dengan 246 galat**. Seluruhnya galat yang sama:

```text
error CS0246: The type or namespace name 'FactAttribute' could not be found
error CS0246: The type or namespace name 'TheoryAttribute' could not be found
error CS0246: The type or namespace name 'InlineDataAttribute' could not be found
```

**Sebabnya.** Folder `QuilvianSystemBackend.Tests/` di akar repository berisi lima berkas uji
xUnit modul Laboratorium. Folder itu **tidak memiliki project file sendiri** dan **tidak terdaftar
pada solution**, sedangkan project web menyertakan seluruh berkas di bawah akar secara otomatis.
Akibatnya berkas uji itu ikut dikompilasi ke dalam aplikasi, yang memang tidak memiliki paket
xUnit. Folder itu masuk lewat merge modul Laboratorium (`c8fc5cb`), bukan lewat pekerjaan ini.

**Yang dikerjakan.** Satu baris pada `QuilvianSystemBackend.csproj`: folder itu dikeluarkan dari
kompilasi aplikasi, dengan cara dan alasan yang sama persis seperti folder `Tests\` yang sudah
lebih dulu dikecualikan di baris yang sama.

**Yang sengaja tidak dikerjakan.** Berkas ujinya **tidak dihapus**. Memindahkannya menjadi project
tersendiri di bawah `Tests\` adalah pekerjaan pemilik modul Laboratorium, bukan task ini.

Perbaikan ini dilaporkan di sini karena ia berada **di luar scope** `BE-RWI-037`. Ia dikerjakan
karena seluruh Definition of Done pada keenam task rangkaian ini menuntut build dan test yang
benar-benar dijalankan, dan tidak satu pun dapat dijalankan sebelum galat itu hilang. Pemilik
repository dipersilakan mencabutnya bila menghendaki penyelesaian yang berbeda.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route, HTTP method, bentuk permintaan, dan bentuk balasan **tidak berubah**. Yang berubah hanya nasib permintaan yang sebelumnya berujung kegagalan sistem |
| Database | `NOT APPLICABLE` untuk task ini. Nol kolom, nol tabel, nol migration. Tidak ada perintah database yang dijalankan |
| Keamanan/Auth | `NOT APPLICABLE`. `[AccessAction("Create", …)]` dan `[AccessPermission("DoctorConsultation", "Create")]` tidak disentuh, dan penjagaan kunjungan IGD pada cabang tanpa antrean tetap berlaku apa adanya. Tidak ada pelonggaran kewenangan |

---

## 4. Dokumentasi endpoint

Task ini tidak menambah maupun mengubah bentuk endpoint. Endpoint yang perilakunya diperbaiki:

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/doctor-consultations` | Membuat catatan pemeriksaan dokter, dengan maupun tanpa nomor antrean | `DoctorConsultation : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **pada commit `c8e83854`, sebelum perubahan apa pun** | **Gagal, `246 Error(s)`** — seluruhnya `CS0246` pada `Fact`, `Theory`, dan `InlineData` | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah; sebabnya di bagian 3.4 |
| `dotnet build QuilvianSystemBackend.csproj` sesudah perbaikan infrastruktur build | Berhasil, `0 Error(s)`, 186 warning — seluruhnya warning dokumentasi XML yang sudah ada sebelumnya | `PASS` | Keluaran perintah |
| `dotnet build QuilvianSystemBackend.sln` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| Catatan tanpa antrean tersimpan dan jumlah baris antrean tidak berubah | Balasan sukses; hitungan antrean sebelum dan sesudah sama-sama `0` | `PASS` | `DoctorConsultationInpatientPathTests.TanpaAntrean_Tersimpan_DanJumlahAntreanTidakBerubah` |
| Kunjungan ikut berpindah keadaan pada cabang tanpa antrean | Keadaan kunjungan menjadi `InConsultation` | `PASS` | `…TanpaAntrean_KunjunganIkutBerpindahKeadaan` |
| Regresi poliklinik lewat antrean | Balasan sukses; keadaan antrean menjadi `InConsultation`, waktu mulai konsultasi terisi | `PASS` | `…JalurAntreanPoliklinik_TetapBerhasil` |
| Regresi kunjungan rawat jalan tanpa antrean | Tetap ditolak `400` dengan kalimat lama; nol catatan tersimpan | `PASS` | `…RawatJalanTanpaAntrean_TetapDitolak` |
| Regresi IGD: catatan pertama diterima, catatan kedua ditolak seperti sebelumnya | Pertama sukses, kedua `400` | `PASS` | `…Igd_PerilakunyaTetapSepertiSebelumnya` |
| `dotnet test` project uji SQLite, seluruh berkas | `Failed: 0, Passed: 219, Skipped: 0` | `PASS` | Keluaran perintah |

Uji manual: `NOT FEASIBLE`. Menjalankan aplikasi backend memerlukan wewenang eksekusi runtime yang
terpisah dan tidak diberikan task ini.

**Tidak dijalankan:**

- Eksekusi migration dan perintah database apa pun — task ini memang tidak menyentuh database.
- Uji lewat HTTP sungguhan. Uji memanggil controller langsung, sesuai pola project uji yang sudah
  ada; pemeriksaan hak akses dan penyaringan permintaan berada di lapisan yang dilewati cara itu.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Membuat catatan untuk kunjungan tanpa baris antrean menghasilkan `201`, bukan `500` | Terpenuhi, **dengan catatan kode status** | Permintaan berhasil dan tidak lagi berujung kegagalan sistem. Kode suksesnya `200`, bukan `201` — lihat catatan di bawah tabel |
| 2. Jumlah baris antrean sebelum dan sesudah permintaan itu **identik** | Terpenuhi | `…TanpaAntrean_Tersimpan_DanJumlahAntreanTidakBerubah`; hitungan sebelum dan sesudah sama-sama `0` |
| 3. Jalur IGD lewat cara lamanya tetap berhasil | Terpenuhi | `…Igd_PerilakunyaTetapSepertiSebelumnya`; kunjungan IGD tanpa antrean berhasil membuat catatan |
| 4. Jalur poliklinik lewat antrean tetap berhasil | Terpenuhi | `…JalurAntreanPoliklinik_TetapBerhasil` |
| 5. Tidak ada kolom maupun tabel yang berubah | Terpenuhi | Diff task ini hanya menyentuh satu berkas controller dan berkas uji. Nol migration lahir dari `BE-RWI-037` |

**Catatan kode status pada kriteria 1.** Roadmap menuliskan `201`. Endpoint yang sebenarnya
membalas `200` beserta pembungkus `ApiResponse<T>`, dan `[ProducesResponseType(…, Status200OK)]`
sudah tertulis pada controller sejak sebelum task ini. `AGENTS.md` menetapkan bahwa untuk hal yang
diturunkan dari source — termasuk kontrak API — **source yang berlaku** dan selisihnya dilaporkan.
Mengubahnya menjadi `201` adalah perubahan kontrak yang merusak consumer frontend dan bukan bagian
dari task ini. Inti kriteria 1 — permintaan berhasil dan bukan lagi kegagalan sistem — terpenuhi.
Selisih ini diteruskan kepada pemilik kontrak untuk diputuskan terpisah.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Kelima acceptance criteria terbukti | Terpenuhi, dengan catatan kode status pada kriteria 1 |
| Empat test hijau | Terpenuhi — enam test hijau, melebihi yang diminta |
| Build lulus | Terpenuhi, `0 Error(s)` |
| Laporan menyebut nol perubahan bentuk data | Terpenuhi — lihat bagian 3.3 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 186 warning, seluruhnya warning dokumentasi XML yang sudah ada sebelum task ini dan tidak berkaitan dengan perubahan ini |
| Masalah yang diketahui | Jalur tanpa antrean masih terbatas pada kunjungan IGD. Membukanya untuk pasien rawat inap adalah pekerjaan `BE-RWI-044`, bukan task ini |
| Risiko tersisa | Rendah. Perubahannya menambahkan penjagaan pada cabang yang sebelumnya pasti gagal, dan cabang berantre tidak berubah perilakunya. Regresi poliklinik, rawat jalan, dan IGD seluruhnya hijau |
| Perubahan sampingan | Dua butir, keduanya disengaja dan tidak ada yang tergenerasi diam-diam. **Pertama**, satu baris pada `QuilvianSystemBackend.csproj` yang memulihkan build — lihat bagian 3.4; ini perbaikan **di luar scope** yang tanpanya tidak ada validasi yang dapat dijalankan. **Kedua**, `DoctorConsultationCompletionTests.cs` disesuaikan karena controller menerima satu dependency baru dari `BE-RWI-043`; tidak ada perilaku uji yang diubah, hanya cara controller dibuat |
| Interupsi | Satu interupsi eksekusi di tengah pembuatan migration `BE-RWI-042`. Pemulihan dilakukan dengan memeriksa keadaan berkas dan daftar migration yang benar-benar ada, lalu melanjutkan dari keadaan terverifikasi. Tidak ada penyuntingan ganda |
| Status Git | 51 baris pada `git status --short`; rinciannya ada pada laporan `BE-RWI-043` yang menutup rangkaian task ini. Tidak ada stage, commit, atau push |
| Langkah berikutnya | `BE-RWI-039` sudah selesai di rangkaian yang sama. Pekerjaan berikutnya yang membuka jalur rawat inap sepenuhnya adalah `BE-RWI-044` |
