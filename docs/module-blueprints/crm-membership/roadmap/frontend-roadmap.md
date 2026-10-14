# Frontend Roadmap — CRM Membership

| Field | Nilai |
|---|---|
| Roadmap ID | `CRM-RM-FE-001` |
| Revision | `1` |
| Status | `approved` (Yoga Aji Pratama, 2026-10-13) |
| Blueprint | `crm-membership` r1, `approved` (`CRM-DEC-030`) |
| Cakupan | Terbatas `MVP-1` s.d. `MVP-2` (`EPIC CRM-05` s.d. `CRM-07`) |
| SHA baseline | FE `f1e5e273a`, BE `1aecf642` |
| Kontrak masukan | API `v2` `approved`. Kerja paralel dengan backend diizinkan karena kontrak sudah approved dan ber-hash |
| Masukan desain | `03-frontend-architecture.md` r1 |
| Owner | Yoga Aji Pratama |

> **Batas dokumen.**
> 1. Bukan izin menulis kode. Setiap task dikerjakan lewat `build-module-frontend` dengan approval task eksplisit.
> 2. Tata letak, route, posisi menu, warna label, dan bentuk dialog adalah `DEV_DISCRETION` (`CRM-FE-003`, `CRM-FE-006`), mengikuti konvensi project.
> 3. Uji unit: `node --import ./tests/helpers/register.mjs --test tests/unit/`. E2E lewat server standalone yang menunjuk backend lokal.

## Grafik Dependency

```text
FE-CRM-001 ─┬─> FE-CRM-003 (butuh BE-CRM-011, BE-CRM-014)
            ├─> FE-CRM-004 (butuh BE-CRM-023)
            └─> FE-CRM-005 (butuh BE-CRM-012, BE-CRM-013)
FE-CRM-002 (mandiri)
FE-CRM-006 (butuh BE-CRM-003)
FE-CRM-007 (butuh BE-CRM-015)
FE-CRM-001 dirilis bersama BE-CRM-015
```

| Gelombang | Task |
|---|---|
| `MVP-1` | `FE-CRM-001`, `FE-CRM-002`, `FE-CRM-006` |
| `MVP-2` | `FE-CRM-003`, `FE-CRM-004`, `FE-CRM-005`, `FE-CRM-007` |

## Rincian Task

### `FE-CRM-001` — Lapisan data ke rute `/admin`

| Field | Isi |
|---|---|
| Outcome | `master-data-patient-membership-slice.jsx` memakai rute `/admin` untuk daftar, detail, ringkasan, dan pilihan. Thunk baru untuk daftar baru, perpanjangan, upgrade, batal, verifikasi, catatan, riwayat, dan lookup. Percobaan rute tingkat `/v1/health-services/master-data/membership-tiers` yang selalu gagal dihapus |
| Requirement/decision | `FR-CRM-063`; `CRM-DEC-018`; `CRM-FE-005` |
| Kontrak | API `v2` |
| Reuse | Slice yang ada, `InstanceAxios` |
| Cakupan | Slice + konstanta endpoint |
| Dependency | Kontrak API `v2`. **Rilis bersama `BE-CRM-015`** |
| Acceptance criteria | Petugas CRM tanpa role Administrator dapat memuat daftar (`AT-24`). Tidak ada lagi permintaan gagal ke rute tingkat lama |
| Verifikasi | Uji unit thunk + pemeriksaan tab Network di E2E |
| Risiko/pemilik | Rilis tidak bersamaan dengan backend membuat halaman putus. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-CRM-002` — Menu CRM

| Field | Isi |
|---|---|
| Outcome | Bagian **CRM** berisi **Pendaftaran Membership** dan **Daftar Membership**, tampil bagi pemegang `PatientMembership : Read` |
| Requirement/decision | `FR-CRM-060`; `CRM-FE-001`, `CRM-FE-002` |
| Kontrak | Izin `v1` |
| Reuse | `src/utils/menu-sidebar/menu-items.jsx` |
| Cakupan | Entri menu + gating izin. Posisi menu `DEV_DISCRETION` |
| Dependency | — |
| Acceptance criteria | Pengguna tanpa izin tidak melihat menu. Pengguna CRM melihat dua menu |
| Verifikasi | E2E dua akun |
| Risiko/pemilik | — |
| DoD | Kriteria terbukti |

### `FE-CRM-003` — Pendaftaran Membership

| Field | Isi |
|---|---|
| Outcome | Alur: cari pasien (≥ 3 karakter) → daftar semua hasil beserta status member → pilih satu → pilih tingkat (empat tingkat, tampil iuran) → catatan → simpan. Isian status, tanggal, nomor, poin, dan belanja dihapus |
| Requirement/decision | `FR-CRM-061`; `CRM-DEC-003`, `010`, `022`, `023`; `CRM-FE-004` |
| Kontrak | API `v2` (`/admin/patient-lookup`, `POST /admin`) |
| Reuse | Halaman `create` dan form view yang ada |
| Cakupan | Halaman create + komponen lookup. NIK/No. Telp tersamar |
| Dependency | `FE-CRM-001`, `BE-CRM-011`, `BE-CRM-014` |
| Acceptance criteria | `UAT-03`, `UAT-04` lulus lewat UI. Tombol simpan terkunci selama permintaan berjalan. Pesan 409 tampil apa adanya |
| Verifikasi | E2E |
| Risiko/pemilik | — |
| DoD | Kriteria terbukti |

### `FE-CRM-004` — Daftar Membership

| Field | Isi |
|---|---|
| Outcome | Daftar menampilkan nomor member, pasien, tingkat, status (label Indonesia), tanggal akhir, dan batas bayar. Filter status, termasuk Perlu Verifikasi dan "berakhir 30 hari lagi" |
| Requirement/decision | `CRM-FE-005`; `04-prd-to-mvp.md` §8 (pengganti pengingat WA) |
| Kontrak | API `v2` (`GET /admin`, `/admin/summary`) |
| Reuse | Halaman daftar yang ada |
| Cakupan | Kolom, filter, label status. Kolom dan urutan `DEV_DISCRETION` |
| Dependency | `FE-CRM-001`, `BE-CRM-023` |
| Acceptance criteria | Filter Perlu Verifikasi menampilkan data lama hasil migration (`UAT-01`) |
| Verifikasi | E2E |
| Risiko/pemilik | — |
| DoD | Kriteria terbukti |

### `FE-CRM-005` — Detail dan aksi

| Field | Isi |
|---|---|
| Outcome | Detail menampilkan riwayat status dan tombol sesuai status dan izin: Perpanjang (dalam jendela), Upgrade, Batalkan (kategori + alasan), Verifikasi (akui dengan periode / tolak dengan alasan), Ubah catatan. Halaman `[slug]/update` dihapus atau dialihkan ke detail |
| Requirement/decision | `FR-CRM-062`; `CRM-DEC-013`, `021`, `022`, `024`, `029`; `CRM-FE-006` |
| Kontrak | API `v2`, validasi `v1` |
| Reuse | Halaman detail yang ada |
| Cakupan | Detail + dialog aksi. Bentuk dialog `DEV_DISCRETION` |
| Dependency | `FE-CRM-001`, `BE-CRM-012`, `BE-CRM-013` |
| Acceptance criteria | `UAT-05`, `UAT-08`, `UAT-09`, `UAT-15` lulus lewat UI. Setelah aksi berhasil, detail dimuat ulang |
| Verifikasi | E2E |
| Risiko/pemilik | — |
| DoD | Kriteria terbukti |

### `FE-CRM-006` — Iuran di form tingkat

| Field | Isi |
|---|---|
| Outcome | Form tingkat (Administrator) memuat isian Nominal Iuran per periode |
| Requirement/decision | `CRM-DEC-004`; `CRM-OQ-001` |
| Kontrak | API `v2` bagian Membership Tier |
| Reuse | `membership-tier-grouped-form.config.jsx`, form view tingkat |
| Cakupan | Satu isian angka (pakai pola desimal yang aman, bukan `type: "number"` yang membuang desimal) |
| Dependency | `BE-CRM-003` |
| Acceptance criteria | Nilai 2.400.000 tersimpan dan tampil kembali tanpa terpotong |
| Verifikasi | E2E |
| Risiko/pemilik | — |
| DoD | Kriteria terbukti |

### `FE-CRM-007` — Formulir pasien berhenti mengirim status member

| Field | Isi |
|---|---|
| Outcome | Centang "Member" dan pilihan tingkat di formulir pasien (master pasien dan pasien baru IGD) dihapus atau dijadikan tampilan saja. Status member dibaca dari proyeksi |
| Requirement/decision | `FR-CRM-041`; `CRM-CAP-004` |
| Kontrak | API `v2` (`PatientController` mengabaikan `IsMember`) |
| Reuse | `patient-constants.jsx`, `emergency-registration.constants.js`, `new-patient-form.jsx`, `emergency-registration.utils.js:923-926` |
| Cakupan | Formulir pasien dan pasien baru IGD |
| Dependency | `BE-CRM-015` |
| Acceptance criteria | `UAT-11` lulus. Pendaftaran pasien baru IGD tetap berhasil |
| Verifikasi | E2E + uji unit util |
| Risiko/pemilik | Menyentuh layar IGD. Pemilik: Yoga, dengan pemberitahuan ke owner IGD |
| DoD | Kriteria terbukti |
