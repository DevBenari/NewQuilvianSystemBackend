# Laporan Perubahan Frontend — `FE-CRM-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-CRM-004` |
| Judul | Daftar Membership |
| Slice | `MVP-2` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/frontend-roadmap.md` § `FE-CRM-004` |
| Trace | `CRM-FE-005`; `FR-CRM-003` (`UAT-01`); `04-prd-to-mvp.md` §8 (pengganti pengingat WA); `03-frontend-architecture.md` §1, §3, §4, §5 |
| Contract version | API `v2` (`GET /admin`, `GET /admin/summary`, `membershipStatus`, `expiringWithinDays`), `approved` (Yoga Aji Pratama, 2026-10-13) |
| Wewenang UI | Pakai ulang halaman: `CRM-FE-005`. Kolom, urutan, dan warna label: `DEV_DISCRETION` (`CRM-FE-003`), mengikuti pola daftar pasien |
| Dependency | `FE-CRM-001` 🟡, `BE-CRM-023` ✅ |
| Klasifikasi | `MEDIUM`: view daftar dibangun di halaman yang ada, berkas kolom, utils daftar, peta label status bersama, hook daftar dirapikan, uji unit dan E2E |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`. Backend hanya laporan ini serta roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f1dfd3edd` (branch `YogaV2`) + perubahan `FE-CRM-001`/`002`/`003`/`006` yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai. E2E 4 skenario lulus dengan API tiruan, dan `UAT-01` lulus terhadap backend lokal 2026-10-14 atas data lama yang disemai (superadmin) |

## 1. Keadaan yang ditemukan di awal

- Halaman `patient-memberships` hanya placeholder ("Data membership pasien tidak lagi memakai tabel standalone ... gunakan tab Membership") **sejak awal riwayat Git**. Tidak ada tabel yang bisa dipulihkan.
- Hook daftar (`use-master-data-patient-membership.jsx`) sudah ada tetapi tidak dipakai view, dan masih memuat `GET /patients/options?pageSize=1000`.
- Kartu ringkasan lama berisi hitungan yang tidak lagi bermakna (Primary, Auto Created, Dari Kiosk, Dari Marketing).
- **Temuan lintas layar:** backend mengirim `membershipStatusName` berupa nama enum yang dipecah, berbahasa Inggris ("Pending Payment", "Needs Verification"). Akibatnya:
  - panel hasil `FE-CRM-003` akan berjudul "Pending Payment" terhadap backend sungguhan (E2E sebelumnya memakai teks Indonesia tiruan, sehingga tidak tertangkap);
  - detail membership dan tab Membership di detail pasien lebih dulu memakai teks itu;
  - pilihan status dari metadata juga berbahasa Inggris.

## 2. Proses bisnis dari sisi pengguna

1. Petugas CRM membuka **CRM → Daftar Membership**.
2. Kartu ringkasan menampilkan: Total, Menunggu Pembayaran, Aktif, Terjadwal, Perlu Verifikasi, Berakhir ≤ 30 Hari, Kedaluwarsa, Dibatalkan.
3. Tabel menampilkan, per membership:
   - **Nomor Member** beserta jenis pendaftaran (Baru, Perpanjangan, Upgrade, Data Lama);
   - **Pasien** dan No. RM;
   - **Tingkat** dan iuran yang dikunci;
   - **Status** berlabel Indonesia dengan warna;
   - **Periode / Berakhir** (tanggal kalender);
   - **Batas Bayar** (WIB, hanya untuk Menunggu Pembayaran).
4. Filter:
   - **Status**: Menunggu Pembayaran, Aktif, Terjadwal, Perlu Verifikasi, Kedaluwarsa, Di-upgrade, Dibatalkan.
   - **Masa berlaku**: "Berakhir ≤ 30 Hari", yaitu membership Aktif yang berakhir dalam 30 hari, sebagai pengganti pengingat WA.
   - Pencarian nomor member, pasien, No. RM, atau tingkat.
   - Urutan dan jumlah baris.
5. Klik ganda atau Enter pada baris membuka detail membership.
6. Tombol **+ Pendaftaran Membership** hanya tampil bagi pemegang `PatientMembership : Create`.
7. Daftar kosong: "Belum ada membership." Galat: pesan backend tampil, dan 403 ditangani `AccessDeniedGate`.

Alasan pembatalan dan catatan tidak tampil di daftar (FE §5).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; view, hook, dan utils daftar membership; `master-data-patients-view.jsx` dan
`patient-table-columns.jsx` (pola daftar); `data-filter`, `data-table` (baris dapat difokus dan
dibuka dengan Enter), `filter-select`, `summary-grid`, `status-badge.css`; `use-permission.jsx`;
`patients-detail-view.jsx` (tab Membership); controller backend (`GetPatientMemberships`,
`ApplyExpiringWithinDays` = Aktif dan berakhir dalam N hari WIB, kunci urutan, `BuildEnumLabel`,
metadata filter tanpa pilihan tingkat).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/.../patient-memberships/master-data-patient-memberships-view.jsx` | Placeholder diganti tabel: `SummaryCards`, `DataFilter` (status, masa berlaku, urutan, arah, jumlah baris), `DataTable` + paginasi, `AccessDeniedGate`, tombol pendaftaran dijaga `usePermission("PatientMembership", "Create")` |
| `src/components/view/.../patient-memberships/patient-membership-table-columns.jsx` | Baru. Tujuh kolom dengan `StatusBadge` |
| `src/utils/.../patient-membership/patient-membership-list-utils.jsx` | Baru. Teks periode, batas bayar (hanya status 7), id baris. Tanpa JSX agar bisa diuji unit |
| `src/lib/constants/.../patient-membership-constants.jsx` | `MEMBERSHIP_STATUS`, `MEMBERSHIP_STATUS_OPTIONS` (9 status, label Indonesia + warna; "Upgrade" → "Di-upgrade"), opsi filter status dan masa berlaku, label jenis pendaftaran, opsi urutan (kunci yang didukung backend), filter bawaan `membershipStatus`/`expiringWithinDays`, kartu ringkasan siklus hidup, teks meta daftar |
| `src/utils/.../patient-membership/patient-membership-display-utils.jsx` | `getMembershipStatusLabel` dari opsi bersama. Fungsi baru: `getMembershipStatusTone`, `getPatientMembershipStatusLabel` (angka status lebih dulu, nama backend hanya bila angka tak dikenal), `getMembershipRegistrationTypeLabel` |
| `src/utils/.../patient-membership/patient-membership-table-filter-utils.jsx` | Pilihan status selalu dari label Indonesia frontend, bukan metadata Inggris |
| `src/utils/.../patient-membership/patient-membership-detail-utils.jsx` | Status di detail memakai label Indonesia dari angka status |
| `src/utils/.../patient-membership/patient-membership-registration-utils.jsx` | Judul panel hasil `FE-CRM-003` memakai label Indonesia dari angka status |
| `src/lib/hooks/.../patient-memberships/use-master-data-patient-membership.jsx` | Tidak lagi memuat 1.000 pasien (`getPatientMembershipMasterOptions` dihapus dari daftar) |
| `src/components/view/.../patients/detail/patients-detail-view.jsx` | Tab Membership: status dari angka (peta lokal ditambah 7/8/9, "Di-upgrade"), bukan nama Inggris backend. Dua baris |
| `tests/unit/crm-membership-list.test.mjs` | Baru, 9 uji |
| `tests/e2e/crm-membership-registration-screen.spec.mjs` | +4 skenario daftar. Tiruan `POST` pendaftaran kini memakai nama Inggris seperti backend sungguhan |

Semua berkas sumber baru ditulis CRLF.

### 3.3 Kepatuhan arsitektur frontend

Komponen dasar daftar yang sama dengan Daftar Pasien. Hook daftar yang sudah ada dipakai (bukan
hook baru). Slice tidak berubah, karena filter baru cukup dikirim lewat parameter yang sudah
disaring `sanitizeQueryParams`. Tidak ada filter tingkat: roadmap tidak memintanya, metadata backend
tidak membawa pilihannya, dan endpoint pilihan tingkat butuh `MembershipTier : Read`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil data membership...", filter dan kartu nonaktif |
| Kosong | "Belum ada membership." / "Ubah filter atau daftarkan membership baru." |
| Gagal | Pesan backend di alert. 403 lewat `AccessDeniedGate` |
| Tanpa hak Create | Tombol pendaftaran disembunyikan, sementara daftar tetap terbaca |
| Privasi | Alasan batal dan catatan tidak tampil, NIK/telepon tidak tampil |
| Aksesibilitas | Baris dapat difokus dan dibuka dengan Enter (bawaan `DataTable`), label status berupa teks, bukan hanya warna |

## 5. Endpoint yang dikonsumsi

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/.../patient-memberships/admin?membershipStatus=&expiringWithinDays=&search=&sortBy=&sortDirection=&pageNumber=&pageSize=` | Daftar | `PatientMembership : Read` |
| `GET` | `/v1/.../patient-memberships/admin/summary` | Kartu ringkasan | `PatientMembership : Read` |
| `GET` | `/v1/.../patient-memberships/admin/filters/metadata` | Pilihan jumlah baris | `PatientMembership : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji unit: label 7 status dari angka walau nama backend Inggris; warna; opsi filter; parameter `membershipStatus=9` dan `expiringWithinDays=30` ke `GET /admin`; kartu ringkasan; periode/batas bayar; jenis pendaftaran; label Indonesia di detail, hasil pendaftaran, dan pilihan status; hook tanpa 1.000 pasien; tombol dijaga izin | 9 lulus | `PASS` | `crm-membership-list.test.mjs` |
| E2E: kolom berisi MBR-000003, Budiman, RM-000103, Menunggu Pembayaran, 20 Okt 2026 09.00, Aktif, periode 28 Okt 2026 – 5 Nov 2026, Perlu Verifikasi, Data Lama. Tanpa teks Inggris. Batas bayar membership Aktif tidak tampil. Daftar 1.000 pasien tidak dimuat | Lulus | `PASS` | `crm-membership-registration-screen.spec.mjs` |
| E2E (`UAT-01` dengan API tiruan): filter Perlu Verifikasi → `membershipStatus=9` → hanya MEM-LAMA-7 | Lulus | `PASS` | idem |
| E2E: Berakhir ≤ 30 Hari → `expiringWithinDays=30` → satu baris Aktif | Lulus | `PASS` | idem |
| E2E: hanya `Read` → tanpa tombol pendaftaran. `Read` + `Create` → tombol tampil | Lulus | `PASS` | idem |
| E2E seluruh spec CRM (menu, pendaftaran dengan panel berlabel Indonesia dari tiruan Inggris, iuran, daftar) | 13 lulus | `PASS` | idem |
| `npx eslint` berkas baru | Bersih tanpa peringatan. Peringatan di `patients-detail-view.jsx` dan hook editor lama sudah ada sebelumnya | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Bersih | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2.807 lulus, 29 gagal | `UNRELATED EXISTING ISSUE` (29) | Nama 29 kegagalan identik dengan baseline sebelum task ini (dibandingkan dengan `diff`) |
| `npm run build` | Berhasil ("Compiled successfully in 51s") | `PASS` | Log build |

Server standalone 3710 dinyalakan untuk E2E lalu dimatikan. `test-results/` tidak berubah.

**Tidak dijalankan pada pengerjaan awal:** `UAT-01` atas data lama sungguhan, karena tabel membership
devYoga kosong. Sudah dijalankan kemudian, lihat di bawah.

**UAT terhadap backend lokal (2026-10-14).** Atas instruksi owner, tiga membership data lama disemai
lewat SQL yang meniru migration `AddPatientMembershipStatusUniqueIndexes` (Perlu Verifikasi, jenis
Data Lama, riwayat `LEGACY_FLAGGED`). Rincian lingkungan dan data ada di
[`FE-CRM-005.md` §6.1](FE-CRM-005.md#61-uat-terhadap-backend-lokal-2026-10-14).

| UAT | Hasil di layar | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `UAT-01` filter Perlu Verifikasi | Tepat 3 baris `UAT-LAMA-001`…`003`, kolom jenis "Data Lama", status "Perlu Verifikasi", periode tanggal kalender | `PASS` | `GET /admin?membershipStatus=9` 200 + tangkapan layar |

Uji di atas memakai superadmin. Daftar juga termuat bagi petugas CRM non-administrator (`AT-24`), lihat [`FE-CRM-001.md` §6.1](FE-CRM-001.md#61-at-24--uat-14-dengan-akun-petugas-crm-sungguhan-2026-10-14).

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Daftar menampilkan nomor member, pasien, tingkat, status (label Indonesia), tanggal akhir, batas bayar | Terpenuhi | E2E + uji unit |
| Filter status termasuk Perlu Verifikasi, dan "berakhir 30 hari lagi" | Terpenuhi | E2E + uji unit |
| Filter Perlu Verifikasi menampilkan data lama hasil migration (`UAT-01`) | Terpenuhi: lulus dengan API tiruan dan terhadap backend lokal 2026-10-14 atas data lama yang disemai | E2E + UAT §6 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perbaikan susulan `FE-CRM-003` | Panel hasil pendaftaran kini berlabel Indonesia dari angka status. Sebelumnya teks Inggris backend akan tampil |
| Saran backend (opsional) | `BuildEnumLabel` menghasilkan label Inggris untuk semua konsumen (kiosk, laporan). Frontend sudah tidak bergantung padanya. Mengganti label backend ke Indonesia adalah keputusan terpisah |
| Masalah yang diketahui | Detail membership masih memakai halaman lama (bagian sumber pembuatan, poin, spend) sampai `FE-CRM-005` |
| Dependency backend | `BE-CRM-023` ✅. `FE-CRM-001` 🟡 rilis bersama `BE-CRM-015` |
| Perubahan sampingan | `NONE`. Build menulis `.next/standalone` |
| Interupsi | `NONE` |
| Status Git | ` M` view daftar, `patients-detail-view.jsx`, konstanta, hook daftar, display/detail/filter/registration utils. `??` kolom, list utils, `crm-membership-list.test.mjs`. Belum di-commit |
| Langkah berikutnya | `FE-CRM-005` (detail: riwayat status, Perpanjang/Upgrade/Batalkan/Verifikasi sesuai status dan izin, ubah catatan, batas bayar, hapus halaman ubah) |
