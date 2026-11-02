# Laporan Verifikasi Backend — `BE-IGD-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-031` |
| Judul | `TrxEmergencyTransfer` menjadi `TrxEmergencyDeparture` — uji `RENAME` balik |
| Slice | `IGD-S05` · `EPIC IGD-05` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) gelombang `MVP-4` |
| Requirement | `FR-IGD-023`…`035` |
| Keputusan | `IGD-DEC-091`; kontrak bagian kepergian `IGD-DEC-108` |
| Contract version | **Nol perubahan** |
| Klasifikasi | `VERIFY` — **nol perubahan source, nol perubahan migration** |
| Target tulis | Laporan ini; bukti uji; baris status roadmap; traceability; status modul; decision log |
| Source yang dibaca | `rizkiG` `cdffb114`; `Migrations/20260826090500_ImplementIgdFullPatientJourney.cs` |
| Laporan sebelumnya | [be-igd-021-035](be-igd-021-035-penyelesaian-perjalanan-pasien.md) — kriteria 1, 3, 4 terpetakan 15 September 2026 |
| Tanggal | 27 Oktober 2026 |
| Status | ✅ **SELESAI — acceptance 1–4 terpenuhi** |

---

## 1. Yang tersisa sebelum laporan ini

Kriteria 2: `RENAME` balik tertulis pada `Down()` migration `20260826090500` (`:266`–`:270`) tetapi
belum pernah diuji.

## 2. Acceptance

| # | Kriteria | Bukti | Keadaan |
| ---: | --- | --- | :---: |
| 1 | Nol baris data hilang | Dipetakan 15 September 2026 (`IGD-UNK-03`: 0 baris di dev). Kini juga **terukur** di kontainer: 6 dari 6 baris uji pindah utuh pada Up (`UP-3`) | ✅ |
| 2 | Langkah mundur berupa `RENAME` balik, diuji | [Bukti uji Down 27 Oktober 2026](../../../evidence/2026-10-27-uji-down-migration-be-igd-026-031.md): sesudah Down `TrxEmergencyTransfer` ada lagi dan `TrxEmergencyDeparture` hilang (`DN-3`); `DepartureNumber`, `DepartureReason`, `SituationSummary`, `HandoverRejectionReason` kembali ke nama lama; keenam baris kembali dengan nomor, `TransferStatus` 1–6, alasan, ringkasan, penolakan, dan penempatan yang sama (`DN-4`); Up lagi menghasilkan skema dan baris identik dengan Up pertama (`RU-2`, `RU-3`) | ✅ |
| 3 | Seluruh route lama tidak lagi ada | Dipetakan 15 September 2026 | ✅ tidak berubah |
| 4 | Frontend ikut berubah dalam rilis yang sama | `DEPARTURE_URL` `emergency-departures` (`FE-IGD-015` ✅) | ✅ tidak berubah |

## 3. Temuan yang dicatat, tidak dikerjakan (`IGD-FACT-246`)

Down mengembalikan **nama**, tetapi bukan seluruh **bentuk** tabel lama:

| Butir | Ukuran |
| --- | --- |
| Index lama, termasuk `UNIQUE` `IX_TrxEmergencyTransfer_TransferNumber` | 12 tidak dipulihkan |
| FK lama ke `AspNetUsers`, `MstServiceUnit`, `TrxEmergencyVisit` | 7 tidak dipulihkan |
| `TransferStatus` | kini `DEFAULT 1` |
| `AcceptedAt`, `AcceptedByUserId` | dibuang Up **tanpa arsip** — 1 baris terisi sebelum, 0 sesudah Down |
| `TrxEmergencyDepartureLegacyPlacement` | tertinggal, **disengaja** (`:244`–`:246`) |

Tidak menggugurkan acceptance 2, yang menuntut `RENAME` balik. Dampaknya juga nihil di dev karena
tabelnya 0 baris saat Up diterapkan. Tidak dibuat kartu baru: memperbaiki Down migration yang sudah
diterapkan di seluruh lingkungan, dan berada di bawah ±250 migration lain, tidak memberi nilai.

## 4. Validasi

| Perintah | Hasil |
| --- | --- |
| `dotnet ef database update <target> --no-build --connection <kontainer>` × 7 | Lihat berkas bukti |
| `node skrip/rakit-hasil.mjs` | 19/19 pemeriksaan lulus; `BE-IGD-031#2` `PASS` |
| QBE checker | Tidak dijalankan — nol perubahan source backend |
| `git status` backend | Hanya dokumen blueprint |

## 5. Langkah berikutnya

Tidak ada untuk kartu ini.
