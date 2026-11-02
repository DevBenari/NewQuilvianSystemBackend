# Laporan Perubahan Backend — `BE-MCU-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-020` |
| Judul | Lab/Rad: pesanan bertanda paket dan penahanan tagihan |
| Slice | `MVP-2` — `EPIC MCU-04` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-020`* |
| Trace | `FR-MCU-031` (sisi Lab/Rad), `FR-MCU-032`; `MCU-DEC-022`, `032`; `MCU-COORD-002` ✅, `MCU-COORD-008` ✅ |
| Contract version | `MCU-API-v1` *Perubahan internal* (Lab, Rad — hanya penambahan ruas baca); `MCU-INT-v1` `INT-MCU-05`, `06` — `approved` 2026-10-21 |
| Dependency | `BE-MCU-003` ✅ (kolom `CoveringMcuOrderId` pada `LabOrder`/`RadOrder`) |
| Klasifikasi | `MEDIUM` — skor 8: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), kontrak API ruas baca (1), lintas modul Lab/Rad/Billing (2), transaksi pemanggil (1), dampak tagihan (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `LabOrderService.cs`, `LabSpecimenService.cs`, `LabOrderDtos.cs`, `RadOrderService.cs`, `RadStudyService.cs`, `RadStudyService.DirectResult.cs`, `RadiologyDtos.cs`; laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — kriteria terbukti di harness dengan produsen fakta klinis sungguhan; uji mutasi membuktikan penjaga |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` (prefix `Lab`) dan `RadiologyManagement` (prefix `Rad`) — modul mitra, perubahan disetujui lewat `MCU-COORD-002`, `MCU-COORD-008` |
| Keberlakuan | `TOUCHED CODE` (service dan DTO yang sudah ada; tanpa entity, tabel, atau endpoint baru) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-TXN-001` (jalur MCU ikut transaksi pemanggil), `QBE-LOG-001`, `QBE-DTO-001`, `QBE-CODE-001`..`006` |
| Checker QBE | `PASS` — 7 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Paket MCU sudah ditagih sebagai **satu baris paket** saat check-in (`BE-MCU-013`). Bila pesanan Lab/Rad dari
paket itu ikut menerbitkan fakta tagih seperti pesanan biasa, pasien ditagih dua kali.

**Contoh.** Paket Rp 350.000 berisi Darah Lengkap, Gula Darah Puasa, dan Thorax. Tanpa penahanan: begitu
spesimen dinyatakan layak dan Thorax diterima mutunya, Billing menambah `LaboratoryCharge` dan `RadiologyCharge`
— total tagihan Rp 760.000 untuk paket Rp 350.000. Sesudah task ini: pesanan bertanda paket tidak menerbitkan
fakta tagih apa pun; tagihan tetap satu baris Rp 350.000.

---

## 2. Proses bisnis

1. *Mulai pemeriksaan* (`BE-MCU-021`, belum dikerjakan) memanggil jalur internal Lab/Rad untuk membuat pesanan
   **bertanda paket** (`CoveringMcuOrderId`) dengan dokter pemberi instruksi = dokter MCU, verifikasi instruksi
   `NotRequired`, di dalam transaksi milik MCU.
2. Lab bekerja seperti biasa: spesimen diterima → dinyatakan layak → pemeriksaan *layak tagih*. Untuk pesanan
   bertanda, langkah terakhir **tidak** menerbitkan fakta `LaboratoryCharge` (juga tidak menerbitkan fakta
   pembatalan tagih saat pemeriksaan dibatalkan — tidak ada tagihan yang perlu dibalik).
3. Radiologi bekerja seperti biasa: mutu citra diterima (konsol) atau hasil disimpan langsung lewat Form Hasil
   (`RAD-DEC-023`). Untuk pesanan bertanda, tidak ada fakta `RadiologyCharge`; study tidak ditandai
   *fakta terkirim* karena memang tidak ada yang dikirim.
4. Detail pesanan Lab/Rad menampilkan ruas baca *dicakup paket MCU* beserta nomor order MCU-nya.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Pesanan **tanpa** tanda (termasuk pemeriksaan MCU di luar paket) | Fakta tagih terbit seperti sebelumnya |
| Jalur MCU dipanggil dengan `InpEpisodeId` | Lab: `ArgumentException`; Rad: `400` validasi *"Pesanan paket MCU tidak boleh menumpang perawatan rawat inap."* (`MCU-DEC-006`); nol pesanan |
| Transaksi pemanggil (MCU) dibatalkan | Pesanan Lab dan Rad yang dibuat di dalamnya ikut batal |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-020`, `021`), `testing/acceptance-test-matrix.md` (`AT-MCU-26`, `28`),
`contracts/integration-contract.md` (`INT-MCU-05`, `06`), `00-interview-decisions.md` (`MCU-DEC-006`, `022`,
`032`), `LabOrderService.cs`, `LabSpecimenService.cs`, `RadOrderService.cs`, `RadStudyService.cs`,
`RadStudyService.DirectResult.cs`, `ClinicalMilestoneFactProducer.cs`, `LabOrderNumberService.cs`,
`RadOrderNumberService.cs`, model `LabOrder`/`RadOrder` (kolom dari `BE-MCU-003`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `LaboratoryManagement/Services/LabOrderService.cs` | `CreateByExaminationsAsync` publik mendelegasikan ke inti `CreateByExaminationsCoreAsync(request, coverage)`; jalur `internal CreateForMcuAsync` + record `McuCoverage`; inti ikut transaksi pemanggil bila ada; detail mengisi `IsCoveredByMcuPackage`, `CoveringMcuOrderNumber` |
| `LaboratoryManagement/Services/LabSpecimenService.cs` | `EmitChargeEligibilityAsync` dan `EmitClinicalCancellationAsync` memulangkan `null` untuk pesanan bertanda; pemanggil pembatalan di `LabOrderService` melewati `null` |
| `LaboratoryManagement/DTOs/LabOrderDtos.cs` | `LabOrderDetailResponse`: `IsCoveredByMcuPackage`, `CoveringMcuOrderNumber` |
| `RadiologyManagement/Services/RadOrderService.cs` | `CreateAsync` mendelegasikan ke `CreateCoreAsync(request, coverage)`; `internal CreateForMcuAsync` + `McuCoverage`; detail mengisi dua ruas baca |
| `RadiologyManagement/Services/RadStudyService.cs` | `EmitChargeEligibilityAsync` memulangkan `null` untuk pesanan bertanda; penilaian mutu memakai `handoff?.Kind` |
| `RadiologyManagement/Services/RadStudyService.DirectResult.cs` | Form Hasil: `handoff == null` berarti tidak ada yang ditagih — bukan kegagalan kirim, tidak dicatat galat |
| `RadiologyManagement/DTOs/RadiologyDtos.cs` | `RadOrderDetailResponse`: dua ruas baca yang sama |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Hanya penambahan dua ruas baca pada detail pesanan Lab dan Rad; endpoint, request, dan perilaku pesanan tanpa tanda tidak berubah |
| Database | Tanpa migration (kolom sudah ada sejak `BE-MCU-003`). **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Jalur pembuatan bertanda sengaja `internal` dan tidak terhubung ke endpoint mana pun — tanda paket hanya bisa ditulis MCU (kriteria `BE-MCU-003`). Izin Lab/Rad yang ada tidak berubah |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Fakta pembatalan Lab | Tidak disebut | Pesanan bertanda juga tidak menerbitkan fakta *pembatalan* tagih | Tidak pernah ada tagihan Lab yang perlu dibalik; mencegah fakta yatim di Billing |
| Lab jalur MCU dengan rawat inap | Ditolak | `ArgumentException` (bukan `LabOrderValidationException`) | Bukan kesalahan petugas — pemanggilnya MCU; `MCU-DEC-006` menjamin order MCU tidak pernah rawat inap |
| Fakta tagih rawat inap saat pesanan dibuat | — | Tidak relevan untuk jalur MCU (rawat inap ditolak lebih dulu) | — |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Ruas baca baru:

| Endpoint yang ada | Ruas baru | Arti |
| --- | --- | --- |
| Detail pesanan Lab (`LabOrderDetailResponse`) | `isCoveredByMcuPackage` (bool), `coveringMcuOrderNumber` (string?) | Pesanan dicakup paket MCU; nomor order MCU penanggung |
| Detail pesanan Rad (`RadOrderDetailResponse`) | Sama | Sama |

---

## 5. Verifikasi

Harness uji **sementara** dibuat ulang (`Tests/QuilvianSystemBackend.McuTests`: SQLite, `ClinicalMilestoneFactProducer`
produksi — fakta dicatat ke `CliClinicalMilestoneFacts` sebelum diserahkan ke Billing, service Lab/Rad produksi,
alokator nomor Lab/Rad jalur non-PostgreSQL), dijalankan, lalu **dihapus** sesuai instruksi user.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 9 lulus | `PASS` | `Passed! - Failed: 0, Passed: 9` |
| `AT-MCU-28` Lab | Spesimen pesanan bertanda dinyatakan layak → *Accepted*, handoff kosong, **nol** fakta; tanpa tanda → handoff ada, **1** fakta | `PASS` | `Lab_spesimen_layak_…` (2 kasus) |
| `AT-MCU-28` Rad (konsol) | Mutu Thorax diterima pada pesanan bertanda → *QualityAccepted*, **nol** fakta, `BillingFactSubmitted` tetap salah; tanpa tanda → **1** fakta | `PASS` | `Rad_mutu_citra_layak_…` (2 kasus) |
| `AT-MCU-28` Rad (Form Hasil `RAD-DEC-023`) | Hasil langsung pada pesanan bertanda → study *QualityAccepted*, **nol** fakta | `PASS` | `Rad_form_hasil_langsung_…` |
| Jalur MCU Lab | 2 pemeriksaan lintas disiplin → 2 pesanan, keduanya bertanda, dokter instruksi = dokter MCU, `NotRequired`; detail `isCoveredByMcuPackage = true`, nomor `MCU-RSMMC-000001` | `PASS` | `Lab_jalur_mcu_…` |
| Jalur biasa Lab + rawat inap | Pesanan biasa tanpa tanda, tanpa dokter instruksi; jalur MCU dengan `InpEpisodeId` ditolak, nol pesanan tambahan | `PASS` | `Lab_jalur_biasa_…` |
| Jalur MCU Rad | Satu pesanan bertanda, dokter MCU, `NotRequired`, ruas baca terisi; rawat inap → `Validation`; `CreateAsync` biasa tetap tanpa tanda | `PASS` | `Rad_jalur_mcu_…` |
| Transaksi pemanggil | Lab + Rad dibuat di dalam transaksi pemanggil lalu di-*rollback* → nol pesanan Lab dan Rad | `PASS` | `Pesanan_mcu_ikut_transaksi_…` |
| Uji mutasi | Penjaga Lab dan Rad dilepas sementara → 3 uji bertanda **gagal** (fakta terbit); kode dipulihkan identik (`cmp`), uji kembali 9/9 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (7 berkas + kontrol) | Nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| DoD *uji Lab/Rad yang ada tetap lulus* | Repo tidak memiliki proyek uji Lab; proyek uji Radiologi sudah dihapus atas instruksi user | `NOT RUN` | `ls Tests/` |
| Penyerahan ke Billing sungguhan; tagihan tetap satu baris paket ujung ke ujung | Ditunda ke `BE-MCU-021` (`AT-MCU-28` ujung ke ujung) | `NOT RUN` | — |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-28` tingkat Lab/Rad: spesimen pesanan bertanda diterima dan Thorax diakuisisi → nol fakta tagih | Terpenuhi | §5 |
| Regresi: pesanan tanpa tanda tetap menerbitkan fakta tagih | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | — |
| DoD: uji Lab/Rad yang ada tetap lulus | Tidak dapat dijalankan — tidak ada proyek uji Lab/Rad di repo | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Jalur `CreateForMcuAsync` hanya terlihat di dalam assembly; uji memanggilnya lewat refleksi (tidak ada `InternalsVisibleTo`) |
| Masalah yang diketahui | Persetujuan tertulis pemilik Billing/Registrasi atas `BE-MCU-013` masih belum tercatat (bawaan task sebelumnya) |
| Risiko tersisa | Penahanan diuji di SQLite dengan produsen fakta sungguhan, belum lewat PostgreSQL dan Billing sungguhan; startup backend ke devYoga masih menunggu migration MCU |
| Perubahan sampingan | Folder uji sementara dibuat dan dihapus; kontrol negatif QBE dibuat dan dihapus |
| Interupsi | Sesi terputus sesudah build lulus; dilanjutkan dari pembuatan harness — tidak ada suntingan ganda |
| Status Git | Perubahan task: 7 berkas Lab/Rad (`M`); laporan ini (`??`); tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-021` (mulai pemeriksaan) kini siap — dependency `BE-MCU-013` ✅ dan `BE-MCU-020` ✅ |
