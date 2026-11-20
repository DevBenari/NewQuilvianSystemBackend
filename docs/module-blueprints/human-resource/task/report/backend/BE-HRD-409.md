# Laporan Perubahan Backend — `BE-HRD-409`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-409` |
| Judul | Privasi gaji SK dan nomor SK atomik lintas instance |
| Slice | `S-FIN-M07` / Rotasi Promosi (alias `FIN-HRD-07-BE-003`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.4, kartu `BE-HRD-409` |
| Trace | `HRD-DEC-031`; instruksi Menu 07 `FIN-HRD-07-BE-003`; QBE-CODE-003/006 |
| Contract version | Kontrak SK Kepegawaian dan penempatan existing (`BE-HRD-024`, `BE-HRD-027`); tidak ada endpoint baru |
| Dependency | `BE-HRD-407` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Services/HrdPersonnelActionOrderService.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji 20 create paralel `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Nomor SK dibuat dengan membaca nomor tertinggi hari itu, lalu `lock` di memori proses + coba ulang 3 kali. Pada dua server (atau dua permintaan yang membaca bersamaan) keduanya mendapat nomor yang sama; penyimpanan kedua gagal di index unik. `lock` lokal tidak melindungi lintas instance (QBE-CODE-003).

## 2. Proses bisnis

1. HR membuat draf SK.
2. Nomor `SK-RSMMC-YYYYMMDD-NNNN` diterbitkan alokator number series bersama (deret `HRD_PERSONNEL_ACTION_ORDER`, diulang harian per tanggal bisnis WIB) di transaksi dan koneksinya sendiri, dengan kunci di database.
3. Nomor tertinggi hari ini yang sudah ada dipakai sebagai batas bawah, sehingga hari peralihan tidak bentrok dengan nomor lama.
4. Gaji tidak pernah tampil di input/output SK (`TargetBaseSalary` selalu `null`).

**Contoh.** 20 HR membuat SK bersamaan pada 20 November 2026 → nomor `SK-RSMMC-20261120-0001` s.d. `-0020` tanpa kembar; satu draf yang batal membuat satu nomor hangus (tidak dipakai ulang).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`GenerateOrderNumberAsync`, `CreateDraftOrderAsync`, `NumberSeriesAllocator`, konfigurasi index unik `ActionOrderNumber`, DTO SK (`TargetBaseSalary`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdPersonnelActionOrderService.cs` | Nomor dari `NumberSeriesAllocator` (reset harian, batas bawah nomor hari ini); `lock` statis dan konstanta yang tidak terpakai dihapus |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Format nomor tetap `SK-RSMMC-YYYYMMDD-NNNN` |
| Database | Tanpa migration; deret baru tercipta otomatis di `NumNumberSeries`; index unik `ActionOrderNumber` sudah ada |
| Keamanan/Auth | Gaji tetap tidak bocor |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Personnel Action Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders` | Buat draf SK bernomor atomik | hak existing |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning nullable dari build pertama diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | 3 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: dua create serentak | Nomor berbeda dari pencacah terkunci | `PASS` | `NumberSeriesAllocator.AllocateAsync` |
| Penelusuran: response SK | `TargetBaseSalary = null` | `PASS` | proyeksi detail/daftar |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) 20 create paralel menghasilkan nomor unik | Terpenuhi (source) | alokator atomik + index unik; uji paralel `NOT RUN` |
| (2) Gaji tidak bocor pada response umum | Terpenuhi | `TargetBaseSalary` null |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | `BE-HRD-410` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
