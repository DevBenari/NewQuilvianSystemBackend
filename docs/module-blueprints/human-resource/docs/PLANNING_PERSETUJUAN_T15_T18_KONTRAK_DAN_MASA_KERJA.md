# Planning — Persetujuan T15–T18 Kontrak & Masa Kerja (perbaikan B6)

| Field | Nilai |
| --- | --- |
| Tanggal | 25 November 2026 |
| Modul | Human Resource — sub-modul 05 Siklus Kepegawaian, menu **Kontrak & Masa Kerja** (Perjanjian Kerja, Perpanjangan Kontrak, Masa Percobaan, Perubahan Status Kerja) dan **Kotak Masuk Persetujuan** |
| Repository terdampak | `NewQuilvianSystemBackend` (utama) dan `QuilvianSystemFrontendDev` |
| Status | Disetujui dan diimplementasikan 25 November 2026 (Fase 0–4). Fase 5 (uji ujung-ke-ujung dengan backend baru) menunggu izin restart backend dan penulisan DB dev — lihat bagian 10 |
| Alur kerja | Analisis → Planning → Implementasi → Testing |
| Kode temuan | **B6** pada panduan uji manual menu Kontrak & Masa Kerja (25 Nov 2026) |

---

## 1. Latar belakang

Empat jenis dokumen di menu Kontrak & Masa Kerja harus disetujui Bagian SDM sebelum berlaku
(`HRD-DEC-081`, `HRD-DEC-085`, `HRD-DEC-089`; kontrak integrasi `INT-HR-30`):

| Kode | Dokumen | Diajukan dari | Diajukan oleh (izin) | Disetujui oleh (keputusan) | Efek bila disetujui |
| --- | --- | --- | --- | --- | --- |
| T15 | Penilaian masa percobaan | Detail Masa Percobaan | Atasan langsung (`WorkforceProbation:Submit`) | Petugas HRD (`HRD-DEC-085` butir 6) | Lulus → masa percobaan selesai; Tidak Lulus → diserahkan ke Pemberhentian |
| T16 | Keputusan kontrak (Perpanjang / Angkat Tetap / Tidak Diperpanjang) | Detail Perjanjian Kerja | `WorkforceEmploymentContract:SubmitDecision` | Petugas HRD (`HRD-DEC-089` butir 2) | Draf kontrak lanjutan dibuat, atau kasus pemberhentian dibuka |
| T17 | Koreksi kontrak | Detail Perjanjian Kerja | `WorkforceEmploymentContract:RequestCorrection` | Petugas HRD lain (`HRD-DEC-089` butir 11) | Isi kontrak diganti versi koreksi |
| T18 | Batch data awal kontrak | Batch Data Awal Kontrak | `WorkforceInitialContractBatch:Submit` | Petugas HRD | Baris batch menjadi riwayat kontrak |

Hari ini tombol **Ajukan** pada keempat layar berhasil (201), tetapi dokumen tidak pernah sampai ke
penyetuju. Statusnya berhenti di **Diajukan** selamanya.

### 1.1 Temuan backend (rusak)

| No | Temuan | Bukti | Dampak |
| ---: | --- | --- | --- |
| B6-1 | Keempat `Start*WorkflowAsync` hanya mengisi `WorkflowInstanceId = Guid.NewGuid()`; tidak ada instance di mesin persetujuan | `LifecycleManagement/Services/HrdEmploymentLifecycleWorkflowIntegrationService.cs:42`, `:63`, `:84`, `:105` | Tidak ada tugas di Kotak Masuk siapa pun |
| B6-2 | `HandleWorkflowCompletedAsync` (penerap hasil persetujuan) tidak dipanggil dari mana pun | `HrdEmploymentLifecycleWorkflowIntegrationService.cs:116`; `grep` pemanggil = 0 | Draf kontrak lanjutan `-R1`/PKWTT tidak pernah dibuat; pemberhentian tidak pernah dibuka |
| B6-3 | Mesin persetujuan tidak mengenal jenis rujukan lifecycle (`WorkforceContractDecision` dan seterusnya) | `WorkflowManagement/Services/WorkflowReferenceLifecycleService.cs:358-482` hanya menangani cuti, lembur, jadwal, resign, perencanaan tenaga, dan penangan terdaftar | Walau instance ada, hasilnya tidak kembali ke dokumen |
| B6-4 | Callback T17 hanya mengubah status; `ApplyCorrectionAsync` yang mengganti isi kontrak tidak dipanggil. Callback T18 menandai batch **Diterapkan** tanpa membuat kontrak; `ApplyInitialBatchAsync` tidak dipanggil | `HrdEmploymentLifecycleWorkflowIntegrationService.cs:223-250`; `HrdEmploymentContractService.cs:1373`; `HrdInitialContractBatchService.cs:409` | Koreksi "disetujui" tetapi kontrak tetap salah; batch "diterapkan" tetapi kosong |
| B6-5 | `ApplyCorrectionAsync` dan `ApplyInitialBatchAsync` tanpa penjaga pembuat ≠ penyetuju dan tanpa penjaga pemanggilan ulang | `HrdEmploymentContractService.cs:1373-1420`; `HrdInitialContractBatchService.cs:409-440` | Begitu disambung, pemanggilan ganda bisa membuat kontrak ganda |
| B6-6 | Tarik usulan tidak menarik instance; ubah usulan hanya boleh saat **Diajukan**, padahal yang perlu diperbaiki adalah usulan **Dikembalikan** | `HrdEmploymentContractService.cs:781-785` (`UpdateDecisionAsync`), `:853-857` (`WithdrawDecisionAsync`) | Sesudah disambung: usulan yang ditarik tetap muncul di Kotak Masuk; usulan yang dikembalikan tidak bisa diperbaiki |
| B6-7 | Laporan `FIN-HRD-08` mencatat `AUD-08-02` **RESOLVED**, padahal yang diperbaiki hanya isi callback; pemicunya tetap GUID acak | `docs/update/kontrak-dan-masa-kerja/LAPORAN_EKSEKUSI_HRD_MENU_08_KONTRAK_DAN_MASA_KERJA.md:39` | Status dokumen menyesatkan |

### 1.2 Temuan konfigurasi di DB dev `QuilvianNewDevHamzah` (dibaca 25 Nov 2026, baca-saja)

| No | Temuan | Bukti | Dampak |
| ---: | --- | --- | --- |
| K-1 | Definisi `T15`, `T16`, `T17`, `T18` ada dan aktif (ditanam `Seeders/OnboardingRuntimeDevSeeder.cs:146-178`), tetapi **0 langkah**, **0 matriks**, **0 alasan penolakan** | Query `MstWorkflowDefinition`/`MstWorkflowStep`/`MstApprovalMatrix`/`MstRejectionReason` | Begitu disambung, pengajuan ditolak mesin: "Workflow definition belum memiliki step aktif" (`WorkflowService.cs` `CreateCoreAsync`) |
| K-2 | Kontrak `INT-HR-30` meminta langkah `SiteHr`. Di DB tidak ada role Identity `SiteHr`, dan tidak satu pun penempatan memiliki `HospitalSiteId` | `contracts/integration-contract.md:529`; `WorkflowService.cs` `ResolveByRoleAsync` memakai `UserManager.GetUsersInRoleAsync` lalu menyaring `HospitalSiteId` | Langkah `SiteHr` pasti tanpa penyetuju (gagal tertutup) |
| K-3 | Tidak ada alasan penolakan untuk jenis permohonan apa pun | Query `MstRejectionReason` = 0 baris | Tombol **Tolak** di Kotak Masuk tidak dapat dikirim (`workflow-display-utils.js:57`) |
| K-4 | `TrxWorkflowInstance` = 0 baris | Query | Mesin persetujuan belum pernah dipakai di DB ini; uji ujung-ke-ujung ini sekaligus uji pertama mesinnya |

Pelajaran dari integrasi sebelumnya (`BE-PBJ-001`, laporan `pengadaan-barang-jasa/.../BE-PBJ-001.md:200-205`)
yang ikut dihindari di rencana ini:

- `WorkflowService.CreateAsync` membuka transaksi sendiri. Memanggilnya di dalam transaksi lain → 500 "The connection is already in a transaction".
- Kode definisi di service harus sama dengan di DB (`HRD_T9_STAFFING_STANDARD` ≠ `T9` membuat pengajuan gagal).
- Mesin menutup instance yang disetujui dengan status **`Completed`**, bukan `Approved`.

### 1.3 Temuan frontend

| No | Temuan | Bukti | Dampak |
| ---: | --- | --- | --- |
| F1 | Unggah bukti pemberitahuan H-7 tidak punya layar; `POST …/notice-evidence` tidak dipanggil | `lifecycle-service.js` tidak punya fungsinya; `HrdEmploymentContractController.cs:233` | Kepatuhan H-7 tidak pernah **Tepat Waktu**; selalu Belum Ada/Terlambat |
| F2 | Panel "Keputusan Terakhir" dan tombol **Catat Pemberitahuan** hanya berasal dari state lokal `lastDecision` sesudah mengajukan | `employment-contract-detail-view.jsx:64`, `:230-233` | Muat ulang halaman → panel dan tombol hilang |
| F3 | Tombol **Catat Pemberitahuan** hanya untuk Tidak Diperpanjang, padahal backend menghitung H-7 untuk semua jenis keputusan | `employment-contract-detail-view.jsx:233`; `HrdEmploymentContractService.cs` `ComputeNoticeCompliance` | Pemberitahuan perpanjangan tidak dapat dicatat |
| F4 | Tombol **Unggah Berkas Lama** memeriksa `RequestCorrection`, backend menuntut `UploadDocument` | `employment-contract-detail-view.jsx:184`; `HrdEmploymentContractController.cs:294` | Tombol tampil untuk orang yang akan ditolak 403, dan tersembunyi dari pemegang izin yang benar |
| F5 | Kotak Masuk hanya mengenal label `T16`/`CONTRACT_DECISION`; baris tidak menyebut nama pegawai, hanya pemohon (staf SDM) | `approval-inbox-view.jsx:46-65`; `workflow-display-utils.js:42-47` | Penyetuju melihat "Yudha Hermawan — Keputusan Perpanjangan" tanpa tahu kontrak siapa |
| F6 | Tidak ada layar memperbaiki usulan **Dikembalikan**: `updateContractDecision`, `withdrawContractDecision`, `updateProbationAssessment`, `withdrawContractCorrection` tidak ada; Batch hanya bisa diubah saat **Draf** | `lifecycle-service.js`; `probation-detail-view.jsx:119`; `initial-contract-batch-detail-view.jsx:10` (`isDraft`) | Tombol **Minta Revisi** penyetuju berujung buntu |

### 1.4 Contoh di rumah sakit

**Hari ini.** Kontrak PKWT perawat Rizky di Rawat Inap berakhir dua minggu lagi. Yudha (Staf
Administrasi Kepegawaian) mengajukan **Perpanjang 12 bulan**. Layar menampilkan "Diajukan". Ibu
Hartini (Kepala Bagian SDM) membuka Kotak Masuk Persetujuan: kosong. Tanggal akhir lewat, kontrak
Rizky menjadi **Lewat Tanpa Keputusan**, dan jadwal dinas Rizky sesudah tanggal itu tidak dapat
dibuat (`HRD-DEC-089` butir 9). Kepala Ruangan menelepon SDM.

**Sesudah diperbaiki.** Begitu Yudha menekan **Ajukan**, Ibu Hartini melihat
"CDA-2026-000002 · T16: Keputusan Kontrak" di Kotak Masuk. Ia menekan **Setujui**, dialog
menampilkan ringkasan (Rizky, kontrak lama berakhir kapan, usulan periode baru, alasan). Sesudah
disetujui, draf kontrak `…-R1` muncul di daftar Perjanjian Kerja berstatus **Draf**. Yudha mencetak,
pegawai menandatangani, Yudha mengunggah scan, dan kontrak baru aktif. Yudha mencatat tanggal
pemberitahuan tertulis beserta buktinya; kolom H-7 menjadi **Tepat Waktu**.

---

## 2. Tujuan

Menyambungkan keempat dokumen T15–T18 ke mesin persetujuan yang sudah dipakai SPPD dan Resign, sehingga
dokumen benar-benar masuk Kotak Masuk Kepala Bagian SDM dan hasil keputusannya berlaku otomatis. Sekaligus
menutup tiga celah layar (bukti H-7, keputusan terakhir dari server, izin Unggah Berkas Lama) dan menyediakan
jalan memperbaiki usulan yang dikembalikan.

---

## 3. Gambaran layar

Data di bawah samaran.

### 3.1 Kotak Masuk Persetujuan (Kepala Bagian SDM)

```text
┌ Kotak Masuk Persetujuan ──────────────────────────────────────────────────────────────┐
│ No. Permohonan            Pemohon               Perihal                     Aksi      │
│ CDA-2026-000002           Yudha Hermawan        Keputusan Perpanjangan /    [Setujui] │
│ T16: Keputusan Kontrak    Kepala Bagian SDM     Pengangkatan Kontrak Kerja  [Revisi]  │
│                                                                             [Tolak]   │
│ PAS-2026-000001           Ns. Endang P.         Penilaian Masa Percobaan    [Setujui] │
│ T15: Hasil Masa Percobaan Kepala Bagian SDM     Staf                        …         │
└───────────────────────────────────────────────────────────────────────────────────────┘

┌ Setujui Permohonan ──────────────────────────────────────────┐
│ Ringkasan dokumen                                            │
│   Pegawai          Rizky M. · Perawat Rawat Inap             │
│   Kontrak berjalan PKWT-0042 · berakhir 09 Des 2026           │
│   Usulan           Perpanjang 12 bulan                        │
│   Periode baru     10 Des 2026 – 09 Des 2027                  │
│   Alasan           Kinerja baik, ruangan kekurangan perawat   │
│   Diajukan         Yudha Hermawan, 25 Nov 2026                │
│   Buka dokumen ↗                                              │
│                                                               │
│ Catatan (opsional)  [_______________________________]         │
│                                        [Batal]  [Setujui]     │
└───────────────────────────────────────────────────────────────┘
```

Ringkasan yang sama tampil pada dialog **Minta Revisi** dan **Tolak**. Gaji tidak pernah ikut ringkasan.

### 3.2 Detail Perjanjian Kerja — panel Keputusan Terakhir (dari server)

```text
┌ Keputusan Terakhir ─────────────────────────────────── [Dikembalikan] ┐
│ Nomor          CDA-2026-000002      Keputusan      Perpanjang         │
│ Usulan mulai   10 Des 2026          Usulan berakhir 09 Des 2027       │
│ Catatan penyetuju  "Lengkapi alasan perpanjangan."                    │
│                              [Tarik Usulan]  [Perbaiki & Ajukan Ulang] │
└───────────────────────────────────────────────────────────────────────┘

┌ Keputusan Terakhir ──────────────────────────────────── [Disetujui] ┐
│ Nomor          CDA-2026-000002      Diputus   Hartini S., 26 Nov 2026 │
│ Kontrak lanjutan  PKWT-0042-R1 (Draf)                       [Buka]   │
│ Pemberitahuan H-7 Belum dicatat · tenggat 02 Des 2026                 │
│                                            [Catat Pemberitahuan]     │
└──────────────────────────────────────────────────────────────────────┘

┌ Catat Pemberitahuan ke Pegawai ─────────────────────┐
│ Tanggal diberitahukan *   [26/11/2026]              │
│ Bukti tertulis *          [Pilih berkas…] PDF/JPG/PNG, maks. 10 MB │
│ Catatan                   [________________]        │
│                               [Batal] [Simpan]      │
└─────────────────────────────────────────────────────┘
```

### 3.3 Detail Masa Percobaan dan Batch Data Awal

- Baris penilaian **Dikembalikan** mendapat tombol **Perbaiki & Ajukan Ulang** (memakai dialog penilaian
  yang sudah ada, terisi nilai lama) dan **Tarik**.
- Batch **Dikembalikan** dapat ditambah/dihapus barisnya dan diajukan ulang, sama seperti **Draf**.

---

## 4. Keputusan

| No | Hal | Rekomendasi | Alternatif |
| ---: | --- | --- | --- |
| 1 | Cara menyambung hasil persetujuan ke dokumen | Penangan baru `HrdEmploymentLifecycleWorkflowLifecycleHandler : IWorkflowReferenceLifecycleHandler` di `LifecycleManagement`, didaftarkan lewat DI — pola yang sama dengan `BusinessTravelWorkflowLifecycleHandler` (SPPD). `WorkflowReferenceLifecycleService` tidak diubah | Menambah cabang ke `WorkflowReferenceLifecycleService` seperti Resign — mengubah file bersama 945 baris yang dipakai semua modul |
| 2 | Kode definisi dan jenis rujukan | Kode definisi `T15`–`T18` (yang sudah ada di DB). Jenis rujukan `WorkforceProbationAssessment`, `WorkforceContractDecision`, `WorkforceContractCorrection`, `WorkforceInitialContractBatch` (`02-backend-architecture.md` bagian 18) | Kode baru `HRD_T16_…` — tidak ada di DB, mengulang kesalahan T9 |
| 3 | Siapa penyetuju | Satu langkah per definisi, sumber **Jabatan** (`Position`) = **Kepala Bagian SDM**, `AllowSelfApproval = false`, `ApprovalMode = Any`. Pergantian pejabat otomatis ikut karena terikat jabatan | `SiteHr` sesuai `INT-HR-30` — butuh role Identity `SiteHr` dan `HospitalSiteId` di penempatan, keduanya belum ada (K-2). `RequesterManager` — salah untuk T15/T16 yang diajukan Kepala Ruangan (atasannya bukan SDM) |
| 4 | Tempat mengatur langkah dan alasan penolakan | Lewat layar master yang sudah ada (**Langkah Workflow**, **Alasan Penolakan**) karena jabatan penyetuju milik masing-masing RS. Untuk DB dev diisi sekali lewat API (Fase 0, butuh izin) | Seeder kode — tidak bisa mengetahui Id jabatan Kepala Bagian SDM di tiap RS |
| 5 | Alasan penolakan bawaan per T15–T18 | Tiga: "Data atau lampiran tidak lengkap", "Tidak sesuai kebijakan/aturan kontrak", "Lainnya" (catatan wajib untuk ketiganya) | Satu alasan umum saja |
| 6 | Bila mesin menolak pengajuan (tidak ada langkah/penyetuju) | **Gagal tertutup**: dokumen yang baru disimpan dibatalkan kembali (usulan dihapus lunak; status masa percobaan/batch dikembalikan), lalu pesan diteruskan: "Alur persetujuan T16 belum memiliki penyetuju aktif. Hubungi admin untuk mengatur Langkah Workflow." Tidak memakai transaksi luar (lihat pelajaran 1.2) | Biarkan dokumen Diajukan tanpa instance — keadaan sekarang |
| 7 | Cara membuat instance | `WorkflowService.CreateAndSubmitOnBehalfAsync` dengan pemohon = pengaju, kunci idempoten `hrd-lifecycle:{t15\|t16\|t17\|t18}:{id:N}`. `WorkflowService` diambil lewat `IServiceProvider` saat dipakai, supaya tidak terjadi ketergantungan melingkar (Workflow → penangan → service kontrak → integrasi → Workflow) | `CreateAsync` + `SubmitAsync` terpisah — dua langkah, tidak idempoten |
| 8 | Pemetaan status | `Submitted`/`InProgress` → Diajukan; `RevisionRequested`/`Returned` → Dikembalikan; `Completed`/`Approved` → Disetujui + efek; `Rejected` → Ditolak; `Withdrawn`/`Cancelled` → Ditarik (batch: Dibatalkan). Instance yang bukan instance terkini dokumen diabaikan; dokumen yang sudah final tidak diubah lagi | — |
| 9 | Efek sesudah disetujui | Memakai service domain yang sudah ada: T16 `ApplyDecisionAsync`, T17 `ApplyCorrectionAsync`, T18 `ApplyInitialBatchAsync`, T15 logika yang sudah ada di `HandleWorkflowCompletedAsync`. T17 dan T18 ditambah penjaga pembuat ≠ penyetuju dan penjaga sudah-diterapkan | Menulis ulang efek di penangan — menggandakan aturan |
| 10 | Memperbaiki usulan Dikembalikan | T16: `PUT …/decisions/{id}` diizinkan saat **Dikembalikan**, lalu instance yang sama diajukan ulang. T15: `PUT …/assessments/{id}` (sudah ada) ikut mengajukan ulang instance. T18: `POST …/submit` (sudah menerima Dikembalikan) ikut mengajukan ulang. T17: tidak ada ubah — **Tarik** lalu ajukan koreksi baru | Menambah endpoint ubah koreksi |
| 11 | Menarik usulan | Hanya pengaju; menarik dokumen sekaligus instance-nya (`WorkflowService.WithdrawAsync`) | Tarik dokumen saja — tugas yatim tertinggal di Kotak Masuk |
| 12 | Isi ringkasan di Kotak Masuk | Backend mengisi `RequestContext` dengan `summary` (daftar label–nilai, tanpa gaji) dan Id sumber. Kotak Masuk menampilkan blok **Ringkasan dokumen** dan tautan **Buka dokumen** pada dialog Setujui/Revisi/Tolak. Berlaku umum: modul lain yang mengisi `summary` otomatis ikut tampil | Penyetuju membuka menu Perpanjangan Kontrak sendiri — dua layar untuk satu keputusan |
| 13 | Penilaian percobaan yang masa percobaannya habis saat masih di Kotak Masuk | Scheduler yang menutup penilaian (`ClosedOnExpiry`) ikut menarik instance-nya lewat metode server baru `WorkflowService.WithdrawOnBehalfAsync` (tanpa endpoint; pola sama dengan `CreateAndSubmitOnBehalfAsync`) | Biarkan; persetujuan sesudah tanggal akhir diabaikan dengan peringatan (`VR-ONB-58`) — tugas "hantu" tetap tampil di Kotak Masuk |
| 14 | Pemberitahuan H-7 | Satu dialog **Catat Pemberitahuan** berisi tanggal + bukti berkas, tampil untuk semua jenis keputusan berstatus **Disetujui** sampai keduanya tercatat. Bukti sekali unggah (backend menolak ganti) | Hanya untuk Tidak Diperpanjang (perilaku sekarang, F3) |
| 15 | Sumber panel Keputusan Terakhir | Parameter baru `contractHistoryId` pada `GET employment-contracts/decisions`; respons keputusan ditambah `noticeEvidenceFileName` dan `followUpContractNumber` | Field `latestDecision` di respons detail kontrak |
| 16 | Penyelarasan bila pemanggilan sesudah aksi gagal | Scheduler per jam yang sudah ada menyelaraskan dokumen **Diajukan** yang instance-nya sudah final (memanggil `WorkflowReferenceLifecycleService.SynchronizeAsync`) | Hanya endpoint sinkron manual |
| 17 | Kontrak `INT-HR-30` | Diberi catatan amandemen: langkah dikonfigurasi per RS, bawaan dev = Jabatan Kepala Bagian SDM; `SiteHr` tetap boleh dipakai bila role dan `HospitalSiteId` kelak tersedia | Biarkan kontrak berbeda dari implementasi |

---

## 5. Isi

| No | Bagian | Isi | Sumber layar / endpoint | Izin |
| ---: | --- | --- | --- | --- |
| 1 | Ajukan keputusan kontrak (T16) | Membuat instance `T16`; gagal tertutup bila tanpa penyetuju | Detail Perjanjian Kerja → `POST employment-contracts/{id}/decisions` | `WorkforceEmploymentContract:SubmitDecision` |
| 2 | Perbaiki & ajukan ulang usulan | Usulan Dikembalikan diubah dan diajukan ulang | `PUT employment-contracts/{id}/decisions/{decisionId}` | `WorkforceEmploymentContract:SubmitDecision` |
| 3 | Tarik usulan | Dokumen + instance ditarik | `DELETE employment-contracts/{id}/decisions/{decisionId}` | `WorkforceEmploymentContract:SubmitDecision` |
| 4 | Ajukan koreksi (T17) / tarik koreksi | Instance `T17`; tarik ikut menarik instance | `POST`/`DELETE employment-contracts/{id}/corrections…` | `WorkforceEmploymentContract:RequestCorrection` |
| 5 | Ajukan / perbaiki / tarik penilaian (T15) | Instance `T15` | `POST`/`PUT probations/{id}/assessments…`, `POST …/withdraw` | `WorkforceProbation:Submit` |
| 6 | Ajukan / ajukan ulang / batalkan batch (T18) | Instance `T18` | `POST initial-contract-batches/{id}/submit`, `…/cancel` | `WorkforceInitialContractBatch:Submit` / `:Cancel` |
| 7 | Putuskan | Setujui, Minta Revisi, Tolak (alasan wajib) | Kotak Masuk → `POST approval-inbox/{assignmentId}/approve`, `/request-revision`, `/reject` | `ApprovalInbox:Approve`, `:RequestRevision`, `:Reject` |
| 8 | Ringkasan dokumen di dialog keputusan | Dibaca dari `workflow.requestContextJson` | `GET approval-inbox/{assignmentId}` | `ApprovalInbox:Read` |
| 9 | Keputusan Terakhir (dari server) | Status, catatan penyetuju, kontrak lanjutan, H-7 | `GET employment-contracts/decisions?contractHistoryId=…&pageSize=1` | `WorkforceEmploymentContract:Read` |
| 10 | Catat pemberitahuan + bukti | Tanggal ≤ hari ini; berkas PDF/JPG/PNG ≤ 10 MB, sekali unggah | `POST …/decisions/{decisionId}/notify`, `POST …/notice-evidence` | `WorkforceEmploymentContract:RecordNotification` |
| 11 | Unggah Berkas Lama | Izin diperbaiki | `POST employment-contracts/{id}/document` | `WorkforceEmploymentContract:UploadDocument` |

### 5.1 Aturan tampil

| Kondisi | Yang terjadi |
| --- | --- |
| Pengguna adalah pengaju dokumen | Tidak mendapat tugas di Kotak Masuk (mesin menolak swa-setuju; tombol sudah disembunyikan bila `isOwnRequest`) |
| Kepala Bagian SDM sendiri yang mengajukan koreksi (T17) | Langkah tanpa penyetuju → pengajuan ditolak dengan pesan. Admin dapat menambah jabatan cadangan di Matriks Persetujuan |
| Usulan Diajukan | Panel menampilkan "Menunggu persetujuan Kepala Bagian SDM"; tombol Tarik Usulan (pengaju) |
| Usulan Dikembalikan | Catatan penyetuju + Perbaiki & Ajukan Ulang + Tarik Usulan |
| Usulan Disetujui (Perpanjang/Angkat Tetap) | Nomor kontrak lanjutan + tombol Buka; Catat Pemberitahuan sampai tanggal dan bukti tercatat |
| Usulan Disetujui (Tidak Diperpanjang) | Catat Pemberitahuan; info "Diserahkan ke Pemberhentian" |
| Usulan Ditolak / Ditarik | Panel tampil tanpa tombol; tombol Ajukan Keputusan aktif lagi (`VR-ONB-37` hanya menahan Diajukan/Disetujui) |
| Validasi dialog pemberitahuan (meniru backend) | Tanggal wajib dan tidak boleh sesudah hari ini; berkas wajib, PDF/JPG/PNG, maks. 10 MB (`HrdLifecycleAttachmentService` `VR-ONB-09`) |
| Validasi perbaikan usulan Perpanjang (meniru backend) | Sama dengan pengajuan awal: `fixed-term-check` (batas PKWT `HrdFixedTermPolicyService`); Tidak Diperpanjang wajib alasan |

---

## 6. File yang dibuat dan diubah

### 6.1 File baru

| File | Isi |
| --- | --- |
| `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LifecycleManagement/Constants/EmploymentLifecycleWorkflowCodes.cs` | Kode definisi `T15`–`T18`, empat jenis rujukan, pemetaan status workflow → status dokumen |
| `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdEmploymentLifecycleWorkflowLifecycleHandler.cs` | `IWorkflowReferenceLifecycleHandler`: `CanHandle` empat jenis rujukan; `SynchronizeAsync` memetakan status dan memanggil penerap efek secara idempoten |
| `NewQuilvianSystemBackend/Tests/QuilvianSystemBackend.HumanResourceTests/LifecycleApprovalWorkflowTests.cs` | Uji integrasi dengan mesin nyata (pola `TravelWorkflowLifecycleTests`) |
| `QuilvianSystemFrontendDev/tests/unit/lifecycle-approval-wiring.test.mjs` | Uji isi berkas: label Kotak Masuk, ringkasan, izin Unggah Berkas Lama, fungsi service baru |

### 6.2 File yang diubah

| File | Perubahan |
| --- | --- |
| `…/LifecycleManagement/Services/HrdEmploymentLifecycleWorkflowIntegrationService.cs` + `IHrdEmploymentLifecycleWorkflowIntegrationService.cs` | `Start*` membuat instance nyata (keputusan 7) dengan `RequestContext.summary`; metode `ResubmitAsync`, `WithdrawAsync`; `HandleWorkflowCompletedAsync` menjadi penerap yang dipanggil penangan (T17/T18 memanggil service domain) |
| `…/LifecycleManagement/Services/HrdEmploymentContractService.cs` | Gagal tertutup di `SubmitDecisionAsync`/`SubmitCorrectionAsync`; `UpdateDecisionAsync` menerima Dikembalikan + ajukan ulang; `WithdrawDecisionAsync`/`WithdrawCorrectionAsync` menarik instance; penjaga di `ApplyCorrectionAsync`; filter `ContractHistoryId` di `GetDecisionsPagedAsync`; `MapToDecisionResponse` menambah `NoticeEvidenceFileName`, `FollowUpContractNumber` |
| `…/LifecycleManagement/Services/HrdProbationService.cs` | Submit/ubah/tarik penilaian memakai instance nyata; gagal tertutup mengembalikan status masa percobaan |
| `…/LifecycleManagement/Services/HrdInitialContractBatchService.cs` | Submit/ajukan ulang/batal memakai instance nyata; penjaga di `ApplyInitialBatchAsync` |
| `…/LifecycleManagement/Services/HrdEmploymentLifecycleSchedulerService.cs` | Penyelarasan per jam (keputusan 16); penilaian kedaluwarsa menarik instance (keputusan 13) |
| `…/LifecycleManagement/DTOs/EmploymentContractDtos.cs` | `ContractDecisionQuery.ContractHistoryId`; dua field baru di `ContractDecisionResponse` |
| `…/WorkflowManagement/Services/WorkflowService.ActionsV2.cs` | Metode server `WithdrawOnBehalfAsync(instanceId, requesterUserId, reason)` — tanpa endpoint, tanpa entity baru |
| `NewQuilvianSystemBackend/Program.cs` | Registrasi penangan sebagai `IWorkflowReferenceLifecycleHandler` (di samping registrasi SPPD baris 1741-1742) |
| `docs/module-blueprints/human-resource/contracts/integration-contract.md` | Catatan amandemen `INT-HR-30` (keputusan 17) |
| `docs/.../kontrak-dan-masa-kerja/LAPORAN_EKSEKUSI_HRD_MENU_08_KONTRAK_DAN_MASA_KERJA.md` | Catatan koreksi status `AUD-08-02` merujuk planning ini |
| `QuilvianSystemFrontendDev/src/components/view/hr/workflow/approval-inbox-view.jsx` | Label empat jenis rujukan; detail dimuat untuk semua dialog aksi; blok Ringkasan dokumen + tautan Buka dokumen |
| `QuilvianSystemFrontendDev/src/utils/hr/approvals/workflow-display-utils.js` | `inboxDocumentSummary(detail)` (parse `requestContextJson` aman) dan `inboxSourceHref(referenceType, context)` |
| `QuilvianSystemFrontendDev/src/components/view/hr/lifecycle/employment-contract-detail-view.jsx` | Keputusan Terakhir dari server; Perbaiki & Ajukan Ulang, Tarik Usulan; dialog Catat Pemberitahuan dengan berkas; izin `UploadDocument` |
| `QuilvianSystemFrontendDev/src/components/view/hr/lifecycle/probation-detail-view.jsx` | Perbaiki & Ajukan Ulang untuk penilaian Dikembalikan |
| `QuilvianSystemFrontendDev/src/components/view/hr/lifecycle/initial-contract-batch-detail-view.jsx` | Aksi baris dan Ajukan juga untuk status Dikembalikan |
| `QuilvianSystemFrontendDev/src/lib/services/hr/lifecycle-service.js` | `updateContractDecision`, `withdrawContractDecision`, `uploadNoticeEvidence`, `withdrawContractCorrection`, `updateProbationAssessment`; `getContractDecisions` meneruskan `contractHistoryId` |
| `QuilvianSystemFrontendDev/src/lib/constants/hr/lifecycle/lifecycle-constants.js` | `LIFECYCLE_PERMISSIONS.uploadDocument`; konstanta jenis rujukan T15–T18 |

Tidak ada tabel, kolom, atau migration baru. Kolom `WorkflowInstanceId`/`WorkflowDefinitionId` di keempat
entity sudah ada. Unit test frontend yang membaca `approval-inbox-view.jsx`
(`approval-inbox-legacy-actions.test.mjs`, `business-travel-v28-gates.test.mjs`) dijaga tetap lulus; bila
teks yang mereka cari berubah, test disesuaikan, bukan dibiarkan gagal.

---

## 7. Urutan pengerjaan

### Fase 0 — Konfigurasi alur di DB dev (butuh izin tulis DB, lewat API)

1. `POST` empat langkah (satu per `T15`–`T18`): `StepCode` `T1x-SDM`, nama "Kepala Bagian SDM", sumber
   `Position`, jabatan Kepala Bagian SDM, `AllowSelfApproval = false`, `ApprovalMode = Any`.
2. `POST` dua belas alasan penolakan (tiga per jenis, keputusan 5).
3. Tambah `ApprovalInbox:RequestRevision` pada jabatan Kepala Bagian SDM (Hartini saat ini hanya punya
   Read/Approve/Reject). Kebijakan dibaca dulu lalu digabung, karena `POST role-access/policies` menimpa semua.
4. Verifikasi baca-saja: tiap definisi punya 1 langkah aktif dan 3 alasan.

### Fase 1 — Backend T16 ujung-ke-ujung

1. Konstanta kode + pemetaan status.
2. `StartContractDecisionWorkflowAsync` membuat instance nyata; gagal tertutup di `SubmitDecisionAsync`.
3. Penangan lifecycle + registrasi DI; T16 memanggil `ApplyDecisionAsync`.
4. Ubah/ajukan ulang (Dikembalikan) dan tarik usulan.
5. Filter `contractHistoryId` dan field respons baru.
6. Uji integrasi T16 (bagian 8 no. 1–6) lulus sebelum lanjut.

### Fase 2 — Backend T15, T17, T18

1. Pola yang sama untuk ketiganya, termasuk ajukan ulang/tarik.
2. Penjaga pembuat ≠ penyetuju dan sudah-diterapkan pada `ApplyCorrectionAsync` dan `ApplyInitialBatchAsync`.
3. Uji integrasi T15/T17/T18.

### Fase 3 — Backend penyelarasan dan kedaluwarsa

1. `WithdrawOnBehalfAsync` di `WorkflowService.ActionsV2.cs`.
2. Scheduler: menyelaraskan dokumen Diajukan yang instance-nya final; penilaian kedaluwarsa menarik instance.
3. Dokumen: amandemen `INT-HR-30`, catatan koreksi `AUD-08-02`.

### Fase 4 — Frontend

1. Kotak Masuk: label + ringkasan + tautan (komponen bersama — regresi SPPD dan Cuti diuji).
2. Detail Perjanjian Kerja: Keputusan Terakhir dari server, aksi Dikembalikan, dialog pemberitahuan + bukti,
   izin Unggah Berkas Lama.
3. Detail Masa Percobaan dan Batch: aksi untuk Dikembalikan.

### Fase 5 — Uji ujung-ke-ujung di browser (butuh restart backend dan izin tulis DB)

Lihat bagian 8 no. 13–20.

### Endpoint yang dipakai

Tidak ada endpoint baru. Satu parameter query baru (`contractHistoryId`) dan dua field respons baru.

#### Corporate / Human Resource / Lifecycle Management / Employment Contracts

Base URL: `api/v1/corporate/human-resource/employment-contracts`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| GET | `/decisions?contractHistoryId={id}&pageSize=1` | Keputusan terakhir satu kontrak (**parameter baru**) | `WorkforceEmploymentContract:Read` |
| POST | `/{id}/decisions` | Ajukan keputusan → instance `T16` | `WorkforceEmploymentContract:SubmitDecision` |
| PUT | `/{id}/decisions/{decisionId}` | Perbaiki usulan **Dikembalikan** lalu ajukan ulang (**aturan diubah**: sebelumnya hanya Diajukan) | `WorkforceEmploymentContract:SubmitDecision` |
| DELETE | `/{id}/decisions/{decisionId}` | Tarik usulan + instance | `WorkforceEmploymentContract:SubmitDecision` |
| POST | `/{id}/decisions/{decisionId}/notify` | Catat tanggal pemberitahuan | `WorkforceEmploymentContract:RecordNotification` |
| POST | `/{id}/decisions/{decisionId}/notice-evidence` | Unggah bukti pemberitahuan (multipart `file`) | `WorkforceEmploymentContract:RecordNotification` |
| POST | `/{id}/corrections` | Ajukan koreksi → instance `T17` | `WorkforceEmploymentContract:RequestCorrection` |
| DELETE | `/{id}/corrections/{correctionId}` | Tarik koreksi + instance | `WorkforceEmploymentContract:RequestCorrection` |
| POST | `/{id}/document` | Unggah berkas kontrak lama | `WorkforceEmploymentContract:UploadDocument` |

#### Corporate / Human Resource / Lifecycle Management / Probation

Base URL: `api/v1/corporate/human-resource/probations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| POST | `/{id}/assessments` | Ajukan penilaian → instance `T15` | `WorkforceProbation:Submit` |
| PUT | `/{id}/assessments/{assessmentId}` | Perbaiki penilaian Dikembalikan lalu ajukan ulang | `WorkforceProbation:Submit` |
| POST | `/{id}/assessments/{assessmentId}/withdraw` | Tarik penilaian + instance | `WorkforceProbation:Submit` |

#### Corporate / Human Resource / Workflow Management / Approval Inbox

Base URL: `api/v1/corporate/human-resource/approval-inbox`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| GET | `/{assignmentId}` | Detail tugas, termasuk `workflow.requestContextJson` dan alasan penolakan | `ApprovalInbox:Read` |
| POST | `/{assignmentId}/approve` | Setujui | `ApprovalInbox:Approve` |
| POST | `/{assignmentId}/request-revision` | Minta revisi (catatan wajib) | `ApprovalInbox:RequestRevision` |
| POST | `/{assignmentId}/reject` | Tolak (alasan wajib) | `ApprovalInbox:Reject` |

Endpoint batch data awal (`initial-contract-batches/{id}/submit`, `/cancel`) tidak berubah bentuknya.

---

## 8. Testing

| No | Pemeriksaan | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | Uji integrasi: ajukan T16 dengan langkah Jabatan | Instance `InProgress`, satu tugas untuk pemegang jabatan Kepala Bagian SDM; usulan menyimpan `WorkflowInstanceId` instance itu |
| 2 | Uji integrasi: setujui Perpanjang | Instance `Completed`; usulan Disetujui; tepat satu draf kontrak lanjutan; panggil sinkron dua kali → tetap satu |
| 3 | Uji integrasi: setujui Tidak Diperpanjang | Usulan Disetujui; satu kasus pemberhentian `ContractNonRenewal` |
| 4 | Uji integrasi: minta revisi → perbaiki → ajukan ulang → tolak | Dikembalikan → Diajukan (instance sama) → Ditolak; tidak ada kontrak lanjutan |
| 5 | Uji integrasi: definisi tanpa langkah | 400/404 dengan pesan; tidak ada usulan tersisa; pengajuan ulang sesudah langkah dibuat berhasil |
| 6 | Uji integrasi: pengaju = penyetuju | Tidak ada tugas untuk pengaju; penerapan menolak bila dipaksa |
| 7 | Uji integrasi T15 Lulus / Tidak Lulus, T17 setuju (isi kontrak berubah), T18 setuju (baris jadi kontrak, sekali) | Sesuai `HRD-DEC-085`/`089` |
| 8 | Uji integrasi: penilaian kedaluwarsa | Penilaian `ClosedOnExpiry`, instance `Withdrawn`, tugas hilang dari Kotak Masuk |
| 9 | `dotnet build` (OutDir scratch karena `bin` terkunci dev server) dan seluruh `HumanResourceTests` | 0 error; tidak ada test lama yang baru gagal (pembanding HEAD) |
| 10 | ESLint file frontend yang berubah | Tanpa error baru |
| 11 | Unit test frontend dibanding HEAD | Tidak ada test baru yang gagal; `lifecycle-approval-wiring.test.mjs` lulus |
| 12 | `npm run build` | Berhasil |
| 13 | Browser — Yudha ajukan Perpanjang kontrak Rizky | Toast sukses; panel Keputusan Terakhir "Diajukan" tetap ada sesudah muat ulang |
| 14 | Browser — Hartini di Kotak Masuk | Baris `T16: Keputusan Kontrak`; dialog Setujui menampilkan ringkasan tanpa gaji; Buka dokumen membuka detail kontrak |
| 15 | Browser — Hartini Setujui | Draf `…-R1` muncul di Perjanjian Kerja; panel Keputusan Terakhir Disetujui + nomor kontrak lanjutan |
| 16 | Browser — Yudha Catat Pemberitahuan + bukti | Kolom H-7 di Perpanjangan Kontrak menjadi Tepat Waktu |
| 17 | Browser — usulan Desi: Hartini Minta Revisi → Yudha Perbaiki & Ajukan Ulang → Hartini Tolak dengan alasan | Status mengikuti; tombol Ajukan Keputusan aktif lagi sesudah Ditolak |
| 18 | Browser — Endang nilai masa percobaan Bagas → Hartini Setujui | Masa percobaan Lulus |
| 19 | Browser — Yudha tidak melihat tugas miliknya; Novi tetap 403 | Sesuai |
| 20 | Lebar ponsel 390 px | Kotak Masuk dan panel Keputusan Terakhir tanpa scroll horizontal |
| 21 | Regresi komponen bersama | Kotak Masuk untuk SPPD dan Cuti tetap tampil dan bisa diputus; dialog lama tanpa `summary` tidak menampilkan blok kosong |

Uji no. 13–19 menulis DB dev (usulan, penilaian, kontrak lanjutan pada pegawai contoh `[SEED-SDM-1125]`).
Data ikut terhapus oleh `sdm-1125-cleanup.sql`; kasus pemberhentian dari uji Tidak Diperpanjang tidak
termasuk pembersih itu dan dicatat terpisah.

---

## 9. Di luar cakupan

| Hal | Alasan |
| --- | --- |
| Kepala Ruangan mengajukan keputusan kontrak (`HRD-DEC-089` butir 2) | `SubmitDecisionAsync` belum membatasi unit; memberi izin itu ke Kepala Ruangan berarti ia bisa mengajukan untuk unit lain. Perlu perbaikan cakupan tersendiri. Untuk sekarang pengaju = Staf Administrasi Kepegawaian |
| Perubahan gaji lewat kontrak lanjutan | `HRD-DEC-089` butir 5: lewat penetapan gaji/SK |
| Konfigurasi langkah T9–T13 (Perencanaan Tenaga Kerja) yang juga 0 langkah | Modul lain; dicatat sebagai temuan |
| Role Identity `SiteHr` dan pengisian `HospitalSiteId` penempatan | Perubahan data organisasi lintas modul; keputusan 3 tidak bergantung padanya |
| Pemberitahuan in-app/SignalR ke penyetuju saat tugas baru masuk | Kotak Masuk sudah menjadi tempat kerja; pemberitahuan menyusul bila diminta |
| Data Cetak kontrak non-rekrutmen menampilkan "-" | Temuan terpisah dari pengujian 25 Nov; tidak terkait persetujuan |

---

## 10. Hasil implementasi (25 November 2026)

### 10.1 Penyimpangan dari rencana

| Hal | Rencana | Yang dikerjakan | Alasan |
| --- | --- | --- | --- |
| Izin Minta Revisi Kepala Bagian SDM (Fase 0 butir 3) | Ditambahkan | Tidak diubah | Jabatan itu ternyata sudah memegang `ApprovalInbox:RequestRevision` (disalin dari Manajer HR) |
| Kode langkah | `T1x-SDM` | Tersimpan `T15_SDM` … `T18_SDM` | Layar/API master menormalkan kode (tanda hubung → garis bawah) |
| Penerapan T15 | Logika di `HandleWorkflowCompletedAsync` | Memanggil `HrdProbationService.ApplyAssessmentDecisionAsync` yang sudah ada | Service domain itu sudah punya penjaga maker-checker, `VR-ONB-58`, dan idempoten; logika ganda di integrasi dihapus |
| Usulan lama berstatus Diajukan tanpa instance | Tidak disebut | `PUT …/decisions/{id}` dan `PUT …/assessments/{id}` boleh memperbaiki dokumen itu lalu mengirimnya sebagai instance baru | Jalan pemulihan untuk dokumen yang tersangkut sejak sebelum perbaikan |
| Satu usulan/koreksi terbuka per kontrak | Tidak disebut | `VR-ONB-37` dan cek koreksi terbuka ikut menghitung status Dikembalikan | Usulan yang dikembalikan masih memegang instance; usulan baru akan bentrok |
| Penyelarasan terjadwal | Hanya instance yang sudah final | Semua dokumen terbuka yang punya instance diselaraskan tiap jam | Penangan idempoten; lebih sederhana dan juga menangkap Dikembalikan yang tertinggal |
| Teks konfirmasi Tarik Usulan | Lewat `message` ConfirmModal | Ditulis sebagai isi modal | Temuan uji browser: `ConfirmModal` tidak menampilkan `message` bila modal punya isi lain |
| Lampiran penilaian masa percobaan | Tidak disebut | Keterangan "Opsional" diganti "Wajib untuk hasil Tidak Lulus" + tanda wajib | Menutup temuan lama (keterangan bertentangan dengan `VR-ONB-56`) |
| Peta kepatuhan H-7 | Di layar Perpanjangan Kontrak | Dipindah ke konstanta bersama `NOTICE_COMPLIANCE` | Dipakai juga oleh panel Keputusan Terakhir |
| Fase 5 | Uji ujung-ke-ujung dengan backend baru | **Belum dijalankan** | Backend yang berjalan masih kode lama; restart dan penulisan DB dev butuh izin terpisah |

### 10.2 Konfigurasi DB dev (Fase 0, lewat API, diizinkan bersama persetujuan rencana)

Skrip `QuilvianSystemFrontendDev/test-with-agy/scripts/sdm-1125-07-b6-workflow-config.mjs` (idempoten; ID dicatat di
`sdm-1125-data.json` → `b6`). Verifikasi baca-saja: `T15`–`T18` masing-masing 1 langkah sumber `Position` = Kepala
Bagian SDM, `AllowSelfApproval = false`, aktif, dan 3 alasan penolakan `CloseRequest`.

### 10.3 Hasil pengujian

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build` backend (OutDir scratch) | Berhasil, 0 error, tanpa peringatan baru di file yang diubah |
| Uji backend HR (salinan proyek uji di scratch) | 427 uji: **426 lulus**, 1 gagal karena uji itu mencari root repo dari folder output scratch (bukan regresi). Termasuk 11 uji baru `LifecycleApprovalWorkflowTests` (mesin workflow nyata di SQLite) dan 24 uji SPPD sebagai regresi refactor `WithdrawAsync` |
| Catatan proyek uji HR di HEAD | Tidak dapat dikompilasi: `WorkforcePayrollData*Tests.cs` merujuk tipe yang tidak ada; semua uji ber-DB gagal `no such function: btrim` karena `PrlOpeningTaxBalanceConfiguration.cs` (commit `77a78924`). Salinan scratch mengecualikan berkas payroll itu dan menambal harness SQLite; repo tidak diubah |
| ESLint frontend (9 berkas) | 0 error; 1 peringatan lama `react-hooks/set-state-in-effect` di `approval-inbox-view.jsx` (kode yang tidak diubah) |
| Pemeriksa CSS (5 view) | 0 temuan |
| Unit test frontend | 5.535 uji, 149 gagal — **0 kegagalan baru** dibanding baseline HEAD (2 uji yang tampak berbeda dilewati di salinan HEAD dan membaca berkas backend kasir/BPJS yang tidak diubah). Berkas baru `lifecycle-approval-wiring.test.mjs`: 7/7 lulus |
| `npm run build` | `✓ Compiled successfully`, exit 0 |
| Uji browser (Chrome headed, data GET tiruan, semua POST/PUT/DELETE dicegat) | 15/15 kasus fungsional PASS: Kotak Masuk (label T16, ringkasan + tautan, payload approve, Tolak wajib alasan, regresi SPPD), Detail Perjanjian Kerja (Dikembalikan, Perbaiki & Ajukan Ulang → PUT sesuai DTO, Tarik → DELETE, Disetujui + H-7, bukti wajib, notify + bukti multipart `file`), Masa Percobaan (Dikembalikan, `VR-ONB-56`, PUT multipart dengan `RowVersion`), Batch Dikembalikan → ajukan ulang. Satu bug aplikasi ditemukan dan diperbaiki (teks konfirmasi Tarik Usulan) |
| Tampilan dan ponsel 390 px | 3/3 PASS: panel Keputusan Terakhir desktop, detail kontrak 390 px tanpa elemen melebar, dialog ringkasan Kotak Masuk 390 px muat di layar |

Skrip dan screenshot: `QuilvianSystemFrontendDev/test-with-agy/scripts/b6-1125-*.mjs`,
`QuilvianSystemFrontendDev/test-with-agy/screenshots/b6-1125/`.

### 10.4 Sisa pekerjaan

1. **Fase 5** — restart backend dengan kode baru, lalu uji ujung-ke-ujung bagian 8 no. 13–19 dengan akun Yudha,
   Hartini, Endang, dan Novi (menulis DB dev pada data `[SEED-SDM-1125]`).
2. Proyek uji HR di HEAD perlu diperbaiki pemilik modul payroll (berkas `WorkforcePayrollData*Tests.cs` dan
   `btrim` di harness SQLite).
