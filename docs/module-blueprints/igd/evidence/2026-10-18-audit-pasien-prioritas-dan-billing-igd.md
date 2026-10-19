# Audit Pasien Prioritas dan Billing IGD — 18 Oktober 2026

| Field | Nilai |
| --- | --- |
| Jenis | Audit baca-saja atas source (pola `trace-existing-capabilities`), dicatat lewat `manage-module-blueprint` |
| Tanggal | 18 Oktober 2026 |
| Snapshot | Backend `rizkiG` `6bb18fe6` + working tree `BE-IGD-083` (belum di-commit); frontend `RizkiV2` `58ee55d32` |
| Dasar | Acuan alur pasien prioritas (PDF 7 langkah dari pemilik); `IGD-DEC-314`…`317` (16 Oktober 2026); slice Billing IGD `IGD-DEC-241`…`254` (11 Oktober 2026); `IGD-DEC-293`, `IGD-OQ-129` |
| Cara | Membaca source dan dokumen blueprint. **Nol kueri basis data, nol uji runtime, nol perubahan source** |
| Fakta yang lahir | `IGD-FACT-137`…`143` (decision log, bagian _Audit pasien prioritas dan billing IGD — 18 Oktober 2026_) |
| Sifat | **Rujukan tetap.** Sesi berikutnya membaca berkas ini sebelum menyentuh reservasi pra-kedatangan, jasa kunjungan IGD, atau billing IGD |

Berkas ini menjawab satu pertanyaan: **seberapa siap alur pasien prioritas dan kasir IGD untuk demo 20–21 Oktober 2026,
dan apa yang harus diperbaiki lebih dulu.**

---

## 1. Ringkasan untuk yang tidak punya waktu

Alur pasien prioritas di PDF dari pemilik punya tujuh langkah. Keadaannya per 18 Oktober 2026:

| No | Langkah PDF | Keadaan di aplikasi | Temuan |
| ---: | --- | --- | --- |
| 1 | Reservasi lewat WhatsApp atau telepon | Form reservasi ada (`BE-IGD-080`, `FE-IGD-067`). Belum pernah diuji lewat layar | A |
| 2 | Konfirmasi dan order ambulans | Isian ambulans ada di form reservasi | A |
| 3 | Admisi mendaftarkan pasien | **Cacat.** _Check-in_ melahirkan kunjungan IGD **tanpa encounter dan tanpa penjamin** | A |
| 4 | Didampingi petugas khusus | Isian petugas pendamping ada di reservasi, sebatas catatan | — |
| 5 | Prioritas medis sesuai triage | ✅ Triage tidak dipengaruhi membership (`IGD-DEC-315`) | — |
| 6 | Prioritas kamar rawat inap | **Belum ada** — tanpa kartu, tanpa kode | E |
| 7 | Bayar sama dengan rawat jalan | **Sebagian.** Jalur tagihan IGD ke invoice ada di source, tetapi belum terbukti runtime; jasa kunjungan IGD belum ada; diskon membership tidak dipakai di billing mana pun; pasien transfer menghasilkan dua invoice | B, C, D, E |

**Akibat terpenting.** Pasien yang didaftarkan lewat _check-in_ reservasi tidak dapat diberi diagnosis yang terhitung, tidak
dapat dikonfirmasi tindak lanjutnya, tidak dapat dibuatkan Form Permintaan Rawat Inap, dan tidak punya tagihan. Pasien
yang sama bila didaftarkan lewat **loket IGD** berjalan normal, karena loket membuat encounter beserta penjaminnya.

_Contoh._ Keluarga Tn. Hadi (member Platinum) menelepon hotline pukul 09.00; admisi mencatat reservasi dan memesan
ambulans. Pukul 09.40 Tn. Hadi tiba; petugas menekan _Konfirmasi Tiba_ dan kunjungan IGD `IGD-261020094012-…` lahir.
Dokter menulis diagnosis, tetapi diagnosis itu tidak terhitung karena kunjungan tidak punya encounter. Dokter memutuskan
rawat inap; perawat menekan _Buat Form_ dan ditolak _"Kunjungan ini belum punya pasien dan encounter. Lengkapi pendaftaran
lebih dulu."_ Keluarga datang ke loket untuk melengkapi pendaftaran, dan loket menolak membuat encounter IGD baru karena
Tn. Hadi dianggap sudah punya kunjungan IGD berjalan.

---

## 2. Temuan A — _check-in_ reservasi tidak melahirkan encounter (`IGD-FACT-137`, `138`)

### 2.1 Yang terjadi di source

| Butir | Isi | Bukti |
| --- | --- | --- |
| Pembuatan kunjungan | `CheckInReservationAsync` membuat `EmgVisit` baru berstatus `Arrived` dan pendaftaran `Registered`. **`EncounterId` tidak diisi**, dan tidak ada penjamin yang dicatat di mana pun | `EmergencyPreArrivalReservationService.cs` `:359`–`460`, objek baru `:419`–`443` |
| Tidak bisa disusulkan | Mengubah `EncounterId` lewat `PUT …/emergency-visits/{id}` ditolak `409` karena dianggap mengubah identitas kunjungan | `EmergencyVisitController.cs` `:615`; `EmergencyVisitService.IdentitasKunjunganBerubah` `:1234` |
| Diagnosis | Diagnosis dihitung per encounter (`KueriDiagnosisTerhitung`), sehingga kunjungan tanpa encounter tidak punya diagnosis terhitung dan tindak lanjut tidak dapat dikonfirmasi | Audit pemilik 18 Oktober 2026 |
| Form rawat inap | _Buat Form_ ditolak `409` _"Kunjungan ini belum punya pasien dan encounter. Lengkapi pendaftaran lebih dulu."_ | `EmergencyDispositionService.AdmissionRequest.cs` `:46`, `:162`–`163` |
| Tagihan | Folio dan invoice dibangun per encounter (`BillingClinicalChargeBridgeService.BuildPlanAsync` `:178`–`185`), jadi kunjungan ini tidak punya tagihan sama sekali | Source jembatan |
| Loket | Loket menolak encounter IGD baru untuk pasien yang punya kunjungan IGD berjalan (`EmergencyEpisodeRule`), kecuali petugas mengisi alasan _override_ | `EmergencyEpisodeRule.GuardEncounterRegistrationAsync` `:108`–`158` |
| Episode ganda | _Check-in_ hanya menolak bila pasien punya **kunjungan** berjalan (`OpenEpisodeKind.Visit`). Bila pasien sudah didaftarkan loket dan encounternya masih menunggu triage, _check-in_ tetap membuat kunjungan kedua tanpa encounter | `EmergencyPreArrivalReservationService.cs` `:382`–`390` |
| Layar | Hook layar hanya memanggil _check-in_ lalu menampilkan nomor kunjungan; tidak ada langkah menyusulkan encounter atau penjamin | `use-emergency-pre-arrival.jsx` `:291`–`325` |

### 2.2 Endpoint yang terlibat

```http
POST /api/v1/health-services/emergency-installation-management/pre-arrival-reservations/{id}/check-in
Hak akses: EmergencyPreArrivalReservation : Update
Body (CheckInEmergencyPreArrivalReservationRequest, semua opsional):
  { "arrivalDateTime": "2026-10-20T02:40:00Z", "arrivalModeId": "…", "caseTypeId": "…",
    "chiefComplaint": "Nyeri dada", "arrivalLocation": "Drop-off IGD", "notes": "…" }
200 ApiResponse<EmergencyPreArrivalReservationResponse> — status reservasi Arrived (2), resultEmergencyVisitId terisi
400 reservasi bukan PendingArrival, pasien belum ditautkan, atau pengaturan IGD tidak ada
404 reservasi tidak ditemukan
409 pasien punya kunjungan IGD lain yang masih berjalan
```

Yang **tidak** dilakukan endpoint ini hari ini: membuat atau menautkan `RegPatientEncounter` berjenis `Emergency`, dan
mencatat penjamin (tunai, asuransi, PKS).

### 2.3 Status task yang dikoreksi

| Task | Status lama | Status baru | Alasan |
| --- | --- | --- | --- |
| `BE-IGD-080` | ✅ (ditulis sesi lain 16 Oktober 2026) | 🟡 | Outcome kartu menjanjikan kunjungan IGD **dan encounter** gawat darurat; source hanya membuat kunjungan. Acceptance 4–7 hanya dipetakan ke nama method di laporan task, tanpa bukti runtime — log laptop 17 Oktober 2026 hanya memuat `GET` `200`. Acceptance 1–3 dan 8 terpenuhi dari source, migration `20261016090645_AddEmgPreArrivalReservation`, dan laporan build |
| `FE-IGD-067` | ✅ (ditulis sesi lain 16 Oktober 2026) | 🟡 | Acceptance 7 (uji layar Chromium) tertulis _"Komponen diverifikasi siap uji browser"_ — itu bukan uji. Outcome _"otomatis membuka kunjungan IGD aktif"_ bergantung pada perbaikan backend. Acceptance 1–6 tetap tercatat dari laporan task |

Laporan task `task/report/backend/BE-IGD-080.md` dan `task/report/frontend/FE-IGD-067.md` milik build skill dan
**tidak** disunting; koreksi ditulis di kartu, register, dan traceability.

### 2.4 Pilihan perbaikan — menunggu pemilik (`IGD-OQ-134`)

| Opsi | Isi | Untung | Rugi |
| --- | --- | --- | --- |
| 1 | _Check-in_ membuat encounter `Emergency` beserta penjamin; form reservasi menangkap penjamin | Satu klik, sesuai `IGD-DEC-314` (C) | Pembuatan encounter milik modul Pendaftaran; IGD perlu jalur resmi dari Pendaftaran, bukan menyalin logika loket. Paling besar untuk tenggat 20 Oktober |
| 2 | _Check-in_ mewajibkan encounter dari loket, lalu menautkannya saat kunjungan dibuat | Memakai jalur loket yang sudah teruji (encounter + penjamin); kunjungan lahir sudah tertaut, sehingga larangan `409` di atas tidak tersentuh | Dua langkah bagi petugas: loket dulu, lalu _check-in_ |
| 3 | SOP sementara: pasien didaftarkan lewat loket, reservasi dibatalkan | Nol kode; aman untuk demo | Reservasi tidak tersambung ke kunjungan; janji _satu klik_ belum terpenuhi |

---

## 3. Temuan B — jasa kunjungan IGD belum ada (`IGD-FACT-139`)

| Butir | Isi | Bukti |
| --- | --- | --- |
| Keputusan | Jasa kunjungan IGD ditagih sekali per kunjungan, sudah termasuk pemeriksaan dokter jaga, dikirim saat DPJP pertama ditetapkan (`IGD-DEC-242`). Menjadi satu-satunya isi yang dijanjikan pada demo Kasir IGD 21 Oktober (`IGD-DEC-251`, menggantikan isi rilis `IGD-DEC-247`/`248`) | Decision log |
| Task | **Tidak ada** di roadmap backend maupun frontend | Register status task |
| Kode | **Tidak ada.** Pencarian _jasa kunjungan_ di seluruh source kosong; tidak ada seeder item atau tarif | `grep` source |
| Jasa dokter | Fakta jasa konsultasi per catatan dokter sengaja tidak dikirim untuk encounter `Emergency` (`IGD-DEC-229`). Akibatnya, sampai jasa kunjungan dibuat, **jasa dokter IGD tidak tertagih sama sekali** | `ConsultationFinalizationService.cs` `:222` |
| Observasi | Observasi per periode (`IGD-DEC-243`) tetap rilis berikutnya | `IGD-DEC-247`, `251` |
| Desain | Slice Billing IGD **belum pernah didesain**: tidak ada bagian di `02`, `03`, `04`, maupun kontrak. Manifest mencatatnya di `belum_direncanakan` | Manifest |
| `IGD-UNK-20` | Decision log mencatatnya _dijawab_ pada 11 Oktober 2026 (jalur fakta klinis → folio → jembatan). Yang belum diputuskan adalah **bentuk** jalurnya untuk jasa kunjungan — lihat 3.1 | Decision log `IGD-FACT-094`, `098` |
| `IGD-UNK-19` | Sisi data **masih terbuka**: belum ada catatan bahwa item _Jasa kunjungan IGD_ dan tarifnya sudah dibuat di _Master Tindakan_ dan _Master Tarif_. Tanpa itu, baris tagihan masuk antrean rekonsiliasi `TARIFF_NOT_FOUND` dan tidak tampil di kasir (`IGD-DEC-250`) | Decision log 11–12 Oktober 2026 |

### 3.1 Dua bentuk jalur yang tersedia di source (`IGD-OQ-135`)

Jembatan sudah menerima kunjungan IGD (`BillingClinicalChargeBridgeService.cs` `:196`, `BE-BKC-FIX-016`) untuk sembilan
jenis sumber, tetapi tidak satu pun berjenis _jasa kunjungan_.

| Bentuk | Cara kerja | Perubahan Billing | Catatan |
| --- | --- | --- | --- |
| **J2 — tindakan otomatis** | Saat DPJP pertama ditetapkan, IGD mencatat satu tindakan berstatus selesai dengan item master _Jasa kunjungan IGD_, lalu menerbitkan fakta tindakan lewat jalur yang sama dengan tindakan perawat IGD (`EnsureCompletedFactAsync`). Harga dicari menurut `ProcedureId` dan kelas pasien | **Nol** — jalur tindakan dan cara cari tarifnya sudah ada (`BillingSourceTariffResolver.cs` `:131`–`151`, `:498`) | Sejalan dengan `IGD-DEC-250` (item master tindakan). Baris ini ikut tampil di daftar tindakan pasien, jadi perlu penanda sumber tersendiri |
| **J3 — sumber tagih baru** | IGD menerbitkan fakta berjenis baru (misalnya `EmergencyVisit`) dengan kunjungan IGD sebagai agregatnya; Billing menambah jenis itu di kontrak sumber, gerbang jembatan, pemetaan domain, titik tagih, dan cabang harga | Sekitar delapan titik di `BillingManagement` (kontrak sumber, `IsCandidateSourceContext`, `MapDomain`, daftar domain gerbang, `MapBillableStatus`, pemilihan identitas butir, `BillingSourceTariffResolver`, label ringkasan) | Lebih rapi secara makna, tetapi menyentuh area Billing yang sedang dirapikan tim BKC; risiko lebih besar untuk 21 Oktober |

Kedua bentuk butuh **encounter** pada kunjungan. Pasien hasil _check-in_ reservasi (temuan A) tidak akan tertagih jasa
kunjungannya sampai temuan A diperbaiki.

---

## 4. Temuan C — tagihan IGD per jenis layanan dan penjamin (`IGD-FACT-140`, `141`)

### 4.1 Jalur per jenis layanan

| Jenis | Titik tagih | Keadaan source | Bukti runtime IGD |
| --- | --- | --- | --- |
| Tindakan oleh perawat IGD | Saat dicatat (langsung selesai) | Fakta diterbitkan sesudah tersimpan (`BE-BKC-FIX-019`) | Belum |
| Tindakan oleh dokter IGD | Saat dikerjakan | **Tidak tertagih.** Layar dokter IGD menyimpan tindakan tanpa `executeImmediately`, jadi tersimpan _Planned_; layar tidak pernah memanggil _execute_. `IGD-FACT-095` masih berlaku untuk tindakan dokter | Belum |
| Laboratorium | Spesimen diterima | Jembatan menerima kunjungan IGD | Belum |
| Radiologi | Kualitas citra diputuskan | Jembatan menerima kunjungan IGD | Belum |
| Obat dan BMHP | Resep difinalkan (tahap 1) dan diserahkan (tahap 2); pemakaian BMHP tanpa resep saat dicatat | Jembatan menerima kunjungan IGD. **Gerbang lunas Farmasi tetap berlaku** — resep IGD biasa belum dibebaskan (`IGD-DEC-249` belum dikerjakan), jadi obat IGD mengikuti pola rawat jalan: dibayar dulu, baru ditelaah dan diserahkan | Belum |
| Konsultasi, alat, bank darah | Masing-masing jalurnya | Jembatan menerima kunjungan IGD; jasa konsultasi per catatan dokter IGD tetap tidak dikirim (`IGD-DEC-229`) | Belum |
| Jasa kunjungan IGD | DPJP pertama | Belum ada (temuan B) | — |
| Biaya administrasi | Dihitung Billing dari invoice ber-layanan `IGD` | `AdministrationFeeCalculationService` tidak dibatasi jenis kunjungan | Belum |

Perubahan jembatan untuk IGD (`BE-BKC-FIX-015`, `016`, `019`) ada di branch, tetapi laporan BKC mencatat uji basis
datanya `NOT RUN`. **Tidak ada satu pun bukti runtime** bahwa tagihan pasien IGD sampai ke kasir.

_Contoh yang dapat diharapkan bila jalurnya benar._ Ny. Rina didaftarkan loket IGD sebagai pasien tunai. Perawat mencatat
pemasangan infus pukul 10.20, laboratorium menerima spesimen darah rutin pukul 10.40, dan dokter memfinalkan resep
ondansetron pukul 10.50. Pukul 11.00 kasir membuka _Invoice & Billing Kasir_, memilih label _IGD_, dan melihat satu invoice
dengan tiga baris (infus, darah rutin, ondansetron) ditambah biaya administrasi IGD. Tindakan yang dicatat **dokter**
(misalnya jahit luka) tidak akan tampil.

### 4.2 Penjamin

| Butir | Keadaan |
| --- | --- |
| Loket IGD | Modal loket IGD mendukung tunai, asuransi, dan PKS; penjamin disimpan di encounter |
| Billing | `BillingCoverageAdapter`, dokumen invoice asuransi dan PKS, serta `AdministrationFeeCalculationService` tidak dibatasi jenis kunjungan |
| Kasir | Layar kasir umum (`cashier-overview`) mengenal label `IGD` (`BillingInvoiceService.cs` `:2399`) |
| Layar IGD | Tab _Tagihan_ di layar pemeriksaan IGD masih berlencana _"Belum tersambung"_ — bukan janji demo; kasir memakai layar Billing (`IGD-DEC-253`) |

### 4.3 Endpoint baca untuk uji

```http
GET /api/v1/health-services/billing-management/billing/invoices/cashier-overview
GET /api/v1/health-services/billing-management/billing/invoices/encounters/{encounterId}/charge-summary
GET /api/v1/health-services/billing-management/billing/charge-reconciliations
```

Dipakai putaran uji K1 (panduan uji sisa, bagian 3E) untuk membuktikan baris tagihan per pasien dan menangkap baris yang
berhenti di antrean rekonsiliasi.

---

## 5. Temuan D — pasien transfer menghasilkan dua invoice (`IGD-FACT-142`)

Invoice kunjungan IGD **tidak** ditautkan ke invoice rawat inap (`IGD-DEC-293`, ditahan sampai owner Billing menjawab
`IGD-OQ-129`). Jalur Transfer tidak membuat `InpAdmissionReferral`, satu-satunya pemicu penautan.

_Contoh._ Tn. Joko dirawat di IGD pukul 08.00–11.00 (infus, laboratorium), lalu di-_admit_ ke Bangsal Melati. Kasir
menyelesaikan **dua** invoice: invoice IGD (ditutup saat itu atau saat pulang) dan invoice rawat inap. Untuk demo, ini
perilaku yang dikenal dan perlu disampaikan, bukan cacat.

---

## 6. Temuan E — prioritas kamar dan diskon membership (`IGD-FACT-143`)

| Butir | Keadaan | Bukti |
| --- | --- | --- |
| Triage independen dari membership | ✅ `IGD-DEC-315` | Skema reservasi terpisah dari triage |
| Prioritas kamar rawat inap (`IGD-DEC-316`) | **Belum ada.** `FR-IGD-124` hanya ada di traceability (`BE-IGD-081`, `FE-IGD-068` tanpa kartu). Antrean Transfer diurutkan menurut waktu keputusan saja dan tidak membaca CITO maupun membership | `InpAdmissionTransferService.cs` `:46`; `InpAdmissionTransferService.Queries.cs`; `EmgAdmissionRequest.IsCito` `:39` |
| Bahan yang sudah ada | Tier membership punya `TierCode`, `PriorityQueue`, `PriorityLevel`; keanggotaan pasien punya status (`Active`) dan masa berlaku | `MstMembershipTier.cs` `:15`, `:32`, `:54`; `MstPatientMembership.cs` `:26`, `:34`, `:37` |
| Diskon membership (`IGD-DEC-317`) | **Belum ada di billing mana pun, termasuk rawat jalan.** Persentase diskon pada `MstMembershipTier` (`RegistrationDiscountPercent` … `PharmacyDiscountPercent`) tidak dibaca kode billing. `FR-IGD-125` hanya ada di traceability (`BE-IGD-082`, `FE-IGD-069` tanpa kartu) | `MstMembershipTier.cs` `:42`–`52` |

Sisi Rawat Inap prioritas kamar diusulkan sebagai butir 5 `IGD-REQ-003`. Diskon membership menunggu owner Billing dan
CRM, karena aturan potongannya belum ada bahkan untuk rawat jalan.

---

## 7. Dampak ke demo

| Demo | Bagian | Sikap yang disarankan |
| --- | --- | --- |
| Selasa 20 Oktober — pasien prioritas | Reservasi, ambulans, pendamping | Dapat diperagakan sesudah satu uji layar |
| | Pendaftaran | Tergantung `IGD-OQ-134`. Tanpa perbaikan: daftarkan lewat loket (opsi 3) |
| | Triage independen | Dapat diperagakan |
| | Transfer ke rawat inap | Jalur IGD terbukti untuk alur normal (`MVP-11` `READY_WITH_CONDITIONS`); pasien harus punya encounter |
| | Prioritas kamar | Hanya bila Rawat Inap mengerjakan butir 5 `IGD-REQ-003` pada 19 Oktober |
| Rabu 21 Oktober — Kasir IGD | Tindakan perawat, laboratorium, obat (tunai, asuransi, PKS) | Hanya bila putaran K1 lulus |
| | Jasa kunjungan IGD | Hanya bila kartu `BE-IGD-084` dibangun dan diuji, dan item serta tarifnya ada di master |
| | Tindakan dokter IGD | Tidak dijanjikan — tidak tertagih (temuan C) |
| | Pasien transfer | Dua invoice; disampaikan sebagai perilaku yang dikenal (temuan D) |
| | Diskon membership | Tidak dijanjikan (temuan E) |

---

## 8. Yang menunggu keputusan

| ID | Pertanyaan | Siapa |
| --- | --- | --- |
| `IGD-OQ-134` | Cara _check-in_ reservasi — opsi 1, 2, atau 3 (bagian 2.4) | Rizki Gunawan |
| `IGD-OQ-135` | Bentuk jalur jasa kunjungan IGD (J2 atau J3) dan letak penunjuk ke item _Jasa kunjungan IGD_ (`IGD-DEC-250`: _"ditetapkan pada desain"_) | Rizki Gunawan; owner Billing bila J3 |
| `IGD-UNK-19` (sisi data) | Item _Jasa kunjungan IGD_ dan tarifnya di _Master Tindakan_ dan _Master Tarif_ dev | Rizki Gunawan atau owner Billing, dilihat di layar |
| `IGD-OQ-136` | Sumber penanda pasien prioritas di Antrean Transfer untuk demo: dibaca langsung dari keanggotaan CRM, atau dibekukan di form IGD saat ditandatangani (`IGD-DEC-316` (A)) | Rizki Gunawan bersama Muhammad Hamzah |
| `IGD-OQ-129` | Penautan invoice IGD ke invoice rawat inap | Owner Billing (tetap terbuka) |
| `IGD-DEC-317` | Aturan diskon membership untuk billing IGD dan rawat jalan | Owner Billing dan owner CRM |

---

## 9. Batas audit

- Tidak diperiksa: isi basis data dev (termasuk master tarif), log backend selain yang disebut audit pemilik, layar kasir
  secara runtime, dan dokumen invoice asuransi/PKS secara runtime.
- Temuan diagnosis per encounter (`KueriDiagnosisTerhitung`) dan modal loket IGD dicatat dari audit pemilik 18 Oktober 2026
  dan tidak diulang.
- Laporan task tidak disunting (`task/report/**` milik build skill).

---

## 10. Keputusan pemilik sesudah audit — 18 Oktober 2026 (malam)

Pertanyaan di bagian 8 dijawab pemilik pada hari yang sama. Bagian 1–9 di atas tidak diubah dan tetap menggambarkan
keadaan saat audit.

| Pertanyaan | Jawaban | Tindak lanjut |
| --- | --- | --- |
| `IGD-OQ-134` cara _check-in_ | **`IGD-DEC-325` — opsi 2.** _Check-in_ memakai encounter IGD dari loket; tanpa encounter → `409` dengan pesan ke loket | Kartu `BE-IGD-085` (R3.19); uji P1 (panduan uji sisa bagian 3F) |
| `IGD-OQ-135` bentuk jalur jasa kunjungan | **`IGD-DEC-326` — J2 tindakan otomatis.** Penunjuk item lewat konfigurasi `EmergencyBilling:VisitFeeProcedureCode`; nol perubahan Billing | Kartu `BE-IGD-084` (R3.19); uji K1 (bagian 3E) |
| `IGD-UNK-19` sisi data | **Belum dicek.** Menjadi syarat uji K1 bagian 2 (SK-02) | Rizki atau owner Billing membuat item dan tarif lewat layar |
| `IGD-OQ-136` sumber penanda prioritas | **`IGD-DEC-327` — baca CRM langsung.** Penanda di form IGD (`IGD-DEC-316` (A)) ditunda | Usulan butir 5 [`IGD-REQ-003`](../approval-requests/2026-10-18-permintaan-pemilik-rawat-inap-transfer-igd.md) bagian 7 — belum dikirim |

Kontrak yang ditulis sesudah keputusan: API `0.20.0` §13, validation `0.20.0` §15, integration `0.11.0` §10 (manifest
bagian 0t, revisi blueprint `15`).
