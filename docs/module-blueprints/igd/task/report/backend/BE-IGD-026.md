# Laporan Verifikasi Backend — `BE-IGD-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-026` |
| Judul | `TrxPatientAssessment.QueueId` menjadi opsional — uji langkah mundur migration |
| Slice | `IGD-S04` · `EPIC IGD-09` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) gelombang `MVP-3` |
| Requirement | `FR-IGD-060` |
| Keputusan | `IGD-DEC-068`; approval sementara `ClinicalManagement` `IGD-DEC-107`/`108` |
| Contract version | **Nol perubahan** |
| Klasifikasi | `VERIFY` — **nol perubahan source, nol perubahan migration** |
| Target tulis | Laporan ini; bukti uji; baris status roadmap; traceability; status modul; decision log |
| Source yang dibaca | `rizkiG` `cdffb114`; `Migrations/20260826090500_ImplementIgdFullPatientJourney.cs` |
| Laporan sebelumnya | [be-igd-021-035](be-igd-021-035-penyelesaian-perjalanan-pasien.md) — kriteria 2 dan 3 terpetakan 15 September 2026 |
| Tanggal | 27 Oktober 2026 |
| Status | ✅ **SELESAI — acceptance 1–3 terpenuhi** |

---

## 1. Yang tersisa sebelum laporan ini

Kartu ini 🟡 sejak 15 September 2026 karena satu kriteria: `Down()` migration `20260826090500` tertulis
tetapi belum pernah dicoba di basis data terpisah. Penerapan 27 Agustus 2026 hanya menjalankan Up.

## 2. Acceptance

| # | Kriteria | Bukti | Keadaan |
| ---: | --- | --- | :---: |
| 1 | Migration punya langkah mundur tertulis dan sudah diuji di basis data terpisah | [Bukti uji Down 27 Oktober 2026](../../../evidence/2026-10-27-uji-down-migration-be-igd-026-031.md): kontainer PostgreSQL 16.15 sekali pakai. Up → `QueueId` nullable; Down → `NOT NULL` kembali (`DN-5`); Up lagi → skema identik dengan Up pertama (`RU-2`, md5 kolom `e10121f3…`, index `d646b70b…`, constraint `c598c9c3…`); sidik jari di luar sasaran tidak berubah di setiap langkah | ✅ |
| 2 | Seluruh test rawat jalan yang menyentuh pengkajian tetap lulus | Historis `IGD-DEC-110` (suite `761 total, 759 lulus`); proyek test dihapus 11 September 2026 atas perintah atasan | ✅ tidak berubah |
| 3 | Nol baris lama berubah nilainya | Kini **terukur**: md5 baris pengkajian ber-`QueueId` `a61d68ce…` sama di keempat tahap B0–B3 (`DN-6`) | ✅ |

## 3. Temuan yang dicatat, tidak dikerjakan

- **Down ditolak begitu ada pengkajian IGD tanpa antrean** — `23502: column "QueueId" of relation
  "TrxPatientAssessment" contains null values`, lalu digulung balik penuh (`NG-1`…`NG-3`). Itu memang
  akibat yang diharapkan dari tujuan task ini; artinya Down tidak dapat dijalankan di dev maupun
  lingkungan lain sesudah IGD dipakai tanpa memindahkan data klinis lebih dulu (`IGD-FACT-247`).
- Bagian lain migration yang sama tidak dipulihkan sempurna oleh Down — milik `BE-IGD-031`, dicatat di
  [laporannya](BE-IGD-031.md) dan `IGD-FACT-246`.

## 4. Validasi

| Perintah | Hasil |
| --- | --- |
| `dotnet ef database update <target> --no-build --connection <kontainer>` × 7 | Lihat tabel hasil pada berkas bukti; dua percobaan yang gagal karena rancangan baris uji dicatat apa adanya |
| `node skrip/rakit-hasil.mjs` | 19/19 pemeriksaan lulus; `BE-IGD-026#1` `PASS` |
| QBE checker | Tidak dijalankan — nol perubahan source backend |
| `git status` backend | Hanya dokumen blueprint |

Basis data dev tidak dialamatkan. `dotnet build` tidak dijalankan; DLL dipakai dari build pemilik
27 Oktober 2026 08.49.36.

## 5. Langkah berikutnya

Tidak ada untuk kartu ini.
