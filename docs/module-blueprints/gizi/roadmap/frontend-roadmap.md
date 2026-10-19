# Roadmap Frontend — Modul Gizi

Register task frontend modul Gizi, disusun 16 Oktober 2026.

## Mengapa register ini baru ada sekarang

`roadmap/backend-roadmap.md` memuat 12 task, dan **seluruhnya `GIZ-BE-*`**. Akibatnya Gizi
hanya dapat dilaporkan sebagai angka backend, dan atasan yang memakai metode audit yang sama
dengan 20 modul lain membaca frontend Gizi sebagai **belum ada** — padahal layarnya sudah
terkirim sejak lama.

Ini cacat pembukuan yang sama dengan Operasi sebelum 15 Oktober 2026, ketika modul itu tercatat
90,9% dengan penanda **B** karena hanya kontrak backend yang dihitung.

Yang terkirim dan sebelumnya tidak pernah ber-ID task:

| Lapisan | Jumlah |
|---|---:|
| Halaman route (`src/app/health-services/nutrition-management`) | 9 |
| View | 7 |
| Service, hook, dan Redux slice | 11 |

Register ini **tidak mengarang pekerjaan**. Tiap task menunjuk layar yang sudah ada dan
keputusan Gizi yang sudah disahkan.

| Hal | Isi |
|---|---|
| Cakupan | V1 frontend modul Gizi |
| Uji | `tests/unit/nutrition-screen-acceptance.test.mjs` — **27 uji, 27 lulus** |
| Register gabungan | [`../../MODULE-COMPLETION-NORMALIZATION.md`](../../MODULE-COMPLETION-NORMALIZATION.md) |

## Task

| Task ID | Outcome | Diturunkan dari | Status | Bukti acceptance |
|---|---|---|---|---|
| `GIZ-FE-001` | Daftar order konsultasi gizi | `GIZ-BE-001`, `GIZ-DEC-001` | **Selesai** | 4 AC: keadaan muat/kosong/galat, saringan status dan risiko gizi, ahli gizi belum ditugaskan dinyatakan, `AccessDeniedGate` |
| `GIZ-FE-002` | Detail asuhan gizi | `GIZ-BE-002`, `GIZ-DEC-007` | **Selesai** | 4 AC: diagnosis + antropometri + dokter pemohon, tautan tidak berlaku ditolak, pembatalan menuntut alasan, kegagalan muat dijelaskan |
| `GIZ-FE-003` | Pembuatan order konsultasi | `GIZ-BE-001`, `GIZ-DEC-007` | **Selesai** | 3 AC: pasien/kunjungan/dokter dipilih, alasan rujukan tersendiri, isian belum lengkap ditahan layar |
| `GIZ-FE-004` | Kebutuhan nutrisi dan revisinya | `GIZ-BE-004`, `GIZ-BE-005`, `GIZ-DEC-012` | **Selesai** | 4 AC: revisi menuntut alasan, riwayat revisi terlihat, master parameter kosong dinyatakan, registry rumus kosong tidak disamakan dengan kegagalan (`GIZ-OQ-007` DEFERRED) |
| `GIZ-FE-005` | Daftar pasien gizi dan diet aktif | `GIZ-BE-006`, `GIZ-DEC-013` | **Selesai** | 3 AC: pencarian + diet aktif, pasien tanpa diet dibedakan dari gagal muat, penolakan perintah dijelaskan |
| `GIZ-FE-006` | Produksi dan distribusi dapur | `GIZ-BE-007`, `GIZ-DEC-013` | **Selesai** | 3 AC: batch produksi dibuat dari layar, **diet berubah setelah batch dibuat diberitahukan**, isian/perintah ditolak dijelaskan |
| `GIZ-FE-007` | Laporan gizi | `GIZ-BE-009` | **Selesai** | 4 AC: saringan rentang tanggal, paging dibaca dari response, keempat bagian laporan, gerbang hak akses |

**Rekap: 7 task, 7 selesai, 0 sedang proses, 0 blocked, 0 belum mulai — 100%.**

Dua uji lintas layar menyertainya: ketiga layar master (`GIZ-BE-008`, `GIZ-BE-003`) dijaga
gerbang hak akses, dan **nol layar Gizi memakai data contoh statis** — penjaga agar tidak ada
layar yang tampak jalan padahal tidak terhubung backend.

## Mengapa statusnya boleh Selesai

Aturan yang berlaku di register ini: **FE tidak dinaikkan menjadi Selesai hanya karena berkas
source ada.** Ketujuh task di atas berstatus Selesai karena acceptance-nya dibuktikan uji, dan
setiap string yang diuji diambil dari source lebih dulu — bukan dikarang. Uji yang menebak
kalimat akan hijau untuk alasan yang salah.

Sebelum 16 Oktober 2026 ketujuhnya **tidak** memenuhi syarat itu: nol uji menyentuh layar Gizi.
Satu-satunya berkas uji yang menyebut `nutrition` adalah
`inpatient-nursing-finishing-roadmap.test.mjs`, dan itu milik Rawat Inap, bukan Gizi.

## Yang tidak dihitung sebagai task frontend

| Hal | Alasan |
|---|---|
| Skrining gizi di layar perawat | **Bukan scope Gizi** — `GIZ-DEC-003` menetapkannya dikerjakan perawat sebelum order; berkasnya ada di bawah `inpatient-management` |
| `nutrition-order-modal.jsx` dan `supporting-nutrition-form.jsx` | Milik **Rawat Inap** (`physician-workspace`), bukan modul Gizi |
| Rumus kalkulasi kebutuhan | `GIZ-OQ-007` **DEFERRED** pada V1; layar menyediakan registry yang berdiri dan kosong, dan itu sudah diuji |

## Catatan runtime

Runtime HTTP layar Gizi belum dijalankan ulang pada 16 Oktober 2026. Bukti runtime backend Gizi
tertanggal 14 Oktober 2026 — `401`/`403`/`200` selektif pada `GIZ-BE-010` — dan layar ini membaca
endpoint yang sama. Yang dibuktikan register ini adalah **acceptance layar**, bukan penelusuran
peramban ujung-ke-ujung.
