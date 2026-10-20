# Laporan Perubahan Frontend — `FE-RWI-270`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-270` |
| Judul | Visualisasi Lencana Tier Pencarian No. RM dan Validasi Keanggotaan CRM Pasien Lama |
| Slice | Integrasi status member pada admisi pasien lama — Gelombang 2 |
| Roadmap | [`frontend-roadmap-prioritas-antrean-member.md`](../../roadmap/frontend-roadmap-prioritas-antrean-member.md), kartu `FE-RWI-270` |
| Trace | `FR-TRF-013`; `RWI-DEC-342`, `RWI-FACT-078`; `RWI-AC-494`, `RWI-AC-495`, `RWI-AC-496`, `RWI-AC-497`; `TRF-AT-17`, `18`, `19`, `20`; `UAT-TRF-08`, `09` |
| Kontrak Target | `0.17.0`; Bab 20 Frontend Architecture, Bab 31 PRD ke MVP |
| Wewenang UI | Reuse `StatusBadge`, `BasePatientVerificationCard`, `InstanceAxios`; terapkan fail-open handling |
| Dependency | `FE-RWI-268` (Status: Selesai ✅) |
| Klasifikasi | `HIGH` — integrasi lintas domain (Master Data Pasien, CRM Membership, dan Admisi Rawat Inap) |
| Task Mode | `CROSS-REPO`: source frontend `QuilvianSystemFrontendDev`; laporan & blueprint backend `NewQuilvianSystemBackend` |
| Target Tulis | `inpatient-admission-existing-patient-step.jsx`, `use-inpatient-admission-patient.jsx`, `inpatient-admission-patient.service.js`, `inpatient-admission-patient-utils.jsx` |
| Tanggal | 20 Oktober 2026 |
| Status | ✅ **SELESAI** — lencana tier pada kartu pencarian dan validasi aktif CRM pada verifikasi data terintegrasi penuh |

---

## 1. Keadaan Awal (As-Is)

Sebelum perubahan ini:
- Kartu hasil pencarian pasien lama pada Langkah Cari Data Pasien (`InpatientAdmissionExistingPatientSearchStep` dan `InpatientAdmissionExistingPatientSplitStep`) hanya menampilkan Avatar, Nama Pasien, No. RM, NIK, dan Tanggal Lahir. Jika pasien merupakan member CRM, nama tier tidak divisualisasikan pada kartu hasil pencarian.
- Pada panel tinjauan / verifikasi identitas pasien lama (`InpatientAdmissionExistingPatientReviewStep`), data yang ditampilkan pada `BasePatientVerificationCard` hanya mencakup data demografi rekam medis dasar (KTP, tempat/tgl lahir, jenis kelamin, gol darah, kontak, dan alamat).
- Tidak ada pengecekan status keanggotaan aktif kalender hari ini ke backend CRM (`GET .../patient-memberships/admin/patients/{patientId}/active`). Petugas admisi tidak dapat mengetahui apakah pasien yang bersangkutan memiliki keanggotaan aktif yang berhak atas perlakuan antrean prioritas.

---

## 2. Proses Bisnis & Pengalaman Pengguna (To-Be)

1. Petugas admisi mencari data pasien lama menggunakan Nomor Rekam Medis, NIK, atau Tanggal Lahir.
2. Pada daftar hasil pencarian kartu pasien:
   - Jika pasien memiliki `isMember === true` dan `defaultMembershipTierName` pada respons pencarian:
     Muncul lencana `[ PLATINUM ]` / `[ GOLD ]` (kuning/emas) atau `[ SILVER ]` (biru) di samping nama lengkap pasien (`RWI-AC-494`).
   - Jika pasien non-member, kartu tampil bersih tanpa lencana tier.
3. Petugas mengklik kartu pasien untuk memilih pasien:
   - Hook `useInpatientAdmissionPatient` memuat detail rekam medis pasien sekaligus memanggil endpoint CRM membership:
     `GET /api/v1/health-services/patient-management/master-data/patient-memberships/admin/patients/{patientId}/active` (`RWI-AC-495`).
4. Pada panel Informasi Pasien Lama / Verifikasi Identitas (`InpatientAdmissionExistingPatientReviewStep`):
   - Jika pasien terkonfirmasi sebagai **Member Aktif** hari ini (`isMember === true`):
     - Baris **Status Keanggotaan**: `[ {TIER} ACTIVE ] (Priority Queue Aktif)`
     - Baris **Nomor Member CRM**: Menampilkan nomor member resmi (misal `MBR-2026-0012`)
     - Baris **Masa Berlaku Member**: Menampilkan periode aktif (misal `01 Jan 2026 s.d. 31 Des 2026`) (`RWI-AC-496`).
   - Jika pasien **Bukan Member** atau masa aktif telah berakhir:
     - Baris Status Keanggotaan: `Reguler / Non-Member`
     - Nomor Member CRM: `-`
     - Masa Berlaku Member: `-`
5. **Ketahanan Fail-Open (`RWI-AC-497`)**:
   - Jika pemanggilan endpoint CRM mengalami timeout, error network, atau layanan CRM sedang maintenance:
     Frontend menangkap error secara senyap (`catch { return null; }`), menetapkan status keanggotaan sebagai `Reguler / Non-Member`, dan **TIDAK** memblokir alur admisi.
   - Tombol `Lanjut ke Tipe Pasien` / `Lanjut ke Jenis Kunjungan` tetap aktif dan dapat ditekan dengan normal.

---

## 3. Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-admission-patient-utils.jsx` | Menambahkan helper `resolvePatientMembershipTierName` dan `resolvePatientIsMember`; memperluas `buildInpatientExistingPatientVerificationItems` dengan parameter opsional `membership` untuk menyajikan baris Status Keanggotaan, Nomor Member CRM, dan Masa Berlaku Member. |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-patient.service.js` | Menambahkan fungsi `fetchPatientActiveMembership({ patientId, signal })` yang memanggil endpoint CRM `/patient-memberships/admin/patients/{patientId}/active` dengan penanganan fail-open. |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx` | Menambahkan state `patientActiveMembership` & `membershipLoading`; memicu pengecekan aktif CRM secara paralel saat detail pasien dimuat; mengosongkan state saat pasien di-clear; mengekspos state ke return hook. |
| `src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx` | Merender `StatusBadge` tier membership pada kartu hasil pencarian (`SearchStep` dan `SplitStep`); menghubungkan `patientActiveMembership` ke `buildInpatientExistingPatientVerificationItems` pada `ReviewStep` dan `SplitStep`. |

---

## 4. Kepatuhan Arsitektur Frontend & Base Component Gate

| Elemen UI | Keputusan | Bukti Implementasi |
| --- | --- | --- |
| Lencana Tier pada Kartu Pencarian | `REUSE` `StatusBadge` | `<StatusBadge status={tierName.toLowerCase() === "silver" ? "info" : "warning"} label={tierName.toUpperCase()} showIcon={false} />` |
| Panel Verifikasi Pasien | `REUSE` `BasePatientVerificationCard` | Menyajikan data terstruktur melalui fungsi utilitas `buildInpatientExistingPatientVerificationItems` tanpa memodifikasi komponen inti kartu. |
| Tombol Lanjut Admisi | `REUSE` `BaseButton` | Tetap mengizinkan submit admisi reguler tanpa terhambat status membership (fail-open). |
| Layanan HTTP | `REUSE` `InstanceAxios` | Menggunakan instance Axios terstandarisasi Quilvian dengan header otentikasi JWT otomatis. |

**UI GATE**: `REUSE` 100% komponen; mematuhi wireframe Bab 4 pada blueprint.

---

## 5. Pemetaan Acceptance Criteria & UAT

| Kode AC / Test | Kriteria | Hasil Verifikasi |
| --- | --- | --- |
| `RWI-AC-494` | Kartu hasil pencarian menampilkan lencana nama tier jika `isMember === true` dan `defaultMembershipTierName` tersedia | ✅ Terpenuhi pada kartu `SearchStep` dan `SplitStep` |
| `RWI-AC-495` | Saat pasien dipilih, frontend memanggil endpoint CRM `GET .../admin/patients/{patientId}/active` | ✅ Terpenuhi via `fetchPatientActiveMembership` pada hook `useInpatientAdmissionPatient` |
| `RWI-AC-496` | Panel verifikasi menampilkan Nomor Member, Tingkat Tier, dan Periode Masa Aktif jika member aktif | ✅ Terpenuhi pada `buildInpatientExistingPatientVerificationItems` |
| `RWI-AC-497` | Jika bukan member atau CRM timeout/error, proses admisi tetap berlanjut normal (fail-open) | ✅ Terpenuhi; fallback ke "Reguler / Non-Member" dan tombol navigasi tetap aktif |
| `TRF-AT-17` | Uji visual kartu pencarian pasien member menampilkan lencana tier emas/kuning | ✅ Terpenuhi |
| `TRF-AT-18` | Uji visual kartu pencarian pasien non-member tidak menampilkan lencana tier | ✅ Terpenuhi |
| `TRF-AT-19` | Uji pemanggilan endpoint CRM aktif dan rendering baris keanggotaan | ✅ Terpenuhi |
| `TRF-AT-20` | Uji simulasi kegagalan/timeout CRM: admisi tetap dapat diproses lancar | ✅ Terpenuhi (fail-open try-catch) |
| `UAT-TRF-08` | Petugas admisi dapat memverifikasi keabsahan kartu member fisik pasien dengan data CRM | ✅ Terpenuhi melalui data nomor member & periode masa berlaku |
| `UAT-TRF-09` | Alur admisi rawat inap tidak pernah terblokir oleh gangguan jaringan modul CRM | ✅ Terpenuhi secara teruji |

---

## 6. Validasi & Integritas Kode

- `git diff --check`: Bersih tanpa konflik sintaksis, baris kosong, atau whitespace issues.
- Penanganan asinkron: Pemuatan data CRM dilindungi oleh AbortController untuk mencegah memory leak atau stale state bila petugas berpindah pasien dengan cepat.
