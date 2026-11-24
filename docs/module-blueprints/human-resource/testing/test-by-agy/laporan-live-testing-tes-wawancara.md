# Laporan Live Testing — Tes/Wawancara

| Field | Nilai |
| --- | --- |
| Menu | Rekrutmen & Seleksi → **Tes/Wawancara** (`/hr/recruitment/evaluations`) |
| Pemilik menu | Recruiter; asesor klinis sebagai penilai |
| Task | `FE-HRD-409`, layar `FE-HRD-405`, menu `FE-HRD-408` |
| Tanggal uji | 23 November 2026 |
| Lingkungan | Frontend build produksi `http://localhost:3000`; backend lokal `https://localhost:7184`; database dev `QuilvianNewDevHamzah` |
| Peramban | Chromium headed, `slowMo` 450 ms |
| Akun uji | `uat.pkb.recruiter@rsmmc.local` (memegang `CandidateEvaluation : ViewAll`), `uat.pkb.asesor@rsmmc.local` (tanpa `ViewAll`) |
| Hasil | **5 dari 5 skenario lulus**; 0 galat server |

## 1. Ringkasan

Kedua jalur (Wawancara HR dan Uji Klinis/Teknis) memanggil backend nyata dengan parameter `track` yang benar. Database dev belum memiliki evaluasi kandidat, sehingga kedua akun melihat "Belum ada uji yang ditugaskan kepada Anda." Asesor hanya melihat butir Tes/Wawancara pada kelompok rekrutmen. Hak `CandidateEvaluation : ViewAll` untuk peran Recruiter terlihat pada layar Manajemen Role.

## 2. Alur proses bisnis yang diuji

Contoh: Bu Sari (asesor ICU) membuka daftar uji yang ditugaskan kepadanya.

1. Bu Sari memilih **Rekrutmen & Seleksi → Tes/Wawancara**. Menu kelompok rekrutmen hanya menampilkan butir ini karena ia tidak memegang hak lain.
2. Layar memanggil daftar evaluasi jalur Wawancara HR dan menampilkan "Belum ada uji yang ditugaskan kepada Anda."

Contoh Recruiter: Pak Dodi membuka layar yang sama, lalu mengganti pemisah jalur ke **Uji Klinis/Teknis**; layar meminta ulang daftar dengan `track=CLINICAL`.

## 3. Rincian test case

| ID | Prioritas | UAT | Skenario | Harapan | Hasil | Status | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| R04 | P0 | HRD-127 | Recruiter membuka jalur HR | `GET evaluations?track=HR` 200; daftar kosong tanpa galat | HTTP 200 | ✅ PASS | `fe-hrd-401/recruiter-02-tes-wawancara-hr.png` |
| R05 | P2 | HRD-127 | Ganti jalur ke klinis | Request `track=CLINICAL` 200 | HTTP 200 | ✅ PASS | — |
| A01 | P1 | HRD-130 | Menu asesor | Hanya Tes/Wawancara dari pintu Paket B | Sesuai | ✅ PASS | — |
| A02 | P0 | HRD-127 | Asesor tanpa uji | "Belum ada uji yang ditugaskan kepada Anda." | Sesuai, HTTP 200 | ✅ PASS | `fe-hrd-401/asesor-01-tes-wawancara-asesor.png` |
| SA-02 | P1 | DoD 43.11 | Hak di Manajemen Role | Posisi "UAT Paket B - Recruiter" memegang hak Candidate Evaluation termasuk `ViewAll` | 3 hak terpilih (Read, Evaluate, ViewAll) | ✅ PASS | `fe-hrd-401/superadmin-02-akses-role-recruiter.png` |

## 4. Endpoint yang diuji

### `[Tags("Corporate / Human Resource / Recruitment Management / Candidate Evaluation")]`

| Method | Path | Deskripsi | Auth | Request | Response teramati |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/recruitment/evaluations` | Daftar Tes/Wawancara | `CandidateEvaluation : Read`; cakupan penuh dengan `ViewAll` | `track` (`HR`/`CLINICAL`), `pendingOnly`, `search`, halaman | `200`, `PagedResult` kosong |

## 5. Temuan

Tidak ada temuan pada layar.

## 6. Batasan

Tiga perilaku belum dapat diuji dengan data nyata karena database dev tidak memiliki lamaran sehingga evaluasi tidak dapat dijadwalkan:

- akun `ViewAll` melihat evaluasi milik dua penilai;
- pesan 403 "Evaluasi ini ditugaskan kepada penilai lain.";
- label veto "Gagal Asesmen".

Ketiganya dibuktikan lewat uji Playwright dengan response tiruan pada `FE-HRD-405`.
