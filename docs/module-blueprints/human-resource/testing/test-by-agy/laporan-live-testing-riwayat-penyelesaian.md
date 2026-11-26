# Laporan Live Testing — Riwayat Penyelesaian

| Field | Nilai |
| --- | --- |
| Menu | Hubungan Industrial & Terminasi → **Riwayat Penyelesaian** (`/hrd/relations/sanctions/history`) |
| Pemilik menu | HR Relations |
| Task | `FE-HRD-409`, layar `FE-HRD-406`, menu `FE-HRD-408` |
| Tanggal uji | 23 November 2026 |
| Lingkungan | Frontend build produksi `http://localhost:3000`; backend lokal `https://localhost:7184`; database dev `QuilvianNewDevHamzah` |
| Peramban | Chromium headed, `slowMo` 450 ms |
| Akun uji | `uat.pkb.relations@rsmmc.local` (HR Relations, memegang `ViewSanctions` dan `ViewIncidents`) |
| Hasil | **2 dari 2 skenario lulus**; 1 temuan backend (P1) |

## 1. Ringkasan

Layar memanggil dua sumber nyata: sanksi selesai dengan parameter `statuses` berulang (`statuses=Expired&statuses=RevokedOnAppeal`) dan insiden berstatus `ResolvedNoSanction`. Bagian **Insiden Ditutup Tanpa Sanksi** menampilkan insiden uji yang diselesaikan dengan pembinaan, tanpa kolom aksi. Bagian **Sanksi Selesai** masih kosong ("Belum ada sanksi yang selesai.") karena belum ada sanksi yang benar-benar berstatus Kedaluwarsa atau Dicabut lewat Banding di database.

## 2. Alur proses bisnis yang diuji

Contoh: Bu Rina (HR Relations) meninjau kasus yang sudah tuntas bulan ini.

1. Perawat Tono salah mengisi jam pulang pada lembar serah terima. Insiden dicatat sebagai *Human Error*, diperiksa dua pemeriksa, lalu diputuskan cukup **pembinaan** (tanpa sanksi). Status insiden menjadi `ResolvedNoSanction`.
2. Bu Rina membuka **Riwayat Penyelesaian**. Layar meminta sanksi selesai dan insiden yang ditutup tanpa sanksi sekaligus.
3. Insiden Tono tampil di bagian kedua sebagai catatan baca-saja.

## 3. Rincian test case

| ID | Prioritas | UAT | Skenario | Harapan | Hasil | Status | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| L05 | P0 | HRD-128 | Dua sumber data | `statuses` berulang dan `status=ResolvedNoSanction`, keduanya 200 | Sanksi 200; insiden 200 (1 baris) | ✅ PASS | — |
| L06 | P1 | HRD-128 | Insiden baca-saja | ≥ 1 baris insiden; tanpa kolom Aksi | 1 baris; 0 kolom Aksi; bagian sanksi kosong | ✅ PASS | `fe-hrd-401/relations-03-riwayat-penyelesaian.png` |

## 4. Endpoint yang diuji

### `[Tags("Corporate - HRD Employee Disciplinary & Just Culture")]`

| Method | Path | Deskripsi | Auth | Request | Response teramati |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/v1/hrd/disciplinary/sanctions?statuses=Expired&statuses=RevokedOnAppeal` | Sanksi selesai | `EmployeeDisciplinary : ViewSanctions` | `statuses` berulang, `level`, `search`, halaman | `200`, kosong |
| `GET` | `/api/v1/hrd/disciplinary/incidents?status=ResolvedNoSanction` | Insiden ditutup tanpa sanksi | `EmployeeDisciplinary : ViewIncidents` | `status`, halaman | `200`, 1 baris |

## 5. Temuan

| ID | Prioritas | Temuan | Dampak | Usulan |
| --- | --- | --- | --- | --- |
| RP-01 | P1 (backend) | Tidak ada kode backend yang pernah menetapkan status sanksi `RevokedOnAppeal`. `POST /sanctions/{id}/appeal` hanya mencatat banding; tidak ada endpoint untuk memutus banding dan mencabut sanksi. | Baris "Dicabut lewat Banding" tidak akan pernah muncul di Riwayat Penyelesaian maupun filter Status Tindak Disiplin | Rencanakan task backend untuk putusan banding (kabul → `RevokedOnAppeal`), atau keputusan pemilik bahwa status ini belum dipakai. |
| RP-02 | Info (backend) | Sanksi berpindah ke `Expired` hanya lewat worker EMP-W1 yang berjalan tiap 6 jam. | Teguran lisan uji (berlaku s.d. 4 Agu 2026) akan muncul di bagian Sanksi Selesai setelah worker berjalan berikutnya | Verifikasi ulang bagian sanksi setelah worker berjalan; tidak perlu perubahan frontend. |

## 6. Batasan

Bagian Sanksi Selesai belum dapat diuji dengan data nyata pada sesi ini karena RP-01 dan RP-02. Perilaku dengan data (dua sanksi selesai berlabel Kedaluwarsa dan Dicabut lewat Banding) dibuktikan lewat uji Playwright dengan response tiruan pada `FE-HRD-406`.
