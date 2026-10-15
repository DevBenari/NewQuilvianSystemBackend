# Laporan Perubahan Frontend — `FE-CRM-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-CRM-003` |
| Judul | Pendaftaran Membership |
| Slice | `MVP-2` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/frontend-roadmap.md` § `FE-CRM-003` |
| Trace | `FR-CRM-061`; `CRM-DEC-003`, `CRM-DEC-010`, `CRM-DEC-022`, `CRM-DEC-023`; `CRM-FE-004`; `03-frontend-architecture.md` §1, §3, §4, §5 |
| Contract version | API `v2` (`GET /admin/patient-lookup`, `POST /admin`, tier `options?registrableOnly=true`), `approved` (Yoga Aji Pratama, 2026-10-13) |
| Wewenang UI | Isian form: `CRM-FE-004` (`approved`: pasien, tingkat, catatan). Pakai ulang halaman: `CRM-FE-005`. Tata letak lookup dan panel hasil: `DEV_DISCRETION` (`CRM-FE-003`), mengikuti form grup yang ada |
| Dependency | `FE-CRM-001` 🟡 (lapisan data), `BE-CRM-011` 🟡, `BE-CRM-014` ✅, `FE-CRM-006` ✅ (iuran tingkat) |
| Klasifikasi | `MEDIUM`: satu halaman diubah, satu hook, dua komponen, satu utils, satu CSS module, satu thunk, uji unit dan E2E |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`. Backend hanya laporan ini serta roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f1dfd3edd` (branch `YogaV2`) + perubahan `FE-CRM-001`/`002`/`006` yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai. E2E 6 skenario lulus dengan API tiruan, dan `UAT-03`/`UAT-04` lulus terhadap backend lokal 2026-10-14 dengan superadmin. Pendaftaran oleh petugas CRM non-administrator (`AT-24`/`UAT-14`) juga lulus |

## 1. Keadaan yang ditemukan di awal

- Halaman `create` memakai form bebas berisi pasien (dropdown 1.000 pasien), tingkat, nomor member, status, tanggal, sumber pembuatan, poin, dan belanja. Sejak `FE-CRM-001`, isian selain pasien/tingkat/catatan diabaikan saat simpan.
- Dropdown pasien memuat `GET /patients/options?pageSize=1000`, yaitu nama 1.000 pasien sekaligus, tanpa pencarian dan tanpa status member.
- **Temuan:** `findPatientNavigationContext` jatuh ke "pasien terakhir dibuka" (`store.latest`) bila tidak ada konteks rute yang cocok. Akibatnya, pada form lama, pasien bisa terisi diam-diam walaupun form dibuka dari menu. Ini bertentangan dengan `CRM-DEC-010`.

## 2. Proses bisnis dari sisi pengguna

1. Petugas CRM membuka **CRM → Pendaftaran Membership**.
2. Di kotak **Cari Pasien**, petugas mengetik No. RM, NIK, atau No. Telp, lalu menekan Enter atau **Cari**. Ketikan kurang dari 3 karakter ditolak dengan pesan "Ketik minimal 3 karakter No. RM, NIK, atau No. Telp." tanpa memanggil server.
3. Semua hasil tampil sebagai kartu pilihan berisi:
   - nama, No. RM, tanggal lahir, NIK tersamar (`3174********0001`), dan No. Telp tersamar;
   - status member: **Member Aktif — Gold** (nomor, berlaku s.d.), **Menunggu Pembayaran** (nomor, batas bayar WIB), atau **Bukan member**.
4. Tidak ada pasien yang terpilih otomatis, termasuk bila hasilnya hanya satu. Petugas memilih satu kartu, lalu tampil "Pasien terpilih: … — No. RM …" dengan tombol **Ganti Pasien**.
5. Bila pasien tidak ditemukan: "Pasien tidak ditemukan. Daftarkan pasien lebih dulu di Master Pasien." Bila pencarian gagal, pesan backend tampil dengan tombol **Coba Lagi**.
6. Petugas memilih **Tingkat Membership**. Hanya tingkat berbayar yang aktif yang muncul, masing-masing dengan iurannya, misalnya "Gold — Rp 2.400.000 / 12 bulan". Petugas boleh mengisi **Catatan** (maksimal 250 karakter).
7. **Daftarkan Membership** mengirim tiga isian. Tombol terkunci selama permintaan berjalan, dan klik ganda tidak mengirim permintaan kedua.
8. Bila berhasil, panel hijau menampilkan "Membership MBR-000003 terdaftar — Menunggu Pembayaran", pasien, tingkat, iuran, dan **batas bayar** (WIB), dengan tombol **Lihat Detail** dan **Daftarkan Pasien Lain**. Form dikosongkan.
9. Bila backend menolak (misalnya pasien masih member Aktif), pesannya tampil apa adanya, misalnya "Pasien sudah member Gold sampai 12 Okt 2027. Gunakan perpanjangan atau upgrade.", dan tombol bisa dipakai lagi.
10. Bila form dibuka dari halaman detail pasien (≤ 30 menit), No. RM pasien itu diisikan ke kotak cari dan pencarian dijalankan. Pasien tetap harus dipilih petugas. **Kembali** mengarah ke detail pasien tersebut.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; halaman `patient-memberships/create`, `patient-memberships-form-view.jsx`,
`use-master-data-patient-membership-editor.jsx`, `patient-membership-editor-utils.jsx`,
`patient-membership-display-utils.jsx`, `patient-membership-constants.jsx`; slice membership;
`base-grouped-editor-view.jsx`, `base-grouped-editor-form.jsx` (`renderField`), `base-button.jsx`,
`base-grouped-editor.module.css`; `patient-resource-navigation-context.jsx` dan
`patients-detail-view.jsx` (konteks dari detail pasien); pola cari pasien rawat inap
(`inpatient-admission-existing-patient-step.jsx`); DTO dan controller backend
(`PatientMembershipLookupResponse`, `PagedResult`, `MembershipTierOptionResponse`, pesan 409 di
`PatientMembershipService`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/.../patient-memberships/create/page.jsx` | Judul tab "Pendaftaran Membership" |
| `src/components/view/.../patient-memberships/add/patient-memberships-form-view.jsx` | Mode create merender `PatientMembershipRegistrationView`. Mode update lama tetap (diganti di `FE-CRM-005`) |
| `src/components/view/.../patient-memberships/add/patient-membership-registration-view.jsx` | Baru. Memakai `BaseGroupedEditorView` yang ada. `formProps.renderField` mengganti isian `patientId` dengan komponen cari pasien. Panel hasil lewat `afterHero` |
| `src/components/view/.../patient-memberships/add/patient-membership-patient-lookup.jsx` | Baru. Kotak cari (Enter / tombol `type="button"`, tanpa form bersarang), hasil sebagai radio dalam `fieldset`/`legend`, status member, kosong, gagal + coba lagi, paginasi |
| `src/lib/hooks/.../patient-memberships/use-patient-membership-registration.jsx` | Baru. Form tiga isian, lookup (abort permintaan lama), pilihan pasien, kunci kirim ganda (`ref` + `actionLoading`), hasil simpan, konteks dari detail pasien |
| `src/utils/.../patient-membership/patient-membership-registration-utils.jsx` | Baru. Normalisasi hasil lookup dan status member, pilihan tingkat beserta iuran, validasi, payload, ringkasan hasil, format tanggal kalender dan WIB, pembaca konteks yang ketat |
| `src/lib/constants/.../patient-membership-constants.jsx` | Konstanta pendaftaran: batas 3 karakter, ukuran halaman lookup 10, catatan 250, field dan grup form |
| `src/lib/state/slice/.../master-data-patient-membership-slice.jsx` | Thunk `getRegistrationMembershipTierOptions` (`onlyActive`, `registrableOnly`) beserta state dan selector |
| `src/style/health-services/patient-management/patient-membership-registration.module.css` | Baru. Gaya lookup dan panel hasil memakai token `--group-*` form grup. Satu kolom pada layar ≤ 640px |
| `tests/unit/crm-membership-registration.test.mjs` | Baru, 11 uji |
| `tests/e2e/crm-membership-registration-screen.spec.mjs` | Baru, 9 skenario (6 untuk task ini, 2 menu `FE-CRM-002`, 1 iuran `FE-CRM-006`). API tiruan lewat `page.route`, nol tulis |

Semua berkas sumber baru ditulis CRLF seperti tetangganya.

**Susulan 2026-10-14 (temuan UAT `AT-24`):** isian tingkat tidak lagi memakai
`optionsSource`/`optionsKey: "membershipTierOptions"`. Kunci itu membuat `BaseGroupedEditorField`
memanggil rute tingkat lama yang 404 lewat registri pilihan bersama. Opsi kini diberikan lewat
`optionMap.membershipTierIdOptions` (`patient-membership-constants.jsx`,
`use-patient-membership-registration.jsx`). Uji unit dan E2E pendaftaran diperluas. Rincian di
[`FE-CRM-001.md` §6.1](FE-CRM-001.md#61-at-24--uat-14-dengan-akun-petugas-crm-sungguhan-2026-10-14).
Petugas Customer Service berhasil mendaftarkan `MBR-000003` lewat layar ini (`AT-24`/`UAT-14`).

### 3.3 Kepatuhan arsitektur frontend

- Halaman dan form view yang ada dipakai ulang (`CRM-FE-005`). Form tetap `BaseGroupedEditorView`, dengan tombol simpan dan kunci `actionLoading` bawaan.
- Data lewat slice Redux yang sama dan `InstanceAxios`. Tidak ada service atau state paralel.
- Hook editor lama tidak diubah. Cabang create-nya kini tidak dipakai, dan akan dirapikan bersama `FE-CRM-005`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mencari pasien..." (`aria-live`). Tombol Cari berlabel "Mencari...". Pilihan tingkat memakai indikator muat form |
| Kosong | "Pasien tidak ditemukan. Daftarkan pasien lebih dulu di Master Pasien." |
| Gagal | Pesan backend + **Coba Lagi** (lookup). Pesan backend di alert form + toast (simpan) |
| 409 aturan bisnis | Pesan backend apa adanya, tombol aktif kembali |
| Kirim ganda | Tombol simpan nonaktif selama permintaan. Penjaga `ref` menolak kiriman kedua |
| Validasi | "Pilih satu pasien dari hasil pencarian.", "Tingkat membership wajib dipilih.", "Catatan maksimal 250 karakter." |
| Tanpa hak akses | Backend menjawab 403, dan pesannya tampil sebagai galat. Menu tidak tampil tanpa izin (`FE-CRM-002`) |
| Privasi | NIK/No. Telp hanya versi tersamar dari backend. Daftar 1.000 pasien tidak lagi dimuat |
| Aksesibilitas | Label kotak cari terhubung (`htmlFor`/`aria-describedby`), hasil berupa radio dalam `fieldset`/`legend` yang bisa dipakai dengan papan ketik, pesan galat `role="alert"` |

## 5. Endpoint yang dikonsumsi

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/.../patient-memberships/admin/patient-lookup?keyword=&pageNumber=&pageSize=10` | Cari pasien beserta status member | `PatientMembership : Read` |
| `POST` | `/v1/.../patient-memberships/admin` | Daftar baru (`patientId`, `membershipTierId`, `notes`) | `PatientMembership : Create` |

#### Administrator / Master Data / Membership Tier

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/administrator/master-data/membership-tiers/options?onlyActive=true&registrableOnly=true` | Pilihan tingkat berbayar beserta iuran | `MembershipTier : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji unit: field form, batas 3 karakter, status member + samaran, WIB, pilihan tingkat, validasi/payload, ringkasan hasil, konteks, thunk tingkat, kunci + 409, pemeriksaan sumber | 11 lulus | `PASS` | `crm-membership-registration.test.mjs` |
| E2E: form hanya cari pasien, tingkat, catatan; daftar pasien 1.000 tidak dimuat | Lulus | `PASS` | `crm-membership-registration-screen.spec.mjs` |
| E2E: kata kunci < 3 karakter ditolak, nol permintaan lookup | Lulus | `PASS` | idem |
| E2E: tiga hasil tampil dengan tiga status. Nol radio terpilih, termasuk saat hasil tunggal. Simpan tanpa pasien → galat, nol `POST` | Lulus | `PASS` | idem |
| E2E: pasien tidak ditemukan → arahan ke Master Pasien | Lulus | `PASS` | idem |
| E2E (`UAT-03` dengan API tiruan): pilih pasien → Gold → catatan → simpan. Tombol nonaktif selama permintaan, klik ganda tetap satu `POST`, body tepat tiga isian, panel menampilkan MBR-000003, Menunggu Pembayaran, batas bayar 20 Okt 2026 09.00 | Lulus | `PASS` | idem |
| E2E (`UAT-04` dengan API tiruan): pasien Gold Aktif daftar Silver → 409 berisi teks persis dari `PatientMembershipService` tampil, tombol aktif kembali | Lulus | `PASS` | idem |
| `npx eslint` berkas yang berubah | 0 galat, 1 peringatan (`react-hooks/set-state-in-effect`: membaca konteks dari penyimpanan browser sesudah hidrasi) | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Bersih | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2.798 lulus, 29 gagal | `UNRELATED EXISTING ISSUE` (29) | Sama dengan baseline (modul non-CRM). Nol kegagalan CRM/membership |
| `npm run build` | Berhasil ("Compiled successfully in 52s") | `PASS` | Log build |

Server standalone 3710 dinyalakan khusus untuk E2E, lalu dimatikan. `test-results/` yang
ter-track tidak berubah, karena keluaran Playwright diarahkan ke scratchpad.

**Tidak dijalankan pada pengerjaan awal:** `UAT-03`/`UAT-04` terhadap backend sungguhan. Sudah
dijalankan kemudian, lihat di bawah.

**UAT terhadap backend lokal (2026-10-14).** Lingkungan, data yang ditulis, dan rincian lengkapnya
ada di [`FE-CRM-005.md` §6.1](FE-CRM-005.md#61-uat-terhadap-backend-lokal-2026-10-14):

| UAT | Hasil di layar | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `UAT-03` daftar baru Gold untuk IKBAL YULIYANTO (RM 00-00-00-15) | Panel "Membership MBR-000002 terdaftar — Menunggu Pembayaran", iuran Rp 2.400.000, batas bayar 21 Okt 2026 11.14 | `PASS` | `POST /admin` 200 |
| `UAT-04` daftar Silver untuk ANDRY ZAINUDIN yang masih Gold Aktif | Hasil cari menandai "Member Aktif — Gold Member"; simpan → "Pasien sudah member Gold Member sampai 30 Apr 2027. Gunakan perpanjangan atau upgrade." | `PASS` | `POST /admin` 409 |
| Notifikasi galat simpan | Sebelumnya tampil biru (info). Diperbaiki di `createToast` (`variant`) | `PASS` sesudah perbaikan | Lihat `FE-CRM-005` §6.1 |

Uji di atas memakai superadmin. Pendaftaran oleh petugas CRM non-administrator (`AT-24`) lulus kemudian, lihat [`FE-CRM-001.md` §6.1](FE-CRM-001.md#61-at-24--uat-14-dengan-akun-petugas-crm-sungguhan-2026-10-14).

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-03` lulus lewat UI | Terpenuhi: lulus dengan API tiruan dan terhadap backend lokal 2026-10-14 (superadmin) | E2E + UAT §6 |
| `UAT-04` lulus lewat UI | Terpenuhi: lulus dengan API tiruan dan terhadap backend lokal 2026-10-14 (superadmin) | E2E + UAT §6 |
| Tombol simpan terkunci selama permintaan berjalan | Terpenuhi | E2E + uji unit |
| Pesan 409 tampil apa adanya | Terpenuhi | E2E + uji unit |
| Isian status, tanggal, nomor, poin, dan belanja dihapus | Terpenuhi | E2E + uji unit |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dependency izin | Pilihan tingkat memakai `GET /membership-tiers/options` yang dijaga **`MembershipTier : Read`**. Petugas CRM butuh izin itu selain `PatientMembership : Read/Create`. Tanpanya, pilihan tingkat kosong dan alert menampilkan pesan 403. Perlu diberikan lewat Role Access, atau diputuskan owner bila endpoint pilihan tingkat sebaiknya dibuka untuk `PatientMembership : Create` |
| Perbaikan susulan | Di `FE-CRM-004` ditemukan bahwa backend mengirim `membershipStatusName` berbahasa Inggris ("Pending Payment"). Judul panel hasil kini memakai label Indonesia dari angka status, dan tiruan E2E memakai nama Inggris seperti backend |
| Masalah yang diketahui | Label "Tingkat Membership" tidak terhubung ke tombol `FilterSelect` (nama aksesibelnya placeholder). Ini perilaku komponen bersama, tidak diubah. Hook editor lama masih memuat cabang create yang tidak terpakai (dirapikan di `FE-CRM-005`) |
| Risiko | Detail membership belum menampilkan batas bayar dan riwayat sampai `FE-CRM-005`. Panel hasil di halaman ini yang menampilkan batas bayar |
| Dependency backend | `BE-CRM-011` 🟡 (`AT-12` paralel PG), `BE-CRM-015` 🟡 (rilis bersama `FE-CRM-001`) |
| Perubahan sampingan | `NONE`. Build menulis `.next/standalone` |
| Interupsi | `NONE` |
| Status Git | ` M` create page, form view, konstanta, slice. `??` dua komponen, hook, utils, CSS module, `crm-membership-registration.test.mjs`, `crm-membership-registration-screen.spec.mjs`. Belum di-commit |
| Langkah berikutnya | `FE-CRM-004` (daftar: kolom/filter status baru) dan `FE-CRM-005` (detail, aksi, riwayat, hapus halaman ubah) |
