# Laporan Perubahan Backend — `BE-OPT-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-031` |
| Judul | Tarif pemeriksaan refraksi sekali saat final |
| Slice | `MVP-2` — `EPIC OPT-02`, `OPT-08` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-031` |
| Trace | `FR-OPT-021`; `OPT-DEC-042`, `047`; `INT-OPT-04`; `OPT-AC-024`; `UAT-03` |
| Contract version | `OPT-INT-v1` `INT-OPT-04` — `approved` sisi Optik |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain. Memakai jalur Billing `BE-OPT-030` (Yasmina, `OPT-DEC-051`) |
| Dependency | `BE-OPT-011` ✅, `BE-OPT-030` ✅ |
| Klasifikasi | `MEDIUM` — satu service baru, perluasan service pemeriksaan, dua ruas jawaban, DI |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Services,DTOs}/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`, `033` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk `OptBillingGateway`; `TOUCHED LEGACY` hanya `Program.cs` (DI). Service dan DTO pemeriksaan adalah kode Optik baru dari `BE-OPT-011` |
| QBE yang berlaku | `QBE-ENT-003` (tanpa harga di Optik), `QBE-SVC-001`, `QBE-LOG-001` (log memuat nomor pemeriksaan, tanpa data pasien), `QBE-DTO-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan (kolom `BillingFactEmittedAt` sudah ada sejak `BE-OPT-002`) · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Pemeriksaan refraksi yang disimpan final belum menghasilkan tagihan. Pasien yang hanya diperiksa
tanpa memesan kacamata tidak tertagih sama sekali, padahal `OPT-DEC-042` menetapkan tarif pemeriksaan
ditagih saat final, terlepas dari jadi atau tidaknya pesanan.

---

## 2. Proses bisnis

1. **Rina (RO) menyimpan final.** **Contoh (`UAT-03`):** visus OS Ibu Sari (samaran) 6/15 disimpan final pukul 09.20. Sesudah pemeriksaan tersimpan, Optik menyerahkan satu fakta tarif pemeriksaan. Billing membaca tarif tindakan "Pemeriksaan Refraksi" (Rp 75.000) dan menambah satu baris di tagihan kunjungan.
2. **Revisi dua kali.** Pukul 09.40 Rina merevisi menjadi 6/12 ("salah ketik"), lalu sekali lagi. Riwayat berisi tiga versi, tetapi tagihan tetap **satu** baris Rp 75.000 (`OPT-AC-024`, `OPT-DEC-047`).
3. **Billing sedang tidak menjawab.** Pemeriksaan tetap final dan tetap bisa dilanjutkan ke resep. Faktanya tersimpan dan dikirim ulang otomatis oleh pekerja latar producer, tanpa fakta kedua.
4. **Tindakan "Pemeriksaan Refraksi" belum diatur Master Data.** Final tetap berhasil, tetapi jawaban membawa pesan "Tarif pemeriksaan belum dapat ditagihkan karena tindakan Pemeriksaan Refraksi belum diatur. Hubungi bagian Master Data." Begitu tindakan dan tarifnya diisi, revisi berikutnya menyerahkan tarif yang tertinggal, tetap sekali.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Billing gagal saat menerima fakta | Pemeriksaan `Final`; fakta `OutcomeUnknown`/`Pending`; `BillingFactEmittedAt` terisi; kirim ulang producer memasukkan tarif |
| Kode tindakan kosong di konfigurasi, atau tindakan tidak aktif | Tidak ada fakta; `BillingFactEmittedAt` kosong; `BillingMessage` terisi. Optik tidak menebak tindakan lain |
| Basis data tidak terjangkau saat penyerahan | Pemeriksaan tetap final; `BillingMessage` "Tarif pemeriksaan belum masuk tagihan karena layanan tagihan sedang tidak dapat diakses."; dicoba lagi saat revisi |
| Draf atau pemeriksaan yang dibatalkan | Tidak ditagih |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OptRefractionExamService` (final, revisi, simpan, proyeksi), `OptRefractionExam` dan configuration (`Version` adalah token konkurensi), `ClinicalMilestoneFactProducer` (`EmitAsync`, `DecideNextStep`, `DispatchAsync`, `ApplyDispatchOutcomeAsync`, `RedispatchAsync`), `ClinicalFactEmissionResult.IsClinicallySafe`, `ClinicalFactDispatchWorker`, `LabOrderService` (pola penerbitan sesudah commit), `OptPractitionerGuard` (pola konfigurasi kode), `MstProcedure`, `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Services/OptBillingGateway.cs` | **Baru.** `EmitExamChargeAsync`: mencari tindakan dari konfigurasi, menyusun fakta deterministik (`SourceAggregateId` = pemeriksaan, `SourceItemId` = tindakan, waktu = waktu final, jumlah 1, `RuleSnapshot` `ExamFinalized`), memanggil producer di luar transaksi, mengembalikan apakah fakta sudah tercatat |
| `Areas/HealthServices/OpticalManagement/Services/OptRefractionExamService.cs` | **Diperbarui.** Parameter opsional `OptBillingGateway` di akhir konstruktor. `BillAfterCommitAsync` sesudah final dan revisi: menyerahkan tarif bila belum tercatat, lalu mengisi `BillingFactEmittedAt` lewat `ExecuteUpdate` bersyarat (`IS NULL`) tanpa menaikkan `Version`. Aman bila producer membersihkan change tracker konteks bersama |
| `Areas/HealthServices/OpticalManagement/DTOs/OptRefractionExamDtos.cs` | **Diperbarui.** `BillingFactEmittedAt`, `BillingMessage` pada `OptRefractionExamResponse` |
| `Program.cs` | **Diperbarui.** `AddScoped<OptBillingGateway>()` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be031ExamBillingTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 7 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint tidak berubah. Dua ruas tambahan pada jawaban pemeriksaan, di luar kontrak dan tanpa mengubahnya |
| Kontrak integrasi | `INT-OPT-04` kolom pemeriksaan terpenuhi: `Optical`/`OpticalExamCharge`, `SourceItemId` = tindakan, `1`/`Pemeriksaan`, `ExamFinalized`, sesudah commit, revisi tidak menerbitkan, `Pending` dikirim ulang producer |
| Konfigurasi | **Baru:** `OpticalManagement:Billing:RefractionExamProcedureCode`. Belum diisi di `appsettings*.json`; tanpa nilai, tidak ada tarif pemeriksaan yang ditagihkan |
| Database | Tanpa migration |
| Keamanan/Auth | Tidak ada izin baru. Fakta diterbitkan atas nama pemeriksa yang menyimpan final |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Perubahan bentuk jawaban:

| Method | Path | Perubahan |
| --- | --- | --- |
| `POST` | `api/v1/health-services/optical-management/refraction-exams/{id}/finalize` | Jawaban memuat `billingFactEmittedAt` dan, bila tarif belum tertagih, `billingMessage` |
| `POST` | `…/refraction-exams/{id}/revisions` | Sama; tarif yang tertunda diserahkan pada revisi |
| `GET` | `…/refraction-exams`, `…/{id}` | Memuat `billingFactEmittedAt` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 173 lulus, 0 gagal (7 baru) | `PASS` | Keluaran `dotnet test` (172 pada putaran penuh + uji DI pada putaran sempit) |
| Kriteria (1) / `OPT-AC-024` / `UAT-03`: final lalu revisi dua kali | Satu item invoice `OPTICAL`/`COMPLETED` Rp 75.000; satu fakta versi 1 `Dispatched`; `ExpectedVersion` dari jawaban final tetap sah untuk revisi | `PASS` | Uji `Kriteria_1_…` dengan rantai producer → folio → jembatan → `BillingInvoiceService` |
| Kriteria (2): Billing tidak menjawab | Pemeriksaan `Final`, `Version` tidak berubah; fakta `Pending`/`OutcomeUnknown`; `RedispatchAsync` memasukkan tarif tanpa fakta kedua | `PASS` | Uji dengan interceptor yang menggagalkan penyimpanan folio |
| Tindakan belum diatur, lalu dilengkapi | Final berhasil dengan `BillingMessage`; revisi berikutnya menagih; revisi ketiga tidak menambah apa pun | `PASS` | Uji |
| Kode tindakan kosong | Tidak ada fakta | `PASS` | Uji |
| Draf dibatalkan | Tidak ada fakta | `PASS` | Uji |
| Gateway tersuntik lewat DI dengan registrasi seperti `Program.cs` | Ruas `_billing` terisi | `PASS` | Uji `DI_menyuntikkan_…` |
| Uji mutasi: penjaga "sudah tertagih" dibuang + waktu fakta diganti jam sekarang | 2 uji gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test`. Mutasi pertama (mengacak `CorrelationId`) lolos karena ruas itu tidak ikut sidik jari producer; komentar kode diperbaiki |
| QBE checker `-Mode Strict -Path` atas 4 berkas | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena 3 aturan; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:** pekerja latar `ClinicalFactDispatchWorker` sendiri (yang diuji adalah `RedispatchAsync` yang ia panggil); uji di PostgreSQL atau backend yang menyala.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-024` | Terpenuhi di harness | Uji `Kriteria_1_…` + uji mutasi |
| (2) Billing tidak menjawab → pemeriksaan tetap final, fakta `Pending` dikirim ulang | Terpenuhi di harness | Uji `Kriteria_2_…` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perlu tindakan sebelum dipakai | (a) Master Data membuat tindakan "Pemeriksaan Refraksi" dan tarifnya. (b) Kode tindakannya diisi di `OpticalManagement:Billing:RefractionExamProcedureCode`. Tanpa keduanya final tetap berjalan, tetapi tarif tidak masuk tagihan |
| Keputusan pelaksanaan yang perlu dikonfirmasi | Desain tidak menyebut dari mana tindakan diambil. Saya memakai kode konfigurasi mengikuti preseden `OPT-DEC-058`, bukan kolom baru di `OptSetting` (itu butuh migration dan perubahan kontrak). Dicatat di tabel penyegaran `00-interview-decisions.md` |
| Risiko tersisa | Pemeriksaan yang gagal diserahkan **sebelum** producer mencatat fakta (basis data mati atau tindakan belum diatur) baru dicoba lagi saat direvisi. Bila tidak pernah direvisi, tarifnya tertinggal. Pilihan penutupnya: tombol "kirim ulang tarif" di `BE-OPT-053`, atau pekerja latar kecil. Layar bisa memakai `BillingMessage` dan `BillingFactEmittedAt` kosong sebagai tanda |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Status Git | Tambahan task ini: `??` `OptBillingGateway.cs` dan laporan ini; `M` `OptRefractionExamService.cs`, `OptRefractionExamDtos.cs`, `Program.cs`, roadmap, traceability, `00-interview-decisions.md`. Berkas uji diabaikan Git |
| Langkah berikutnya | `BE-OPT-032` (pesanan: draft, ubah, tagihkan per item, batal awal, status bayar), lalu `BE-OPT-034`. `BE-OPT-051` (Medical Fee) kini juga terbuka karena bergantung pada `031` |
