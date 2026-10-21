# CRM Membership — Arsitektur Frontend

| Field | Nilai |
|---|---|
| Revision | `1`, status `approved` (2026-10-13, `CRM-DEC-030`) |
| Owner | Yoga Aji Pratama (product). Wewenang UI di bawah ini |
| Frontend SHA | `f1e5e273a` |
| Masukan | `CRM-FE-001` s.d. `CRM-FE-005`, `contracts/api-contract.md` v2 |

Dokumen ini memuat kontrak fungsional, bukan tata letak. Hierarki wewenang:
keamanan/privasi/invariant → keputusan owner (`CRM-FE-*`) → konvensi project → `DEV_DISCRETION`.

## 1. Kebutuhan Layar

| Kebutuhan | Dasar | Yang sudah ada | Perubahan |
|---|---|---|---|
| Bagian menu **CRM** dengan satu menu **Daftar Membership** (diubah 2026-10-16, `CRM-DEC-033`; semula juga menu Pendaftaran Membership) | `CRM-FE-001` | Tidak ada di sidebar (`src/utils/menu-sidebar/menu-items.jsx`) | Tambah. Posisi di sidebar `DEV_DISCRETION` mengikuti konvensi menu |
| Daftar Membership | `CRM-FE-005` | `src/app/health-services/patient-management/master-data/patient-memberships/` | Dipakai ulang. Kolom status baru, filter status, tanggal akhir, dan nomor member. Memuat tombol **+ Pendaftaran Membership** bagi pemegang `PatientMembership : Create`, satu-satunya pintu ke formulir pendaftaran |
| Pendaftaran Membership | `CRM-FE-004`, `CRM-DEC-033` | Halaman `create` dengan isian bebas | Diubah: cari pasien (lookup) → pilih satu → pilih tingkat → catatan → simpan. Isian status, tanggal, nomor, poin, dan belanja dihapus |
| Detail membership | — | Halaman `[slug]` | Tambah riwayat status, tombol Perpanjang, Upgrade, Batalkan, Verifikasi (sesuai status dan izin), serta ubah catatan |
| Halaman ubah (`[slug]/update`) | `CRM-DEC-022` | Ada | Dihapus atau dialihkan ke detail, karena ubah penuh tidak lagi diizinkan |

## 2. Aksi per Peran

| Aksi | Petugas CRM | Admisi | Kasir | Kiosk |
|---|:---:|:---:|:---:|:---:|
| Lihat daftar dan detail | Ya | Tidak (cukup status di layar pendaftaran sendiri) | Tidak | Tidak |
| Daftar baru / perpanjang / upgrade | Ya | Tidak | Tidak | Tidak |
| Batalkan, verifikasi data lama, ubah catatan | Ya | Tidak | Tidak | Tidak |
| Lihat status member pasien | Ya | Ya | Ya | Lewat pencarian nomor member |

Tombol hanya tampil bila izin dan status mengizinkan. Backend tetap menjadi penjaga akhir
(403/409).

## 3. Data dan Status yang Dikonsumsi

| Data | Endpoint | Catatan |
|---|---|---|
| Daftar dan detail | `GET /admin`, `GET /admin/{id}` | **Ganti** dari rute ber-policy kiosk (`GET /`) ke rute `/admin` (`CRM-DEC-018`) |
| Riwayat | `GET /admin/{id}/status-histories` | Urut terbaru di atas |
| Cari pasien | `GET /admin/patient-lookup?keyword=` | Tampilkan semua hasil beserta status member. Tidak ada pilihan otomatis (`CRM-DEC-010`) |
| Perintah | `POST /admin`, `/renewals`, `/upgrades`, `/cancel`, `/verify`, `PATCH /notes` | Kirim `expectedRowVersion` dari data yang sedang ditampilkan |
| Pilihan tingkat | `GET /v1/administrator/master-data/membership-tiers/options` | Hanya empat tingkat berbayar. **Hapus** percobaan rute `/v1/health-services/master-data/membership-tiers` yang selalu gagal (`01-existing-capability-map.md`) |

Label status (Bahasa Indonesia): Menunggu Pembayaran, Aktif, Terjadwal, Kedaluwarsa, Di-upgrade,
Dibatalkan, Perlu Verifikasi. Warna label `DEV_DISCRETION` mengikuti komponen status yang ada.

## 4. Penanganan Keadaan

| Keadaan | Perilaku |
|---|---|
| Memuat | Indikator muat standar project |
| Kosong | Daftar: "Belum ada membership." Lookup: "Pasien tidak ditemukan. Daftarkan pasien lebih dulu di Master Pasien." |
| Gagal | Tampilkan pesan dari `ApiResponse` dan tombol coba lagi |
| 409 `RowVersion` | Pesan "Data ini baru saja diubah petugas lain", lalu muat ulang detail |
| 409 aturan bisnis | Tampilkan pesan backend apa adanya (sudah berbahasa pengguna, lihat `validation-matrix.md`) |
| Kirim ganda | Tombol simpan dikunci selama permintaan berjalan |
| Data basi | Detail dimuat ulang setelah setiap perintah berhasil |
| Batas pembayaran | Detail Menunggu Pembayaran menampilkan "Batas bayar: 20 Okt 2026 09.00" |

## 5. Privasi dan Aksesibilitas

- NIK dan No. Telp di hasil lookup ditampilkan tersamar sebagian (misalnya `3174********0001`),
  mengikuti pola tampilan pasien yang sudah ada. Bila belum ada polanya: `DEV_DISCRETION`.
- Alasan pembatalan dan catatan tidak ditampilkan di daftar, hanya di detail.
- Formulir dapat dipakai dengan papan ketik, dan label isian terbaca pembaca layar.

## 6. Matriks Wewenang UI

| ID | Area | Owner | Status | Ruang | Bukti |
|---|---|---|---|---|---|
| `CRM-FE-001` | Nama bagian dan menu | Yoga Aji Pratama | `approved` | CRM → Daftar Membership. Pendaftaran lewat tombol di daftar, tanpa menu sendiri (diubah 2026-10-16) | PDF poin 1; `CRM-DEC-033` |
| `CRM-FE-002` | Hak tampil menu | Yoga Aji Pratama | `approved` | Hanya pemegang `PatientMembership : Read` | `CRM-DEC-011` |
| `CRM-FE-003` | Route, tata letak, kolom, filter, halaman vs modal | — | `DEV_DISCRETION` | Ikuti konvensi | — |
| `CRM-FE-004` | Isian formulir | Yoga Aji Pratama | `approved` | Pasien, tingkat, catatan | `CRM-DEC-022`, `023` |
| `CRM-FE-005` | Pakai ulang halaman yang ada | Yoga Aji Pratama | `approved` | Diperluas, bukan dibuat ulang | `CRM-DEC-017` |
| `CRM-FE-006` | Bentuk dialog batal/verifikasi/upgrade | — | `DEV_DISCRETION` | Wajib memuat alasan (batal/verifikasi) dan tingkat tujuan (upgrade) | `VAL-10`, `VAL-14` |

## 7. Ketergantungan Uji

Uji E2E memakai server frontend standalone yang menunjuk backend lokal dengan migration
`ExtendPatientMembershipLifecycle` sudah diterapkan, serta akun dengan izin
`PatientMembership : *` tanpa role Administrator (`AT-24`). Aktivasi end-to-end menunggu S2.
Sampai saat itu, uji status Aktif memakai jalur verifikasi data lama (`AT-27`, `AT-28`).
