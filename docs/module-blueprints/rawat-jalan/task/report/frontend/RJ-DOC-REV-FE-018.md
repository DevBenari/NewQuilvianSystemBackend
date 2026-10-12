# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-018` |
| Judul | Master Institusi dan Dokter Perujuk |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `19` |
| Trace | `RJ-DOC-DEC-073`, `080`, `083`; `03` *PM-FE.1*, *PM-FE.5* |
| Contract version | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) |
| Wewenang UI | Butir menu di Pelayanan Kesehatan → Master Data; bentuk mengikuti standar master data (`DEV_DISCRETION` hanya letak butir) |
| Dependency | `RJ-DOC-REV-BE-017` ✅ |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-DOC-DEC-083`) |
| Commit frontend | `de323430` (`sukmagpV2`), belum di-commit |
| Commit backend rujukan | `77caf434` + perubahan `BE-017` (belum di-commit) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Keadaan awal

Belum ada layar master Institusi atau Dokter Perujuk. Backend baru menyediakan sembilan endpoint standar untuk keduanya pada `BE-017`.

## 2. Proses dari sisi pengguna

1. Petugas master data membuka **Pelayanan Kesehatan → Master Data → Institusi Perujuk**. Layar menampilkan ringkasan *Total*, *Aktif*, *Nonaktif*, *Bermitra*, filter *Kemitraan*, dan tabel dengan badge **Mitra** / **Bukan Mitra** serta jumlah dokter aktif.
2. *+ Tambah Institusi Perujuk*: isi Kode (misalnya kode faskes), Nama, Alamat, Telepon, dan sakelar **Bermitra dengan Rumah Sakit**. Kode disimpan dalam huruf besar. Kode yang sudah dipakai ditolak dengan pesan dari backend.
3. **Dokter Perujuk**: pilih institusi (hanya institusi aktif), lalu isi nama dokter. Daftar dokter dapat disaring per institusi.
4. Detail hanya punya tombol *Kembali*, *Perbarui*, *Hapus*. Institusi yang masih punya dokter atau sudah dipakai kunjungan tidak dapat dihapus; pesan backend tampil dan pengguna diarahkan menonaktifkan saja.

## 3. Perubahan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/master-data/referral-institutions/referral-institutions-constants.jsx` | Baru — `REFERRAL_INSTITUTIONS_CONFIG` |
| `src/lib/constants/health-services/master-data/referral-doctors/referral-doctors-constants.jsx` | Baru — `REFERRAL_DOCTORS_CONFIG`, parameter options institusi |
| `src/lib/state/slice/health-services/master-data/master-data-referral-{institutions,doctors}-slice.jsx` | Baru — sembilan thunk; `options` membaca `PagedResult.items` |
| `src/utils/health-services/master-data/referral-{institutions,doctors}/…-utils.jsx` | Baru — label kemitraan, payload `guid`, validasi GUID |
| `src/lib/hooks/health-services/master-data/referral-{institutions,doctors}/use-master-data-*.jsx` | Baru — list, detail, editor; editor dokter mengisi select institusi dari `getReferralInstitutionsOptions` |
| `src/components/view/health-services/master-data/referral-{institutions,doctors}/…` | Baru — list, detail, form |
| `src/app/health-services/master-data/referral-{institutions,doctors}/…` | Baru — route tipis (list, create, `[slug]`, `[slug]/update`) |
| `src/lib/state/store.jsx` | Diperbarui — 2 reducer |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui — 2 butir menu ber-`requiredPermission`, ikon `RiHospitalLine`/`RiStethoscopeLine` |

### 3.3 Kepatuhan arsitektur

Bentuk disalin dari master Health Services generasi job-level (`blood-bank-reasons`), lalu diisi ulang tanpa generator atau hook generik, sesuai standar master data §1. Relasi dokter → institusi memakai `optionResource` dan thunk `options` milik master yang dirujuk (standar §8). Institusi lama yang sudah nonaktif tetap muncul pada form update dengan label "(nonaktif)".

**UI GATE: 8 elemen — REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0**

| Kebutuhan UI | Base component | Status |
| --- | --- | --- |
| Hero | `Hero` | REUSE |
| Kartu ringkasan | `SummaryGrid` | REUSE |
| Filter + search | `DataFilter`, `FilterSelect`, `FilterDatePicker` | REUSE |
| Tabel + paginasi | `DataTable` | REUSE |
| Badge status & kemitraan | `StatusBadge` | REUSE |
| Detail | `BaseDetailView` | REUSE |
| Form create/update (termasuk switch & select) | `BaseEditorView` | REUSE |
| Konfirmasi hapus, toast, akses ditolak | `ConfirmModal`, toast, `AccessDeniedGate` (lewat view template) | REUSE |

## 4. State yang ditangani

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat institusi perujuk..." / "Memuat dokter perujuk..." |
| Kosong | "Belum ada institusi perujuk" + "Belum ada data yang sesuai dengan filter ini." |
| Gagal | Alert merah berisi pesan backend; toast pada aksi |
| Tanpa hak akses | `AccessDeniedGate`; butir menu tersembunyi lewat `requiredPermission` |

## 5. Endpoint yang dikonsumsi

#### Health Services / Master Data / Referral Institution

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/referral-institutions/filters/metadata`, `/summary`, `/`, `/options`, `/{id}` | Halaman list, detail, select institusi | `ReferralInstitution : Read` |
| `POST`, `PUT`, `PATCH`, `DELETE` | `/`, `/{id}`, `/{id}/status`, `/{id}` | Create, update, status, hapus | `ReferralInstitution : Create/Update/Delete` |

#### Health Services / Master Data / Referral Doctor

Sama, base `/v1/health-services/master-data/referral-doctors`, resource `ReferralDoctor`.

## 6. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` seluruh berkas baru + `store.jsx` + `menu-items.jsx` | 0 error, 0 warning | `PASS` |
| `npm run build` | exit 0; 10 route baru terbentuk | `PASS` |
| Uji layar Playwright — build produksi `localhost:3100`, API 7184 dialihkan ke backend uji 7185 (CORS 3100 ditambahkan lewat env saat menjalankan backend uji) | **12/12 PASS** | `PASS` |

| ID | Skenario | Hasil |
| --- | --- | --- |
| U1–U2 | Daftar: 4 kartu ringkasan, badge Mitra/Bukan Mitra; menu memuat kedua butir | PASS |
| U3 | Filter *Bermitra* → request `isPartner=true`, hanya baris Mitra | PASS |
| U4–U5 | Tambah institusi mitra → detail; kode dinormalkan huruf besar; tombol detail hanya Kembali/Perbarui/Hapus | PASS |
| U6–U7 | Kode ganda → pesan backend, tetap di form; nama kosong → validasi tanpa request | PASS |
| U8–U9 | Select institusi berisi institusi aktif; tambah dokter → detail memuat institusi + kemitraan | PASS |
| U10 | Filter institusi di daftar dokter → `referralInstitutionId`, satu baris | PASS |
| U11 | Perbarui: Bermitra dimatikan → *Tidak Bermitra* | PASS |
| U12 | Hapus institusi yang masih punya dokter → pesan backend, tetap di detail | PASS |

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru; repository tidak memakai Jest.`

Data uji UI (2 institusi `PMTEST-UI-*`, 1 dokter) dihapus lewat API sesudah uji.

## 7. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. List/detail/create/update/status/hapus untuk keduanya | Terpenuhi — status lewat sakelar *Status Aktif* di form update (pola standar; detail tanpa tombol status) | U1–U12 |
| 2. Switch *Bermitra* tersimpan dan badge Mitra tampil | Terpenuhi | U4, U11, U1 |
| 3. Dokter perujuk memilih institusi aktif | Terpenuhi | U8, U9 |
| 4. Tanpa izin → `AccessDeniedGate` | Terpenuhi lewat template view + `requiredPermission` menu; tidak diuji dengan akun tanpa izin | Source |
| 5. Lint tanpa error baru, build `PASS` | Terpenuhi | Bagian 6 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Kolom *Dibuat Oleh* menampilkan `-` karena respons backend tidak membawa nama pembuat (sama dengan template) |
| Dependency backend | Tidak ada yang terbuka |
| Perubahan sampingan | `NONE` |
| Lingkungan uji | Dev server 3000 dan backend 7184 milik pengguna sedang mati saat uji; uji memakai build produksi di 3100 |
