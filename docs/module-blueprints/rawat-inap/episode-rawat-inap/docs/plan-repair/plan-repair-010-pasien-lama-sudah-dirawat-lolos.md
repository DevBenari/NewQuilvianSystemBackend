# PLAN-REPAIR-010 — Langkah 1 admisi menolak pasien yang sedang dirawat atau masih punya kunjungan aktif

```yaml
plan_id: PLAN-REPAIR-EPS-010
issue: ../issue/issue-010-pasien-lama-sudah-dirawat-lolos.md
status_rencana: SELESAI  # 3/3 perbaikan ✅; uji runtime bagian 8 menunggu pemilik
tanggal_rencana: "2026-10-13"
diputuskan_oleh: "Pemilik sistem (Muhammad Hamzah) — lewat laporan yang sama: 'perbaiki masalah tersebut ya, agar tidak terjadi duplikat'"
tanggal_keputusan: "2026-10-13"
basis_source_backend: "8fce8c966803d21d763faa31de4ff3c049d557cf (MHamzah)"
basis_source_frontend: "1b0ad5dd4b347fa3493554a3b6484514cf2c2a43 (HamzahV2)"
```

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-010-01` | `ISS-EPS-010-01`, `-02`, `-03`, `-T1` | Endpoint baca `GET /episodes/admission-eligibility`; `OpenAdmissionAsync` menolak pasien yang sedang hadir sebelum menulis; aturan kunjungan aktif pendaftaran dipakai bersama | BE | 1 | `BE-RWI-230` | ✅ SELESAI | [`BE-RWI-230.md`](../../task/report/backend/BE-RWI-230.md) — build 0 error/284 warning, QBE strict 6 berkas PASS; uji API runtime `NOT RUN` |
| `FIX-EPS-010-02` | `ISS-EPS-010-01`, `-02`, `-03`, `-T1`, `-T2`, `-T3` | Langkah 1 Pasien Lama dan Informasi Pasien (Kamar Pulih) memeriksa kelayakan saat pasien dipilih dan saat Lanjut ditekan; Pilih Bed memeriksa ulang sebelum membuat kunjungan | FE | 1 | `FE-RWI-244` | ✅ SELESAI | [`FE-RWI-244.md`](../../task/report/frontend/FE-RWI-244.md) — unit 8/8 PASS, ESLint 0 error, build 492 halaman PASS; uji layar `NOT FEASIBLE` sampai backend dijalankan ulang |
| `FIX-EPS-010-03` | `ISS-EPS-010-01`, `-03` | Amandemen dokumen: skema 3.5.1, kontrak API, matriks validasi `VAL-ADM-10`–`12`, keputusan `RWI-DEC-297` | Dokumen | 2 | — (perbaikan dokumen) | ✅ SELESAI | Bagian "Amandemen PLAN-REPAIR-EPS-010" pada [`05-skema-tampilan.md`](../../05-skema-tampilan.md), [`api-contract.md`](../../contracts/api-contract.md), [`validation-matrix.md`](../../contracts/validation-matrix.md) (`VAL-ADM-10`–`12`), [`00-interview-decisions.md`](../../../00-interview-decisions.md) (`RWI-DEC-297`, `RWI-AC-432`–`435`) |

**Ringkasan: 3 dari 3 perbaikan selesai pada tingkat source, build, dan dokumen. Uji runtime oleh pemilik (bagian 8) belum dijalankan — backend perlu di-build dan dijalankan ulang lebih dulu.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-010-01, ISS-EPS-010-03, ISS-EPS-010-T1 — Langkah 1 tidak membaca aturan yang pasti menolak

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Satu endpoint baca kelayakan admisi di modul rawat inap, memakai ulang `FindPresentEpisodeAsync`, `CheckForAdmissionAsync`, dan aturan kunjungan aktif pendaftaran | Satu jawaban untuk semua aturan; kalimat persis sama dengan penolakan server; aturan berikutnya cukup ditambah di satu tempat | Endpoint baru + DTO baru |
| B | Layar memanggil `GET /episodes?patientId=&status=` dan `GET /patient-encounters/admin?patientId=` lalu menyimpulkan sendiri | Tanpa perubahan backend | Definisi "hadir" (`DischargePending` tanpa `PhysicallyLeftAt`) dan "kunjungan menghalangi" (berklinik, bukan IGD, status ≤ Sedang Konsultasi, saklar konfigurasi) tidak dapat dinyatakan dengan penyaring daftar; logika server tersalin ke layar dan akan menyimpang; petugas admisi belum tentu punya hak baca kunjungan |
| C | Tetap seperti sekarang, hanya memperjelas pesan di Pilih Bed | Perubahan paling kecil | Tidak memenuhi permintaan pemilik |

**Solusi terpilih: Opsi A.**

1. Memakai ulang tiga pemeriksaan server yang sudah ada tanpa menyalin aturannya ke layar —
   prinsip "validasi layar mencerminkan aturan server".
2. Kalimat penolakan berasal dari sumber yang sama dengan penolakan tulisnya, sehingga petugas
   membaca kalimat yang sama di langkah 1 dan di Pilih Bed.
3. Radius dampak kecil: endpoint baca baru, satu penjaga tambahan pada `OpenAdmissionAsync`, dan
   pemindahan saklar/kalimat kunjungan aktif ke `OutpatientEncounterRules` tanpa mengubah perilaku
   pendaftaran.

**Kenapa bukan opsi lain.** B menyalin aturan server ke layar dan bergantung pada hak baca modul
lain. C tidak menutup permintaan pemilik.

### ISS-EPS-010-02 — Kunjungan dan Draft sisa

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | `OpenAdmissionAsync` menolak 409 `PATIENT_ALREADY_ADMITTED` sebelum menulis; layar memeriksa ulang kelayakan sebelum `POST /patient-encounters/admin` | Episode sisa mustahil lahir dari jalur mana pun; kunjungan sisa tidak lahir pada jalur layar | Satu `GET` tambahan saat Lanjut ke Pemesanan |
| B | Gulung balik kunjungan dan episode di layar bila pemesanan ditolak | Tidak mengubah backend | Pembatalan dari layar bisa gagal di tengah; meninggalkan riwayat batal yang tidak perlu |
| C | Satukan kunjungan, episode, dan pemesanan dalam satu endpoint atomik (seperti transfer) | Paling tuntas | Perubahan kontrak besar, di luar permintaan |

**Solusi terpilih: Opsi A.** Penjaga server menutup jalur API mana pun (termasuk transfer, yang
kodenya ikut diteruskan `InpAdmissionTransferService.BedManagement.cs:27-30`), dan pemeriksaan ulang
layar mencegah kunjungan rawat inap sisa. Sisa yang masih mungkin hanya bila pasien ditempatkan
petugas lain di antara pemeriksaan ulang dan `POST /episodes` (hitungan detik) — dicatat sebagai
risiko, bukan diperbaiki dengan opsi C.

**Kenapa bukan opsi lain.** B menulis lalu membatalkan — lebih banyak jejak, bukan lebih sedikit. C
terlalu besar untuk laporan ini.

### ISS-EPS-010-T2 — Draft lain tidak diberitahukan

**Hanya satu cara yang wajar:** keputusan "Draft ganda bukan penolakan" (`validation-matrix.md`
bagian 3) tetap berlaku; langkah 1 menampilkan peringatan kuning dengan tombol utama **Lanjutkan
Admisi {nomor}** (rute pelanjutan yang sama dengan Daftar Pasien Rawat Inap), dan tombol Lanjut tetap
dapat dipakai bila memang admisi terpisah. Bila pemilik ingin Draft ikut ditolak, lihat K-01.

### ISS-EPS-010-T3 — Badge statis

**Hanya satu cara yang wajar:** teks badge mengikuti hasil pemeriksaan (bagian 3).

---

## 3. Skema tampilan sebelum → sesudah

### 3.1 Langkah 1 Pasien Lama — pasien sedang dirawat

**SEBELUM** (screenshot 1):

```text
  LANGKAH 1 — PASIEN LAMA
  Cari & Verifikasi Pasien
  ┌── IDENTITAS (KIRI) ─────────────────────┐ ┌── PENCARIAN (KANAN) ─────────────┐
  │ PASIEN TERPILIH   Verifikasi Data Pasien │ │ (•) Nomor RM  ( ) NIK            │
  │ Tn. Umar          [Siap Melanjutkan]     │ │ [ 00-00-00-91            ] [Cari]│
  │ RM 00-00-00-91    Nama · RM · NIK · ...  │ │ ┌ TU  Tn. Umar  ✓ TERPILIH ┐     │
  │                   [ Ganti Pasien ]       │ │ └ No. RM 00-00-00-91       ┘     │
  └──────────────────────────────────────────┘ └──────────────────────────────────┘
                              [ Kembali ]  [ Lanjut ke Jenis Kunjungan & Kategori ]
  → klik Lanjut: langsung maju ke langkah 2 tanpa pemeriksaan apa pun
```

**SESUDAH:**

```text
  LANGKAH 1 — PASIEN LAMA
  Cari & Verifikasi Pasien
  ┌── IDENTITAS (KIRI) ─────────────────────┐ ┌── PENCARIAN (KANAN) ─────────────┐
  │ PASIEN TERPILIH   Verifikasi Data Pasien │ │ (•) Nomor RM  ( ) NIK            │
  │ Tn. Umar          [Sedang Dirawat]       │ │ [ 00-00-00-91            ] [Cari]│
  │ RM 00-00-00-91    Nama · RM · NIK · ...  │ │ ┌ TU  Tn. Umar  ✓ TERPILIH ┐     │
  │                   [ Ganti Pasien ]       │ │ └ No. RM 00-00-00-91       ┘     │
  └──────────────────────────────────────────┘ └──────────────────────────────────┘
  ┌─ ⚠ Pasien tidak dapat diadmisi ──────────────────────────────────────────────┐
  │ Tn. Umar sudah dirawat pada episode RI-2610…-46CC di Kamar 02 BED 001. Bila    │
  │ memang pindah kamar, pakai perpindahan, bukan admisi baru.                     │
  │ [ Pindah Tempat Tidur ]  [ Lihat Detail Episode ]                              │
  └────────────────────────────────────────────────────────────────────────────────┘
                              [ Kembali ]  [ Lanjut ke Jenis Kunjungan & Kategori ]
  → klik Lanjut: memeriksa ulang ke server; tetap ditolak → layar tidak maju, alert digulir ke
    pandangan dan diberi fokus
```

### 3.2 Varian alert di bawah dua panel

```text
  Kunjungan aktif (laporan 2):
  ┌─ ⚠ Pasien tidak dapat diadmisi ──────────────────────────────────────────────┐
  │ Pasien masih memiliki kunjungan aktif bernomor ENC-RSMMC-00xxx tanggal        │
  │ 30 Jul 2026. Selesaikan atau batalkan kunjungan tersebut sebelum mendaftar     │
  │ ke poliklinik atau layanan lain.                                               │
  │ Bila dokter poliklinik/IGD memutuskan rawat inap, pakai jalur Transfer IGD/Poli.│
  └────────────────────────────────────────────────────────────────────────────────┘

  Permintaan Kamar Pulih menunggu (T1):
  ┌─ ⚠ Pasien tidak dapat diadmisi dari jalur ini ───────────────────────────────┐
  │ Pasien punya permintaan admisi dari kamar pulih; buka admisi dari permintaan   │
  │ itu.                              [ Buka Jalur Kamar Pulih ]                   │
  └────────────────────────────────────────────────────────────────────────────────┘

  Admisi lain sedang disiapkan (T2) — peringatan, Lanjut tetap dapat dipakai:
  ┌─ ⚠ Ada admisi yang sedang disiapkan ─────────────────────────────────────────┐
  │ Pasien ini punya admisi lain yang sedang disiapkan, yaitu RI-2610…-9B4C sejak  │
  │ 13 Oktober 2026 16:51. Lanjutkan admisi itu, atau lanjut di sini bila memang   │
  │ admisi terpisah.                  [ Lanjutkan Admisi RI-2610…-9B4C ]           │
  └────────────────────────────────────────────────────────────────────────────────┘

  Pemeriksaan gagal:
  ┌─ ⚠ Kelayakan admisi belum dapat diperiksa ───────────────────────────────────┐
  │ <alasan dari server atau jaringan>                 [ Periksa Ulang ]           │
  └────────────────────────────────────────────────────────────────────────────────┘
```

**Wilayah**

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Badge kartu identitas | "Memeriksa…" / "Siap Melanjutkan" / "Sedang Dirawat" / "Kunjungan Aktif" / "Permintaan Kamar Pulih" / "Ada Admisi Disiapkan" / "Belum Terperiksa" | `GET /episodes/admission-eligibility` | `BasePatientVerificationCard` prop `badgeText` (sudah ada) |
| Alert kelayakan | Kalimat server apa adanya + tombol tindak lanjut | idem | `InformationAlert` (sudah ada) + `BaseButton`; dibungkus komponen kecil `InpatientAdmissionEligibilityNotice` |

**Tombol**

| Tombol | Jenis | Kapan tampil/aktif | Yang terjadi |
| --- | --- | --- | --- |
| Lanjut ke Jenis Kunjungan & Kategori | utama | Pasien terpilih (perilaku mati tanpa pasien tidak berubah); **tidak** dimatikan karena hasil pemeriksaan | Memeriksa ulang ke server; boleh → maju; ditolak/gagal → tetap di langkah 1, alert digulir dan difokus. Selama pemeriksaan berjalan tombol menampilkan loading |
| Pindah Tempat Tidur | kedua | Ditolak karena episode hadir | Membuka Detail Episode bagian Perpindahan Tempat Tidur (`buildInpatientEpisodeTransferRoute`) |
| Lihat Detail Episode | kedua | Ditolak karena episode hadir | Membuka Detail Episode (`buildInpatientEpisodeDetailRoute`) |
| Buka Jalur Kamar Pulih | kedua | Ditolak karena permintaan Kamar Pulih menunggu, pada jalur Pasien Lama | Beralih ke jalur Kamar Pulih (`flow.selectEntryMode`) |
| Lanjutkan Admisi {nomor} | kedua | Ada Draft lain | Membuka rute pelanjutan admisi (`buildInpatientAdmissionResumeRoute`, langkah tanpa pemesanan) |
| Periksa Ulang | kedua | Pemeriksaan gagal | Mengulang pemeriksaan |

**Keadaan**

| Keadaan | Tampilan |
| --- | --- |
| Belum ada pasien | Tidak berubah dari sekarang; tidak ada pemeriksaan |
| Memeriksa | Badge "Memeriksa…"; tombol Lanjut menampilkan loading bila ditekan |
| Boleh | Badge "Siap Melanjutkan"; tanpa alert |
| Ditolak | Badge sesuai sebab; alert danger dengan tombol tindak lanjut |
| Peringatan Draft | Badge "Ada Admisi Disiapkan"; alert warning; Lanjut tetap maju |
| Gagal dimuat | Badge "Belum Terperiksa"; alert danger + Periksa Ulang; Lanjut tidak maju |
| Tidak berhak `InpatientEpisode : Read` | 403 dari server tampil sebagai "gagal dimuat" dengan kalimat server |

### 3.3 Langkah Informasi Pasien Lama (jalur Kamar Pulih)

Kartu tinjauan yang sama memakai badge dan alert yang sama; pemeriksaan membawa
`admissionReferralId` permintaan yang dipilih sehingga permintaan itu sendiri tidak dihitung sebagai
penghalang. Tombol "Lanjut ke Tipe Pasien" berperilaku seperti tombol Lanjut pada 3.1.

### 3.4 Langkah Pilih Bed

Tidak ada perubahan tampilan. "Lanjut ke Pemesanan" memeriksa ulang kelayakan sebelum
`POST /patient-encounters/admin`; bila ditolak, kalimat server tampil di alert yang sudah ada
(`FE-RWI-243`) dan tidak ada permintaan tulis apa pun.

---

## 4. Rincian perbaikan

### FIX-EPS-010-01 — Endpoint kelayakan admisi dan penjaga `OpenAdmissionAsync`

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-010-01`, `-02`, `-03`, `-T1` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend; tanpa migration |
| **Berkas yang diubah** | `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterRules.cs` (saklar, pencarian, kalimat kunjungan aktif menjadi publik); `RegistrationManagement/Controllers/PatientEncounterController.cs` (memakai ketiganya — perilaku tidak berubah); `InPatientManagement/DTOs/InpatientAdmissionEligibilityDtos.cs` (baru); `InPatientManagement/Services/InpEpisodeService.AdmissionEligibility.cs` (baru); `InpEpisodeService.cs` (penjaga `OpenAdmissionAsync`, dependency `IConfiguration`); `Controllers/InpatientEpisodeController.cs` (action baru) |
| **Radius dampak** | Pendaftaran rawat jalan/IGD/penunjang — memakai kalimat dan saklar yang sama, tanpa perubahan perilaku. Admisi transfer IGD/Poli — ikut ditolak 409 `PATIENT_ALREADY_ADMITTED` lebih awal (sebelumnya ditolak di pemesanan dalam transaksi yang sama). Endpoint lain tidak berubah |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. `OutpatientEncounterRules`: tambah `BlockActiveEncounterConfigKey`,
   `IsActiveEncounterBlockEnabled(IConfiguration?)`, `FindBlockingEncounterAsync(...)`, dan
   `BuildActiveEncounterMessage(...)`; method private controller pendaftaran memanggil ketiganya.
2. DTO `InpatientAdmissionEligibilityQuery` (`PatientId` wajib, `AdmissionReferralId?`) dan
   `InpatientAdmissionEligibilityResponse` (`PatientId`, `CanContinue`, `BlockingCode?`, `Message?`,
   `PresentEpisode?`, `ActiveEncounter?`, `PendingAdmissionReferral`, `DraftEpisode?`, `Warnings`).
3. `InpEpisodeService.GetAdmissionEligibilityAsync`: pasien aktif → episode hadir
   (`FindPresentEpisodeAsync`) → permintaan Kamar Pulih (`CheckForAdmissionAsync`) → kunjungan aktif
   (bila saklar hidup) → Draft lain (Draft lewat batas digugurkan lebih dulu lewat
   `ExpireDraftIfDueAsync`). Penghalang pertama menjadi `BlockingCode` dan `Message`.
4. `GET /api/v1/health-services/inpatient-management/episodes/admission-eligibility` —
   `InpatientEpisode : Read`, `ApiResponse<InpatientAdmissionEligibilityResponse>`; 400 bila
   `patientId` kosong, 404 bila pasien tidak ada/nonaktif.
5. `OpenAdmissionAsync`: sesudah pasien terbaca dan sebelum pemeriksaan lain, panggil
   `FindPresentEpisodeAsync`; ada → 409 dengan `RejectionMessage` dan kode `PATIENT_ALREADY_ADMITTED`.

**Acceptance criteria.**

1. Pasien `Admitted` di "Kamar 02 BED 001" → `GET …/admission-eligibility?patientId=` 200 dengan
   `canContinue=false`, `blockingCode="PATIENT_ALREADY_ADMITTED"`, `message` = "Tn. Umar sudah dirawat
   pada episode RI-… di Kamar 02 BED 001. Bila memang pindah kamar, pakai perpindahan, bukan admisi
   baru.", `presentEpisode.episodeNumber` dan `locationText` terisi. Pasien `DischargePending` yang
   `PhysicallyLeftAt`-nya sudah terisi → tidak menghalangi (`RWI-DEC-054`).
2. Saklar `BlockActiveEncounter=true` dan pasien punya kunjungan rawat jalan berklinik status
   Draft–Sedang Konsultasi yang belum batal/selesai → `canContinue=false`,
   `blockingCode="ACTIVE_ENCOUNTER_EXISTS"`, `message` sama persis dengan penolakan
   `POST /patient-encounters/admin`; saklar `false` → tidak menghalangi. Kunjungan IGD, rawat inap, dan
   penunjang tanpa klinik tidak menghalangi.
3. Pasien punya permintaan Kamar Pulih `Pending` dan `admissionReferralId` kosong →
   `blockingCode="INP-ADM-REF-001"`; dengan `admissionReferralId` permintaan itu → tidak menghalangi;
   permintaan sudah selesai → `INP-ADM-REF-002`.
4. Pasien punya Draft lain yang belum lewat batas → `canContinue=true`, `draftEpisode` terisi, satu
   kalimat peringatan pada `warnings`; Draft yang lewat batas digugurkan lebih dulu dan tidak
   dilaporkan.
5. Pasien tanpa semua keadaan di atas → `canContinue=true`, `blockingCode=null`, `warnings` kosong.
6. `patientId` kosong → 400 "Pasien belum dipilih."; pasien tidak ada atau nonaktif → 404.
7. `POST /episodes` untuk pasien `Admitted` → 409, `errors.code="PATIENT_ALREADY_ADMITTED"`, tanpa
   kunjungan, episode, atau riwayat status yang tertulis.
8. `POST /patient-encounters/admin` dan validasi pendaftaran lain menghasilkan kalimat dan perilaku
   yang sama seperti sebelum perubahan.

**Verifikasi.** Build backend (perintah ringan pada memori kerja); QBE strict untuk berkas baru; uji
API terarah oleh pemilik (build dijalankan pemilik — keputusan 10-09-2026).

**Risiko.** `IConfiguration` baru pada konstruktor `InpEpisodeService` — DI menyediakannya; tidak ada
konstruksi manual di repository. Pembacaan kelayakan dapat menggugurkan Draft lewat batas (menulis
saat `GET`) — pola yang sama dengan daftar episode (`RWI-RULE-022`).

### FIX-EPS-010-02 — Langkah 1, Informasi Pasien, dan Pilih Bed memeriksa kelayakan

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-010-01`, `-02`, `-03`, `-T1`, `-T2`, `-T3` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/lib/services/health-services/inpatient-management/inpatient-episode.service.js` (fungsi baca); `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-eligibility.jsx` (baru); `src/utils/health-services/inpatient-management/inpatient-admission-eligibility-utils.js` (baru: normalisasi, badge, aksi); `src/components/view/health-services/inpatient-management/inpatient-admission-eligibility-notice.jsx` (baru); `inpatient-admission-existing-patient-step.jsx`; `inpatient-admission-view.jsx`; `use-inpatient-admission-doctor.jsx` |
| **Radius dampak** | Hanya alur Admisi Rawat Inap (jalur Pasien Lama dan Kamar Pulih; Pilih Bed pada semua jalur non-transfer). Jalur Transfer IGD/Poli tidak memakai pemeriksaan ini |
| **Bergantung pada** | `FIX-EPS-010-01` (endpoint) |

**Langkah.**

1. Fungsi `getInpatientAdmissionEligibility({ patientId, admissionReferralId, signal })`.
2. Hook kelayakan: memuat ulang saat pasien berganti (permintaan lama dibatalkan), `verify()` untuk
   pemeriksaan segar saat Lanjut ditekan, `retry()`.
3. Komponen alert kelayakan sesuai bagian 3.2.
4. Langkah 1 Pasien Lama: badge, alert, dan tombol Lanjut yang memeriksa ulang sebelum maju.
5. Langkah Informasi Pasien Lama (Kamar Pulih): sama, membawa `admissionReferralId`.
6. `use-inpatient-admission-doctor.jsx` `submit`: sebelum `POST /patient-encounters/admin`, panggil
   pemeriksaan; ditolak atau gagal → kembalikan kalimatnya tanpa menulis apa pun.

**Acceptance criteria.**

1. Memilih pasien `Admitted` → badge "Sedang Dirawat", alert danger berisi kalimat server, tombol
   "Pindah Tempat Tidur" (ke `…/episodes/{id}#episode-transfer-section`) dan "Lihat Detail Episode";
   menekan Lanjut tidak memindahkan langkah dan URL tetap pada langkah 1.
2. Memilih pasien dengan kunjungan aktif → badge "Kunjungan Aktif", alert berisi kalimat server
   ("Pasien masih memiliki kunjungan aktif bernomor …") dan petunjuk jalur Transfer IGD/Poli; Lanjut
   tidak maju.
3. Pasien dengan permintaan Kamar Pulih menunggu pada jalur Pasien Lama → tombol "Buka Jalur Kamar
   Pulih"; Lanjut tidak maju. Pada jalur Kamar Pulih dengan permintaan itu sendiri → boleh lanjut.
4. Pasien dengan Draft lain → alert warning dengan tombol "Lanjutkan Admisi {nomor}" menuju rute
   pelanjutan; Lanjut tetap maju.
5. Pasien bersih → badge "Siap Melanjutkan", Lanjut maju ke langkah 2 seperti sekarang.
6. Pemeriksaan gagal (jaringan/5xx/403) → alert "Kelayakan admisi belum dapat diperiksa" dengan
   kalimat server + "Periksa Ulang"; Lanjut tidak maju.
7. Mengganti pasien membatalkan pemeriksaan pasien sebelumnya; hasil pasien lama tidak pernah tampil
   pada pasien baru.
8. Di Pilih Bed, pasien yang menjadi `Admitted` setelah langkah 1 → "Lanjut ke Pemesanan" menampilkan
   kalimat penolakan dan **tidak** mengirim `POST /patient-encounters/admin` maupun `POST /episodes`.
9. Tidak ada UUID yang tampil di layar; nomor episode dan nomor kunjungan tampil apa adanya.

**Verifikasi.** `npx eslint` pada berkas yang disentuh, `npm run build`, dan test unit terarah untuk
utilitas normalisasi (`--import ./tests/helpers/register.mjs`). Uji layar oleh pemilik.

**Risiko.** Satu `GET` tambahan saat pasien dipilih, saat Lanjut, dan saat Lanjut ke Pemesanan;
ketiganya ringan (pencarian berindeks per pasien). Penolakan yang sebelumnya terlambat kini terlihat
lebih awal — tidak ada pasien yang sebelumnya **berhasil** diadmisi yang kini ditolak.

### FIX-EPS-010-03 — Amandemen dokumen hulu

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-010-01`, `-03` (sisi dokumen `DESIGN_CHANGE`) |
| **Area** | Dokumen blueprint |
| **Berkas yang diubah** | `05-skema-tampilan.md`, `contracts/api-contract.md`, `contracts/validation-matrix.md`, `../00-interview-decisions.md`, roadmap admisi-pendaftaran (BE dan FE) |
| **Bergantung pada** | Tidak ada |

**Acceptance criteria.** Setiap baris pada bagian 7 tertulis sebagai amandemen bertanggal; tabel
lama tidak dihapus.

---

## 5. Urutan pengerjaan

```text
✅ FIX-EPS-010-03   tanpa dependency — dokumen
✅ FIX-EPS-010-01   tanpa dependency — endpoint dan penjaga server
└── ✅ FIX-EPS-010-02   memanggil endpoint FIX-01
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- |
| — | Pasien yang sedang dirawat dan pasien berkunjungan aktif tidak boleh lanjut dari langkah 1 | **Sudah diambil** 13-10-2026 → `RWI-DEC-297` | Tidak |
| K-01 | Draft lain ikut ditolak, atau tetap peringatan? | Tetap peringatan + "Lanjutkan Admisi {nomor}" (keputusan berlaku) | Tidak — dikerjakan dengan rekomendasi |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `05-skema-tampilan.md` | 3.5.1 baris "Lanjut ke Jenis Kunjungan" | "Tepat 1 pasien berhasil diverifikasi → Maju ke Langkah 2" | "Tepat 1 pasien terpilih; menekan Lanjut memeriksa `admission-eligibility` — boleh → maju; ditolak/gagal → tetap di langkah 1 dengan alert kelayakan (bagian 3 plan repair 010)" |
| `contracts/api-contract.md` | Amandemen baru | — | `GET /episodes/admission-eligibility`; `POST /episodes` 409 `PATIENT_ALREADY_ADMITTED` |
| `contracts/validation-matrix.md` | 1.1 (baris baru) | — | `VAL-ADM-10` episode hadir, `VAL-ADM-11` kunjungan aktif, `VAL-ADM-12` Draft lain (peringatan) |
| `../00-interview-decisions.md` | Amandemen baru | — | `RWI-DEC-297`, `RWI-AC-432`–`RWI-AC-435` |
| `02-backend-architecture.md` §1.5 | Lapis 1 `INV-INP-10` | "Sebelum menempatkan pasien" | Dibaca bersama amandemen kontrak: juga saat admisi dibuka dan dibaca lewat endpoint kelayakan |

---

## 8. Verifikasi menyeluruh

1. Pasien sedang dirawat → langkah 1 menolak; tombol Pindah Tempat Tidur membuka bagian perpindahan
   di Detail Episode; Daftar Pasien Rawat Inap tidak bertambah baris.
2. Pasien berkunjungan poli aktif → langkah 1 menolak dengan kalimat yang sama seperti pendaftaran.
3. Pasien bersih → admisi berjalan sampai Cetak Persetujuan seperti sebelumnya.
4. Pasien dengan Draft "Sedang Disiapkan" → peringatan + Lanjutkan Admisi membuka admisi lama.
5. Jalur Kamar Pulih dengan permintaannya sendiri → tidak terhalang.
6. Jalur Transfer IGD/Poli untuk pasien bersih → tidak berubah.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Rencana dibuat, 3 perbaikan; disetujui pemilik lewat laporan yang sama ("perbaiki masalah tersebut ya") | `diagnose-module-issue` |
| 2026-10-13 | `FIX-EPS-010-01` ✅ sebagai `BE-RWI-230`. Delta: response memuat `blockers` (seluruh penghalang, bukan hanya yang pertama) dan `draftEpisode.hasActiveReservation` untuk memilih langkah pelanjutan; kalimat Draft ganda dipakai bersama `OpenAdmissionAsync` | `build-module-backend` |
| 2026-10-13 | `FIX-EPS-010-02` ✅ sebagai `FE-RWI-244`. Delta: pesan galat pemeriksaan tidak pernah meneruskan teks Axios Inggris (404 tanpa body = "belum tersedia di server", 403 = hak baca episode); langkah 1 *fail-closed* selama backend lama berjalan | `build-module-frontend` |
| 2026-10-13 | `FIX-EPS-010-03` ✅ — amandemen skema 3.5.1, kontrak API, matriks validasi `VAL-ADM-10`–`12`, `RWI-DEC-297` + `RWI-AC-432`–`435`. Rencana `SELESAI` | `diagnose-module-issue` |
