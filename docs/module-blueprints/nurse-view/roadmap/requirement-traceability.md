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
| `FR-NV-010` bed per lantai per bangsal | `NV-DEC-006`, `025` | api §3 | `BE-NV-005` | `FE-NV-005` | `NV-AC-001`, `021` | ✅ pengelompokan per bangsal terbukti: BE-NV-005 B8 (lantai 2, 17 kartu = jumlah bed aktif di DB) + FE-NV-005 L7, L2 (66 dari 66 bed) `PASS` (3 Nov); bed kamar tanpa `FloorPlanRoomKey` ikut tampil (`NV-AC-021`): AC021 `PASS` (4 Nov) dengan kamar uji yang lalu dinonaktifkan. Kamar tanpa `FloorCode` memang tidak tampil menurut desain dan hanya terpetakan ke source. UAT belum dijalankan ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-011` identitas samaran | `NV-DEC-008` | §4.7 masking | `BE-NV-005` | `FE-NV-005` | `NV-AC-002`, `UAT-04`, `UAT-05` | ✅ nol kebocoran nama lengkap, No. RM, tanggal lahir, ID, alergen, DPJP di `/board`: BE-NV-005 B1, B2 + FE-NV-005 L1, L2, L3 `PASS` (3 Nov). Tanda J pada `UAT-04` ikut `FR-NV-015`. Verifikasi developer, UAT belum dijalankan ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-012` perawat PJ | `NV-DEC-012` | api §3 | `BE-NV-005` | `FE-NV-005` | `NV-AC-008` | ✅ "Belum ada PJ" dan nama PJ: BE-NV-005 B2 + FE-NV-005 L1, L2 `PASS` (3 Nov) ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-013` TTV sah terakhir | `NV-DEC-011` | §4.6 langkah 6 | `BE-NV-005` | `FE-NV-005` | `NV-AC-007` | ✅ TTV dibatalkan kembali ke TTV sah sebelumnya: BE-NV-005 B5 + FE-NV-005 L9 `PASS` (3 Nov) ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-014` kartu kritis | `NV-DEC-013` | api §3 `IsCritical` | `BE-NV-005` | `FE-NV-005` | `NV-AC-003` | ✅ EWS 17 `Critical` di urutan pertama, merah berkedip, tanpa suara: BE-NV-005 B2 + FE-NV-005 L1, L3 `PASS` (3 Nov) ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-015` ikon keselamatan | `NV-DEC-018`–`020` | §4.6 langkah 7–8 | `BE-NV-005` | `FE-NV-005` | `NV-AC-011`–`013` | ✅ tanda alergi tanpa nama zat terbukti (`NV-AC-012`: BE-NV-005 B1, B2 + FE-NV-005 L1, 3 Nov); tanda risiko jatuh hanya pada Morse 65, tidak pada Morse 30 (`NV-AC-011`), dan ikon isolasi beserta "Bed tidak sesuai isolasi" untuk pasien butuh isolasi di bed biasa (`NV-AC-013`) terbukti dengan data nyata: AC011, AC013 `PASS` (4 Nov), sesudah Morse disahkan dan isolasi disetel DPJP atas persetujuan Hamzah. UAT belum dijalankan ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-016` bed tak berpenghuni | `NV-DEC-015` | api §3 `StatusLabel` | `BE-NV-005` | `FE-NV-005` | `NV-AC-010` | ✅ "Dibersihkan", "Dipesan — pasien akan datang" tanpa identitas, "Kosong": BE-NV-005 B2, B6 + FE-NV-005 L2 `PASS` (3 Nov) ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-017` header perawat jaga | `NV-DEC-012`, `021` | §4.6 langkah 9 | `BE-NV-006` | `FE-NV-006` | `NV-AC-009`, `017` | ✅ aman-gagal terbukti dengan data nyata (`NV-AC-017`): roster terbit tanpa shift dan bangsal tanpa `OrganizationUnitId` → "Roster shift ini belum tersedia", tanpa nama ditebak: BE-NV-006 R1, R3 + FE-NV-006 S1, S2 `PASS` (3 Nov). `NV-AC-009` terbukti dengan data nyata: roster `Published` berisi tiga perawat, satu pegawai tanpa profesi, dan satu non-perawat → `/board` dan kepala bangsal Rawat Inap hanya menampilkan ketiga perawat: AC009 `PASS` (4 Nov), sesudah perbaikan lintas modul HR `RosterPeriodService` (restu Rizki). Delta kontrak `0.1.2` (`NV-DEC-033`). UAT belum dijalankan ([`BE-NV-006.md`](../task/report/backend/BE-NV-006.md), [`FE-NV-006.md`](../task/report/frontend/FE-NV-006.md)) |
| `FR-NV-018` hanya lantai sendiri | `NV-DEC-009` | permission §3 | `BE-NV-003`, `BE-NV-005` | `FE-NV-003` | `NV-AC-015`, `UAT-06` | ✅ akun manusia ditolak dan akun TV dikunci ke papan: BE-NV-003 N8 + FE-NV-003 N2 `PASS` (2 Nov); lantai tidak bisa dipilih dari permintaan dan `/board` menolak SuperAdmin serta LOKET: BE-NV-005 B7, T1–T2 `PASS` (3 Nov). `UAT-06` belum dijalankan ([`BE-NV-003.md`](../task/report/backend/BE-NV-003.md), [`FE-NV-003.md`](../task/report/frontend/FE-NV-003.md), [`BE-NV-005.md`](../task/report/backend/BE-NV-005.md)) |
| `FR-NV-020` ganti-seluruh interval | `NV-DEC-010` | api §2, `VAL-NV-09`–`13` | `BE-NV-004` | `FE-NV-004` | `UAT-08` | ✅ ganti-seluruh, validasi, dan riwayat terbukti: FE-NV-004 §6 no. 1–12 `PASS`; jejak rujukan no. 13 `PASS` pada uji ulang (2 Nov), lewat pesan log yang tidak permanen. Verifikasi developer, UAT belum dijalankan ([`BE-NV-004.md`](../task/report/backend/BE-NV-004.md), [`FE-NV-004.md`](../task/report/frontend/FE-NV-004.md)) |
| `FR-NV-021` tanda lewat jadwal | `NV-DEC-010` | api §3 `IsOverdue` | `BE-NV-005` | `FE-NV-005` | `NV-AC-005`, `UAT-07` | ✅ interval Tinggi 240 menit, TTV 275 menit → lewat jadwal 35 menit, hilang sesudah interval dicabut: BE-NV-005 B4 `PASS` (3 Nov); tampilan "Lewat jadwal 30 menit" dengan jawaban tiruan: FE-NV-005 L5. `UAT-07` belum dijalankan ([`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-022` aman-gagal tanpa interval | `NV-DEC-010`, `G-01` | api §2 `IsConfigured` | `BE-NV-004`, `BE-NV-005` | `FE-NV-004`, `FE-NV-005` | `NV-AC-004` | ✅ `IsConfigured` dan status layar terbukti: FE-NV-004 §6 no. 1 dan 8 `PASS` (2 Nov); di papan tanpa interval tidak ada tanda, hanya "Diukur … lalu": BE-NV-005 B3 + FE-NV-005 L2 `PASS` (3 Nov) ([`BE-NV-004.md`](../task/report/backend/BE-NV-004.md), [`FE-NV-004.md`](../task/report/frontend/FE-NV-004.md), [`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-030` muat ulang 60 detik | `NV-DEC-023`, `026` | api §3 `/current` | `BE-NV-003` | `FE-NV-005` | `NV-AC-014`, `020`, `UAT-09` | ✅ `/current` memberi 60 detik: BE-NV-003 N1 `PASS` (2 Nov); `/board` dipanggil setiap 60 detik dan perubahan TTV tampil di TV dalam 57 detik tanpa muat ulang halaman: FE-NV-005 L9 `PASS`; pindah lantai terbukti lewat HTTP: BE-NV-005 B6 (3 Nov). Pemulangan pasien dan uji dua TV belum dicoba; `UAT-09` belum dijalankan ([`BE-NV-003.md`](../task/report/backend/BE-NV-003.md), [`BE-NV-005.md`](../task/report/backend/BE-NV-005.md), [`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-031` pita data basi | `NV-DEC-014` | fe §5 | — | `FE-NV-005` | `NV-AC-006`, `UAT-10` | ✅ papan lama tetap tampil, pita "Data terakhir 09.29 — koneksi terputus" muncul saat data berumur lebih dari 120 detik dan hilang sesudah tersambung: FE-NV-005 L4 `PASS` (3 Nov). `UAT-10` belum dijalankan ([`FE-NV-005.md`](../task/report/frontend/FE-NV-005.md)) |
| `FR-NV-032` push TTV | `NV-DEC-023`, `027` | api §4 | `BE-NV-007` | `FE-NV-007` | `NV-AC-019`, `022` | ✅ sinyal `NurseViewBoardChanged` tanpa identitas tiba di TV lantai pasien untuk `POST`, `PUT`, `cancel`; TTV IGD dan `verify` tanpa sinyal; akun TV ditolak `JoinInpatientBedSelection` (`NV-AC-022`): BE-NV-007 H1–H11 `PASS`. Di layar, TTV Kritis 325-B tampil "Kritis" di kelompok kritis teratas 0,19 detik sesudah simpan tanpa bunyi (`NV-AC-019`, `NV-AC-003`), batas 1×/5 detik, sambung ulang dengan join ulang, muat ulang 60 detik tetap jalan: FE-NV-007 F0–F5 `PASS` (4 Nov). UAT belum dijalankan ([`BE-NV-007.md`](../task/report/backend/BE-NV-007.md), [`FE-NV-007.md`](../task/report/frontend/FE-NV-007.md)) |
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
| `BE-NV-006` | `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001` permintaan 2 | Laporan task dirujuk dari folder TLM; status TLM diperbarui — **terpenuhi 3 Nov** (§7 baris 2026-11-03) |
| `BE-NV-007` | `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001` permintaan 1 | Idem — **terpenuhi 4 Nov** (§7 baris 2026-11-04) |
| `BE-NV-005` | `rawat-inap/episode-rawat-inap/docs/task-lintas-modul/TLM-NV-001` | Hanya baca; tidak ada perubahan kode Rawat Inap |

## Revision 2 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| Blueprint | `nurse-view` revision `2` `approved` (`NV-DEC-055`) |
| Roadmap revision | `2` — 2026-11-05 · **`approved`** — Rizki Gunawan, 2026-11-05 |
| Contract version | `0.2.0` `approved` |
| SHA | BE `0c6eab34` (`rizkiG`), FE `91a11b08f` (`RizkiV2`) |
| Hash masukan | Lihat `blueprint-manifest.md` blok Revision 2 (`input_hashes`, `artifact_hashes`) |
| Gerbang | `NV-OQ-028` ditutup 2026-11-05 (`NV-DEC-056`); `NV-OQ-022` hanya runtime warna |
| Task | Backend `BE-NV-008`–`BE-NV-016`; frontend `FE-NV-008`–`FE-NV-015` |

Kolom **Status** diisi build skill sesudah laporan task ada. Saat bagian ini ditulis, seluruhnya
belum dikerjakan. Untuk backend, kolom **Bukti** berisi bukti verifikasi menurut
`rules/backend/TEST_POLICY.md` (QBE, review diff, build oleh Rizki, uji HTTP/hub), bukan unit test.
Untuk frontend, buktinya lint dan uji layar; unit test tidak diwajibkan.

### R2.1 Functional requirement → task → bukti

| Requirement | Decision | Desain/kontrak | Task backend | Task frontend | Bukti | Status |
|---|---|---|---|---|---|---|
| Fondasi data (4 tabel, 6 enum) | `NV-DEC-039`, `048`, `049`; `AD-01`, `03` | kamus data R2.2, R2.4; backend R2.7, R2.8 | `BE-NV-008` | — | Migration `AddNurseViewPatientCall` diterapkan Rizki; index diperiksa `SELECT` read-only | ✅ 6 Nov 2026: source 4 model, 6 enum, 4 configuration, 4 `DbSet`; QBE Strict `PASS` (15 berkas, 0 violation); migration `20261106015006_AddNurseViewPatientCall` diterapkan Rizki dan diperiksa bersih; 12 index beserta filternya terverifikasi `SELECT` read-only ([`BE-NV-008.md`](../task/report/backend/BE-NV-008.md)). Riwayat: 🟡 5 Nov |
| `FR-NV-033` daftarkan tablet, satu bed satu tablet | `NV-DEC-035`; `AD-01`, `03` | api §5, `VAL-NV-17`–`19` | `BE-NV-009` | `FE-NV-008` | `UAT-12` sisi admin, `UAT-13`, `VAL-NV-19` | ✅ 6 Nov 2026 (backend): `NvwBedTabletService.CreateAsync`, `GET /bed-options`; uji HTTP no. 2–10 `PASS` — `NVT-000001`, `NVT-000002` berurutan, tablet kedua bed yang sama `409` `VAL-NV-19`, `VAL-NV-17`/`18` sesuai matriks ([`BE-NV-009.md`](../task/report/backend/BE-NV-009.md)). ✅ 6 Nov 2026 (frontend): layar daftar dan form tablet, pemilih lantai → bed, uji layar L2–L8 `PASS` ([`FE-NV-008.md`](../task/report/frontend/FE-NV-008.md)). Riwayat: 🟡 6 Nov pagi |
| `FR-NV-034` login sekali, atur ulang, nonaktif beralasan | `NV-DEC-035`; `AD-02`; `G-17` | api §5, §6; state §4; `VAL-NV-21`–`27` | `BE-NV-009`, `BE-NV-010` | `FE-NV-008`, `FE-NV-009` | Uji HTTP buat → login → atur ulang → nonaktif; tablet nonaktif ditolak runtime | 🟡 6 Nov 2026: bagian `BE-NV-009` ✅ — uji HTTP no. 11–25 `PASS`: buat login sekali, atur ulang (sandi lama `401`, sandi baru `200`), nonaktif beralasan (login `401`), aktifkan kembali, dan tolak aktifkan kembali bila bed sudah dipakai tablet lain; `VAL-NV-21`–`25` sesuai matriks ([`BE-NV-009.md`](../task/report/backend/BE-NV-009.md)). Bagian `BE-NV-010` ✅ 8 Nov 2026: tablet nonaktif ditolak runtime `403` `VAL-NV-26` 30 detik sesudah penonaktifan; refresh `401` dan login sandi benar `401` dengan pesan `AD-02` (`VAL-NV-27`); token lama sesudah atur ulang `403` `NV-SESSION-STALE` — uji tablet no. 10–13 `PASS` ([`BE-NV-010.md`](../task/report/backend/BE-NV-010.md)). Bagian `FE-NV-008` ✅ 6 Nov 2026: Buat Login, Atur Ulang Login, Nonaktifkan beralasan dari layar, uji L5, L6, L10 `PASS` ([`FE-NV-008.md`](../task/report/frontend/FE-NV-008.md)). `FE-NV-009` belum dikerjakan |
| `FR-NV-035` pindah bed tanpa panggilan aktif | `AD-04` | api §5, `VAL-NV-20` | `BE-NV-009` | `FE-NV-008` | `VAL-NV-20` (bukti runtime saat `BE-NV-011`) | ✅ 9 Nov 2026: `BE-NV-009` ✅ — `NvwBedTabletService.UpdateAsync` menolak pindah bila bed lama punya panggilan `Waiting`/`Claimed`, lalu memeriksa bed baru (`VAL-NV-19`); uji HTTP pindah sukses (no. 24) dan tolak pindah ke bed terpakai (no. 9) `PASS` ([`BE-NV-009.md`](../task/report/backend/BE-NV-009.md)). Bukti runtime `VAL-NV-20` 9 Nov 2026: pindah tablet dari bed yang punya panggilan aktif → `409` `VAL-NV-20` (uji `BE-NV-011` no. 14) ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). `FE-NV-008` ✅ 6 Nov 2026: pindah bed dari layar Perbarui (uji L9 `PASS`); pesan `409` dari backend ditampilkan dan pilihan dimuat ulang ([`FE-NV-008.md`](../task/report/frontend/FE-NV-008.md)) |
| `FR-NV-036` runtime tablet: bed dari akun, bed kosong terkunci, tanpa identitas, pemisahan TV dan tablet | `NV-DEC-046`, `056`; `G-09` | api §6 `/current`; permission R2 §8 | `BE-NV-010` | `FE-NV-009` | `UAT-12`, `UAT-14`, `VAL-NV-14` × tablet, `VAL-NV-26` × TV | 🟡 Bagian `BE-NV-010` ✅ 8 Nov 2026: `GET /bed-tablet-runtime/current` mengembalikan bed dari tablet (bed terisi `isBedOccupied` true, bed kosong false), tanpa kunci identitas, dan nama serta No. RM pasien tidak muncul. Tablet ditolak papan TV `403`; TV, SuperAdmin, LOKET, dan perawat ditolak runtime tablet `403`. Uji tablet no. 1–7 `PASS` ([`BE-NV-010.md`](../task/report/backend/BE-NV-010.md)). `FE-NV-009` belum dikerjakan |
| `FR-NV-037` tekan panggil, satu panggilan aktif per bed, kunci permintaan | `NV-DEC-039` | api §6 `POST /calls` | `BE-NV-011` | `FE-NV-011` | `NV-AC-029`, uji kirim ganda | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026: panggilan baru, tekan ulang, satu panggilan aktif per bed walau dua tekanan paralel, `Idempotency-Key` wajib — uji no. 1, 4, 5, 12 `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). `FE-NV-011` belum dikerjakan |
| `FR-NV-038` naik Darurat, tidak pernah turun | `NV-DEC-039`; `G-13` | api §6; state §5 | `BE-NV-011` | `FE-NV-011` | `NV-AC-029` | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026: Normal naik Darurat (`EscalatedAt` terisi, jam awal tetap); tekan pada Darurat tetap Darurat — uji no. 4c, 6 `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). `FE-NV-011` belum dikerjakan |
| `FR-NV-039` klaim tunggal, ambil alih, lepas | `NV-DEC-040` | api §7; `VAL-NV-33`–`35`, `38` | `BE-NV-012` | `FE-NV-012` | `NV-AC-032`, `033`, `UAT-17` | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: klaim tunggal, klaim kedua `409` `VAL-NV-33` dengan nama pengklaim, ambil alih, lepas hanya oleh pengklaim, klaim bersamaan satu menang — uji no. 7–11 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-012` belum dikerjakan |
| `FR-NV-040` "Perawat hadir" tahan 3 detik | `NV-DEC-041`, `050` | api §6 `nurse-arrived` | `BE-NV-011` | `FE-NV-011` | `NV-AC-034`, `042`, `UAT-15` | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026: "Perawat hadir" → `Handled`, `IsBedsideConfirmed = true`, jejak `NurseArrived` pelaku tablet — uji no. 9, 16 `PASS`; bukti sesudah koreksi penempatan dikecualikan atas keputusan Rizki 9 Nov 2026 ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). `FE-NV-011` belum dikerjakan |
| `FR-NV-041` tutup dari konsol dengan alasan | `NV-DEC-053` | api §7 `close`; `VAL-NV-36` | `BE-NV-012` | `FE-NV-012` | `NV-AC-044`, `UAT-18` | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: alasan wajib (2–4), Lainnya wajib keterangan 5–250, `Handled` + `IsBedsideConfirmed = false` + jejak `ClosedFromConsole`; keterangan tidak masuk response maupun log — uji no. 13–15 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-012` belum dikerjakan |
| `FR-NV-042` pasien membatalkan panggilan biasa yang menunggu | `NV-DEC-054` | api §6 `cancel`; `VAL-NV-31` | `BE-NV-011`, `BE-NV-012` | `FE-NV-011` | `NV-AC-045` | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026: batal panggilan biasa menunggu → `Cancelled`; Darurat → `409` `VAL-NV-31` — uji no. 7, 13 `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). Batal pada `Claimed` → `409` `VAL-NV-31` terbukti 9 Nov 2026 (uji `BE-NV-012` no. 12) ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)); `FE-NV-011` belum dikerjakan |
| `FR-NV-043` tutup otomatis saat pindah/pulang ≤ 60 detik | `NV-DEC-046`, `050`, `052`, `057`; `AD-05` | state §5; integration R2 §5 | `BE-NV-013` | `FE-NV-011` (tablet kembali "Bed belum berpenghuni") | `NV-AC-038`, `042`, `UAT-19` | 🟡 Bagian `BE-NV-013` ✅ 9 Nov 2026: pemeriksaan tiap 30 detik menutup panggilan aktif bila penempatannya berakhir `Transfer` ("pasien pindah") atau `EpisodeClosed`/`PatientDeparted` ("pasien pulang"); sejak `NV-DEC-057` juga `AdmissionCancelled` ("admisi dibatalkan") dan koreksi ke bed lain ("pasien pindah"), sesudah mengikuti rantai koreksi; koreksi di bed yang sama tidak — terpetakan ke source, QBE `PASS`; uji HTTP 9/9 `PASS`, panggilan pasien yang dipindah tertutup 1,47 detik sesudah penempatan berakhir ([`BE-NV-013.md`](../task/report/backend/BE-NV-013.md)). `FE-NV-011` belum dikerjakan |
| `FR-NV-044` satu kejadian satu jejak | `NV-DEC-049` | kamus data `NvwPatientCallEvent`; permission R2 §9 | `BE-NV-011`, `BE-NV-012`, `BE-NV-013` | — | Hitungan jejak lewat `SELECT` read-only per skenario | ✅ 9 Nov 2026 — ketiga bagian backend selesai. Bagian `BE-NV-011`: setiap perintah tablet menulis tepat satu jejak, kirim ulang tanpa jejak; hitungan lewat SELECT — 10 jejak untuk 10 perubahan per run ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). Bagian `BE-NV-012` ✅ 9 Nov 2026: setiap perintah konsol tepat satu jejak `ActorKind = Nurse` — 11 jejak untuk 11 perintah ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). Bagian `BE-NV-013`: penutupan sistem menulis panggilan dan satu jejak `ClosedBySystem` (`ActorKind = System`) dalam satu `SaveChanges`; runtime: 3 jejak untuk 3 perubahan panggilan uji, dan putaran berikutnya tidak menambah jejak ([`BE-NV-013.md`](../task/report/backend/BE-NV-013.md) uji no. 3, 7) |
| `FR-NV-045` gagal kirim → pesan bel dinding, kirim ulang tanpa ganda | `NV-DEC-037`, `039` | api §6 `Idempotency-Key`; fe R2.9 | `BE-NV-011` | `FE-NV-011` | `NV-AC-039`, `UAT-20` | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026 (sisi server): kunci yang sama → keadaan saat ini tanpa jejak baru — uji no. 5, 10 `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). Pesan bel dinding dan kirim ulang milik `FE-NV-011`, belum dikerjakan |
| `FR-NV-046` strip tetap di atas, berurutan, tanpa identitas, "+k lainnya" | `NV-DEC-044`, `047`; `NV-FE-006` | api §9 | `BE-NV-015` | `FE-NV-014` | `NV-AC-030`, `031`, `037`, `UAT-21`, `UAT-22` | Belum dikerjakan |
| `FR-NV-047` penanda kartu bed, TV tanpa suara | `NV-DEC-013`, `047` | api §9 `ActiveCallPriority` | `BE-NV-015` | `FE-NV-014` | `UAT-21` | Belum dikerjakan |
| `FR-NV-048` pembaruan lewat sinyal ≤ 5 detik, cadangan muat ulang | `NV-DEC-047` | api §10; integration R2 §8 | `BE-NV-014` | `FE-NV-010`, `FE-NV-014` | `NV-AC-028`, `NFR-008` | 🟡 Bagian `BE-NV-014` sebagian 9 Nov 2026: sinyal `NurseViewPatientCallChanged` sesudah setiap perintah ke grup TV, konsol, dan tablet — terpetakan ke source, QBE `PASS`; build dan uji hub ditunda ([`BE-NV-014.md`](../task/report/backend/BE-NV-014.md)). Frontend belum |
| `FR-NV-049` konsol dengan akun sendiri, pilih lantai, izin tanpa roster | `NV-DEC-043`; `AD-06` | api §7; permission R2 §6–§8 | `BE-NV-012` | `FE-NV-012` | `NV-AC-036` | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: resource `NurseViewPatientCall` Read/Update terbit lewat seeder; tanpa izin `403`; berizin tanpa pemeriksaan roster mengklaim lantai 4 — uji no. 1, 3, 16 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-012` belum dikerjakan |
| `FR-NV-050` isi dan aksi daftar konsol | `NV-DEC-038`, `040`, `044` | api §7 | `BE-NV-012` | `FE-NV-012` | `NV-AC-030`, `032`, `037` | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: daftar per lantai berurutan, inisial · JK · umur, lama menunggu, `WaitLevel`, `AvailableActions` per pengguna — uji no. 4–7 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-012` belum dikerjakan |
| `FR-NV-051` bunyi, nada Darurat, pengingat, tunda, "Aktifkan suara" | `NV-DEC-045`; `G-14`; `NV-FE-007` | fe R2.6; state §7 | — | `FE-NV-013` | `NV-AC-035`, `UAT-23`, `UAT-24` | Belum dikerjakan |
| `FR-NV-052` prototipe `nurseCallContext.jsx` dihapus | `NV-DEC-051` | fe R2.8 | — | `FE-NV-012` | `NV-AC-043`: berkas tidak ada, pencarian `import` nol | Belum dikerjakan |
| `FR-NV-053` ambang kuning/merah dengan rujukan SK; kosong → tanpa warna | `NV-DEC-048`; `G-15` | api §8; `VAL-NV-40` | `BE-NV-016` | `FE-NV-015`; warna di `FE-NV-012`, `FE-NV-014` | `UAT-25`, `NV-AC-040` | 🟡 6 Nov 2026: bagian `BE-NV-016` ✅ — uji HTTP no. 1–14 `PASS`: kosong → `isConfigured` false, tolakan `VAL-NV-40`, simpan dan ganti dengan baris lama bertanda hapus beserta pelakunya, cabut kembali tanpa warna, rujukan tercatat di log; `ResolveWaitLevel` lewat source ([`BE-NV-016.md`](../task/report/backend/BE-NV-016.md)). `FE-NV-015` ✅ 6 Nov 2026: layar ambang, uji L11–L16 `PASS` ([`FE-NV-015.md`](../task/report/frontend/FE-NV-015.md)). Warna di `FE-NV-012`, `FE-NV-014` belum dikerjakan |
| Regresi login (syarat lead) | `NV-DEC-030`, `056` | backend R2.5; fe R2.7 | `BE-NV-010` | `FE-NV-009` | `NV-AC-041`, `UAT-26` | 🟡 Bagian `BE-NV-010` ✅ 8 Nov 2026: regresi login backend **172/172** terhadap baseline 12 skenario (tujuh akun manusia, Kiosk, display antrean, TV, dua login gagal) yang direkam sebelum perubahan berjalan. Satu-satunya beda klaim adalah `is_nurse_view_bed_tablet_account = "false"`, plus stempel sesi TV karena atur ulang ([`BE-NV-010.md`](../task/report/backend/BE-NV-010.md)). Regresi redirect frontend milik `FE-NV-009`, belum dikerjakan |

### R2.2 Acceptance criteria → task

| Acceptance | Inti | Task backend | Task frontend | Bukti yang diharapkan | Status |
|---|---|---|---|---|---|
| `NV-AC-028` | Panggilan tampil di strip dan konsol tanpa menunggu 60 detik | `BE-NV-014` | `FE-NV-010`, `FE-NV-012`, `FE-NV-014` | Rekaman berjam TV + konsol | 🟡 Bagian `BE-NV-014` sebagian 9 Nov 2026: terpetakan ke source; uji hub ditunda ([`BE-NV-014.md`](../task/report/backend/BE-NV-014.md)). Frontend belum |
| `NV-AC-029` | Tekan ulang dan Darurat tetap satu item sejak tekan pertama | `BE-NV-011` | `FE-NV-011` | Response `POST /calls` ketiga + `GET /active` | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026: tekan, tekan lagi, Darurat → satu panggilan sejak tekan pertama — uji no. 4a–4d `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). `GET /active` ✅ 9 Nov 2026: panggilan yang ditekan ulang dan naik Darurat tampil sebagai satu item berurutan (uji `BE-NV-012` no. 5) ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-011` belum dikerjakan |
| `NV-AC-030` | Urutan Darurat dulu, lalu paling lama menunggu | `BE-NV-012`, `BE-NV-015` | `FE-NV-012`, `FE-NV-014` | Tangkapan konsol dan strip | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: konsol berurutan Darurat lalu paling lama menunggu — uji no. 5 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `BE-NV-015`, `FE-NV-012`, `FE-NV-014` belum dikerjakan |
| `NV-AC-031` | Strip tidak ikut berganti halaman | `BE-NV-015` | `FE-NV-014` | Rekaman TV 30 detik | Belum dikerjakan |
| `NV-AC-032` | Klaim kedua → Ambil alih; jejak mencatat keduanya | `BE-NV-012` | `FE-NV-012`, `FE-NV-014` | Response `409` `VAL-NV-33`, response ambil alih, jejak | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: klaim kedua `409` `VAL-NV-33`, ambil alih `200`, jejak `Claimed` + `TakenOver` mencatat keduanya — uji no. 7 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-012`, `FE-NV-014` belum dikerjakan |
| `NV-AC-033` | Lepas → `Waiting`, jam awal tetap | `BE-NV-012` | `FE-NV-012` | Response `release` | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: lepas → `Waiting`, jam awal tetap; bukan pengklaim `403` — uji no. 9 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-012` belum dikerjakan |
| `NV-AC-034` | Tahan < 3 detik tetap aktif; 3 detik → `Handled` | `BE-NV-011` | `FE-NV-011` | Rekaman tablet + jejak `NurseArrived` | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026 (sisi server): `Handled` + jejak `NurseArrived` — uji no. 9 `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). Penahanan 3 detik milik `FE-NV-011`, belum dikerjakan |
| `NV-AC-035` | Pengingat berbunyi; senyap bila semua diklaim; panggilan baru tetap berbunyi saat ditunda | — | `FE-NV-013` | Rekaman video bersuara | Belum dikerjakan |
| `NV-AC-036` | Tanpa izin → `403`; berizin tanpa roster → klaim diterima | `BE-NV-012` | `FE-NV-012` | Dua response | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: tanpa izin `403` ×4; berizin tanpa roster mengklaim lantai 4 `200` — uji no. 3, 16 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-012` belum dikerjakan |
| `NV-AC-037` | Strip tanpa identitas; konsol hanya inisial · JK · umur | `BE-NV-012`, `BE-NV-015` | `FE-NV-012`, `FE-NV-014` | JSON tersimpan; pencarian nama lengkap → nol | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026 (sisi konsol): item hanya inisial · JK · umur; nama lengkap dan No. RM nol di JSON dan log — uji no. 6 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `BE-NV-015`, `FE-NV-012`, `FE-NV-014` belum dikerjakan |
| `NV-AC-038` | Pindah → `ClosedBySystem` ≤ 60 detik; tablet "Bed belum berpenghuni" | `BE-NV-013` | `FE-NV-011` | Rekaman berjam + jejak `ClosedBySystem` | 🟡 Bagian `BE-NV-013` ✅ 9 Nov 2026: pasien 325-A dipindah ke 420-B → panggilan `ClosedBySystem` "pasien pindah" dalam 1,47 detik; tablet `IsBedOccupied = false`; lantai 4 tanpa panggilan — uji no. 3–5 `PASS` ([`BE-NV-013.md`](../task/report/backend/BE-NV-013.md)). `FE-NV-011` belum dikerjakan |
| `NV-AC-039` | Tablet terputus → pesan bel dinding; tersambung → tepat satu panggilan | `BE-NV-011` | `FE-NV-011` | Rekaman + jejak satu `Pressed` | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026 (sisi server): kirim ulang dengan kunci sama → satu panggilan, satu jejak — uji no. 5, 10 `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). `FE-NV-011` belum dikerjakan |
| `NV-AC-040` | Ambang kosong → tanpa warna | `BE-NV-016`, `BE-NV-012`, `BE-NV-015` | `FE-NV-012`, `FE-NV-014`, `FE-NV-015` | Tangkapan + `IsConfigured = false` | 🟡 6 Nov 2026: bagian `BE-NV-016` ✅ — `GET /` ambang kosong → `isConfigured` false (uji no. 1, 13); `ResolveWaitLevel` → `None` bila ambang kosong (source) ([`BE-NV-016.md`](../task/report/backend/BE-NV-016.md)). Bagian `FE-NV-015` ✅: status "Belum dikonfigurasi — panggilan tampil tanpa warna" (uji L11, [`FE-NV-015.md`](../task/report/frontend/FE-NV-015.md)). Bagian `BE-NV-012` ✅ 9 Nov 2026: konsol `thresholdsConfigured` false, `waitLevel` `None` (uji no. 4, 5) ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `BE-NV-015`, `FE-NV-012`, `FE-NV-014` belum dikerjakan |
| `NV-AC-041` | Regresi login empat akun sesudah tablet ditambahkan | `BE-NV-010` | `FE-NV-009` | Rekaman login | 🟡 Bagian `BE-NV-010` ✅ 8 Nov 2026: rekaman login backend sesudah build 172/172 sama dengan baseline, kecuali klaim baru yang diizinkan ([`BE-NV-010.md`](../task/report/backend/BE-NV-010.md)). Rekaman sisi frontend milik `FE-NV-009`, belum dikerjakan |
| `NV-AC-042` | Koreksi di bed yang sama tidak menutup panggilan; "Perawat hadir" tetap bisa. Batal admisi dan koreksi ke bed lain menutupnya (`NV-DEC-057`) | `BE-NV-011`, `BE-NV-013` | `FE-NV-011` | Rekaman + response `nurse-arrived` | 🟡 Bagian `BE-NV-011`: bukti runtime dikecualikan atas keputusan Rizki 9 Nov 2026 — koreksi penempatan tidak dapat dipulihkan lewat API Rawat Inap (temuan untuk tim Hamzah); "Perawat hadir" tanpa cek penempatan terpetakan ke source ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). Bagian `BE-NV-013` ✅ 9 Nov 2026 (task selesai): sesuai `NV-DEC-057`, koreksi di bed yang sama tidak menutup panggilan, sedangkan batal admisi dan koreksi ke bed lain menutupnya — terpetakan ke source; bukti runtime belum ada karena koreksi dan batal admisi tidak dapat dipulihkan, dicatat sesuai kartu ([`BE-NV-013.md`](../task/report/backend/BE-NV-013.md)). `FE-NV-011` belum dikerjakan |
| `NV-AC-043` | `nurseCallContext.jsx` tidak ada dan tidak dirujuk | — | `FE-NV-012` | Hasil pencarian kosong | Belum dikerjakan |
| `NV-AC-044` | Tutup dari konsol wajib beralasan; tercatat tidak dikonfirmasi di bed | `BE-NV-012` | `FE-NV-012` | Dua response + jejak `ClosedFromConsole` | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: tanpa alasan dan Lainnya tanpa keterangan `400`; alasan 3 → `IsBedsideConfirmed = false` + jejak `ClosedFromConsole` — uji no. 13–15 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-012` belum dikerjakan |
| `NV-AC-045` | Batal hanya untuk panggilan biasa yang menunggu | `BE-NV-011`, `BE-NV-012` | `FE-NV-011` | Response `409` + response berhasil | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026: batal biasa `200`, Darurat `409` — uji no. 7, 13 `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). Batal pada `Claimed` → `409` `VAL-NV-31` terbukti 9 Nov 2026 (uji `BE-NV-012` no. 12) ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). `FE-NV-011` belum dikerjakan |
| `VAL-NV-14` × tablet | Akun tablet ditolak papan dan strip TV | `BE-NV-010`, `BE-NV-015` | — | Dua response `403` | 🟡 Bagian `BE-NV-010` ✅ 8 Nov 2026: token tablet ke `/display-runtime/current` dan `/board` → `403`, `403` (uji tablet no. 5) ([`BE-NV-010.md`](../task/report/backend/BE-NV-010.md)). Strip `BE-NV-015` belum dikerjakan |
| `VAL-NV-26` × TV | Akun TV dan Administrator ditolak runtime tablet | `BE-NV-010` | — | Response `403` | ✅ 8 Nov 2026: TV `NVW-000002`, SuperAdmin, LOKET, perawat → `403` ×4 (uji tablet no. 6); kesepuluh akun non-tablet pada rekaman regresi juga `403` ([`BE-NV-010.md`](../task/report/backend/BE-NV-010.md)) |
| `VAL-NV-19` | Tablet kedua untuk bed yang sama ditolak | `BE-NV-009` | `FE-NV-008` | Response `409` | ✅ 6 Nov 2026 (backend): `409` pada buat (no. 7), pindah bed (no. 9), dan aktifkan kembali (no. 25), pesan persis matriks ([`BE-NV-009.md`](../task/report/backend/BE-NV-009.md)). ✅ 6 Nov 2026 (frontend): bed bertablet terkunci di pemilih; permintaan yang tetap terkirim → pesan `VAL-NV-19` di bawah isian Bed (uji L7, L8, [`FE-NV-008.md`](../task/report/frontend/FE-NV-008.md)). Riwayat: 🟡 6 Nov pagi |
| `VAL-NV-28` | Bed kosong tidak bisa memanggil; layar tanpa tombol | `BE-NV-011` | `FE-NV-011` | Response + tangkapan | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026: bed kosong → `409` ×2 — uji no. 2 `PASS`; kasus bed kosong yang masih punya panggilan aktif dikecualikan atas keputusan Rizki 9 Nov 2026 (bukti source) ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). Layar tanpa tombol milik `FE-NV-011`, belum dikerjakan |
| `VAL-NV-29` | `POST /calls` tanpa `Idempotency-Key` → `400` | `BE-NV-011` | — | Response | ✅ 9 Nov 2026: tanpa kunci, bukan UUID, UUID nol → `400` ×3 — uji no. 1 `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)) |
| `VAL-NV-38` | Klaim bersamaan → satu `409` | `BE-NV-012` | `FE-NV-012` | Dua response | 🟡 Bagian `BE-NV-012` ✅ 9 Nov 2026: klaim bersamaan → satu `200`, satu `409` `VAL-NV-38`; versi lama `409` — uji no. 10, 11 `PASS` ([`BE-NV-012.md`](../task/report/backend/BE-NV-012.md)). Batal/hadir terbukti di `BE-NV-011` uji no. 8; `FE-NV-012` belum dikerjakan |
| `VAL-NV-41` | Akun TV/tablet ditolak grup konsol | `BE-NV-014` | — | Log hub | 🟡 9 Nov 2026: `JoinNurseViewCallConsole` menolak akun perangkat (klaim) dan pengguna tanpa `NurseViewPatientCall : Read` — terpetakan ke source; uji hub ditunda ([`BE-NV-014.md`](../task/report/backend/BE-NV-014.md)) |
| Idempotensi | Kunci sama dua kali → satu jejak | `BE-NV-011` | `FE-NV-011` | Response + jejak | 🟡 Bagian `BE-NV-011` ✅ 9 Nov 2026: kunci sama → satu jejak (tekan dan perawat hadir) — uji no. 5, 10 `PASS` ([`BE-NV-011.md`](../task/report/backend/BE-NV-011.md)). `FE-NV-011` belum dikerjakan |
| Logging | `CloseNote` dan kata sandi tablet tidak masuk log | `BE-NV-009`, `BE-NV-012`, `BE-NV-014` | — | Potongan log | 🟡 6 Nov 2026: bagian `BE-NV-009` ✅ — pemindaian log no. 29: nol kemunculan kedua kata sandi tablet dan pola `Nvt@<hex>aA1!`; delapan jejak aksi admin tercatat dengan pelaku ([`BE-NV-009.md`](../task/report/backend/BE-NV-009.md)). `BE-NV-012`, `BE-NV-014` belum dikerjakan. Catatan 9 Nov 2026: `BE-NV-014` 🟡 — payload sinyal hanya lima kunci tanpa `CloseNote` (source) ([`BE-NV-014.md`](../task/report/backend/BE-NV-014.md)) |

### R2.3 Skenario UAT → task yang harus selesai lebih dulu

UAT dijalankan tim UAT terpisah. Kolom kedua menyebut task yang wajib ✅ sebelum skenario itu
masuk akal diuji.

| UAT | Task prasyarat | Catatan |
|---|---|---|
| `UAT-12` | `BE-NV-009`, `BE-NV-010`, `FE-NV-008`, `FE-NV-009`, `FE-NV-011` | Layar `Siap` dengan dua tombol baru ada di `FE-NV-011` |
| `UAT-13` | `BE-NV-009`, `FE-NV-008` | — |
| `UAT-14` | `BE-NV-010`, `BE-NV-015` | `/active-calls` baru ada di `BE-NV-015` |
| `UAT-15` | `BE-NV-011`, `BE-NV-012`, `FE-NV-011`, `FE-NV-012` | — |
| `UAT-16` | `BE-NV-011`, `BE-NV-012`, `FE-NV-012`, `FE-NV-014` | — |
| `UAT-17` | `BE-NV-011`, `BE-NV-012`, `FE-NV-011`, `FE-NV-012` | — |
| `UAT-18` | `BE-NV-012`, `FE-NV-012` | — |
| `UAT-19` | `BE-NV-013`, `FE-NV-011` | Butuh pindah pasien lewat layar Rawat Inap |
| `UAT-20` | `BE-NV-011`, `FE-NV-011` | — |
| `UAT-21` | `BE-NV-015`, `FE-NV-014` | — |
| `UAT-22` | `FE-NV-014` | — |
| `UAT-23` | `FE-NV-013` | Butuh rekaman bersuara |
| `UAT-24` | `BE-NV-012`, `FE-NV-012`, `FE-NV-013` | — |
| `UAT-25` | `BE-NV-016`, `FE-NV-015`, `FE-NV-012`, `FE-NV-014` | Warna tampil di konsol dan strip |
| `UAT-26` | `BE-NV-010`, `FE-NV-009` | Regresi login, syarat lead |

### R2.4 Kebutuhan non-fungsional → task

| NFR | Kebutuhan | Task | Bukti |
|---|---|---|---|
| `NFR-008` | Panggilan baru tampil ≤ 5 detik | `BE-NV-014`, `FE-NV-010`, `FE-NV-012`, `FE-NV-014` | Jam event vs jam tekan pada rekaman berjam |
| `NFR-009` | Satu panggilan aktif per bed dijamin index | `BE-NV-008`, `BE-NV-011` | `pg_indexes` + uji dua tekanan paralel |
| `NFR-010` | `POST` runtime tablet idempoten | `BE-NV-011`, `FE-NV-011` | Kunci sama dua kali → satu jejak |
| `NFR-011` | Concurrency `Version`, konflik → `409` | `BE-NV-008`, `BE-NV-009`, `BE-NV-011`, `BE-NV-012` | Uji klaim paralel |
| `NFR-012` | Pemeriksaan tiap 30 detik; galat satu putaran tidak menghentikan host | `BE-NV-013` | Penelusuran source + catatan jeda |
| `NFR-013` | Jumlah query tetap | `BE-NV-012`, `BE-NV-015` | Penelusuran source; hitungan query bila log SQL dinyalakan |
| `NFR-014` | Simpan UTC, tampil Asia/Jakarta, lama menunggu dari jam server | `BE-NV-011`–`BE-NV-015`; `FE-NV-011`, `FE-NV-012`, `FE-NV-014` | Response `ServerTimeUtc` + tampilan |
| `NFR-015` | Kata sandi tablet dan `CloseNote` tidak masuk log maupun sinyal | `BE-NV-009`, `BE-NV-012`, `BE-NV-014` | Potongan log dan payload hub |

### R2.5 Definition of Done PRD → pemenuhan

| Butir DoD `04-prd-to-mvp.md` R2.19 | Dipenuhi oleh | Keadaan |
|---|---|---|
| Restu lead tercatat dan syaratnya terbukti | `NV-DEC-056` (tercatat); syarat terbukti lewat `BE-NV-010`, `FE-NV-009` | Restu tercatat 2026-11-05. Syarat sisi backend terbukti 8 Nov 2026: `BE-NV-010` regresi login 172/172. Syarat sisi frontend (`FE-NV-009`) belum |
| Satu tablet memanggil dan ditutup di bed | `UAT-12`, `UAT-15` | Belum |
| Banyak panggilan berurutan tanpa ganda | `UAT-16`, `NV-AC-029`, `030` | Belum |
| Klaim tunggal saat bersamaan | `UAT-17`, `VAL-NV-38` | Belum |
| Panggilan tidak tertinggal tanpa jalan tutup | `UAT-18`, `UAT-19`, `NV-AC-042`, `044` | Belum |
| Gagal kirim tanpa ganda dan menunjuk bel dinding | `UAT-20`, `NV-AC-039` | Belum |
| Identitas tidak bocor di tablet dan TV | `UAT-14`, `UAT-21`, `NV-AC-037` | Belum |
| Konsol berbunyi dan berhenti sesuai aturan | `UAT-23`, `NV-AC-035` | Belum |
| Tiga butir menu terdaftar dan terjangkau | `FE-NV-008`, `FE-NV-012`, `FE-NV-015` | 🟡 6 Nov 2026: dua dari tiga butir terpasang — "Tablet Bed Nurse View" (`FE-NV-008` ✅) dan "Ambang Panggilan Nurse View" (`FE-NV-015` ✅), tampil bagi pemegang izin dan tersembunyi bagi LOKET (uji L1, L16, L18 `PASS`; [`FE-NV-008.md`](../task/report/frontend/FE-NV-008.md), [`FE-NV-015.md`](../task/report/frontend/FE-NV-015.md)). Butir `FE-NV-012` belum dikerjakan |
| Hak akses diberikan tanpa menimpa policy lain | Langkah operasional R2.9 | Belum |
| Prototipe lama terhapus | `FE-NV-012` AC-10 (`NV-AC-043`) | Belum |
| Daftar periksa go-live `G-16` terpenuhi | R2.8 | Belum diperiksa |

### R2.6 Pekerjaan pendamping yang dilacak

| Butir | Bentuk | Pemilik | Dilacak di | Status |
|---|---|---|---|---|
| Pemberitahuan ketergantungan baca `InpBedPlacement.EndDateTime`/`EndReason` ke Muhammad Hamzah | Dokumen `TLM-NV-002` | Rizki Gunawan menyampaikan | R2.7 | Disampaikan dan dijawab Muhammad Hamzah 2026-11-09 (`TLM-NV-002` bagian 8). `AdmissionCancelled` dan koreksi beda bed dijawab berbeda dari `NV-DEC-050`; diputuskan `NV-DEC-057` (2026-11-09) |
| Kabar butir menu Konsol Panggilan di grup Rawat Inap (`NV-FE-008`) | `TLM-NV-002` bagian 4 | Rizki Gunawan menyampaikan | R2.7 | Disampaikan 2026-11-09; Hamzah tidak keberatan (`TLM-NV-002` bagian 8 no. 5) |
| Penghapusan `src/lib/helpers/nurseCallContext.jsx` (`NV-DEC-051`) | Cakupan (9) dan AC-10 `FE-NV-012` | Rizki Gunawan | R2.1 baris `FR-NV-052`, R2.2 baris `NV-AC-043` | Belum dikerjakan. Fakta 2026-11-05 di `91a11b08f`: berkas masih ada, hanya merujuk dirinya sendiri, tidak di-`import` berkas lain |
| Daftar periksa go-live `G-16` (`NV-UNK-005`–`007`) | Pemeriksaan lapangan | Pengelola sarana, kepala ruang, Admin IT | R2.8 | Belum diperiksa |

### R2.7 Dokumen lintas modul

| Task | Dokumen | Kewajiban | Status |
|---|---|---|---|
| `BE-NV-010`, `BE-NV-011`, `BE-NV-013` | `rawat-inap/episode-rawat-inap/docs/task-lintas-modul/TLM-NV-002-akhir-penempatan-bed-dan-menu-konsol-panggilan.md` bagian 2–3 | Pemberitahuan baca; tidak ada perubahan kode Rawat Inap. Sebaiknya disampaikan sebelum `BE-NV-013` dimulai; tidak menahan task. Laporan `BE-NV-013` dirujuk dari dokumen itu | `DIJAWAB` 2026-11-09 (bagian 8); ditindaklanjuti `NV-DEC-057` |
| `FE-NV-012` | Dokumen yang sama, bagian 4 | Kabar butir menu di grup Rawat Inap. Bila Hamzah keberatan, letak menu menjadi keputusan baru Rizki Gunawan | `DIJAWAB` 2026-11-09: tidak keberatan |

`TLM-NV-001` (keperawatan dan episode) tidak berubah: revision 2 tidak menyentuh
`PatientVitalSignController` maupun `ProjectBatchAsync`.

### R2.8 Daftar periksa go-live `G-16`

Butir ini tidak mengubah desain dan bukan task kode. Butir ini menahan **go-live**, tidak menahan
implementasi maupun uji developer. Setiap butir diperiksa per lantai yang akan memakai tablet bed.

| Butir | Pertanyaan | Penanggung jawab | Cara memeriksa | Bukti yang dicatat | Kapan | Bila hasilnya buruk | Status |
|---|---|---|---|---|---|---|---|
| `NV-UNK-005` | Apakah setiap bed rawat inap punya bel kabel yang berfungsi? | Pengelola sarana rumah sakit | Tekan bel di setiap bed lantai sasaran, lalu pastikan bunyinya terdengar di nurse station. **Contoh:** lantai 3 berisi 40 bed → 40 baris "berfungsi/tidak" | Daftar bed bertanggal dan bernama pemeriksa | Sebelum UAT di lantai uji dan sebelum go-live setiap lantai | Bed tanpa bel yang berfungsi menahan go-live lantai itu sampai bel diperbaiki, karena tablet hanya pelengkap (`NV-DEC-037`) | Belum diperiksa |
| `NV-UNK-006` | Apakah petugas mencatat pindah bed tepat saat pasien benar-benar berpindah? | Kepala ruang | Amati atau tanyakan tiga transfer: jam pencatatan di layar Rawat Inap dibandingkan jam pasien meninggalkan bed. **Contoh:** dicatat 10.00, pasien baru pindah 10.20 → tablet bed lama terkunci 20 menit selagi pasien masih di sana | Catatan tiga contoh beserta selisih menitnya | Sebelum go-live | Bila sering lebih awal: edukasi pencatatan; bel kabel tetap cadangan. Lanjut atau tidaknya go-live diputuskan Rizki Gunawan | Belum diperiksa |
| `NV-UNK-007` | Apakah tablet contoh sanggup menjalankan layar tablet bed? | Admin IT | Uji satu tablet contoh: versi browser, layar tetap menyala, mode kiosk (pasien tidak bisa keluar dari layar), sambung ulang Wi-Fi, login bertahan sesudah tablet dinyalakan ulang, dan izin bunyi | Lembar uji dan foto layar | Sebelum pengadaan tablet | Tablet yang gagal tidak dibeli; pilih tipe lain | Belum diperiksa |

### R2.9 Acceptance tanpa task — coverage gap

| Butir | Sebab | Penanganan |
|---|---|---|
| `G-16` prasyarat lapangan | Pekerjaan operasional, bukan kode | Daftar periksa R2.8 |
| `NV-OQ-022` angka ambang sah | Kebijakan Komite Keperawatan | Langkah operasional sesudah `FE-NV-015`; tanpa angka, panggilan tampil tanpa warna (`NV-DEC-048`) |
| Pemberian hak akses | Konfigurasi Akses Role, bukan kode | Sesudah `BE-NV-009`, `012`, `016`: Admin IT diberi `NurseViewBedTablet : Read/Create/Update` dan `NurseViewPatientCallWaitThreshold : Read/Update`; perawat rawat inap dan kepala ruang diberi `NurseViewPatientCall : Read/Update`. Tambahkan pada policy peran, jangan menimpa seluruhnya: salin, tambah, lalu pulihkan bila hanya untuk uji. Bukti: tangkapan layar Akses Role |
| Pendaftaran tablet untuk bed lantai uji | Data operasional | Lewat layar `FE-NV-008` oleh Admin IT; tablet uji dinonaktifkan sesudah uji bila tidak dipakai |
| Pelaksanaan `UAT-12`–`UAT-26` | Milik tim UAT | Agent tidak menulis "UAT lulus"; laporan task hanya memuat verifikasi developer |
| Laporan waktu tanggap | Ditunda (`NV-DEC-049`, PRD R2.8) | Bukan gap: datanya sudah dicatat `BE-NV-011`–`013` |

Tidak ada functional requirement revision 2 yang tidak punya task. Tidak ada task backend yang
mensyaratkan automated test.

### R2.10 Pemeriksaan konsistensi roadmap

| Pemeriksaan | Backend | Frontend |
|---|---|---|
| Jumlah task | 9 (`BE-NV-008`–`016`) | 8 (`FE-NV-008`–`015`) |
| Panah grafik = entri kolom `Dependency` | 11 = 11 | 12 = 12 |
| Jumlah node grafik (batas 15) | 9 | 15 (8 task + 7 cermin backend) |
| Bebas siklus | Ya | Ya |
| Task ⛔ | 0 | 0 |
| Bentuk grafik | Mermaid, mengikuti grafik revision 1 di berkas yang sama (§13) | Mermaid, idem |
