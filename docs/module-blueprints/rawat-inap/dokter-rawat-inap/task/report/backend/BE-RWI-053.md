# Laporan Perubahan Backend — `BE-RWI-053`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-053` |
| Judul | DPJP memverifikasi catatan profesi lain, dan keterlambatannya terpantau |
| Slice | `DOK-MVP-6` — catatan terpadu dan verifikasi |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-053` |
| Trace | `EPIC DOK-04`; `FR-DOK-017` s.d. `FR-DOK-022`; `INV-DOK-11`; `AC-CAP021-03`; `RWI-RULE-021`, `RWI-RULE-030`; `contracts/state-transition-matrix.md` §3; `contracts/api-contract.md` §3; `VAL-DOK-07`, `VAL-DOK-24`, `VAL-DOK-25` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-040` 🟡 **sebagian** ([laporan](BE-RWI-040.md)) — kolom verifikasi sudah ada. `BE-RWI-046` **selesai** ([laporan](BE-RWI-046.md)) |
| Klasifikasi | `MEDIUM`, skor 10: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 3, kontrak API 2, database 0, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, satu controller `MedicalRecordManagement`, `Program.cs`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9be5526d248d9813a4044f063e43066a2364dd7d` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | ✅ **Selesai.** Keenam acceptance criteria terbukti. **Nol angka batas waktu ditanam di kode.** Nol migration |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement`, menyentuh satu controller `MedicalRecordManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli` — `ACTIVE / LEGACY`; `MedicalRecordManagement / Mrc` — `ACTIVE` |
| Applicability | `NEW CODE` untuk service verifikasi; `TOUCHED LEGACY` untuk controller catatan terpadu dan controller koreksi |
| QBE berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-LOG-001` |
| Entity operasional baru | `NONE`. Kolom verifikasi sudah dibuat `BE-RWI-040` |
| Archetype | Transaksi. Dua endpoint baca baru dan satu aksi bernama; nol `PATCH /{id}/status` generik |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

**DPJP tidak punya cara menyatakan sudah membaca catatan profesi lain, dan keterlambatannya
tidak terpantau.**

Lembar catatan terpadu diisi banyak profesi: perawat, ahli gizi, fisioterapis, farmasi. DPJP
bertanggung jawab atas keseluruhan perawatan pasien, sehingga ia perlu membaca catatan-catatan
itu. Sebelum task ini, tidak ada cara menyatakan bahwa ia sudah membacanya, dan tidak ada cara
mengetahui catatan mana yang terlewat.

Kolom penyimpanannya sudah dibuat `BE-RWI-040`; yang belum ada adalah mesin yang mengisinya.

---

## 2. Proses bisnis

### 2.1 Verifikasi memantau, ia tidak menahan

Ini keputusan yang paling menentukan pada task ini, dan `RWI-RULE-021` menyatakannya:

> Catatan yang belum diverifikasi **tetap sah**, tetap terbaca, dan tidak menahan penulisan
> catatan berikutnya. Menjadikan verifikasi sebagai gerbang pelayanan akan menghentikan
> pendokumentasian setiap kali DPJP sedang di kamar operasi — bahaya yang jauh lebih besar
> daripada catatan yang belum terbaca.

### 2.2 Nol angka batas waktu ditanam di kode

Nilai batas waktu verifikasi **belum disahkan**: `RWI-RULE-021` menunggu pemilik klinis yang
belum ditunjuk.

Mekanismenya dibangun penuh dan berjalan dengan **kebijakan kosong**:

| Keadaan | Yang terjadi |
| --- | --- |
| Tidak satu pun catatan diberi batas waktu | Seluruh catatan berstatus tidak-diwajibkan; daftar pantau kosong; pencatatan berjalan penuh |
| Sebuah catatan diberi batas waktu, dan batasnya lewat | Catatan itu muncul pada daftar pantau bertanda terlambat |

Menanam angka bawaan berarti mengarang kebijakan klinis, dan itu dilarang. Bawaan status yang
dipilih adalah **tidak-diwajibkan**, bukan menunggu — bawaan menunggu akan membuat setiap catatan
perawat langsung terhitung menunggu verifikasi pada rumah sakit yang tidak mewajibkannya, dan
daftar pantau penuh sejak hari pertama.

Layar menerima penanda `isVerificationPolicyEmpty` supaya ia dapat menyatakan "kebijakan
verifikasi belum aktif", bukan menampilkan daftar kosong yang tampak seperti semuanya sudah beres.

### 2.3 Verifikator bukan penulis, dan itu inti aturannya

`INV-DOK-11`. Verifikasi **tidak pernah** menulis ulang penulis catatan. Yang tersimpan adalah
dua nama pada dua kolom berbeda:

| Kolom | Isinya | Tanggung jawabnya |
| --- | --- | --- |
| Penulis | Perawat atau profesi lain yang menulis | Kebenaran isi catatan |
| Verifikator | DPJP yang membacanya | Pernyataan bahwa ia sudah membacanya |

Verifikator juga **tidak boleh sama** dengan penulis: menandatangani bacaan atas tulisan sendiri
bukan verifikasi.

### 2.4 DPJP yang aktif saat verifikasi, bukan saat catatan ditulis

`RWI-RULE-030`. DPJP yang menerima alih rawat hari ini bertanggung jawab atas pasiennya,
termasuk atas catatan yang ditulis sebelum ia mengambil alih. DPJP lama justru sudah tidak
berwenang lagi.

### 2.5 Koreksi mengembalikan ke keadaan menunggu

Verifikasi menyatakan "saya sudah membaca isi ini". Begitu isinya bertambah lewat koreksi,
pernyataan itu berhenti berlaku. Membiarkannya tetap terverifikasi berarti menampilkan tanda
tangan DPJP atas isi yang belum pernah ia baca.

Catatan berstatus **tidak-diwajibkan** tidak ikut dinaikkan: rumah sakit yang tidak mewajibkan
verifikasi tidak boleh tiba-tiba punya daftar pantau hanya karena ada koreksi.

### 2.6 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Pengguna tidak terhubung ke dokter mana pun | "Verifikasi hanya dapat dilakukan DPJP pasien ini." | `403` |
| Dokter bukan DPJP perawatan itu | Kalimat sama | `403` |
| Verifikator adalah penulis catatan itu sendiri | "Catatan Anda sendiri tidak dapat Anda verifikasi." | `403` |
| Catatan sudah diverifikasi sebelumnya | Ditolak | `409` |
| Catatan sudah dibatalkan | Ditolak | `400` |
| Catatan tidak berada di bawah perawatan rawat inap | Ditolak; verifikasi DPJP memang tidak berlaku baginya | `422` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria dan DoD |
| `contracts/state-transition-matrix.md` §3, §3.1 | Mesin status verifikasi dan aturan yang tidak boleh dilanggar |
| `contracts/api-contract.md` §3 | Bentuk endpoint dan catatan bahwa `Verify` adalah Action baru |
| `contracts/validation-matrix.md` §2, §7 | `VAL-DOK-07`, `VAL-DOK-24`, `VAL-DOK-25` |
| `contracts/permission-audit-matrix.md` §1.1, §2, §4 | Butir hak akses baru dan jejak audit yang diwajibkan |
| `Areas/HealthServices/ClinicalManagement/Enums/CpptVerificationStatus.cs` | Empat keadaan verifikasi dari `BE-RWI-040` |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | Pemeriksaan penugasan dokter berperiode |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/CpptVerificationService.cs` | **Baru.** Verifikasi oleh DPJP aktif; keadaan verifikasi per perawatan beserta daftar pantau; pengembalian ke keadaan menunggu setelah koreksi |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Endpoint `PATCH /{id}/verify`, `GET /episodes/{episodeId}/verification-status`, dan `GET /episodes/{episodeId}`; pemetaan pengguna ke baris dokter lewat data |
| `Areas/HealthServices/MedicalRecordManagement/Controllers/ClinicalNoteAddendumController.cs` | Memanggil pengembalian keadaan verifikasi setelah koreksi tersimpan |
| `Program.cs` | Pendaftaran service baru pada dependency injection |
| `Tests/.../ClinicalManagement/CpptVerificationTests.cs` | **Baru.** Delapan uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint baru sesuai `api-contract.md` §3, seluruhnya sebelumnya berstatus **Rencana**. Nol perubahan pada endpoint yang sudah ada |
| Database | Nol perubahan schema, nol migration, nol eksekusi database. **Nol tabel kebijakan verifikasi dibuat** — kebijakannya belum disahkan, dan membuat tabelnya sekarang berarti menebak bentuknya |
| Keamanan/Auth | Action baru **`Verify`** pada Resource `PatientIntegratedProgressNote` yang sudah ada. Namanya **sama persis** pada `[AccessAction]` dan `[AccessPermission]`, dan kesamaannya diuji |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Integrated Progress Note

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/verify` | DPJP menyatakan sudah membaca catatan profesi lain. **Tidak mengubah penulis aslinya** | `PatientIntegratedProgressNote : Verify` |
| `GET` | `/episodes/{episodeId}/verification-status` | Catatan yang menunggu dan yang lewat batas, beserta penanda kebijakan kosong | `PatientIntegratedProgressNote : Read` |
| `GET` | `/episodes/{episodeId}` | Lini masa catatan terpadu lintas profesi satu perawatan, terurut waktu catatan | `PatientIntegratedProgressNote : Read` |

**Kode status verifikasi:** `200` berhasil; `400` catatan sudah dibatalkan; `403` bukan DPJP
perawatan itu, atau memverifikasi catatannya sendiri; `404` catatan tidak ditemukan; `409` sudah
diverifikasi sebelumnya; `422` catatan bukan milik perawatan rawat inap.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `0 Error(s)`, `185 Warning(s)` | `PASS` | Keluaran perintah |
| `dotnet test` project uji SQLite | `Failed: 0, Passed: 320` | `PASS` | Keluaran perintah |
| Verifikasi tidak mengubah penulis asli | Penulis tetap perawat; verifikator DPJP; keduanya berbeda; jenis profesi dan isi catatan tidak berubah | `PASS` | `Verifikasi_TidakMengubahPenulisAsliDanMenyimpanVerifikatorTerpisah` |
| Setelah pergantian DPJP, DPJP lama ditolak dan DPJP baru diterima | `403` bagi DPJP lama dengan kalimat `VAL-DOK-07` persis; `200` bagi DPJP baru; verifikator tersimpan DPJP baru | `PASS` | `SetelahPergantianDpjp_DpjpLamaDitolakDanDpjpBaruDiterima` |
| Dokter jaga yang bukan DPJP ditolak | `403`; status catatan tetap menunggu; verifikator tetap kosong | `PASS` | `DokterJagaYangBukanDpjp_Ditolak403` |
| Kebijakan verifikasi kosong → daftar pantau kosong, pencatatan penuh | Tiga catatan lahir berstatus tidak-diwajibkan tanpa batas waktu; daftar pantau kosong; penanda kebijakan kosong benar | `PASS` | `KebijakanVerifikasiKosong_DaftarPantauKosongDanPencatatanBerjalanPenuh` |
| Catatan lewat batas muncul pada daftar pantau dan tidak menahan | Satu baris bertanda terlambat; catatan berikutnya tetap dapat ditulis | `PASS` | `CatatanLewatBatas_MunculPadaDaftarPantauTanpaMenahanCatatanBerikutnya` |
| Koreksi catatan terverifikasi mengembalikannya ke menunggu | Status kembali menunggu; waktu dan verifikator dikosongkan; penulis asli tetap perawat | `PASS` | `KoreksiCatatanTerverifikasi_MengembalikannyaKeMenungguVerifikasi` |
| Koreksi catatan tidak-diwajibkan tidak menaikkannya | Status tetap tidak-diwajibkan | `PASS` | `KoreksiCatatanTidakDiwajibkan_TidakMenaikkannyaKeMenungguVerifikasi` |
| Nama penanda aksi dan penanda hak akses `Verify` sama persis | Keduanya `Verify`; Resource-nya `PatientIntegratedProgressNote` | `PASS` | `PenandaAksiDanPenandaHakAksesVerify_BernamaSamaPersis` |
| `dotnet test` project uji InMemory | `Failed: 1, Passed: 908` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `BillingFinalizationServiceTests`, berkas tidak disentuh task ini |
| `dotnet test` project uji PostgreSQL | `Failed: 54, Passed: 34` | `EXISTING / ENVIRONMENT ISSUE` | Satu sebab: `BLOCKED_BY_TEST_DB_CONFIGURATION` |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** migration dan perintah basis data apa pun.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Verifikasi **tidak mengubah** penulis asli; verifikator tersimpan terpisah | Terpenuhi | `Verifikasi_TidakMengubahPenulisAsliDanMenyimpanVerifikatorTerpisah` |
| 2. Verifikator wajib DPJP yang **aktif saat verifikasi**, bukan yang aktif saat catatan ditulis | Terpenuhi | `SetelahPergantianDpjp_DpjpLamaDitolakDanDpjpBaruDiterima` |
| 3. Dokter jaga yang bukan DPJP ditolak `403` | Terpenuhi | `DokterJagaYangBukanDpjp_Ditolak403` |
| 4. Kebijakan verifikasi kosong berarti seluruh catatan tidak diwajibkan dan daftar pantau kosong, sedangkan pencatatan berjalan penuh | Terpenuhi | `KebijakanVerifikasiKosong_DaftarPantauKosongDanPencatatanBerjalanPenuh` |
| 5. Catatan yang lewat batas muncul pada daftar pantau dan **tidak menahan** penulisan catatan berikutnya | Terpenuhi | `CatatanLewatBatas_MunculPadaDaftarPantauTanpaMenahanCatatanBerikutnya` |
| 6. Koreksi catatan terverifikasi mengembalikannya ke menunggu verifikasi | Terpenuhi | `KoreksiCatatanTerverifikasi_MengembalikannyaKeMenungguVerifikasi` |

### Definition of Done

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terbukti | ✅ |
| Test kebijakan kosong hijau | ✅ `KebijakanVerifikasiKosong_...` |
| Laporan menyatakan nol angka batas waktu ditanam di kode | ✅ Bagian 2.2 dan bagian 7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan build baru |
| **Nol angka batas waktu ditanam** | Tidak satu angka pun muncul di source: tidak pada service, tidak pada controller, tidak pada nilai bawaan enum. Batas waktu dibaca dari kolom `VerificationDueAt` yang hari ini **tidak diisi siapa pun**, dan keadaan itu justru yang menghasilkan perilaku kebijakan kosong yang benar |
| **Keadaan terlambat diturunkan, bukan disimpan** | Keterlambatan dihitung saat dibaca, dengan membandingkan batas waktu terhadap saat ini. Menyimpannya sebagai status menuntut pekerjaan latar yang berjalan setiap menit hanya untuk menaikkan nilai, dan hasilnya tetap basi di antara dua jalannya |
| **Perubahan pada `MedicalRecordManagement`** | Sama seperti `BE-RWI-047`: satu parameter konstruktor dan satu pemanggilan pada `ClinicalNoteAddendumController`. Aturan bisnisnya berada penuh pada service milik `ClinicalManagement`. Utang pemberitahuan kepada pemilik modul tercatat pada [laporan `BE-RWI-047`](BE-RWI-047.md) |
| **Delta kontrak yang dilaporkan** | `GET /episodes/{episodeId}` — lini masa catatan terpadu lintas profesi — tercantum pada `api-contract.md` §3 berstatus Rencana, tetapi tidak disebut pada kolom Scope `BE-RWI-053`. Endpoint itu dibuat di sini karena daftar pantau verifikasi tidak berarti apa-apa tanpa permukaan yang menampilkan catatannya. Dilaporkan sebagai penambahan permukaan teknis, bukan kebijakan baru |
| Masalah yang diketahui | Belum ada permukaan yang **mengisi** batas waktu verifikasi maupun menaikkan catatan menjadi menunggu. Keduanya menunggu kebijakan yang disahkan pemilik klinis. Sampai saat itu, seluruh catatan berstatus tidak-diwajibkan, dan itu perilaku yang benar |
| Risiko tersisa | Verifikasi hanya berlaku bagi catatan yang berada di bawah perawatan rawat inap. Catatan terpadu poliklinik dan IGD ditolak `422` bila seseorang mencoba memverifikasinya. Itu disengaja: `CAP-021` aturan 5 adalah aturan rawat inap |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Bersih sebelum task; tidak ada stage, commit, maupun push |
| Langkah berikutnya | Pemilik klinis ditunjuk, lalu nilai batas waktu `RWI-RULE-021` disahkan; setelah itu permukaan pengisian batas waktu dapat dibuat |
