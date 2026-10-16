# Laporan Perubahan Frontend — `FE-RAD-14`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-14` |
| Judul | Menu v1, Daftar Pasien Radiologi, dan Riwayat Pemeriksaan |
| Slice | Paritas v1, Fase 1 — `RAD-EVD-001` bagian 6 |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 5c. Dibuka atas instruksi pemilik modul Yoga Aji Pratama, 2026-10-14 |
| Trace | `RAD-DEC-018`..`RAD-DEC-020`; `RAD-EVD-001` bagian 2.2, 2.4, 3 |
| Contract | `RAD-API-001` revision 13 (`BE-RAD-18`) |
| Task mode | `FRONTEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `478f08aa9`, branch `YogaV2` |
| Tanggal | 2026-10-14..15 |
| Status | **Selesai — batas verifikasi.** Layar terhadap API tiruan 31/31; terhadap backend sungguhan menunggu migration `BE-RAD-18` |

---

## 1. Yang tampil

Menu grup Radiologi kini diawali **Daftar Pasien Radiologi** dan **Riwayat Pemeriksaan** (padanan v1), lalu menu
khusus V2 di bawahnya — pola `FE-LAB-49`.

| Layar | Rute | Isi |
| --- | --- | --- |
| Daftar Pasien Radiologi | `/health-services/radiology-management/rad-patients` | Lima tab; RJ/RI/IGD hanya hari ini (WIB) tanpa batal; Persiapan; Perjanjian (bukan hari ini). Penyaring cari + cito. Paginasi 10 baris |
| Riwayat Pemeriksaan | `/health-services/radiology-management/rad-history` | Tab sama tanpa batas hari ini, batal ikut tampil; penyaring periode; kolom seragam + Status Pemeriksaan |
| Buat Pesanan Radiologi | `/rad-orders/create` | Centang **Pasien Butuh Persiapan** pada panel *Kesegeraan dan Persiapan* |

Kolom per tab mengikuti v1 tayang (`RAD-EVD-001` bagian 2.2): Persiapan menambah *Tipe Kunjungan*; Perjanjian
menambah *Tipe Kunjungan* dan *Tgl Pemeriksaan* tanpa *Pemesan*. Aksi baris: **Konfirmasi** (`PUT …/accept`),
**Buka Pemeriksaan** (konsol study `FE-RAD-08`, sehingga gerbang keselamatan tetap dilalui — `RAD-DEC-019`),
**Batalkan Order** (alasan wajib).

## 2. Selisih sengaja dari v1

| v1 | V2 | Alasan |
| --- | --- | --- |
| Tidak ada tab IGD | Tab **Gawat Darurat** | Pesanan IGD di v1 tidak tampil di mana pun |
| Tab Pasien Luar | Dipasang **bertanda "Fitur belum tersedia"** (sejak `FE-RAD-17`, `RAD-DEC-022`) | Di v1 isinya data contoh (`makeRows`); semula tidak diport, diubah atas keputusan pemilik modul 2026-10-15 |
| Penanda persiapan dari master pemeriksaan (`isButuhPersiapan`) | Dicentang pemesan | Master tindakan V2 tidak punya ruas itu |
| Print Hasil/Label, Rincian, Kirim Hasil | Belum ada | `FE-RAD-17`/`BE-RAD-23`; tombol yang tidak melakukan apa pun tidak dibuat |
| Pembayaran Lunas/Belum Lunas | + Asuransi / Penjamin, Ditagih Kemudian, Belum Ditagih | Label sama dengan Daftar Pasien Lab |

## 3. Berkas

Baru: `app/.../rad-patients/{page,rad-patients-client}.jsx`, `app/.../rad-history/{page,rad-history-client}.jsx`,
`components/view/.../rad-patients/rad-patient-list-view.jsx`, `lib/hooks/.../rad-patient-list-rules.js`,
`lib/hooks/.../use-rad-patient-list.jsx`, `tests/unit/rad-patient-list-rules.test.mjs`.
Diubah: `utils/menu-sidebar/menu-items.jsx`, `radiology-constants.jsx` (`RADIOLOGY_ROUTES.patients/history`),
`rad-order-constants.jsx` (salinan persiapan), `use-rad-order-form.jsx`, `rad-order-form-view.jsx`.

Seluruh penyaringan di server lewat parameter tab; browser hanya memotong halaman 10 baris.

## 4. Validasi

| Bukti | Hasil |
| --- | --- |
| Uji unit `rad-patient-list-rules` + `rad-order-form-rules` | 17/17 lulus (9 baru) |
| ESLint berkas yang disentuh | 0 error; 1 warning lama `use-rad-order-form.jsx:127` (bukan dari task ini) |
| `npm run build` | Lulus; rute `rad-patients` dan `rad-history` terbentuk |
| Layar standalone 3710, API tiruan penuh, tulis lain diblokir | **31/31 lulus** |

Yang dibuktikan di layar: lima tab dan parameter tiap tab (RJ `encounterType=1&examinationDay=today&excludeCancelled=true`,
RI `encounterType=3`, Persiapan `onlyPreparation`, Perjanjian `examinationDay=notToday`; Riwayat tanpa `examinationDay`
dan tanpa `excludeCancelled`, Perjanjian `onlyScheduled`); judul kolom tiap tab sama dengan v1; No. Foto bertumpuk,
diagnosa, pemesan, penjamin, nama konfirmator, CITO, Lunas/Belum Lunas, "Belum Terkonfirmasi"; paginasi 10 + 2;
menu aksi; Konfirmasi `409` menampilkan pesan server dan modal tetap terbuka, lalu berhasil dan daftar dimuat ulang;
menu v1 di atas menu V2; centang persiapan di form tanpa modal.

## 5. Batas verifikasi

Terhadap backend sungguhan belum dicoba: migration `AddRadOrderRequiresPreparation` belum diterapkan ke devYoga.
Sesudah diterapkan, cukup buka kedua layar dengan akun Radiografer — kodenya tidak perlu disentuh.
