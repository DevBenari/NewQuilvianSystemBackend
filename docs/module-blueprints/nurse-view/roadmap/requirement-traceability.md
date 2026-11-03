# Requirement Traceability — Nurse View

| Field | Nilai |
|---|---|
| Blueprint | `nurse-view` revision `1` `approved` (`NV-DEC-028`) |
| Roadmap revision | `1` — 2026-10-28 |
| Contract version | `0.1.0` `approved` |
| SHA | BE `a6f977a3`, FE `76b0c6e4b` |
| Hash masukan | Lihat `blueprint-manifest.md` `artifact_hashes` |

Kolom **Status** diisi build skill sesudah laporan task ada. Saat roadmap ini ditulis, seluruhnya
belum dikerjakan.

## 1. Functional requirement → task → bukti

| Requirement | Decision | Desain/kontrak | Task backend | Task frontend | Bukti | Status |
|---|---|---|---|---|---|---|
| `FR-NV-001` daftarkan perangkat, kode number series | `NV-DEC-016` | api §1, `VAL-NV-01`–`03` | `BE-NV-002` | `FE-NV-002` | `UAT-01`, `VAL-NV-02` | ✅ FE-NV-002 §6 no. 5 `PASS` uji ulang 2 Nov: toast hijau tampil, `POST` `201` satu kali, kode `NVW-000003` (29 Okt sempat `FAIL`); uji HTTP BE-NV-002 no. 1–3 dan layar no. 3–4 `PASS` (29 Okt). Verifikasi developer, UAT belum dijalankan ([`BE-NV-002.md`](../task/report/backend/BE-NV-002.md), [`FE-NV-002.md`](../task/report/frontend/FE-NV-002.md)) |
| `FR-NV-002` buat login sekali | `NV-DEC-022` | state §1, `VAL-NV-05` | `BE-NV-002` | `FE-NV-002` | `NV-AC-018`, `UAT-03` | ✅ BE-NV-002 §5 no. 4–6 + FE-NV-002 §6 no. 8–9 `PASS` (29 Okt); kata sandi tidak ada di halaman, storage, respons detail, dan log ([`BE-NV-002.md`](../task/report/backend/BE-NV-002.md), [`FE-NV-002.md`](../task/report/frontend/FE-NV-002.md)) |
| `FR-NV-003` atur ulang login | `NV-DEC-022` | state §1, `VAL-NV-06` | `BE-NV-002` | `FE-NV-002` | uji HTTP token lama | ✅ BE-NV-002 §5 no. 10 + FE-NV-002 §6 no. 14–16 `PASS` (29 Okt) ([`BE-NV-002.md`](../task/report/backend/BE-NV-002.md), [`FE-NV-002.md`](../task/report/frontend/FE-NV-002.md)) |
| `FR-NV-004` nonaktifkan | `NV-DEC-016` | state §1, `VAL-NV-07`, `08`, `15` | `BE-NV-002`, `BE-NV-003` | `FE-NV-002`, `FE-NV-003` | `NV-AC-016`, `UAT-02` | ✅ bagian admin: BE-NV-002 §5 no. 8–9 + FE-NV-002 §6 no. 11–13 `PASS` (29 Okt); penolakan runtime: BE-NV-003 §5 N5 `PASS` (2 Nov), TV kembali ke login 59 detik ([`BE-NV-003.md`](../task/report/backend/BE-NV-003.md), [`FE-NV-003.md`](../task/report/frontend/FE-NV-003.md)). `UAT-02` belum dijalankan |
| `FR-NV-005` online/offline | `G-07` | api §1 `IsOnline` | `BE-NV-002`, `BE-NV-003` | `FE-NV-001` | tangkapan layar | ✅ kolom Online tampil (FE-NV-001 ✅); `LastOnlineAt` terisi dan ditulis berjarak 60 detik: BE-NV-003 §5 `PASS` (2 Nov) ([`BE-NV-003.md`](../task/report/backend/BE-NV-003.md)) |
| `FR-NV-010` bed per lantai per bangsal | `NV-DEC-006`, `025` | api §3 | `BE-NV-005` | `FE-NV-005` | `NV-AC-001`, `021` | 🟡 pengelompokan per bangsal terbukti: BE-NV-005 B8 (lantai 2, 17 kartu = jumlah bed aktif di DB) + FE-NV-005 L7, L2 (66 dari 66 bed) `PASS` (3 Nov); `NV-AC-021` dan kamar tanpa `FloorCode` tidak bisa diuji karena DB dev tidak punya datanya, hanya terpetakan ke source ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-011` identitas samaran | `NV-DEC-008` | §4.7 masking | `BE-NV-005` | `FE-NV-005` | `NV-AC-002`, `UAT-04`, `UAT-05` | ✅ nol kebocoran nama lengkap, No. RM, tanggal lahir, ID, alergen, DPJP di `/board`: BE-NV-005 B1, B2 + FE-NV-005 L1, L2, L3 `PASS` (3 Nov). Tanda J pada `UAT-04` ikut `FR-NV-015`. Verifikasi developer, UAT belum dijalankan ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-012` perawat PJ | `NV-DEC-012` | api §3 | `BE-NV-005` | `FE-NV-005` | `NV-AC-008` | ✅ "Belum ada PJ" dan nama PJ: BE-NV-005 B2 + FE-NV-005 L1, L2 `PASS` (3 Nov) ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-013` TTV sah terakhir | `NV-DEC-011` | §4.6 langkah 6 | `BE-NV-005` | `FE-NV-005` | `NV-AC-007` | ✅ TTV dibatalkan kembali ke TTV sah sebelumnya: BE-NV-005 B5 + FE-NV-005 L9 `PASS` (3 Nov) ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-014` kartu kritis | `NV-DEC-013` | api §3 `IsCritical` | `BE-NV-005` | `FE-NV-005` | `NV-AC-003` | ✅ EWS 17 `Critical` di urutan pertama, merah berkedip, tanpa suara: BE-NV-005 B2 + FE-NV-005 L1, L3 `PASS` (3 Nov) ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-015` ikon keselamatan | `NV-DEC-018`–`020` | §4.6 langkah 7–8 | `BE-NV-005` | `FE-NV-005` | `NV-AC-011`–`013` | 🟡 tanda alergi tanpa nama zat terbukti (`NV-AC-012`: BE-NV-005 B1, B2 + FE-NV-005 L1); tanda risiko jatuh (`NV-AC-011`) dan isolasi (`NV-AC-013`) belum terbukti dengan data nyata: instrumen belum `Approved` dan isolasi hanya bisa disetel DPJP berizin. Tampilan J dan I baru terlihat dengan jawaban tiruan (L5) ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-016` bed tak berpenghuni | `NV-DEC-015` | api §3 `StatusLabel` | `BE-NV-005` | `FE-NV-005` | `NV-AC-010` | ✅ "Dibersihkan", "Dipesan — pasien akan datang" tanpa identitas, "Kosong": BE-NV-005 B2, B6 + FE-NV-005 L2 `PASS` (3 Nov) ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-017` header perawat jaga | `NV-DEC-012`, `021` | §4.6 langkah 9 | `BE-NV-006` | `FE-NV-006` | `NV-AC-009`, `017` | belum |
| `FR-NV-018` hanya lantai sendiri | `NV-DEC-009` | permission §3 | `BE-NV-003`, `BE-NV-005` | `FE-NV-003` | `NV-AC-015`, `UAT-06` | ✅ akun manusia ditolak dan akun TV dikunci ke papan: BE-NV-003 N8 + FE-NV-003 N2 `PASS` (2 Nov); lantai tidak bisa dipilih dari permintaan dan `/board` menolak SuperAdmin serta LOKET: BE-NV-005 B7, T1–T2 `PASS` (3 Nov). `UAT-06` belum dijalankan ([`BE-NV-003.md`](../task/report/backend/BE-NV-003.md), [`FE-NV-003.md`](../task/report/frontend/FE-NV-003.md), [`BE-NV-005.md`](../task/report/backend/BE-NV-005.md)) |
| `FR-NV-020` ganti-seluruh interval | `NV-DEC-010` | api §2, `VAL-NV-09`–`13` | `BE-NV-004` | `FE-NV-004` | `UAT-08` | ✅ ganti-seluruh, validasi, dan riwayat terbukti: FE-NV-004 §6 no. 1–12 `PASS`; jejak rujukan no. 13 `PASS` pada uji ulang (2 Nov), lewat pesan log yang tidak permanen. Verifikasi developer, UAT belum dijalankan ([`BE-NV-004.md`](../task/report/backend/BE-NV-004.md), [`FE-NV-004.md`](../task/report/frontend/FE-NV-004.md)) |
| `FR-NV-021` tanda lewat jadwal | `NV-DEC-010` | api §3 `IsOverdue` | `BE-NV-005` | `FE-NV-005` | `NV-AC-005`, `UAT-07` | ✅ interval Tinggi 240 menit, TTV 275 menit → lewat jadwal 35 menit, hilang sesudah interval dicabut: BE-NV-005 B4 `PASS` (3 Nov); tampilan "Lewat jadwal 30 menit" dengan jawaban tiruan: FE-NV-005 L5. `UAT-07` belum dijalankan ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-022` aman-gagal tanpa interval | `NV-DEC-010`, `G-01` | api §2 `IsConfigured` | `BE-NV-004`, `BE-NV-005` | `FE-NV-004`, `FE-NV-005` | `NV-AC-004` | ✅ `IsConfigured` dan status layar terbukti: FE-NV-004 §6 no. 1 dan 8 `PASS` (2 Nov); di papan tanpa interval tidak ada tanda, hanya "Diukur … lalu": BE-NV-005 B3 + FE-NV-005 L2 `PASS` (3 Nov) ([`BE-NV-004.md`](../task/report/backend/BE-NV-004.md), [`FE-NV-004.md`](../task/report/frontend/FE-NV-004.md), [`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-030` muat ulang 60 detik | `NV-DEC-023`, `026` | api §3 `/current` | `BE-NV-003` | `FE-NV-005` | `NV-AC-014`, `020`, `UAT-09` | ✅ `/current` memberi 60 detik: BE-NV-003 N1 `PASS` (2 Nov); `/board` dipanggil setiap 60 detik dan perubahan TTV tampil di TV dalam 57 detik tanpa muat ulang halaman: FE-NV-005 L9 `PASS`; pindah lantai terbukti lewat HTTP: BE-NV-005 B6 (3 Nov). Pemulangan pasien dan uji dua TV belum dicoba; `UAT-09` belum dijalankan ([`BE-NV-003.md`](../task/report/backend/BE-NV-003.md), [`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-031` pita data basi | `NV-DEC-014` | fe §5 | — | `FE-NV-005` | `NV-AC-006`, `UAT-10` | ✅ papan lama tetap tampil, pita "Data terakhir 09.29 — koneksi terputus" muncul saat data berumur lebih dari 120 detik dan hilang sesudah tersambung: FE-NV-005 L4 `PASS` (3 Nov). `UAT-10` belum dijalankan ([`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-032` push TTV | `NV-DEC-023`, `027` | api §4 | `BE-NV-007` | `FE-NV-007` | `NV-AC-019`, `022` | belum |
| Regresi login (syarat lead) | `NV-DEC-030` | §4.10, fe §7 | `BE-NV-003` | `FE-NV-003` | `NV-AC-023`–`027`, `UAT-11` | ✅ regresi 9/9 sama dengan baseline, 125 cek: BE-NV-003 §5 ✅, FE-NV-003 §6 ✅; rute lain normal (3 Nov) ([`BE-NV-003.md`](../task/report/backend/BE-NV-003.md), [`FE-NV-003.md`](../task/report/frontend/FE-NV-003.md)). `UAT-11` belum dijalankan |
| Registry `Nvw` | `NV-DEC-029` | §5 | `BE-NV-001` | — | baris registry ditulis; QBE Strict `PASS`; [`BE-NV-001.md`](../task/report/backend/BE-NV-001.md) | ✅ 2026-10-28 — migration `20261028082048_AddNurseViewManagement` diterapkan |

## 2. Acceptance criteria tanpa task — coverage gap

| Butir | Sebab | Penanganan |
|---|---|---|
| `G-06` prasyarat data go-live (`FloorCode`, versi instrumen `Approved`, `OrganizationUnitId` bangsal) | Pekerjaan data/operasional, bukan kode | Daftar periksa go-live sebelum UAT; dicatat di laporan `FE-NV-005` sebagai kondisi uji. **3 Nov:** `FloorCode` terisi; instrumen risiko jatuh belum `Approved` ([`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md) §8) |
| `NV-OQ-010` angka interval sah | Kebijakan klinis Komite Keperawatan | Langkah operasional sesudah `FE-NV-004`; tanpa angka papan aman-gagal |
| Pemberian hak akses peran Admin IT | Konfigurasi Akses Role, bukan kode | Langkah operasional sesudah `BE-NV-002`/`BE-NV-004`; tambahkan, jangan timpa policy peran |
| `NFR-002` (±2 detik untuk 40 bed) | Butuh data uji sebanyak itu | Diukur di `BE-NV-005` bila data cukup; bila tidak, dicatat sebagai belum terbukti, bukan lulus. **3 Nov:** median 121 ms untuk 66 bed, tetapi hanya 5 bed terisi; lantai dengan 40 bed terisi belum diukur ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md)) |
| `G-02` papan sebagai alat bantu | Materi pelatihan/UAT | Dicantumkan di skenario UAT oleh tim UAT |

## 3. Dokumen lintas modul

| Task | Dokumen | Kewajiban |
|---|---|---|
| `BE-NV-006` | `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001` permintaan 2 | Laporan task dirujuk dari folder TLM; status TLM diperbarui |
| `BE-NV-007` | `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001` permintaan 1 | Idem |
| `BE-NV-005` | `rawat-inap/episode-rawat-inap/docs/task-lintas-modul/TLM-NV-001` | Hanya baca; tidak ada perubahan kode Rawat Inap |
