# Register Masalah Terbuka — Modul Sumber Daya Manusia (SDM)

| Field | Nilai |
|---|---|
| Jenis dokumen | Daftar gabungan semua masalah yang **masih terbuka** di modul SDM, beserta pemilik, prioritas, dan rekomendasinya. Status resmi task tetap di [frontend-roadmap.md](frontend-roadmap.md), [frontend-unfinished-work.md](frontend-unfinished-work.md), dan [backend-unfinished-work.md](backend-unfinished-work.md) |
| Per tanggal | Rabu, 18 November 2026 |
| Source frontend | `QuilvianSystemFrontendDev` branch `HamzahV2`, commit `f7a1f0917` |
| Source backend | `NewQuilvianSystemBackend` branch `MHamzah`, commit `fcdd9f4d` |
| Sumber data | Laporan task `FE-HRD-R01` s.d. `FE-HRD-R46`, laporan uji live Gelombang 4 (`testing/test-by-agy/`), `backend-unfinished-work.md`, folder `docs/issue/` |
| Pembaca | Pemilik modul SDM, tim backend, tim frontend, dan penguji |

---

## 1. Ringkasan

Hingga 18 November 2026 tercatat **34 masalah terbuka**, ditambah **9 keputusan pemilik** yang masih ditunggu dan **3 isu lama** yang belum ditutup.

| Prioritas | Arti | Jumlah | Nomor |
|---|---|---:|---|
| **P0 — Kritis** | Alur utama berhenti total, ada celah keamanan, atau build gagal | 3 | `MSL-01` s.d. `MSL-03` |
| **P1 — Kepatuhan & data** | Data salah/tidak lengkap, aturan bisnis atau hak akses dilanggar | 10 | `MSL-04` s.d. `MSL-13` |
| **P2 — Kontrak backend belum ada** | Layar sudah siap, tetapi endpoint/kolom yang dibutuhkan belum disediakan backend | 14 | `MSL-14` s.d. `MSL-27` |
| **P2/P3 — Frontend** | Navigasi, pesan, dan kerapian | 6 | `MSL-28` s.d. `MSL-33` |
| **Lingkungan uji** | Data uji dev kosong | 1 | `MSL-34` |

| Pemilik | Jumlah masalah |
|---|---:|
| Tim backend | 26 (`MSL-01`, `02`, `04`–`27`) |
| Tim frontend | 7 (`MSL-03`, `28`–`33`) |
| Pemilik modul / penguji | 1 (`MSL-34`) + 9 keputusan |

**Lima hal yang paling mendesak:**
1. **`MSL-01`** — Plh tidak bisa ditunjuk sama sekali. Selama pejabat cuti, persetujuan cuti dan lembur bawahannya macet.
2. **`MSL-02`** — layar **Penilaian Kinerja Saya** selalu galat, dan endpoint-nya membuka peluang melihat penilaian orang lain.
3. **`MSL-03`** — `npm run build` frontend gagal karena satu impor yang salah, sehingga aplikasi tidak bisa dirilis.
4. **`MSL-04`** — setelah slip gaji diterbitkan, pegawai tetap tidak akan bisa melihat slipnya sendiri.
5. **`MSL-05`** dan **`MSL-06`** — daftar delegasi dan penilaian proctoring FPPE belum dibatasi sesuai pemiliknya.

---

## 2. Cara membaca dan memperbarui

- Setiap masalah punya nomor tetap `MSL-xx`. Nomor tidak dipakai ulang walaupun masalahnya sudah selesai.
- **Pemilik** adalah tim yang mengubah source: `BE` (backend), `FE` (frontend), atau `PO` (pemilik modul, untuk keputusan dan data uji).
- Bila masalah selesai, ubah kolom status menjadi `✅ <tanggal> — <tautan laporan>`, lalu pindahkan ringkasannya ke bagian 11. Jangan menghapus barisnya.
- Kode bukti ditulis sebagai `berkas:baris` pada commit di atas. Nomor baris bisa bergeser setelah commit berikutnya.

---

## 3. Masalah kritis (P0)

### `MSL-01` — Penerbitan surat kuasa Plh selalu gagal (HTTP 500)

| Field | Isi |
|---|---|
| Pemilik | BE |
| Status | Terbuka — menahan `FE-HRD-R45` (⛔) |
| Ditemukan | 18 Nov 2026, uji live Gelombang 4 ([laporan](../testing/test-by-agy/laporan-live-testing-gelombang4-operasional.md)) |

**Contoh di rumah sakit.** Kepala Ruang ICU akan cuti tiga hari dan menunjuk Ketua Tim sebagai Plh. Setelah ia menekan **Terbitkan Surat Kuasa**, muncul pesan teknis berbahasa Inggris, dan surat kuasa tidak terbit. Selama cuti, permohonan cuti dan lembur perawat ICU menumpuk tanpa ada yang bisa menyetujui.

**Alur yang gagal:**
1. Pejabat mengisi formulir Plh (pengganti, tanggal, cakupan "Semua jenis permohonan", alasan).
2. Frontend mengirim `POST /api/v1/hrd/workflow/delegations` dengan `delegatedRequestTypes: "ALL"`.
3. `ApprovalDelegationV2Service` membuat baris `TrxApprovalDelegation` tanpa mengisi `AppliesToAllWorkflows`, `WorkflowDefinitionId`, maupun `ScopeDefinitionJson`.
4. PostgreSQL menolak karena constraint `CK_TrxApprovalDelegation_WorkflowScope` (migration `20260730050557_initializeTrxWorkflowApproverAssignments`) mewajibkan salah satunya terisi (galat `23514`).

`[Tags("Corporate - HRD Workflow Delegations")]`

| Method | Path | Auth | Request | Respons saat ini |
|---|---|---|---|---|
| POST | `/api/v1/hrd/workflow/delegations` | JWT + `AccessPermission` | `{ delegateEmployeeId, startDate, endDate, delegatedRequestTypes, delegationReason }` | `500` `"An error occurred while saving the entity changes..."` |

**Rekomendasi.** Isi `AppliesToAllWorkflows = true` bila cakupan `"ALL"`. Untuk cakupan per jenis permohonan, isi `ScopeDefinitionJson`. Tambahkan test integrasi pembuatan delegasi. Setelah diperbaiki, jalankan ulang `test-with-agy/scripts/hrw4-1118-r45-delegation.mjs`.

### `MSL-02` — `GET my-appraisal`: galat untuk semua pengguna dan celah melihat penilaian orang lain

| Field | Isi |
|---|---|
| Pemilik | BE |
| Status | Terbuka — menahan `FE-HRD-R46` (🟡) |
| Bukti | `PerformanceAppraisalsController.cs:42-56`, `PerformanceAppraisalService.GetMyAppraisalAsync` |

**Contoh di rumah sakit.** Perawat membuka **Manajemen Kinerja → Penilaian Kinerja** untuk mengisi penilaian diri akhir tahun, tetapi layar menampilkan galat server. Hal yang sama terjadi di layar **Keberatan Penilaian**.

**Penyebab:**
1. Endpoint mengambil `workforceProfileId` dan `cycleId` dari **query string**, bukan dari akun login.
2. Frontend tidak mengirim keduanya, sehingga nilainya menjadi `Guid.Empty`.
3. Service mencari penilaian dengan ID kosong. Karena tidak ada, service **membuat baris baru di dalam GET** (`SaveChangesAsync`), lalu gagal karena foreign key, sehingga HTTP 500.
4. **Celah akses:** pengguna yang berhak `PerformanceAppraisal : ViewOwn` bisa mengisi ID pegawai lain di query, lalu membuka atau bahkan membuat lembar penilaian pegawai tersebut.

`[Tags("Corporate - HRD Performance Appraisals & Multi-Level Review")]`

| Method | Path | Auth | Request | Respons saat ini |
|---|---|---|---|---|
| GET | `/api/v1/corporate/hrd/performance/appraisals/my-appraisal` | JWT + `PerformanceAppraisal : ViewOwn` | query `workforceProfileId?`, `cycleId?` | `500` |

**Rekomendasi.** Tentukan profil pegawai dari akun login, seperti `OvertimeSelfServiceContextService` (email akun → `WorkforceProfile`), dan tentukan siklus aktif di server. GET tidak boleh membuat data; pembuatan lembar penilaian dipindah ke aksi POST yang eksplisit.

### `MSL-03` — `npm run build` frontend gagal

| Field | Isi |
|---|---|
| Pemilik | FE (pekerjaan Menu 07, commit `f7a1f0917`) |
| Status | Terbuka |
| Bukti | `src/components/view/hr/workforce-core/employee-job-descriptions-view.jsx:19` |

**Penyebab.** Berkas itu mengimpor `getTodayDate` dari `@/utils/hr/workforce-core/workforce-core-display-utils`, padahal fungsi tersebut tidak diekspor di sana. Turbopack menghentikan build dengan "Export getTodayDate doesn't exist in target module" dan tidak melaporkan galat lain.

**Dampak.** Aplikasi tidak dapat dibangun untuk rilis. `next dev` tetap jalan karena hanya meng-compile rute yang dibuka.

**Rekomendasi.** Pakai helper tanggal yang sudah ada (misalnya `todayInputValue` di `@/utils/hr/hr-display-utils`), atau tambahkan `getTodayDate` ke util tersebut. Setelah itu jalankan ulang `npm run build`.

---

## 4. Masalah kepatuhan dan data (P1)

### `MSL-04` — Slip gaji memakai ID akun login, bukan ID pegawai

| Field | Isi |
|---|---|
| Pemilik | BE |
| Bukti | `PayrollRunModernController.GetCurrentUserId` (`ClaimTypes.NameIdentifier`); `PayrollPayslipDistributionService.cs:53-55` (daftar) dan `:131` (verifikasi PIN) |

**Contoh.** Bulan depan slip gaji November diterbitkan. Ns. Mira membuka **Slip Gaji Saya**, tetapi daftarnya kosong. Saat ia memasukkan PIN tanggal lahirnya yang benar, server tetap menyatakan PIN salah, dan setelah tiga kali aksesnya terkunci 30 menit.

**Penyebab.** Endpoint slip mencocokkan ID akun login dengan `WorkforceProfileId`, `EmployeeId`, atau `MstEmployee.Id`. Pemeriksaan baca-saja pada 18 Nov 2026 untuk akun `mira.safitri@rsmmc.local` menunjukkan ketiganya **berbeda** dari ID akun.

`[Tags("HR Payroll - Self-Service Payslips")]` — `self-service/my-payslips`, `self-service/verify-pin`, `self-service/payslips/{id}/detail`, `self-service/payslips/{id}/download-pdf` di bawah `/api/v1/corporate/human-resource/payroll/runs`.

**Rekomendasi.** Petakan akun login ke profil pegawai (email → `WorkforceProfile` → `MstEmployee`) sebelum query slip, verifikasi PIN, rincian, dan unduhan.

### `MSL-05` — Daftar delegasi tidak disaring per pemberi kuasa

| Field | Isi |
|---|---|
| Pemilik | BE |
| Sumber | Laporan [FE-HRD-R33](../task/report/frontend/FE-HRD-R33.md) |

`GET /api/v1/hrd/workflow/delegations` (`[Tags("Corporate - HRD Workflow Delegations")]`) mengembalikan **semua** surat kuasa Plh tanpa menyaring pemberi kuasa. Akibatnya, seorang kepala ruang bisa melihat daftar Plh unit lain.

**Rekomendasi.** Saring berdasarkan pegawai login (sebagai pemberi atau penerima kuasa). Daftar lintas unit hanya untuk pemegang hak administrasi.

### `MSL-06` — Penilaian proctoring FPPE tidak dicocokkan dengan proctor yang ditunjuk

| Field | Isi |
|---|---|
| Pemilik | BE |
| Sumber | Laporan [FE-HRD-R32](../task/report/frontend/FE-HRD-R32.md) |

Backend tidak memeriksa bahwa `ProctorWorkforceId` sama dengan dokter proctor yang ditunjuk komite. Frontend sudah membatasi tombolnya, tetapi panggilan API langsung tetap diterima. Contoh: dokter lain bisa mengisi hasil proctoring seorang dokter yang sedang diawasi.

`[Tags("Corporate - HRD FPPE Incident & Focused Evaluation")]` — `/api/v1/corporate/hrd/clinical-performance/fppe/...`.

**Rekomendasi.** Tolak penilaian bila pengguna login bukan proctor yang ditugaskan, dengan `403` dan pesan Bahasa Indonesia.

### `MSL-07` — Restitusi: hak lihat klaim sendiri, anti self-approval, dan audit pengaturan

| Field | Isi |
|---|---|
| Pemilik | BE |
| Sumber | Laporan [FE-HRD-R40](../task/report/frontend/FE-HRD-R40.md) |

1. `GET claims` mewajibkan `RestitutionClaim : Read`. Pegawai yang hanya punya `ViewOwn` tidak bisa melihat riwayat klaim kacamata atau berobatnya sendiri.
2. Pencegahan pegawai menyetujui klaimnya sendiri (anti self-approval) belum ditegakkan server.
3. Pengaturan plafon tidak punya `PUT` khusus dan log audit perubahan.
4. Sinkron piutang Finance tidak dapat dipicu dari layar.

`[Tags("HR Deductions - Restitution Claims")]` — `/api/hr-deductions/...`.

### `MSL-08` — Presensi mandiri tanpa gerbang hak akses

| Field | Isi |
|---|---|
| Pemilik | BE |
| Sumber | Laporan [FE-HRD-R43](../task/report/frontend/FE-HRD-R43.md) |

`AttendanceSelfServiceController` (`[Tags("Self Services / Human Resource / Attendance")]`, `/api/v1/self-services/human-resource/attendance`) tidak memakai `AccessPermission`, sehingga menu **Presensi Saya** tidak bergerbang. **Rekomendasi:** tambahkan `AccessPermission` (misalnya `MyAttendance : Read`) dan pasang `requiredPermission` di menu.

### `MSL-09` — PDF slip gaji tidak berkata sandi (VR-PAY-24)

| Field | Isi |
|---|---|
| Pemilik | BE |
| Bukti | `PayrollPayslipDistributionService.GeneratePayslipPdfDocument` (sekitar baris 452) menyusun PDF mentah tanpa enkripsi |

VR-PAY-24 mewajibkan PDF slip berproteksi sandi. Layar lama menjanjikan "Password PDF: tanggal lahir DDMMYYYY", tetapi PDF dari server tidak berkata sandi; petunjuk itu sudah dihapus frontend pada 18 Nov 2026. **Rekomendasi:** pakai pustaka PDF yang mendukung enkripsi, lalu tampilkan kembali petunjuk sandi di layar.

### `MSL-10` — Pengaturan lembur hanya disimpan di memori server

| Field | Isi |
|---|---|
| Pemilik | BE |
| Sumber | Laporan [FE-HRD-R35](../task/report/frontend/FE-HRD-R35.md); `backend-unfinished-work.md` Kelompok C |

`OvertimeSettingService` menyimpan parameter di memori statis (`_cachedSettings`). Akibatnya pengaturan hilang saat server dimulai ulang, berbeda antar-instans, dan tidak teraudit. Karena itu layar **Pengaturan Lembur** dibuat baca-saja. `[Tags("HR Overtime - Settings")]`, `/api/v1/corporate/human-resource/overtime-settings`. **Rekomendasi:** simpan di tabel DB beserta jejak audit.

### `MSL-11` — Galat 500 meneruskan pesan teknis EF ke pengguna

| Field | Isi |
|---|---|
| Pemilik | BE |
| Contoh | `POST /api/v1/hrd/workflow/delegations` → `"An error occurred while saving the entity changes. See the inner exception for details."` |

Pengguna melihat kalimat teknis berbahasa Inggris dan tidak tahu harus berbuat apa. **Rekomendasi:** tangkap `DbUpdateException` di handler global, kirim pesan Bahasa Indonesia tanpa detail teknis, dan catat detailnya hanya di log server.

### `MSL-12` — Selisih uang muka perjalanan dinas tanpa nomor bukti wajib

| Field | Isi |
|---|---|
| Pemilik | BE |
| Sumber | Laporan [FE-HRD-R34](../task/report/frontend/FE-HRD-R34.md) |

`PaymentOrRefundRef` tidak diwajibkan server saat ada selisih lebih atau kurang bayar. Frontend sudah mewajibkannya, tetapi panggilan API langsung bisa menyelesaikan settlement tanpa nomor bukti setor atau bayar. `[Tags("Corporate - HRD Business Travel Settlements")]`.

### `MSL-13` — Worker latar tetap berjalan walau backend dijalankan sebagai Web

| Field | Isi |
|---|---|
| Pemilik | BE / infrastruktur |
| Bukti | `Program.cs`: 43 dari 66 `AddHostedService` (baris 722–1777) berada di luar blok `if (runBackgroundJobs)` (baris 1842) |

Env `Runtime__Role=Web` seharusnya hanya menyalakan API. Pada kenyataannya, sebagian besar worker (HR, `BilInvoiceSyncWorker`, `PrescriptionReservationExpiryWorker`, `ClinicalFactDispatchWorker`, dan lain-lain) tetap berjalan. Akibatnya setiap pengembang yang menyalakan backend untuk uji ikut menulis ke basis data dev bersama. **Rekomendasi:** pindahkan worker ke dalam gerbang `runBackgroundJobs`, kecuali yang memang dikonsumsi endpoint HTTP.

---

## 5. Kontrak backend yang belum ada (P2)

Layar-layar berikut sudah dibangun jujur: tidak ada data karangan, dan kekurangan dijelaskan di layar. Fungsinya baru lengkap setelah backend menyediakan kontraknya.

| No. | Area | Yang belum ada di backend | Contoh dampak di rumah sakit | Task tertahan | Keputusan terkait |
|---|---|---|---|---|---|
| `MSL-14` | Daftar lintas pegawai | Daftar pembatasan kerja & rekam kesehatan K3RS; daftar sanksi, skorsing, exit clearance; daftar sanggahan penilaian & pengajuan jenjang karier | Kepala K3RS tidak bisa menjawab "siapa saja perawat yang sedang dibatasi tugasnya?" tanpa membuka satu per satu | `R02`–`R04`, `R14`–`R16`, `R53` | `DEC-FER-05`, `K-02` |
| `MSL-15` | Unggah berkas | Struk perjalanan; nota restitusi; lampiran COB (SEP/kuitansi); dokumen kandidat & MCU; surat sakit; lampiran workflow; bukti koreksi presensi kuasa; logbook lembur; spreadsheet batch kontrak awal; nota pertimbangan override rekredensial (VR-OPP-19) | Pegawai harus menyerahkan kuitansi kacamata secara fisik ke HR | `R24`, `R25`, `R28`, `R32`–`R35`, `R37`, `R39`, `R40` | `DEC-FER-02` |
| `MSL-16` | Kredensial | Worklist peer reviewer (dengan butir kewenangan yang dinilai), worklist pleno komite, worklist SPK-RKK menunggu tanda tangan Direktur, verifikasi publik anonim via `VerificationHash`, daftar siklus rekredensial + OPPE | Direktur tidak punya daftar SPK-RKK yang menunggu tanda tangannya | `R30` | — |
| `MSL-17` | OPPE / FPPE / rekredensial | Daftar rapor OPPE per dokter/periode, daftar penugasan FPPE, daftar putusan rekredensial, log akses ditolak SIMRS | Ketua KSM tidak bisa melihat rapor OPPE seluruh dokternya | `R20`, `R32` | — |
| `MSL-18` | Perencanaan tenaga | Endpoint batal rencana tahunan, `availableActions` langkah Finance, flag melewati batas formasi, jumlah kursi terisi, riwayat rencana pemenuhan | Rencana tahunan yang salah tidak bisa dibatalkan dari layar | `R27`, `R36` | — |
| `MSL-19` | Presensi | Metrik rekap periode (total jam kerja, jam dinas luar, potongan terlambat, persen kepatuhan); kontrak self-service izin pulang cepat dan dinas luar yang terikat ke pegawai login | Ringkasan penutupan periode presensi tidak menampilkan persentase kepatuhan | `R28`, `R29` | — |
| `MSL-20` | Persetujuan terpadu | Simpan matriks persetujuan, VR-WF-09, ringkasan kuota di portal ESS, unduh PDF bukti persetujuan | Matriks persetujuan hanya bisa dilihat, tidak bisa diubah dari layar | `R33` | — |
| `MSL-21` | Perjalanan dinas | Opsi kategori biaya, opsi rekening uang muka (`BankAccountId`), total halaman daftar, pemeriksaan status | Petugas mengetik kategori biaya bebas karena tidak ada pilihan baku | `R34` | — |
| `MSL-22` | Layanan SDM | Pratinjau PDF surat draf, diff data lama/baru sebelum tiket diselesaikan (`FE-HRD-181` ⛔), direktori analis lintas organisasi | Analis menyelesaikan tiket ubah data tanpa melihat apa yang akan berubah | `R19`, `R31` | — |
| `MSL-23` | Cuti | `attachmentId` di kotak masuk persetujuan, endpoint penyesuaian saldo retrospektif | Atasan menyetujui cuti sakit tanpa bisa membuka surat dokternya dari kotak masuk | `R37` | — |
| `MSL-24` | Roster | `ShiftCode` di sel roster, `ExpectedResponseMinutes` on-call, jendela koreksi mundur dari pengaturan (kini tetap 3 hari) | Label shift memakai dua huruf pertama nama shift | `R38` | — |
| `MSL-25` | Rekrutmen | Endpoint daftar lamaran, filter `applicationId` pada seleksi, `CandidateId` di daftar seleksi, tahap persetujuan permintaan | Petugas mencari lamaran lewat nama kandidat, lalu menyaring manual | `R39` | — |
| `MSL-26` | Pelatihan | Persentase kepatuhan per pelatihan, jumlah *shift buddy safeguard* | Dasbor diklat tidak bisa menunjukkan berapa persen pegawai wajib yang sudah lulus pelatihan BHD atau PPI | `R05` | — |
| `MSL-27` | Lembur | Filter antrean anomali, verifikasi batch, estimasi beban finansial saat handoff ke payroll | Verifikasi lembur biometrik harus satu per satu | `R35` | — |

---

## 6. Masalah frontend (P2/P3)

| No. | Prioritas | Masalah | Contoh dampak | Rekomendasi | Sumber |
|---|---|---|---|---|---|
| `MSL-28` | P2 | Banyak butir menu tanpa `requiredPermission` sehingga terlihat oleh semua pegawai, padahal halamannya menolak. Terlihat oleh pegawai biasa: K3RS 8/8, Hubungan Industrial 7/7, Diklat 8/8, Kinerja 8/8, Onboarding 9/9, Manfaat 9/11, Cuti 6/7, Persetujuan V2 3/6, SPPD 4/8, Lembur 4/7 | Perawat membuka menu "Insiden Disiplin" lalu ditolak; navigasi membingungkan | Tambahkan `requiredPermission` sesuai `[AccessPermission]` backend di `menu-items.jsx`. Butir layanan mandiri tetap terbuka | [Uji live R44](../testing/test-by-agy/laporan-live-testing-gelombang4-klinis-keselamatan.md), [R45](../testing/test-by-agy/laporan-live-testing-gelombang4-operasional.md), [R46](../testing/test-by-agy/laporan-live-testing-gelombang4-pengembangan-administrasi.md) |
| `MSL-29` | P3 | **Pengaturan Perencanaan** yang dibuka pegawai tanpa hak lewat URL langsung tidak menampilkan pesan "belum punya hak"; pilihan rumah sakit kosong diam-diam (`403`) | Pegawai mengira datanya hilang | Tampilkan `InformationAlert` tanpa hak | [Uji live R42](../testing/test-by-agy/laporan-live-testing-gelombang4-perencanaan-tenaga-kerja.md) |
| `MSL-30` | P3 | Rute lama `/self-services/human-resource/overtime` tidak dirujuk menu maupun portal ESS (portal menunjuk `/hr/self-service/overtime`) | Dua layar untuk satu proses | Alihkan ke rute baru atau hapus | `DEC-FER-04` |
| `MSL-31` | P3 | Tombol "Ajukan cuti pengganti (CTO)" hanya membuka formulir cuti, tanpa mengisi jenis cuti otomatis | Pegawai memilih jenis cuti sendiri dan bisa salah pilih | Terima parameter jenis cuti lewat URL | [FE-HRD-R35](../task/report/frontend/FE-HRD-R35.md) |
| `MSL-32` | P2 | Kisi roster belum punya cara menambah shift ke sel kosong | Kepala ruang harus membuat ulang periode untuk menambah satu dinas | Tambahkan aksi "Tambah shift" pada sel kosong | [FE-HRD-R38](../task/report/frontend/FE-HRD-R38.md) |
| `MSL-33` | P3 | Status `FE-HRD-R14` s.d. `R20` di roadmap masih "live browser tertahan 401", padahal sejak 18 Nov 2026 login lokal berhasil dan layarnya sudah dibuka di Gelombang 4 | Pembaca roadmap mengira layar belum pernah dibuka | Perbarui status saat audit `FE-HRD-R47` | [frontend-unfinished-work.md](frontend-unfinished-work.md) |

---

## 7. Lingkungan uji

### `MSL-34` — Data uji di basis data dev tidak cukup, dan penulisan data belum diizinkan

| Field | Isi |
|---|---|
| Pemilik | PO + penguji |

- Tabel `PrlPayrollPayslip` berisi **0 baris** (dicek 18 Nov 2026), sehingga Slip Gaji tidak bisa diuji dengan data nyata.
- Data K3RS, Hubungan Industrial, dan kredensial di DB dev sedikit; kebanyakan daftar kosong.
- Pada Gelombang 4, pemilik hanya mengizinkan **satu** penulisan (delegasi Plh). Karena itu alur tulis di bawah ini belum teruji di browser per peran:

| Alur yang belum teruji | Task |
|---|---|
| Keputusan kelaikan kerja, catat insiden/sanksi, verifikasi PSV, keputusan FPPE, eksekusi sinkron SIMRS | `R44` |
| Setujui/tolak di kotak masuk V2, settlement SPPD, resolusi tiket, verifikasi lembur | `R45` |
| Simpan pengaturan dan permintaan headcount `S-D1` | `R42` |
| Uji per peran layar presensi `S-B1` | `R43` |
| Slip Gaji dengan PIN asli, rincian, dan unduh PDF | `R46` (juga menunggu `MSL-04`) |
| Tombol tulis Onboarding dan ESS | `R10`, `R25` |

**Rekomendasi.** Pemilik menetapkan akun uji per peran dan izin menulis data uji yang dapat dibersihkan (dengan penanda alasan "Uji ..."), atau menyediakan basis data uji terpisah.

---

## 8. Keputusan pemilik yang masih ditunggu

Rincian pilihan dan konsekuensinya ada di [frontend-unfinished-work.md bagian 9](frontend-unfinished-work.md#9-keputusan-yang-dibutuhkan-pemilik).

| Kode | Pertanyaan | Rekomendasi | Task tertahan |
|---|---|---|---|
| `DEC-FER-01` | Perlukah base component Drawer (dipakai 22 kali)? | A — buat `BaseDrawer` | `R48`, `R50` |
| `DEC-FER-02` | Perlukah base component unggah berkas (dipakai 10 kali)? | A — buat `BaseFileUpload` | `R49`, `R50`, juga `MSL-15` |
| `DEC-FER-03` | Ke mana layar anomali variansi dan pengaturan payroll lama? | A — bangun di modul `/payroll`, hapus 7 view lama | `R51` |
| `DEC-FER-04` | Alamat mana yang menjadi rute utama (`/hr/*`, `/hrd/*`, `/corporate/hrd/*`)? | A — alamat di menu menjadi utama, sisanya dialihkan | `R52`, juga `MSL-30` |
| `DEC-FER-05` | Tambah endpoint daftar lintas pegawai di backend? | A — tambah, dengan hak akses khusus data sensitif | `R53`, juga `MSL-14` |
| `DEC-FER-06` | Di mana modal hard-stop dan co-signature SIMRS dipasang? | A — di layar order modul klinis | `R54` |
| `DEC-FER-07` | Nasib `FE-HRD-014` dan `FE-HRD-019`? | A — tandai digantikan `S-C1` dan `S-C6` | `R55` |
| `K-02` | Daftar lintas pegawai untuk disiplin, pelatihan, kinerja | Lihat [plan-repair-001](../docs/plan-repair/plan-repair-001-layar-hr-kosong-salah-alamat-api.md) | `R56` |
| `K-03` | Perlakuan pemanggilan API tanpa padanan pasti | Lihat plan-repair-001 | `R56` |

---

## 9. Isu lama yang statusnya belum ditutup

Status di bawah adalah status yang **tercatat di dokumen isu**. Sebagian perbaikannya mungkin sudah dikerjakan lewat task R, sehingga perlu dicek ulang dan ditutup bila sudah tuntas.

| Isu | Judul | Status tercatat | Perlu dicek terhadap |
|---|---|---|---|
| [ISSUE-001](../docs/issue/issue-001-layar-hr-kosong-salah-alamat-api.md) | Layar HR tampak kosong: alamat API salah, pesan gagal tertelan, kolom tidak cocok | TERBUKA | `FE-HRD-R02` s.d. `R11` (alamat diperbaiki), Gelombang 4 (layar termuat) |
| [ISSUE-002](../docs/issue/issue-002-tampilan-workforce-core-tanpa-gaya.md) | Lima layar Administrasi Kepegawaian tanpa gaya (class Tailwind) | DALAM_PERBAIKAN | Pemeriksa `check-css-usage.mjs` pada layar Workforce Core |
| [ISSUE-004](../docs/issue/issue-004-kelola-data-saya-ess-workforce.md) | ESS Kelola Data Saya: alur usulan, validasi 400, tab informasi pribadi, footer menutupi isi | TERBUKA | [plan-repair-004](../docs/plan-repair/plan-repair-004-kelola-data-saya-ess-workforce.md) |

---

## 10. Keterbatasan backend yang sudah diterima

Butir berikut berstatus 🟡 di [backend-unfinished-work.md](backend-unfinished-work.md). Keputusannya sudah diambil, tetapi batasannya perlu diketahui pengguna:

- **Notifikasi** hanya lewat lonceng di aplikasi (`HrdNotification` + SignalR). Email dan WhatsApp belum tersedia (`BE-HRD-314`). Semua pengingat worker (radar lisensi, booster vaksin, eskalasi skorsing, dan lain-lain) memakai kanal ini.
- **Indikator OPPE** hanya ditarik untuk 4 tipe dari tabel pelayanan. Indikator KMKP dan clinical pathway belum punya tabel sumber (`BE-HRD-225`).
- **Kurs perjalanan dinas luar negeri** memakai master Keuangan `MstExchangeRate` (`BE-HRD-333`). Bila kurs belum diisi Keuangan, hitungan tidak bisa berjalan.
- **Pejabat untuk Plh otomatis** adalah manajer aktif yang punya bawahan (`BE-HRD-316`).

---

## 11. Sudah diperbaiki pada Gelombang 4 (riwayat)

| Masalah | Perbaikan | Bukti |
|---|---|---|
| Portofolio SKP crash (`q is not defined`) | Baris liar dihapus | [FE-HRD-R46](../task/report/frontend/FE-HRD-R46.md) |
| Pesan "Request failed with status code 404" di 4 layar ESS lama | Memakai pesan server (`getHrApiErrorMessage`) | FE-HRD-R46 |
| PSV menampilkan 0 saat radar lisensi gagal | Ringkasan "-" dan pesan galat | [FE-HRD-R44](../task/report/frontend/FE-HRD-R44.md) |
| Slip Gaji berdata contoh; PIN `123456` diterima saat API gagal; token PIN tidak dikirim | Ditulis ulang memakai API, header `X-Payroll-PIN`, nominal tersamar | FE-HRD-R46 |

---

## 12. Urutan penanganan yang disarankan

1. **Segera (sebelum rilis):** `MSL-03` (build), `MSL-01` (delegasi), `MSL-02` (my-appraisal + celah akses).
2. **Keamanan dan hak akses:** `MSL-04`, `MSL-05`, `MSL-06`, `MSL-07`, `MSL-08`, lalu `MSL-28` di frontend.
3. **Kepatuhan data:** `MSL-09`, `MSL-10`, `MSL-11`, `MSL-12`, `MSL-13`.
4. **Keputusan pemilik:** `DEC-FER-05` dan `DEC-FER-02` lebih dulu, karena membuka `MSL-14` dan `MSL-15` yang paling banyak menahan layar.
5. **Kontrak backend lainnya:** `MSL-16` s.d. `MSL-27` sesuai kebutuhan operasional rumah sakit.
6. **Uji ulang:** setelah `MSL-34` diselesaikan (akun dan data uji per peran), jalankan ulang uji live alur tulis, lalu audit akhir `FE-HRD-R47`.
