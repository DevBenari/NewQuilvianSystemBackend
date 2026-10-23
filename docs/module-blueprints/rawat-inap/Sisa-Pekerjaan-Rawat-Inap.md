# Sisa Pekerjaan Modul Rawat Inap

## 1. Identitas dokumen

| Atribut | Nilai |
|---|---|
| Tujuan | **Hanya memuat yang belum selesai.** Task yang belum dikerjakan, task yang baru sebagian, dan butir yang masih menggantung pada task yang sudah ✅ |
| Yang **tidak** ada di sini | Task yang sudah selesai. Untuk itu bacalah roadmap sub-modul masing-masing |
| Tanggal | 23 Oktober 2026 |
| Blueprint | `RWI-BP-001` revision `16`, bentuk `COMPOSITE`, status `approved` |
| Backend | `NewQuilvianSystemBackend`, branch `MHamzah` |
| Frontend | `QuilvianSystemFrontendDev`, branch `HamzahV2` |
| Sifat dokumen | Catatan kerja. Ia **menurunkan** dari roadmap, tidak menciptakan task baru |

Bila dokumen ini berbeda dari roadmap sub-modul, **roadmap yang berlaku**. Letaknya
`<sub-modul>/roadmap/`.

---

## 2. ATURAN TETAP — cara menandai task backend selesai

> ### Task backend tidak perlu menunggu build
>
> **Begitu implementasi kodenya ada, task backend ditandai `SELESAI`.**
>
> `dotnet build`, `dotnet restore`, `dotnet test`, dan uji migration **tidak dijalankan** dan
> **tidak menahan** status task. Ini instruksi pemilik, berlaku terus, bukan pengecualian
> sekali pakai.
>
> **Yang tetap wajib:** butir verifikasi yang tidak dijalankan ditulis **`NOT RUN` apa adanya**
> pada laporan task — **jangan dihapus dari daftar**. Task boleh ✅ dengan butir `NOT RUN` di
> dalamnya; yang tidak boleh adalah butir itu hilang tanpa jejak.
>
> Aturan ini **tidak** berlaku untuk penerapan migration ke database. Itu tetap menuntut
> wewenang terpisah.

---

## 3. Ringkasan — apa yang tersisa per 23 Oktober 2026

| Sub-modul | Backend | Frontend | Status Backend |
|---|---|---|:---:|
| `episode-rawat-inap` | Seluruh task implementasi ✅ (termasuk Bed Management `BE-RWI-208`..`221`, ODC, Denah, Realtime Bed `BE-RWI-250`..`251`, Transfer IGD `BE-RWI-206`..`207`, Member Priority). Sisa tertahan: `BE-RWI-190`, `191`, `203` (modul luar) | Selesai mayoritas (V1, V2, Workspace PPRI, Denah, Peta Bangsal, ODC). Sisa: `FE-RWI-022`, `035`, `059`, `061`, `227`..`230`, `264`, `265`..`267` 🟡; `FE-RWI-221` ⛔; `FE-RWI-060` ⛔ dibatalkan | **Selesai Implementasi (Sisa 3 Eksternal ⛔)** |
| `dokter-rawat-inap` | Seluruh 40 task ✅ (V1: 20, V2: 18, Finishing: 5, Repair: 2) | Selesai mayoritas (V1: 9, V2: 14, Finishing: 7). Sisa: `FE-RWI-095` 🟡, `FE-RWI-172` 🟡 | **0 task (100% Selesai)** |
| `keperawatan` | Seluruh 40 task ✅ (V1: 12, V2: 21, Finishing: 7) | Seluruh 32 task ✅ (V1: 6, V2: 14, Finishing: 12) | **0 task (100% Selesai)** |
| `integrasi-billing` | Seluruh 22 task ✅ (`BE-RWI-127`..`134`, `BE-RWI-146`..`159`) | 11 selesai (`FE-RWI-095`..`100`, `166`..`169`, `171`). Sisa: `FE-RWI-170` 🟡 (menunggu ChangeReason) | **0 task (100% Selesai)** |
| **TOTAL KESELURUHAN** | **Seluruh task implementasi Backend ✅** | **Fokus pada penyelesaian gap Frontend & Verifikasi** | **100% Backend Terimplementasi** |

> **Catatan Pembaruan 23 Oktober 2026 — SINKRONISASI STATUS BACKEND RAWAT INAP:**
> - Seluruh 14 task backend Bed Management (`BE-RWI-208` s.d. `BE-RWI-221`) kodenya lengkap, EF update PASS, dan resmi ditandai ✅ SELESAI (runtime UAT dicatat `NOT RUN` sesuai Aturan Tetap Bagian 2).
> - Seluruh 12 task backend Finishing Billing (`BE-RWI-147` s.d. `BE-RWI-159`) kodenya lengkap dan resmi ditandai ✅ SELESAI.
> - Task backend Realtime Bed (`BE-RWI-250`, `BE-RWI-251`), Admisi Transfer IGD (`BE-RWI-206`, `BE-RWI-207`), dan Dokter V2 (`BE-RWI-127`, `BE-RWI-128`) resmi ditandai ✅ SELESAI.
> - Tiga task backend yang berstatus ⛔ (`BE-RWI-190`, `191`, `203`) murni menunggu keputusan eksternal (`RegistrationManagement` dan `BillingManagement`).
> - Sisa pekerjaan kini terpusat pada beberapa gap antarmuka Frontend, sinkronisasi `ChangeReason` untuk `FE-RWI-170`, dan UAT/verifikasi runtime oleh pemilik.

---

## 4. Kelompok A — Task yang Belum Selesai / Status Terkini

### 4.1 Satu task sebagian

#### 🟡 `FE-RWI-035` — Alur bisnis utama terbukti berjalan ujung ke ujung

**6 dari 8** acceptance criteria terpenuhi. Kriteria 8 ditutup 12 September 2026 setelah
`FE-RWI-039` selesai.

| Kriteria | Keadaan | Yang menahan |
|---|---|---|
| 1 | Sebagian — rangkaian sampai `Closed` terbukti, tetapi formulir pendaftaran pasien baru **tidak dikendarai** e2e | `RWI-UI-GAP-007`, ditambah `FilterDatePicker` dan lima pilihan wilayah berantai yang sulit dikendarai otomatis |
| 5 | Sebagian — **8 dari 19** layar punya e2e gerbang peran | Frontend **belum menerima katalog hak akses per butir** dari backend. Sebelas layar sisanya tidak punya penjaga di sisi layar untuk dibuktikan — cacat yang sama yang menahan `FE-RWI-003` kriteria 2 |

**Keduanya bukan pekerjaan koding pada task ini.** Kriteria 1 menunggu data pada lingkungan
target; kriteria 5 menunggu kontrak hak akses per butir dari backend.

> **Ketetapan Pemilik (22 Oktober 2026 — `RWI-DEC-354`):**
> Verifikasi alur admisi tidak mencakup transaksi kasir atau metode pembayaran (`PaymentMethodId`). Pengisian nominal rencana deposit di admisi sudah tuntas dan terbukti benar (`FE-RWI-058` / `FE-INP-20`). Pengisian saldo fisik deposit dan transaksi kasir berada di Kasir (Billing Management).

### 4.2 Status Task Tertahan Deposit & Clearance

`BE-BKC-039` dan `BE-BKC-040` telah selesai di `BillingManagement` (`BillingDepositService`). Berdasarkan hal tersebut, task backend `BE-RWI-071` dan `BE-RWI-072` telah selesai diimplementasikan secara penuh pada 17 September 2026.

| Task | Status | Keterangan Pembaruan 17 September & 22 Oktober 2026 |
|---|---|---|
| ✅ `BE-RWI-071` | ✅ **SELESAI** | Selesai 17 September 2026. Endpoint `GET /monitoring/deposit-shortfall` terpasang di `InpatientMonitoringController`, DTO query & paged result lengkap di `InpatientMonitoringDtos.cs`, logika monitoring terpasang di `InpCensusQueryService.GetDepositShortfallAsync`, dan adapter `IInpBillingDepositAdapter` fail-safe terintegrasi ke `BillingDepositService`. |
| ✅ `BE-RWI-072` | ✅ **SELESAI** | Selesai 17 September 2026. Validasi kelayakan keuangan otoritatif terpasang di `InpDischargeService.Closure.cs` (`MarkFinancialClearanceAsync`) via `IInpBillingDepositAdapter`, menolak status `Cleared` jika ada kekurangan tagihan, ada kelebihan deposit yang belum direfund, atau jika Billing offline. |
| 🟡 `FE-RWI-059` | Tertahan Frontend | Backend `BE-BKC-039` (`GET /deposit-policies`) sudah tersedia di backend; pekerjaan frontend menunggu alokasi task UI. |
| ⛔ `FE-RWI-060` | ⛔ **DIBATALKAN** | **Dibatalkan 22 Oktober 2026 (`RWI-DEC-354`).** Pengisian saldo deposit sesungguhnya, penentuan metode pembayaran (`PaymentMethodId`), dan pemanggilan `POST /patient-funds/deposits/{encounterId}/top-ups` berada di Kasir (Billing Management). Di admisi hanya mengisi nominal deposit saja tanpa metode pembayaran, dan implementasinya saat ini sudah selesai serta benar (`FE-RWI-058`). |
| 🟡 `FE-RWI-061` | Tertahan Frontend | Backend `BE-BKC-040` (`GET /deposits/episodes/{episodeId}`) sudah tersedia di backend; pekerjaan frontend menunggu alokasi task UI. |


#### ⛔ `FE-RWI-060` — DIBATALKAN (22 Oktober 2026 — `RWI-DEC-354`)

Task ini **resmi dibatalkan** atas arahan langsung pemilik (Muhammad Hamzah).

| Hal | Ketetapan Bisnis Otoritatif |
|---|---|
| Kepemilikan Transaksi Deposit | **Kasir (Billing Management)**, bukan Admisi Rawat Inap. |
| Alur Admisi Rawat Inap | **Hanya mengisi nominal rencana deposit saja** (`FE-RWI-058` / `FE-INP-20`). Alur ini tidak mengenal dan tidak memerlukan metode bayar (`PaymentMethodId`). |
| Status Kode Saat Ini | Pengisian nominal rencana deposit di admisi **sudah benar dan tidak perlu diubah lagi**. |
| Route `POST /top-ups` | Tidak dipanggil dari wizard pendaftaran rawat inap; mutasi saldo dan kwitansi dilakukan di kasir saat keluarga pasien melakukan pembayaran fisik. |

---

## 5. Kelompok B — butir menggantung pada task yang sudah ✅

Task-nya sudah selesai dan tidak perlu dikerjakan ulang. Yang tersisa bukan kode.

### 5.1 Satu migration belum diterapkan — perlu perhatian

| Hal | Isinya |
|---|---|
| Migration | `20260909065125_AddInpatientEpisodeContextToPatientDiagnosis`, milik `BE-RWI-068` |
| Keadaan | **Belum diterapkan ke database mana pun** |
| Kenapa perlu perhatian | **Langkah mundurnya tidak simetris.** Mengembalikan `ConsultationId` menjadi `NOT NULL` akan **gagal** begitu ada satu baris diagnosis rawat inap tanpa nomor konsultasi |
| Urutan mundur yang benar | Kembalikan validasi lebih dulu, tangani baris yang telanjur ada bersama pemilik klinis, baru turunkan migration |
| Wewenang | Penerapannya menuntut **wewenang terpisah**. Aturan bagian 2 tidak mencakup ini |

### 5.2 Dua repository wajib rilis satu gelombang

| Task | Keadaan |
|---|---|
| `BE-RWI-073` | ✅ selesai, **masih lokal, belum di-commit** |
| `FE-RWI-062` | ✅ selesai, **masih lokal, belum di-commit** |

| Bila | Akibatnya |
|---|---|
| Backend turun lebih dulu | Tiga berkas test frontend gagal, karena menguji kode penolakan yang tidak pernah terbit lagi |
| Frontend turun lebih dulu | Petugas menerima penolakan tanpa pesan yang dapat dibaca |

Commit dan push dijalankan pemilik, bukan agent.

### 5.3 Butir verifikasi yang tercatat `NOT RUN`

Seluruhnya tercatat apa adanya pada laporan task masing-masing, sesuai aturan bagian 2.
**Tidak satu pun menahan status task.**

| Sumber | Isinya |
|---|---|
| Backend, seluruh task Gelombang 1A dan `BE-RWI-068` | `dotnet build` dan `dotnet test` `NOT RUN` atas instruksi pemilik |
| Backend, uji integrasi | `NOT RUN` — folder `Tests/` sudah tidak ada, dan `rules/backend/TEST_POLICY.md` melarang membuatnya kembali tanpa permintaan pemilik. Digantikan penelusuran source beserta contoh berangka |
| Frontend, bukti peramban | `NOT RUN` — repository **tidak memiliki `playwright.config.*`**, sehingga `npm run test:e2e` tidak dapat dijalankan |
| Frontend, bukti runtime | Tertahan `RWI-UI-GAP-007` |
| `BE-RWI-077` | Pembatalan tanda vital final `NOT RUN` — `ClinicalDocumentKind.VitalSign` belum ditegakkan mesin keutuhan dokumen. Dilacak `V2-UNK-01`, pemiliknya `MedicalRecordManagement` |
| `BE-RWI-078` | `AC-KEP-050` belum terpenuhi — peran nyata pada skenario negatif belum dapat dicatat |

### 5.4 Satu selisih dokumen yang perlu dibetulkan pemilik

| Dokumen | Yang perlu dibetulkan |
|---|---|
| Kartu `FE-RWI-058` pada `episode-rawat-inap/roadmap/frontend-roadmap.md` | Kriteria 1 menulis jalur pasien lama **sembilan** langkah dengan Deposit di urutan **keempat**. Jalur itu **sudah** sembilan langkah sebelum Deposit ada; angkanya disusun ketika masih delapan. Yang benar: **sepuluh langkah, Deposit di urutan kelima** |
| `episode-rawat-inap/03-frontend-architecture.md` bagian 3A.3 | Selisih yang sama. Barisnya menulis rentang "4–9" untuk **tujuh** butir |

Memaksakan angka sembilan menuntut menghapus satu langkah yang benar-benar dipakai.

---

## 6. Gerbang lingkungan dan approval yang masih terbuka

| ID | Isinya | Menahan apa | Cara menutupnya |
|---|---|---|---|
| `RWI-UI-GAP-007` | Data master rawat inap pada lingkungan target belum layak: pengaturan `DEFAULT` tidak ada, butir administrasi kosong, papan menunjukkan nol bed | Pembuktian runtime seluruh layar; `FE-RWI-035` kriteria 1 | **Menyiapkan data**, bukan menulis kode. Menanam data tiruan di frontend **dilarang** |
| `RWI-OQ-053` | Pemilik `BillingManagement` belum bernama | `BE-BKC-039`, `BE-BKC-040`, dan karenanya lima task pada bagian 4.2 | Menunjuk pemiliknya |
| `V2-UNK-01` | Sembilan jenis dokumen belum ditegakkan mesin keutuhan | Kelengkapan jalur pengganti `BE-RWI-077` | Pemilik `MedicalRecordManagement` |
| Approval `FE-INP-20` | Skema langkah Deposit ditulis 12 September 2026 pada `episode-rawat-inap/05-skema-tampilan.md` bagian 3.5A, revision `0.5`, status `draft` | Tidak menahan `FE-RWI-058` yang sudah ✅, tetapi bentuk layarnya **belum terkunci** | Persetujuan pemilik |
| Katalog hak akses per butir | Frontend belum menerimanya dari backend | `FE-RWI-035` kriteria 5; `FE-RWI-003` kriteria 2 | Kontrak baru dari backend |

---

## 7. Kemampuan yang belum menjadi task sama sekali

Sembilan kemampuan yang ditemukan audit 11 September 2026. **Belum boleh diturunkan menjadi
task**, jadi jangan mencari nomornya di roadmap — nomornya memang belum ada.

| Kemampuan | Status audit | Catatan |
|---|---|---|
| Medication Administration Record | `Missing` | Nol model, nol service, nol endpoint |
| Observasi transfusi dan reaksi | `Missing` | Yang ada hanya jenis consent dan alasan bank darah |
| Sliding scale | `Missing` | Menuntut protokol berversi yang juga belum ada |
| Handover antar shift keperawatan | `Missing` | Yang ada hanya serah terima kasir |
| Clinical handover transfer antarunit | `Missing` | Transfer bed atomik sudah ada, artefak klinisnya belum |
| Adverse drug reaction | `Missing` | Bergantung pada MAR |
| Intake, output, drain, dan WSD | `Reuse with adapter` | Pola sudah berjalan di IGD dan layak jadi rujukan bentuk |
| Lima cara keluar | `Extend` | Enum baru punya tiga nilai; `Death` dan `Absconded` menunggu sign-off klinis |
| Bukti consent per episode | `Extend` | Yang kurang hanya versi template dan penegakan keutuhan |

**Langkah yang tepat untuk kelompok ini bukan perencanaan, melainkan
`requirement-completeness-gate` lebih dulu.** Sepuluh butir `OPEN-MVP-001` sampai
`OPEN-MVP-010` pada PRD V2 masih terbuka, dan `RWI-DEC-097` **tidak** membukanya.

**Kelayakan keuangan berdiri sendiri.** Ia tetap `P0`, berstatus `external dependency`, dan
**bukan** `P1`. Sesuai `RWI-DEC-102`, selama kontrak authoritative Billing belum ada:
`AC-MVP-021` sampai `AC-MVP-023` belum boleh dinyatakan lulus, penandaan manual **tidak sah**
sebagai dasar penutupan episode normal di produksi, dan kesiapan MVP penuh belum boleh
dinyatakan selesai.

---

## 8. Catatan operasional

| Hal | Ketentuannya |
|---|---|
| **Task backend** | **Tanpa build. Implementasi kode selesai berarti task ✅.** Lihat bagian 2 |
| Butir yang tidak dijalankan | Ditulis **`NOT RUN` apa adanya** pada laporan, **bukan dihilangkan** |
| Skill yang dipakai | `build-module-backend` untuk task backend; `build-module-frontend` untuk task frontend |
| Wewenang | Setiap pemanggilan butuh **task ID** dan **wewenang tulis yang disebut eksplisit** |
| Repo frontend | Sesi terpisah di `QuilvianSystemFrontendDev`. Wewenang tulis backend tidak berlaku di sana |
| Penerapan migration | **Menuntut wewenang terpisah.** Jangan menerapkan ke environment mana pun tanpa itu |
| Git | Commit dan push dijalankan pemilik, bukan agent |
| Penutupan task | Selesai satu task berarti laporan ditulis **dan** register pada roadmap ikut diperbarui |

---

## 9. Nomor yang tidak boleh dipakai ulang

Nomor task yang sudah dipensiunkan: `BE-RWI-041` sampai `BE-RWI-043`, dan `FE-RWI-042` sampai
`FE-RWI-045` pada sisi `episode-rawat-inap`.

~~**ID bebas berikutnya: `BE-RWI-079` dan `FE-RWI-063`.**~~ — **basi sejak 16 September 2026.**
Keduanya sudah dipakai fase `RLN-PH-07`, yang menurunkan **48 task backend** `BE-RWI-079` s.d.
`BE-RWI-126` dan **32 task frontend** `FE-RWI-063` s.d. `FE-RWI-094` ke berkas roadmap **baru**
`roadmap/backend-roadmap-v2.md` dan `roadmap/frontend-roadmap-v2.md` pada ketiga sub-modul.

**ID bebas berikutnya per 16 September 2026: `BE-RWI-127` dan `FE-RWI-095`.** Sebelum menambah
task, baca **enam** berkas roadmap — tiga lama dan tiga baru per lapisan — beserta pohon
`task/report/`. Rinciannya pada [`blueprint-manifest.md`](./blueprint-manifest.md) bagian 0-B.8.
