# Laporan Perubahan Backend — `BE-RWI-062`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-062` |
| Judul | Tagihan yang gagal tidak menghapus catatan klinis |
| Slice | Gelombang `KEP-MVP-3` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-021`, `FR-KEP-022`; PRD `CAP-014` aturan 5; `AC-CAP014-02`, `AC-CAP014-03`; `RWI-AC-176`; `INT-KEP-05`, `INT-KEP-06`; `VAL-KEP-06`, `VAL-KEP-07` |
| Contract version | API `0.3.0` `PATCH /{id}/finalize`, `POST /{id}/addendums`, `GET /{id}/addendums`, `GET /{id}/billing-dispatch`; integration `0.3.0` `INT-KEP-05`, `INT-KEP-06`; permission `NursingIntervention : Amend` |
| Dependency | `BE-RWI-061` — 🟡 sebagian. Yang dibutuhkan slice ini (tabel, kolom keadaan tagihan, dan endpoint pencatatan) **sudah mendarat penuh**; yang tertunda pada `BE-RWI-061` adalah uji PostgreSQL idempotency, dan itu tidak menyentuh satu pun kriteria task ini |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah > 3 (1), logika bisnis sedang (1), memakai kontrak yang sudah ada (1), **nol** entity dan **nol** migration (0), keamanan hak akses baru (1), workflow status baru (1). Total **5** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `6f7d81e0` pada branch `MHamzah` |
| Tanggal | 7 September 2026 |
| Status | **Selesai.** Keenam acceptance criteria terbukti. **Nol tabel baru, nol kolom baru; migration task ini kosong** |

---

## 1. Masalah yang diperbaiki

`BE-RWI-061` membuat tindakan keperawatan dapat dicatat, tetapi catatannya belum dapat
**difinalkan**, belum dapat **dikoreksi**, dan keadaan pengiriman tagihannya belum bergerak sama
sekali.

Yang paling berbahaya dari ketiganya adalah yang ketiga. Bila keadaan klinis dan keadaan tagihan
dijadikan satu, catatan klinis dapat hilang karena masalah keuangan.

**Contoh yang menjelaskan seluruh task ini.** Ns. Sari memasang infus pukul 02.00, saat sistem
Billing sedang mati.

| | Yang benar | Yang salah |
| --- | --- | --- |
| Catatan tindakan | Tersimpan, berkeadaan tercatat | Ikut gagal tersimpan |
| Penanda pengiriman | Gagal, dapat dicoba ulang | — |
| Pukul 08.00 | Ada bukti infus pernah dipasang | **Tidak ada bukti apa pun** |

Masalah kedua: catatan yang sudah final tidak boleh disunting diam-diam, tetapi juga tidak boleh
menjadi mustahil dibetulkan. Perawat yang salah menulis lokasi infus harus punya jalan membetulkannya
**tanpa** mengubah isi aslinya.

---

## 2. Proses bisnis

**Tujuan.** Ketika pengiriman tagihan ke Billing gagal, tindakan keperawatannya tetap tersimpan,
dan kegagalan itu terlihat sebagai keadaan integrasi tersendiri yang dapat dicoba ulang.

**Pelaku.** Perawat penulis catatan; kepala ruangan untuk koreksi atas nama penulis lain.

**Langkah yang berurutan.**

1. Perawat mencatat tindakan — `BE-RWI-061`. Catatan lahir berkeadaan **tercatat**, dan penanda
   pengirimannya **menunggu dikirim** bila tindakan itu dapat ditagih.
2. Selama masih tercatat, penulisnya boleh menyunting isinya. Petugas lain ditolak `403`.
3. Perawat menyatakan catatan **final**. Pada saat yang sama — dalam satu penyimpanan — catatan
   **didaftarkan** ke mesin keutuhan rekam medis sebagai dokumen `Procedure` **tertanda tangan**
   atas nama penulisnya.
4. Bila pendaftaran itu gagal, **finalisasi ikut batal**. Catatan tetap tercatat, dan tidak ada
   baris keutuhan yang menggantung.
5. Sesudah final, isinya tidak dapat disunting siapa pun. Pembetulan dilakukan lewat **koreksi
   bernomor** beserta alasannya; isi aslinya tidak berubah sedikit pun, dan statusnya **tetap**
   final.
6. Pengiriman tagihan berjalan **terpisah**. Berhasil atau gagal, keadaan klinis catatan tidak
   bergeser satu langkah pun.

**Dua mesin status yang hidup berdampingan.**

| Mesin | Nilainya | Siapa yang menggerakkan |
| --- | --- | --- |
| Keadaan klinis catatan | `Recorded` → `Finalized` | Perawat, lewat `PATCH /{id}/finalize` |
| Keadaan pengiriman tagihan | `NotApplicable`, `Pending` → `Dispatched` atau `Failed` → `Dispatched` | Pengirim tagihan, terpisah dari jalur klinis |

> **Kenapa `Amended` tidak ada.** Kontrak `0.3.0` mencabutnya lewat `RWI-DEC-091`. Bila status
> dokumen ikut berubah saat dikoreksi, ada **dua** sumber jawaban atas pertanyaan "apakah dokumen
> ini pernah dikoreksi": status dokumen dan riwayat koreksinya. Yang berlaku adalah riwayat
> koreksinya.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Menyunting catatan yang sudah final | `400` beserta arahan memakai koreksi; isi aslinya tidak berubah |
| Menyunting catatan orang lain yang belum final | `403` — *"Catatan ini ditulis petugas lain. Anda tidak dapat mengubahnya."* (`VAL-KEP-06`) |
| Mengoreksi catatan final oleh bukan penulisnya | `403` (`VAL-KEP-07`, `AC-CAP014-03`); nol addendum terbentuk |
| Pendaftaran keutuhan gagal saat finalisasi | `400`; catatan **tetap** tercatat, nol baris keutuhan |
| Finalisasi diulang | `200` beserta catatan yang sudah final — bukan galat |
| Pengiriman tagihan gagal | Penanda menjadi gagal beserta sebab dan hitungan percobaannya; catatan klinis utuh |

**Hasil akhir.** Satu catatan tindakan memiliki keadaan klinis dan keadaan tagihan yang bergerak
sendiri-sendiri, riwayat koreksi bernomor, dan isi asli yang tidak pernah berubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` kartu `BE-RWI-062` dan bagian 2.1, 2.3, 5 | Scope, dan bahwa substansi `RWI-OQ-051` sudah terpenuhi di source |
| `contracts/state-transition-matrix.md` `0.3.0` bagian 3, 3.1, dan 4 | Dua mesin status dan syarat teknis pendaftaran keutuhan |
| `contracts/integration-contract.md` `0.3.0` `INT-KEP-05`, `INT-KEP-06` | Arah integrasi dan keadaan modul tujuan |
| `contracts/validation-matrix.md` bagian 2 | `VAL-KEP-06`, `VAL-KEP-07` |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | Daftar jenis dokumen yang ditegakkan, dan aturan pemakaian transaksinya |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalNoteAddendumService.cs` | Pemeriksaan kewenangan bertingkat `RM-DEC-004` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` baris 1048–1161 | Pola penerusan koreksi yang sudah dipakai `BE-RWI-057` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` baris 1083 | Pola pendaftaran tertanda tangan saat finalisasi |

**Temuan penting yang dikonfirmasi ulang.** `ClinicalDocumentIntegrityService.JenisYangDitegakkan`
sudah memuat **empat** nilai — `ProgressNote`, `Consultation`, `Assessment`, dan **`Procedure`**.
Substansi `RWI-OQ-051` memang sudah terpenuhi, persis seperti yang dicatat roadmap bagian 2.1.
Slice ini karena itu **tidak** membangun penjaga penguncian sendiri, yang justru dilarang
`RWI-DEC-087`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/NursingInterventionService.cs` | `UpdateAsync`, `FinalizeAsync`, `RecordBillingDispatchOutcomeAsync`, `GetBillingDispatchAsync`, `CreateAddendumAsync`, `ListAddendumsAsync`, `GetAuthorNamesAsync`; dua ketergantungan baru pada mesin keutuhan dan mesin koreksi |
| `Areas/HealthServices/ClinicalManagement/DTOs/NursingInterventionDtos.cs` | `UpdateNursingInterventionRequest`, `CreateInterventionAddendumRequest`, `BillingDispatchResponse` |
| `Areas/HealthServices/ClinicalManagement/Controllers/NursingInterventionController.cs` | Lima endpoint baru beserta butir hak akses `Amend` |
| `Program.cs` | Tidak berubah — `NursingInterventionService` sudah terdaftar pada `BE-RWI-061` |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingInterventionBillingSeparationTests.cs` | **Baru.** Sepuluh uji acceptance |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingInterventionTests.cs` | Penolong `BuatService` mengikuti ketergantungan baru |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Bertambah.** Empat endpoint sesuai `api-contract.md` `0.3.0`, ditambah satu delta yang dijelaskan bagian 7 |
| Database | **Nol tabel baru, nol kolom baru.** `dotnet ef migrations has-pending-model-changes` menjawab *"No changes have been made to the model since the last migration."* — **migration task ini kosong**. Kolom keadaan tagihan sudah lahir bersama tabelnya pada `BE-RWI-061` |
| Keamanan/Auth | **Action baru `Amend`** pada resource `NursingIntervention` yang sudah ada, ditambah pemakaian `Update`. Kewenangan koreksi ditegakkan `ClinicalNoteAddendumService` berdasarkan **kepemilikan data**, bukan nama peran |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Nursing Intervention

Base URL: `api/v1/health-services/clinical-management/nursing-interventions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/{id}` | Menyunting catatan tindakan selama belum final; hanya penulisnya | `NursingIntervention : Update` |
| `PATCH` | `/{id}/finalize` | Menyatakan catatan final, sekaligus mendaftarkannya sebagai dokumen `Procedure` tertanda tangan | `NursingIntervention : Update` |
| `POST` | `/{id}/addendums` | Menambah koreksi pada catatan yang sudah final; isi asli tidak berubah | `NursingIntervention : Amend` |
| `GET` | `/{id}/addendums` | Daftar koreksi satu catatan, terurut nomor | `NursingIntervention : Read` |
| `GET` | `/{id}/billing-dispatch` | Keadaan pengiriman tagihan beserta keadaan klinisnya sekaligus | `NursingIntervention : Read` |

### Kode status dan artinya bagi pengguna

| Kode | Artinya |
| --- | --- |
| `200` | Catatan difinalkan, disunting, atau keadaannya terbaca |
| `201` | Koreksi tersimpan |
| `400` | Catatan sudah final sehingga tidak dapat disunting, atau pendaftaran rekam medis gagal sehingga finalisasi dibatalkan |
| `403` | Anda bukan penulis catatan ini |
| `404` | Catatan tindakan tidak ditemukan |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | `Build succeeded`, `213 Warning(s)`, `0 Error(s)` | `PASS` | Jumlah warning sama persis dengan garis dasar `6f7d81e` |
| `dotnet ef migrations has-pending-model-changes` | *"No changes have been made to the model since the last migration."* | `PASS` | Bukti bahwa **migration task ini kosong** |
| `dotnet test` project SQLite, tapis `NursingIntervention` | `Failed: 0, Passed: 21, Total: 21` | `PASS` | 11 uji `BE-RWI-061` dan 10 uji task ini |
| `dotnet test` project SQLite, seluruhnya | `Failed: 0, Passed: 448, Total: 448` | `PASS` | Garis dasar sebelum slice ini 394 |
| `dotnet test` project InMemory, seluruhnya | `Failed: 9, Passed: 917, Total: 926` | `EXISTING / ENVIRONMENT ISSUE` | Kesembilan kegagalan berada di `BillingManagement`; worktree pada commit `6f7d81e` tanpa perubahan slice ini menghasilkan angka yang sama persis |
| Skenario Billing gagal lalu pembacaan ulang catatan | Catatan utuh — nama, hasil, dan keadaan klinisnya tidak berubah; penanda pengiriman gagal beserta sebabnya | `PASS` | `BillingGagal_CatatanKlinisTetapTersimpan` |
| Pemeriksaan bahwa dua mesin status tidak saling mengunci | Catatan final berdampingan dengan pengiriman gagal; percobaan ulang berhasil tidak mengubah keadaan klinis | `PASS` | `DuaMesinStatus_TidakSalingMengunci` |
| **Test yang memaksa pendaftaran keutuhan gagal** | `400`; catatan **tetap** tercatat; nol baris keutuhan terbentuk | `PASS` | `PendaftaranKeutuhanGagal_FinalisasiIkutBatal` |
| Skenario koreksi oleh bukan penulis | `403`; nol addendum terbentuk | `PASS` | `KoreksiOlehBukanPenulis_Ditolak403` |
| Skenario koreksi oleh penulisnya | Dua koreksi bernomor 1 dan 2 beserta alasannya; isi asli tidak berubah; status tetap final | `PASS` | `Koreksi_TersimpanSebagaiAddendumBernomorTanpaMengubahIsiAsli` |
| Percobaan menyunting catatan final | `400`; isi asli tidak berubah | `PASS` | `MenyuntingCatatanFinal_Ditolak` |
| Penyuntingan catatan belum final | Oleh penulisnya `200`; oleh petugas lain `403` beserta kalimat `VAL-KEP-06` | `PASS` | `MenyuntingCatatanBelumFinal_OlehPenulisnya_Diterima`, `MenyuntingCatatanBelumFinal_OlehBukanPenulis_Ditolak403` |
| Skenario koreksi oleh kepala ruangan | Tidak dijalankan lewat endpoint ini | `NOT APPLICABLE` | Jalurnya memakai endpoint pengganti milik `MedicalRecordManagement`; penjelasannya pada bagian 7 |

Uji manual: `NOT FEASIBLE` — alasannya sama dengan `BE-RWI-059`.

**Tidak dijalankan:**

- **Eksekusi migration.** Task ini tidak membuat migration sama sekali.
- **Pengiriman sungguhan ke `BillingManagement`.** Modul itu belum memiliki kemampuan transaksi
  yang menerima pemicu `INT-KEP-05`; yang dibangun di sini adalah mesin keadaannya beserta
  permukaan bacanya.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Catatan klinis tetap tersimpan ketika Billing gagal, dan keadaan pengirimannya `Failed` | Terpenuhi | `RecordBillingDispatchOutcomeAsync` hanya menyentuh kolom keadaan pengiriman; uji `BillingGagal_CatatanKlinisTetapTersimpan` |
| 2. Status klinis tetap `Recorded` atau `Finalized` apa pun keadaan tagihannya; kedua mesin status **tidak saling mengunci** | Terpenuhi | `DuaMesinStatus_TidakSalingMengunci` memeriksa keadaan klinis tidak bergeser pada kegagalan **maupun** pada percobaan ulang yang berhasil |
| 3. Finalisasi mendaftarkan catatan sebagai dokumen `Procedure` tertanda tangan; bila pendaftaran gagal, **finalisasi ikut batal** | Terpenuhi | `FinalizeAsync` memanggil `RegisterSignedAsync` lalu satu `SaveChangesAsync`; kegagalan keluar **sebelum** penyimpanan. Uji `Finalisasi_MendaftarkanDokumenProcedureTertandaTangan` dan `PendaftaranKeutuhanGagal_FinalisasiIkutBatal` |
| 4. Catatan final hanya dapat dikoreksi penulisnya atau kepala ruangan; selain itu ditolak `403` (`VAL-KEP-07`, `AC-CAP014-03`) | Terpenuhi | Jalur penulis ditegakkan `ClinicalNoteAddendumService.ResolveAuthorityAsync`; uji `KoreksiOlehBukanPenulis_Ditolak403`. Jalur kepala ruangan memakai endpoint pengganti milik `MedicalRecordManagement`, sama seperti yang sudah ditetapkan `BE-RWI-057`; penjelasannya pada bagian 7 |
| 5. Percobaan menyunting langsung isi catatan yang sudah `Finalized` ditolak | Terpenuhi | Dua penjaga berlapis — pemeriksaan status di `UpdateAsync` dan `EnsureMutableAsync` milik mesin keutuhan; uji `MenyuntingCatatanFinal_Ditolak` |
| 6. Setiap koreksi tercatat beserta alasannya, dan isi asli tidak berubah (`RWI-AC-176`) | Terpenuhi | `Koreksi_TersimpanSebagaiAddendumBernomorTanpaMengubahIsiAsli` memeriksa nomor urut, alasan, isi asli, dan status yang tetap final |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Transisi status terpasang | Terpenuhi — `Recorded → Finalized` |
| Pendaftaran keutuhan dalam satu transaksi | Terpenuhi |
| Mesin pengiriman tagihan terpisah | Terpenuhi |
| Tiga endpoint | **Terlampaui** — lima endpoint; penjelasannya pada bagian 7 |
| Hak akses `NursingIntervention : Amend` | Terpenuhi |
| Keenam acceptance criteria terbukti | Terpenuhi |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement`; menyentuh `MedicalRecordManagement` hanya sebagai **konsumen** service-nya, nol berkas modul itu berubah |
| Pemilik / prefix registry | `ClinicalManagement / Clinical` — prefix **`Cli`**, lifecycle `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` untuk seluruh method dan endpoint baru; **nol** entity dan **nol** migration |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-MOD-*`, `QBE-CODE-*` — task ini tidak membuat entity, configuration, tabel, maupun nomor bisnis |
| `QBE-TXN-001` | **Ditegakkan.** Finalisasi dan pendaftaran keutuhan berada pada satu `SaveChanges`; kegagalan pendaftaran keluar sebelum penyimpanan sehingga tidak ada keadaan setengah jadi |

### Delta kontrak yang dilaporkan ke pemilik kontrak

| Delta | Isinya | Kenapa |
| --- | --- | --- |
| **`PUT /{id}` ditambahkan** | Kontrak `0.3.0` bagian 3 tidak mencantumkannya | Mesin status bagian 3 memuat baris `Recorded → Recorded` "Menyunting" oleh penulisnya, dan acceptance criteria 5 menuntut percobaan menyunting catatan **final** ditolak. Tanpa jalur penyuntingan, tidak ada yang dapat ditolak, dan `VAL-KEP-06` tidak punya tempat berlaku |
| **Endpoint yang dibangun berjumlah lima, bukan tiga** | Kartu task menyebut tiga | Kontrak `0.3.0` sendiri mencantumkan **empat** endpoint baru pada grup ini (`finalize`, `POST /addendums`, `GET /addendums`, `GET /billing-dispatch`). Yang kelima adalah `PUT /{id}` di atas |
| **Jalur kepala ruangan tidak dibuat di sini** | Acceptance criteria 4 menyebut "penulisnya atau kepala ruangan" | Koreksi atas nama penulis lain memakai endpoint pengganti milik `MedicalRecordManagement`, yang mengenal penetapan berhalangan dan butir hak akses `ClinicalNoteAddendum : CreateAsSubstitute`. Membangun jalur kedua di `ClinicalManagement` berarti melahirkan mesin koreksi tandingan — dilarang `RWI-DEC-087`. Batas yang sama sudah ditetapkan dan dilaporkan `BE-RWI-057` |
| **Perubahan keadaan tagihan tidak punya endpoint** | Kontrak hanya meminta `GET /{id}/billing-dispatch` | Sesuai kontraknya. Penggeraknya adalah `RecordBillingDispatchOutcomeAsync` pada service, yang kelak dipanggil pengirim milik `BillingManagement`. Membuka endpoint yang membiarkan siapa pun menyetel keadaan tagihan akan membuat penanda itu tidak lagi bermakna sebagai bukti integrasi |
| **Jenis dokumen `Procedure` kini dipakai dua tabel** | `TrxPatientProcedure` dan `CliNursingIntervention` sama-sama mendaftar sebagai `Procedure` | Sesuai `state-transition-matrix.md` bagian 4 yang menetapkan `Procedure` untuk catatan tindakan. Baris keutuhan dikunci pasangan jenis **dan** id dokumen, sehingga keduanya tidak pernah bertabrakan. Dicatat supaya pembaca laporan rekam medis tahu satu jenis dokumen kini punya dua asal |

### Ringkasan lain

| Hal | Isi |
| --- | --- |
| Peringatan | Jumlah warning solusi tetap `213`, sama persis dengan garis dasar |
| Masalah yang diketahui | Keadaan tagihan hanya dapat bergerak lewat service, belum lewat penjadwal atau pengirim mana pun. Sampai `BillingManagement` menyediakan penerima pemicunya, seluruh tindakan yang dapat ditagih menunggu di keadaan `Pending` — dan itu memang perilaku yang dirancang `INT-KEP-05` |
| Risiko tersisa | Dependency `BE-RWI-061` berstatus 🟡 karena uji PostgreSQL idempotency belum dapat dijalankan. Butir itu **tidak** menyentuh satu pun kriteria task ini, tetapi tabel yang dipakai task ini lahir dari migration yang sama dan karena itu juga belum pernah diterapkan ke database mana pun |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 40 entri; nol berkas `MedicalRecordManagement` maupun modul lain yang berubah |
| Langkah berikutnya | `BE-RWI-063` menyalurkan catatan keperawatan ke catatan terpadu; setelah itu wewenang eksekusi migration diminta terpisah |
