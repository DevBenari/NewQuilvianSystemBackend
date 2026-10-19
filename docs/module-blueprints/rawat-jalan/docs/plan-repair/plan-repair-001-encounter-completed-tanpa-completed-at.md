# PLAN-REPAIR-001 — Isi CompletedAt saat endpoint admin mengubah status kunjungan jadi Completed

```yaml
plan_id: PLAN-REPAIR-RJDOC-001
issue: ../issue/issue-001-encounter-completed-tanpa-completed-at.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-10-16"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "e70f5abf (Yasmina)"
basis_source_frontend: "Tidak relevan - temuan backend murni"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-RJDOC-001-01` | `ISS-RJDOC-001-01` | Isi `CompletedAt` di `UpdateEncounterStatus` saat status diubah jadi `Completed` | BE | 1 | — | BELUM DIKERJAKAN | — |

**Ringkasan: 0 dari 1 perbaikan selesai.** Menunggu persetujuan pemilik modul rawat-jalan atas
K-01 pada `issue-001` sebelum didaftarkan ke roadmap.

## 2. Solusi terpilih per temuan

### ISS-RJDOC-001-01 — Endpoint UpdateEncounterStatus mengizinkan status Completed tanpa mengisi CompletedAt

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Di `PatientEncounterController.cs` method `UpdateEncounterStatus`, sebelum `SaveChangesAsync()`, tambahkan: bila `request.EncounterStatus == EncounterStatus.Completed` dan `entity.CompletedAt` belum terisi, set `entity.CompletedAt = now` (variabel waktu yang sudah ada di method ini) | Perubahan satu baris kondisional, konsisten dengan pola `BillingFinalizationService.cs:632-633`; tidak mengubah kontrak request/response endpoint; tidak memutus pemanggil existing (termasuk `EmergencyVisitService`) | Endpoint tetap menjadi jalur ganda untuk mencapai `Completed` (bersama jalur pelunasan billing) — kerapian arsitektur tidak sepenuhnya diselesaikan, hanya efek sampingnya |
| B | Tolak `request.EncounterStatus == EncounterStatus.Completed` di endpoint ini (kembalikan 422/403), paksa seluruh transisi ke `Completed` lewat jalur resmi (pelunasan billing atau endpoint khusus baru) | Satu sumber kebenaran untuk transisi ke `Completed` | Breaking change — perlu audit semua pemanggil existing yang mengandalkan endpoint ini untuk status `Completed` (terbukti dipakai modul IGD, `EmergencyVisitService.cs:636-637`, kemungkinan juga modul lain); butuh task terpisah untuk menyediakan jalur pengganti bagi pemanggil yang sah; radius dampak jauh lebih besar dan berisiko regresi lintas modul |
| C | Biarkan apa adanya, cukup catat sebagai keterbatasan diketahui | Tidak ada perubahan kode/risiko regresi | Tidak menyelesaikan masalah — data `CompletedAt` kosong pada kunjungan `Completed` akan terus terjadi setiap kali endpoint ini dipakai untuk itu |

**Solusi terpilih: Opsi A**, tunduk pada persetujuan pemilik modul atas K-01 (lihat bagian 6) —
ini adalah pilihan (a) yang direkomendasikan pada K-01.

1. Radius dampak paling kecil — satu baris kondisional, tidak mengubah kontrak endpoint.
2. Tidak memutus pemanggil existing lintas modul (IGD dan kemungkinan lainnya) yang sudah
   bergantung pada endpoint ini untuk menandai `Completed`.
3. Konsisten dengan pola yang sudah terbukti benar di jalur pelunasan billing.

**Kenapa bukan opsi lain.** Opsi B berpotensi memecah pemanggil existing dan butuh audit lintas
modul yang jauh lebih besar daripada temuan ini — kalau pemilik modul memilih arah ini, perlu
task/issue terpisah untuk audit pemanggil dan menyediakan jalur pengganti, bukan bagian dari
`FIX-RJDOC-001-01`. Opsi C tidak menyelesaikan masalah yang dilaporkan.

## 3. Skema tampilan sebelum → sesudah

Tidak ada perubahan tampilan — ini murni perbaikan backend pada satu field yang tidak pernah
ditampilkan langsung ke pengguna di endpoint ini. `CompletedAt` sudah tampil di tempat lain
(mis. kartu konteks kunjungan pada layar Billing) begitu nilainya terisi dengan benar.

## 4. Rincian perbaikan

### FIX-RJDOC-001-01 — Isi CompletedAt saat UpdateEncounterStatus mengubah status jadi Completed

| | |
| --- | --- |
| **Menutup** | `ISS-RJDOC-001-01` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` — method `UpdateEncounterStatus` (sekitar baris 1990-2035, assignment status di baris 2003) |
| **Radius dampak** | Seluruh pemanggil endpoint `PATCH {id}/status` dan `PATCH admin/{id}/status` yang mengirim `EncounterStatus = Completed` — dikonfirmasi dipakai modul Emergency/IGD (`EmergencyVisitService.cs:636-637`). Perlu dicek pemanggil lain sebelum implementasi (lihat langkah 1) |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Sebelum implementasi, `grep` seluruh pemanggil endpoint ini (frontend dan modul backend lain)
   yang mengirim `EncounterStatus: Completed`, untuk memastikan tidak ada asumsi tersembunyi bahwa
   `CompletedAt` sengaja dibiarkan kosong oleh jalur ini.
2. Pada method `UpdateEncounterStatus`, setelah `entity.EncounterStatus = request.EncounterStatus;`
   dan sebelum `SaveChangesAsync()`, tambahkan kondisi: bila `request.EncounterStatus ==
   EncounterStatus.Completed` dan `entity.CompletedAt` belum terisi (`is null`), set
   `entity.CompletedAt = now` (pakai variabel `now`/`DateTime.UtcNow` yang pola penamaannya
   sudah dipakai method ini — ikuti konvensi existing, jangan membuat variabel waktu baru bila
   sudah ada satu di scope method).
3. Jangan menimpa `CompletedAt` bila sudah terisi (idempotent terhadap pengiriman ulang status
   `Completed` yang sama) — hanya isi bila sebelumnya `null`.
4. Jangan mengosongkan `CompletedAt` bila status diubah MENJAUH dari `Completed` ke status lain
   lewat endpoint yang sama kecuali ada bukti eksplisit itu memang perilaku yang diinginkan — di
   luar cakupan `ISS-RJDOC-001-01` (yang hanya soal kekosongan `CompletedAt` saat masuk ke
   `Completed`); catat sebagai pertanyaan terpisah ke pemilik bila ditemukan kasus itu saat
   implementasi.

**Acceptance criteria.**

1. Memanggil `PATCH {id}/status` dengan `EncounterStatus = Completed` pada kunjungan yang
   `CompletedAt`-nya masih kosong menghasilkan `CompletedAt` terisi dengan waktu saat request
   diproses.
2. Memanggil endpoint yang sama dua kali berturut-turut dengan `EncounterStatus = Completed` tidak
   mengubah `CompletedAt` pada pemanggilan kedua (tetap nilai dari pemanggilan pertama).
3. Pemanggilan endpoint dengan `EncounterStatus` selain `Completed` tidak mengubah `CompletedAt`
   sama sekali (perilaku existing, regresi nol).
4. Jalur pelunasan billing (`BillingFinalizationService`) tidak terpengaruh — tetap mengisi
   `CompletedAt` lewat jalurnya sendiri seperti sebelumnya.

**Verifikasi.** `dotnet build` pada project yang terdampak (tidak dijalankan otomatis — jalankan
manual sesuai instruksi kerja repository ini); test terarah bila project test untuk
`PatientEncounterController` tersedia; verifikasi manual lewat Swagger/Postman pada endpoint
`PATCH {id}/status` dengan payload `EncounterStatus: Completed` pada data uji.

**Risiko.** Bila ada pemanggil lain yang diam-diam bergantung pada `CompletedAt` tetap kosong
setelah lewat endpoint ini (tidak terlihat dari pembacaan source saat ini), perbaikan ini bisa
mengubah perilaku yang diasumsikan konsumen tersebut — karena itu langkah 1 (audit pemanggil)
wajib dilakukan sebelum implementasi, bukan opsional.

## 5. Urutan pengerjaan

```text
K-01 (keputusan pemilik modul rawat-jalan)
└── FIX-RJDOC-001-01
```

## 6. Keputusan pemilik yang dibutuhkan

Disalin dari `issue-001` bagian 7:

| No | Keputusan | Pilihan | Rekomendasi diteruskan dari sesi diagnosis Billing (K-03 pada `issue-003` modul Billing) | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Apakah `CompletedAt` perlu diisi di endpoint `UpdateEncounterStatus` setiap kali status diubah jadi `Completed`? | (a) Ya, perbaiki endpoint / (b) Tidak, tolak transisi ke `Completed` lewat endpoint ini | **(a)** | `FIX-RJDOC-001-01` |

Rekomendasi (a) berasal dari sesi diagnosis modul Billing (16-10-2026), diputuskan oleh pemilik
yang menangani sesi itu — **bukan** pemilik modul rawat-jalan. Dicatat sebagai rekomendasi kuat,
bukan keputusan final; pemilik modul ini tetap perlu mengonfirmasi atau memilih opsi lain sebelum
`FIX-RJDOC-001-01` didaftarkan ke roadmap.

## 7. Dokumen hulu yang ikut direvisi

Tidak ada. `FIX-RJDOC-001-01` berjenis `BUG`, bukan `DESIGN_CHANGE`.

## 8. Verifikasi menyeluruh

Setelah `FIX-RJDOC-001-01` selesai:

1. Ubah status kunjungan uji menjadi `Completed` lewat endpoint `PATCH {id}/status` dan pastikan
   `CompletedAt` terisi.
2. Ubah lagi dengan payload yang sama, pastikan `CompletedAt` tidak berubah dari pengisian pertama.
3. Jalankan alur pelunasan billing pada kunjungan lain, pastikan `CompletedAt` tetap terisi benar
   lewat jalur itu (regresi nol pada `BillingFinalizationService`).
4. Jalankan alur IGD yang memanggil endpoint ini (`EmergencyVisitService`) dan pastikan tidak ada
   pemanggilan yang gagal akibat perubahan ini.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Rencana dibuat, 1 perbaikan, menunggu persetujuan pemilik modul rawat-jalan (K-01) | `diagnose-module-issue` |
