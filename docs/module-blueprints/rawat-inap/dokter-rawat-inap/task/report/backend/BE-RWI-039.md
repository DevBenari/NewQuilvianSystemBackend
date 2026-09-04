# Laporan Perubahan Backend — `BE-RWI-039`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-039` |
| Judul | Satu tempat menjawab "dokumen ini milik perawatan yang mana" |
| Slice | `DOK-MVP-1` — fondasi konteks, kolom, tabel visite, pelonggaran |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-039` |
| Trace | `INT-DOK-01`; `CON-INP-015`; `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03`, `INV-DOK-13`; `RWI-RULE-026`; `02-backend-architecture.md` §3.4 |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-037` — **selesai** pada rangkaian yang sama, lihat [laporan](BE-RWI-037.md) |
| Klasifikasi | `MEDIUM`, skor 6: repository 0, berkas diperiksa 1, berkas diubah 1 (+2 berkas uji), logika bisnis 1, kontrak API 0, database 1, keamanan/auth 2, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, `Program.cs`, project uji, dan dokumen tracked sub-modul `dokter-rawat-inap` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `c8e83854af240186b5091da412fadde3810afcb1` pada branch `MHamzah` |
| Tanggal | 3 September 2026 |
| Status | **Selesai.** Ketujuh acceptance criteria terbukti; nol tabel dan nol kolom |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli`, `ACTIVE` |
| Applicability | `NEW CODE` — service ini berkas baru, sehingga wajib mengikuti kontrak canonical dan bukan pola controller legacy di sekitarnya |
| QBE berlaku | `QBE-SVC-001`, `QBE-MOD-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-ENUM-001` |
| QBE tidak berlaku | `QBE-ENT-001`, `QBE-NAM-001`, `QBE-CFG-001`, `QBE-CODE-001` s.d. `QBE-CODE-006` — task ini tidak membuat entity persisted maupun nomor bisnis |
| Archetype | Bukan permukaan API. Service domain yang dipanggil controller; tidak ada endpoint baru |
| Database authority | `NONE`. Nol tabel, nol kolom, nol migration pada task ini |
| Frontend | Tidak diperiksa dan tidak disentuh; task ini tidak mengubah kontrak yang dibaca frontend |

---

## 1. Masalah yang diperbaiki

Sebelum ini, sistem hanya mengenal dua macam pasien ketika sebuah dokumen klinis dibuat: pasien
yang membawa nomor antrean, dan pasien IGD. Pasien yang sedang menginap tidak termasuk keduanya.

Akibatnya, pertanyaan yang paling mendasar bagi dokumentasi rawat inap tidak punya jawaban di
dalam sistem: **"catatan ini milik perawatan yang mana?"** Penelusuran `InpEpisode` maupun
`EpisodeId` pada dua controller klinis yang menerima dokumen tidak menemukan satu pun cabang rawat
inap.

Tanpa jawaban itu, empat hal tidak dapat dijaga:

| Yang tidak terjaga | Akibat nyatanya |
| --- | --- |
| Dokumen menempel pada perawatan yang benar | Catatan hari ke-3 perawatan pertama bisa tercampur dengan perawatan kedua pasien yang sama |
| Perawatan yang belum dimulai atau sudah ditutup | Dokumen baru bisa lahir pada perawatan yang sudah selesai |
| Pasien pada dokumen sama dengan pasien pada perawatan | Salah pasien tidak tertangkap |
| Dokter memang berwenang atas pasien itu | Dokter mana pun bisa menulis untuk pasien siapa pun |

Godaan jalan pintasnya nyata: membuatkan **baris antrean semu** supaya jalur lama terpakai. Jalan
itu ditolak dengan sadar, karena antrean semu akan muncul di layar antrean poliklinik dan ikut
terhitung pada laporan kunjungan — dua masalah baru untuk menghindari satu.

---

## 2. Proses bisnis

**Tujuan.** Setiap dokumen klinis rawat inap dapat membuktikan pasien, kunjungan, perawatan, dan
kewenangan dokternya — tanpa satu pun baris antrean semu dibuat.

**Pelaku.** Bukan pengguna langsung. Yang memakainya adalah jalur pembuatan catatan dokter dan
jalur pengkajian; keduanya memanggil satu tempat yang sama.

**Pemicu.** Sebuah dokumen klinis hendak dibuat, dibaca, atau dikoreksi untuk sebuah kunjungan.

**Langkah yang berurutan.**

1. Kunjungannya dicari. Bila tidak ada, permintaan ditolak `404`.
2. Perawatan rawat inap milik kunjungan itu dicari. Bila kunjungan itu tidak punya perawatan
   sama sekali, permintaan ditolak `422`.
3. Bila pemanggil ikut mengirim penanda perawatan dan penandanya **tidak cocok** dengan perawatan
   milik kunjungan itu, permintaan ditolak `400`.
4. Bila perawatannya masih berstatus konsep — pasien belum benar-benar masuk kamar — permintaan
   ditolak `422`.
5. Bila perawatannya sudah ditutup atau dibatalkan, **dokumen baru** ditolak `422`, sedangkan
   **koreksi** atas dokumen lama tetap diterima.
6. Bila pasien pada dokumen berbeda dari pasien pada perawatan, permintaan ditolak `400`.
7. Bila dokter yang hendak menulis tidak memiliki penugasan yang berlaku pada saat itu, permintaan
   ditolak `403`.
8. Bila seluruhnya lolos, konteksnya dikembalikan: identitas perawatan beserta nomornya,
   kunjungan, pasien, unit pelayanan, status perawatan, penanda apakah perawatannya masih
   berjalan, DPJP yang berwenang saat itu, dan penanda kewenangan dokter yang ditanyakan.

**Aturan yang berlaku.**

- **Perawatan yang berjalan** berarti pasien sudah masuk kamar atau sedang menunggu pulang. Masa
  menunggu pulang ikut dihitung berjalan: pasien masih di kamar sampai ia benar-benar
  meninggalkan rumah sakit, dan dokumentasi pada masa itu tetap sah.
- **Kewenangan diturunkan dari data, bukan dari nama peran.** Yang diperiksa adalah penugasan
  dokter yang periodenya memuat saat yang ditanyakan. Tidak ada pemeriksaan nama peran, nama
  jabatan, nama departemen, maupun jenis pengguna.
- **Penugasan bersifat berperiode.** Catatan yang ditulis untuk pemeriksaan kemarin dinilai dengan
  DPJP yang berwenang kemarin, bukan DPJP hari ini. Contoh berangka: DPJP A bertugas 1–3
  September dan DPJP B sejak 3 September. Catatan untuk pemeriksaan 2 September menerima DPJP A,
  bukan B.
- Satu perawatan boleh memiliki lebih dari satu dokter berwenang pada saat yang sama, misalnya
  setelah pendelegasian. Karena itu yang diperiksa adalah **keberadaan penugasan milik dokter itu**,
  bukan kesamaannya dengan satu DPJP terpilih.
- **Nol baris antrean.** Service ini hanya membaca dan tidak pernah menyentuh tabel antrean.

**Status yang dihasilkan.** Tidak ada. Service ini tidak mengubah satu baris pun.

**Hasil akhirnya.** Satu jawaban yang sama dipakai jalur catatan dokter dan jalur pengkajian,
sehingga aturannya tidak lahir dua kali dengan dua bentuk berbeda.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs`, `InpDoctorAssignment.cs`
- `Areas/HealthServices/InPatientManagement/Enums/InpEpisodeStatus.cs`
- `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Assignments.cs`
- `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs`
- `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs`
- `Program.cs` bagian pendaftaran dependency
- `contracts/integration-contract.md` §1, `02-backend-architecture.md` §3.4

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | **Baru.** Service konteks klinis beserta jenis hasil, sebab penolakan, dan pemetaannya ke kode HTTP |
| `Program.cs` | Pendaftaran service pada dependency injection |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/InpatientClinicalContextServiceTests.cs` | **Baru.** Enam belas uji, satu per cabang penolakan ditambah uji hitungan baris antrean |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Infrastructure/RawatInapTestData.cs` | **Baru.** Penyiapan kunjungan, perawatan, dokter master, dan penugasan DPJP berperiode |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Task ini tidak menambah, mengubah, maupun menghapus endpoint |
| Database | `NOT APPLICABLE`. Nol tabel, nol kolom, nol migration. Service hanya membaca, seluruhnya dengan `AsNoTracking` |
| Keamanan/Auth | Menambah **pemeriksaan kewenangan berbasis data**: penugasan dokter berperiode pada perawatan. Tidak ada nama peran, nama jabatan, nama departemen, maupun `UserType` yang dipakai sebagai penentu kewenangan. Mesin hak akses `[AccessAction]`/`[AccessPermission]` tidak disentuh; pemeriksaan ini melengkapinya, bukan menggantikannya |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Task ini tidak menyentuh satu pun endpoint. Pemasangan service ke jalur
pembuatan dokumen adalah pekerjaan `BE-RWI-044`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| Perawatan berjalan mengembalikan konteks lengkap | Pasien, kunjungan, perawatan, status, DPJP, dan penanda kewenangan terisi | `PASS` | `InpatientClinicalContextServiceTests.PerawatanBerjalan_MengembalikanKonteksLengkap` |
| Kunjungan tanpa perawatan rawat inap | Ditolak `422` | `PASS` | `…KunjunganTanpaPerawatan_Ditolak422` |
| Perawatan berstatus konsep | Ditolak `422` | `PASS` | `…PerawatanDraft_Ditolak422` |
| Perawatan ditutup dan dibatalkan | Dokumen baru ditolak `422`; koreksi diterima | `PASS` | `…PerawatanTertutup_MenolakDokumenBaru_TetapiMenerimaKoreksi`, dua varian |
| Pasien dokumen tidak cocok | Ditolak `400` | `PASS` | `…PasienTidakCocok_Ditolak400` |
| Dua dokter berbeda pada satu perawatan | Dokter berpenugasan diterima; dokter lain ditolak `403` | `PASS` | `…DokterTanpaKewenangan_Ditolak403` |
| Penugasan yang sudah berakhir | Ditolak `403` untuk saat ini, diterima untuk saat penugasannya masih berlaku | `PASS` | `…PenugasanYangSudahBerakhir_TidakLagiBerwenang` |
| Penanda perawatan tidak cocok | Ditolak `400` | `PASS` | `…PenandaPerawatanTidakCocok_Ditolak400` |
| Hitungan baris antrean sebelum dan sesudah tujuh pemanggilan pada enam keadaan berbeda | Sebelum `0`, sesudah `0` | `PASS` | `…SeluruhJalur_TidakMembuatBarisAntrean` |
| Kunjungan tidak ada | Ditolak `404`, bukan kegagalan sistem | `PASS` | `…KunjunganTidakAda_Ditolak404` |
| Lima status perawatan diuji satu per satu | Hanya `Admitted` dan `DischargePending` dianggap berjalan | `PASS` | `…PerawatanBerjalan_HanyaAdmittedDanDischargePending`, lima varian |
| `dotnet test` seluruh berkas uji SQLite | `Failed: 0, Passed: 219` | `PASS` | Keluaran perintah |

Uji manual: `NOT APPLICABLE`. Task ini tidak menghasilkan permukaan yang dapat dicoba pengguna.

**Tidak dijalankan:** perintah database apa pun. Task ini memang tidak menyentuh database.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Untuk kunjungan yang punya perawatan berjalan, service mengembalikan pasien, kunjungan, perawatan, status, dan kewenangan dokter | Terpenuhi | `…PerawatanBerjalan_MengembalikanKonteksLengkap` |
| 2. Kunjungan tanpa perawatan rawat inap ditolak `422` | Terpenuhi | `…KunjunganTanpaPerawatan_Ditolak422` |
| 3. Perawatan berstatus `Draft` ditolak `422` | Terpenuhi | `…PerawatanDraft_Ditolak422` |
| 4. Perawatan `Closed` atau `Cancelled` ditolak untuk dokumen **baru** | Terpenuhi | `…PerawatanTertutup_MenolakDokumenBaru_TetapiMenerimaKoreksi`, dijalankan untuk `Closed` dan `Cancelled` |
| 5. Pasien dokumen yang tidak cocok dengan pasien perawatan ditolak `400` | Terpenuhi | `…PasienTidakCocok_Ditolak400` |
| 6. Dokter yang tidak berwenang atas pasien itu ditolak `403` | Terpenuhi | `…DokterTanpaKewenangan_Ditolak403`, memakai dua dokter berbeda pada satu perawatan |
| 7. **Nol baris antrean dibuat** pada seluruh jalur | Terpenuhi | `…SeluruhJalur_TidakMembuatBarisAntrean`; hitungan sebelum dan sesudah sama-sama `0` |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Ketujuh acceptance criteria terbukti | Terpenuhi |
| Service terdaftar pada dependency injection | Terpenuhi — `Program.cs`, `AddScoped<InpatientClinicalContextService>()` |
| Laporan menyebut nol tabel dan nol kolom | Terpenuhi — lihat bagian 3.3 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru yang berasal dari berkas task ini |
| Masalah yang diketahui | Service sudah ada dan terdaftar, tetapi **belum dipasang** pada jalur pengkajian. Pemasangan pada kedua controller adalah pekerjaan `BE-RWI-044`. Jalur catatan dokter sudah memakainya sebatas yang dibutuhkan `BE-RWI-043` |
| Risiko tersisa | Rendah. Service bersifat baca saja dan belum menjadi gerbang bagi jalur mana pun yang sudah melayani pasien |
| Dipakai bersama | Sub-modul `keperawatan` membutuhkan aturan yang sama lewat `INT-KEP-01`. Service ini dibuat **sekali**; roadmap `keperawatan` kelak menerima baris dependency, bukan salinan task — `INT-DOK-09` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada bagian task ini |
| Status Git | Tidak ada stage, commit, maupun push. Rincian `git status --short` ada pada laporan `BE-RWI-043` |
| Langkah berikutnya | `BE-RWI-040` dan `BE-RWI-042` sudah dikerjakan pada rangkaian yang sama. `BE-RWI-044` memasang service ini sebagai gerbang pada kedua controller |
