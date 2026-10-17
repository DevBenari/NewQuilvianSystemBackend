# ISSUE-RJ-DOC-001 — Endpoint ubah status kunjungan bisa menjadikan status Selesai tanpa mengisi waktu selesainya

```yaml
issue_id: ISSUE-RJ-DOC-001
module_id: rawat-jalan
submodule: null  # blueprint berbentuk SINGLE; <blueprint-root> = folder modul
layar: "Tidak terikat satu layar UI tertentu - endpoint backend generik PATCH api/v1/.../patient-encounters/{id}/status dan alias admin/{id}/status (PatientEncounterController.UpdateEncounterStatus), dipakai berbagai layar admin/petugas untuk mengubah status kunjungan secara manual"
sumber_laporan: "Diteruskan dari modul Billing (blueprint billing-kasir): ISS-BKC-003-02 pada issue-003-kunjungan-pasien-tampil-identik.md, ditemukan agent saat menelusuri pertanyaan pemilik produk tentang kunjungan berstatus Completed dengan CompletedAt kosong. Keputusan K-03 pada issue tersebut (16-10-2026, oleh pemilik yang menangani sesi diagnosis - bukan pemilik modul ini): 'Ya - perbaiki endpoint agar mengisi CompletedAt'. Keputusan itu dicatat di sini sebagai rekomendasi kuat, BUKAN sebagai persetujuan final modul ini - lihat bagian 6. 1 butir, 0 lampiran baru (bukti sepenuhnya dari pembacaan source, lihat butir 1)"
tanggal_issue: "2026-10-16"
status: TERBUKA
keparahan_tertinggi: Medium
source_sha_backend: "e70f5abf (Yasmina)"
source_sha_frontend: "Tidak relevan - temuan backend murni, tidak ada layar frontend yang terdampak langsung oleh perbaikan ini"
rencana_perbaikan: ../plan-repair/plan-repair-001-encounter-completed-tanpa-completed-at.md
```

## 1. Ringkasan

Endpoint `PATCH /patient-encounters/{id}/status` (dan aliasnya `PATCH /admin/{id}/status`) pada
`PatientEncounterController` mengizinkan status sebuah `RegPatientEncounter` diubah menjadi
`Completed` tanpa pernah mengisi kolom `CompletedAt`. Ini berbeda dari jalur resmi penyelesaian
kunjungan lewat pelunasan tagihan (dikerjakan modul Billing), yang selalu mengisi `CompletedAt`
bersamaan dengan mengubah status. Akibatnya, kunjungan yang statusnya diubah lewat endpoint ini
tampak "Selesai" tanpa jejak kapan sebenarnya selesai — ditemukan satu baris data nyata dengan
kondisi ini saat modul Billing menelusuri masalah lain. Ini murni temuan dari pembacaan source;
belum ada laporan pengguna langsung ke modul ini.

## 2. Laporan asli

| No. laporan | Keluhan | Lampiran |
| ---: | --- | --- |
| 1 | Diteruskan dari `ISS-BKC-003-02` (modul Billing): "Encounter berstatus Completed tapi CompletedAt kosong" — pemilik produk (dalam sesi diagnosis dropdown kunjungan Billing) bertanya "Seharusnya ketika pasien membayar dan hasilnya lunas, encounter status menjadi selesai dan nilai completedAt langsung terisi otomatis. Mengapa bisa [CompletedAt kosong]?", disertai capture data `RegPatientEncounter` nyata (`EncounterNumber` disamarkan, `EncounterStatus=9`/`Completed`, `CompletedAt=NULL`) | Tidak ada lampiran baru di sini; capture data asli ada pada `issue-003` modul Billing (`ISS-BKC-003-01` bagian verifikasi P-01) |

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | ID perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-RJDOC-001-01` | 1 | Endpoint `UpdateEncounterStatus` mengizinkan status `Completed` tanpa mengisi `CompletedAt` | `BUG` | Backend | Medium | `SUDAH-VERIFIKASI` di source; `DARI-CAPTURE` untuk baris data spesifik (dari modul Billing) | `FIX-RJDOC-001-01` |

## 4. Rincian per temuan

### ISS-RJDOC-001-01 — Endpoint UpdateEncounterStatus mengizinkan status Completed tanpa mengisi CompletedAt

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source; `DARI-CAPTURE` untuk baris data spesifik |
| **Perbaikan** | `FIX-RJDOC-001-01` |

**Apa yang terjadi.** Saat status sebuah kunjungan diubah menjadi "Selesai" lewat endpoint
pengubah status umum (dipakai petugas/admin untuk koreksi status manual), sistem tidak mencatat
kapan kunjungan itu benar-benar selesai. Kolom yang seharusnya menyimpan waktu selesai tetap
kosong, padahal statusnya sudah menunjukkan "Selesai".

**Kenapa terjadi.** Ditemukan dua jalur kode yang bisa mengubah status jadi `Completed`, dan hanya
satu yang benar:

1. Jalur resmi pelunasan billing —
   `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs:632-633`
   (method `TryCompleteEncounterForInvoiceAsync`, dipicu
   `BillingManagement/Billing/Services/BillingSettlementService.cs:779` setelah invoice lunas) —
   **mengisi `CompletedAt` dengan benar**, bersamaan dengan mengubah status:
   ```csharp
   encounter.EncounterStatus = EncounterStatus.Completed;
   encounter.CompletedAt = occurredAt.UtcDateTime;
   ```
2. Jalur admin generik — `PatientEncounterController.cs:2003`, method `UpdateEncounterStatus`
   (dipanggil endpoint `PATCH {id}/status` dan alias `PATCH admin/{id}/status`) — menerima
   `EncounterStatus` apa pun (termasuk `Completed`) dari pemanggil lalu langsung
   `entity.EncounterStatus = request.EncounterStatus;` dan `SaveChangesAsync()` **tanpa pernah
   mengisi `CompletedAt`** di method ini. Ada juga komentar pada modul lain
   (`Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs:636-637`)
   yang secara eksplisit menyebut endpoint inilah yang dipakai sebagai jalur paralel ke
   `Completed` — menandakan ini bukan jalur tersembunyi/tidak terpakai, tapi memang dipakai, hanya
   lupa mengisi `CompletedAt`.

Endpoint ini juga tidak mencatat audit log untuk perubahan status, sehingga tidak ada jejak siapa
yang mengubah status dan kapan untuk baris yang terdampak.

**Apakah ini menyimpang dari desain?** `BUG`. Tidak ditemukan dokumen desain yang secara eksplisit
menetapkan bahwa endpoint admin ini boleh mengubah status jadi `Completed` tanpa mengisi
`CompletedAt` — perilaku jalur resmi (`BillingFinalizationService`) justru membuktikan bahwa
`CompletedAt` memang seharusnya selalu terisi bersamaan dengan status `Completed`.

**Dampak nyata.** Satu kunjungan nyata ditemukan modul Billing dengan status `Completed` namun
`CompletedAt` kosong — kemungkinan besar diubah lewat endpoint ini, bukan lewat pelunasan tagihan
resmi. Dampaknya: laporan atau proses lain yang bergantung pada `CompletedAt` sebagai jejak waktu
penyelesaian kunjungan (pelaporan, rekonsiliasi, filter berbasis tanggal selesai) berpotensi
menampilkan kunjungan "selesai" tanpa waktu selesai yang valid, atau melewatkannya dari filter
rentang tanggal yang mengandalkan `CompletedAt`.

**Rekomendasi.** Isi `CompletedAt` di `PatientEncounterController.cs:2003` setiap kali
`EncounterStatus` diubah menjadi `Completed` lewat endpoint ini, konsisten dengan jalur pelunasan
billing. Rincian opsi di plan repair.

## 5. Tanya-jawab pelapor

Tidak ada pertanyaan eksplisit dari pemilik modul ini — isu ini diteruskan dari sesi diagnosis
modul lain (lihat bagian 2). Pertanyaan asli yang memicu temuan ini ("mengapa CompletedAt kosong
saat status Selesai") sudah dijawab pada `issue-003` modul Billing dan diringkas pada bagian 1 di
atas.

## 6. Temuan tambahan

Tidak ada.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada — bukti sudah lengkap dari pembacaan source dan satu baris capture
data yang diteruskan dari modul Billing.

**Untuk pemilik (modul rawat-jalan, pemegang `RegistrationManagement`/`PatientEncounterController`):**

| No | Keputusan | Pilihan | Rekomendasi diteruskan dari sesi diagnosis Billing (K-03) | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Apakah `CompletedAt` perlu diisi di endpoint `UpdateEncounterStatus` setiap kali status diubah jadi `Completed`? | (a) Ya, perbaiki endpoint agar mengisi `CompletedAt` / (b) Tidak — endpoint ini sebaiknya menolak transisi ke `Completed` sama sekali (paksa semua jalur ke "Selesai" lewat pelunasan billing atau mekanisme lain yang resmi) | **(a)** — perbaiki endpoint agar mengisi `CompletedAt`, konsisten dengan jalur pelunasan billing | `FIX-RJDOC-001-01` |

Pilihan (a) dan (b) di atas punya konsekuensi berbeda yang hanya bisa diputuskan pemilik modul
ini: (a) mempertahankan endpoint sebagai jalur sah untuk override status manual (dipakai juga
modul lain seperti IGD, lihat `EmergencyVisitService.cs:636-637`) — cukup tambahkan satu baris
pengisian field. (b) mengubah batas wewenang endpoint ini secara lebih mendasar — berisiko memecah
pemanggil existing (termasuk `EmergencyVisitService`) yang sudah bergantung pada endpoint ini
untuk menandai `Completed`, sehingga butuh audit pemanggil lebih dulu sebelum dipilih.

## 8. Catatan pola

Pola yang sama (dua jalur berbeda mengubah field status terkait, hanya satu yang konsisten mengisi
field pendampingnya) patut dicurigai muncul lagi di modul lain yang punya jalur "resmi" dan jalur
"admin override" terpisah untuk field status/timestamp berpasangan. Disarankan audit serupa pada
endpoint admin generik lain yang bisa mengubah status ke nilai terminal.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Issue dibuat, diteruskan dari `ISS-BKC-003-02` pada `issue-003` modul Billing | `diagnose-module-issue` |
