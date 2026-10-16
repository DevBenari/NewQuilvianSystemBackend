# Kontrak Integrasi — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.10.0` — amandemen 15 Oktober 2026 (`IGD-DEC-308`, `309`), **Rencana (belum tersedia)**, status **`approved`** (`IGD-DEC-308`/`309`, Rizki Gunawan & Muhammad Hamzah, 15 Oktober 2026): bagian 9.6 — integrasi pemanggilan fungsi tolak admisi dan evaluasi penutupan susulan oleh Rawat Inap. *Sebelumnya:* `0.9.0` — amandemen 14 Oktober 2026 (`IGD-DEC-304`, `305`), **Rencana (belum tersedia)**, status **`approved`** (`IGD-DEC-306`, Rizki Gunawan, 14 Oktober 2026): bagian 9.5 — form bertanda tangan sebagai surat pengantar ke Antrean Transfer IGD/Poli. *Sebelumnya:* `0.8.0` — Form Permintaan Rawat Inap dari IGD, 12 Oktober 2026, **Rencana (belum tersedia)**, status **`approved`** (`IGD-DEC-294`): bagian 9 baru (`IGD-DEC-267`…`293`). *Sebelumnya:* `0.7.0` — Dokter jaga IGD, 11 Oktober 2026, **Rencana (belum tersedia)**, status **`approved`** (`IGD-DEC-240`): bagian 8 baru — IGD membaca jadwal dokter, unit layanan, klinik (Master Data) dan master dokter (Human Resource) secara sinkron tanpa salinan. Masukan: `00-interview-decisions.md` **240 keputusan**, terakhir `IGD-DEC-240`; capability map suplemen 3.5. *Sebelumnya:* `0.6.0` — Ruang Kerja Dokter IGD, 6 Oktober 2026, **Rencana (belum tersedia)**, status **`approved`** (`IGD-DEC-230`): bagian 7 baru — sepuluh modul disentuh secara sinkron tanpa salinan data; fakta tagih jasa konsultasi IGD tidak dikirim. Masukan: `00-interview-decisions.md` **229 keputusan**, terakhir `IGD-DEC-229` (grill-me 6 Oktober 2026 sore); capability map suplemen 3.4 (backend `43dab6da`, frontend `6680278a2`). *Sebelumnya:* `0.5.0` — penutupan kunjungan lewat disposisi, 23 September 2026, **Rencana (belum tersedia)**, status `draft`: bagian 6 baru (penutupan lewat disposisi, `IGD-DEC-163`…`169`). **Aditif** — nol modul baru yang disentuh. Sebelumnya `0.4.0` — encounter-first, 22 September 2026, **Rencana (belum tersedia)**. **Aditif pada dokumen**: bagian 5 baru; bagian 1–4 tidak diubah. Secara perilaku **memutus** untuk Registrasi (penolakan Emergency pada jalur umum, pintu encounter memanggil aturan IGD). Sebelumnya `0.3.0` |
| Status | `draft`, **kecuali bagian 9 (Form Permintaan Rawat Inap dari IGD) yang `approved`** (`IGD-DEC-294`, 12 Oktober 2026; amandemen 9.5 `IGD-DEC-306`, 14 Oktober 2026; amandemen 9.6 `IGD-DEC-308`/`309`, 15 Oktober 2026); `draft`, **kecuali bagian 8 (dokter jaga IGD) yang `approved`** (`IGD-DEC-240`, 11 Oktober 2026); **kecuali bagian 7 (Ruang Kerja Dokter IGD) yang `approved`** (`IGD-DEC-230`, 6 Oktober 2026); **kecuali bagian 5 (encounter-first) yang `approved`** (`IGD-DEC-157`, 22 September 2026). Bagian 6 **`approved`** (`IGD-DEC-170`, 23 September 2026) |
| Owner | Product/Domain Owner IGD: **Rizki Gunawan** (`IGD-DEC-089`) |
| `approved_by` / `approved_at` | **Rizki Gunawan & Muhammad Hamzah / 2026-10-15** — amandemen bagian 9.6 (`IGD-DEC-308`, `309`). **Rizki Gunawan / 2026-10-14** — amandemen bagian 9.5 (`IGD-DEC-306`). **Rizki Gunawan / 2026-10-11** — bagian 8 (dokter jaga IGD) lewat `IGD-DEC-240` (sementara, pola `IGD-DEC-174`). **Rizki Gunawan / 2026-10-06** — bagian 7 (Ruang Kerja Dokter IGD) lewat `IGD-DEC-230`. **Rizki Gunawan / 2026-09-22** — bagian 5 (encounter-first) lewat `IGD-DEC-157` — termasuk koreksi B1 pada §5.2. **Rizki Gunawan / 2026-09-23** — bagian 6 (penutupan lewat disposisi) lewat `IGD-DEC-170`. Bagian 1–4 tetap `draft` |
| Versi sebelumnya | `0.9.0`, `0.8.0`, `0.7.0`, `0.6.0`, `0.5.0`; sebelumnya `0.2.0` |

---

## 1. Modul yang disentuh

| Modul | Arah | Sifat | Menahan rilis |
| --- | --- | --- | :-: |
| Registration Management | IGD menulis dan membaca | Kolom baru `OriginEncounterId`; jenis kunjungan berubah | Tidak |
| Clinical Management | IGD menulis dan membaca | Pelonggaran kewajiban; penanda versi | **Ya** |
| Pharmacy Management | IGD menulis dan membaca | Pelonggaran kewajiban | **Ya** |
| Laboratory Management | IGD menulis dan membaca | Tidak ada perubahan — dipakai apa adanya | Tidak |
| Master Data | IGD membaca | Kolom baru `OrganizationUnitId` | Tidak |
| Corporate/HR | IGD membaca | Tidak ada perubahan | Tidak |
| Inpatient Management | Saling membaca | Modulnya belum ada | Tidak |
| Billing Management | Tidak disentuh | Penutupan klinis terpisah dari tagihan — `IGD-DEC-021` | Tidak |

---

## 2. Titik sentuh yang mengikat

### 2.1 IGD → Registration Management

| Kejadian | Yang dilakukan IGD | Kontrak |
| --- | --- | --- |
| Kunjungan IGD dibuat | Mengirim `encounterType = Emergency` | Ditolak `400` bila nilai lain |
| Kunjungan rawat inap dibuat dari disposisi `RANAP` | **Bukan IGD yang membuat.** Rawat Inap membuatnya dan mengisi `OriginEncounterId` dengan Id kunjungan IGD | `IGD-DEC-075`, `RWI-RULE-029` |
| Dokter aktif berubah | Memperbarui `RegPatientEncounter.DoctorId` sebagai nilai efektif, dalam transaksi yang sama dengan penulisan riwayat. *Nama tabel diselaraskan `IGD-DEC-132`* | `IGD-DEC-082` |

### 2.2 IGD → Inpatient Management

| Kejadian | Kontrak | Keputusan |
| --- | --- | --- |
| Disposisi `RANAP` dijalankan | Kunjungan IGD ditutup **dan** kunjungan rawat inap dibuat sebagai **satu tindakan utuh**. Gagal salah satu berarti tidak ada yang berubah | `IGD-DEC-067`, `RWI-RULE-029` aturan 4 |
| Serah terima gagal di tengah | Kunjungan IGD **tetap terbuka**; pasien tetap tercatat di IGD | `RWI-RULE-029` aturan 5 |
| Waktu tiba pasien | `InpBedPlacement` **membaca** waktu tiba dari kejadian `Arrived` pada catatan kepergian IGD; tidak menetapkan sendiri | `IGD-DEC-071` |
| Catatan klinis selama di IGD | **Tetap menempel** pada kunjungan IGD. Tidak dipindah, tidak disalin | `RWI-RULE-029` aturan 6 |
| Tempat tidur | Sepenuhnya milik Rawat Inap. IGD tidak memesan dan tidak mengubah `MstBed.BedStatus` | `IGD-DEC-069` |

> **Keadaan sekarang.** Modul Inpatient Management **belum ada di source**. Sampai ia dibangun,
> disposisi `RANAP` berhenti pada catatan kepergian IGD: kunjungan IGD dapat diselesaikan, dan
> kunjungan rawat inap tidak terbentuk. Ini **bukan** kegagalan, melainkan batas yang disadari.
> Petugas admisi membuka admisi rawat inap secara manual, sesuai jalan sementara yang sudah
> dicatat `RWI-CAP-038`.

### 2.3 IGD → Clinical Management dan Pharmacy Management

| Kebutuhan | Bentuk kontrak | Keputusan |
| --- | --- | --- |
| Pengkajian, konsultasi tanpa antrean | `QueueId` menjadi boleh kosong **untuk kunjungan bertipe `Emergency` dan `Inpatient` saja** | `IGD-DEC-068` |
| Diagnosis, tindakan, resep tanpa konsultasi antrean | `ConsultationId` menjadi boleh kosong dengan syarat tipe kunjungan yang sama | `IGD-DEC-068` |
| Perilaku rawat jalan | **Tidak berubah sedikit pun** | `IGD-DEC-068`, `RWI-RULE-026` aturan 6 |
| Koreksi catatan klinis | Penanda versi `IsEffective`, `Amends…Id`, `AmendmentReason` | `IGD-DEC-080` |

**Syarat mutlak:** perubahan ini menyentuh modul yang **bukan milik IGD** dan **bukan milik
Rawat Inap**. Pemilik kedua modul belum ditunjuk. Permintaan persetujuan **wajib diajukan
bersama** `DEC-INP-001` milik Rawat Inap — dua permintaan terpisah untuk pembatas yang sama
berisiko dijawab berbeda dan menghasilkan dua perilaku.

### 2.4 IGD → Laboratory Management

| Kebutuhan | Bentuk kontrak | Keputusan |
| --- | --- | --- |
| Memesan pemeriksaan | Memakai `POST api/v1/health-services/laboratory-management/lab-orders` apa adanya | `IGD-DEC-087` |
| Perubahan pada `LabOrder` | **Tidak ada.** IGD tidak menambah kolom dan tidak membuat entity tandingan | `IGD-DEC-087` |
| Status dan hasil pemeriksaan | **Tidak tersedia.** Bagian penunjang pada daftar sikap pesanan dinyatakan belum dapat dihitung sistem | `IGD-DEC-087` |

### 2.5 Corporate/HR → IGD

| Yang dibaca IGD | Berkas | Kontrak |
| --- | --- | --- |
| Profil pegawai milik pengguna | `Models/ApplicationUser.cs` `WorkforceProfileId` | Kosong berarti pengguna tidak dapat diberi kewenangan unit |
| Penugasan organisasi yang sedang berlaku | `WfpOrganizationAssignment` | Dibaca dengan penyaring `EffectiveStartDate <= sekarang` dan `EffectiveEndDate` kosong atau di masa depan |
| Simpul organisasi | `MstOrganizationUnit` | Dibandingkan dengan `MstServiceUnit.OrganizationUnitId` |

IGD **hanya membaca**. Tidak ada penulisan ke tabel milik Corporate/HR.

---

## 3. Perilaku saat gagal

| Kegagalan | Perilaku | Keputusan |
| --- | --- | --- |
| Pembuatan kunjungan rawat inap gagal setelah kunjungan IGD ditutup | **Tidak mungkin terjadi** — keduanya satu transaksi. Bila transaksi gagal, keduanya batal | `RWI-RULE-029` aturan 4 |
| Sistem tidak dapat dipakai saat pasien tiba | Petugas memakai catatan manual, lalu mencatat menyusul dengan waktu kedatangan sebenarnya | `IGD-DEC-065` |
| Pemberitahuan koreksi kejadian gagal terkirim | Koreksi **tetap berlaku**; kegagalan kirim tercatat sebagai pekerjaan yang belum tuntas | `IGD-DEC-085` |
| Master kelas pasien IGD belum diisi | Pendaftaran IGD ditolak dengan pesan yang menyebut master mana yang kurang | `IGD-DEC-076` |
| Simpul organisasi unit belum dipetakan | Tindakan yang dijaga kewenangan unit **ditolak `403`** (fail-closed) dengan pesan validation §7 aturan 3, yang meminta pemetaan unit ke Master Data; jalan keluar beralasan ditunda. Pelayanan klinis darurat tidak terpengaruh. *Diselaraskan 5 Oktober 2026; sebelumnya "Belum diputuskan — `IGD-OQ-071`"* | `IGD-DEC-092`, `IGD-DEC-195` |
| Data pesanan tidak dapat dibaca dari modul pemiliknya | Daftar sikap menampilkan keterangan bahwa daftarnya tidak lengkap, bukan daftar kosong yang tampak lengkap | `IGD-DEC-078` |

Prinsip yang berlaku di seluruh tabel: **konfigurasi atau bukti yang belum tersedia bersifat
menolak tindakan privileged, bukan menolak pelayanan klinis darurat.**

---

## 4. Yang sengaja tidak diintegrasikan

| Yang dipertimbangkan | Ditolak karena |
| --- | --- |
| Pemberitahuan realtime lewat `QueueHub` | `IGD-TRQ-07` menyatakannya `LATER SLICE`. Daftar pantau cukup dimuat ulang berkala |
| Pengiriman data ke SATUSEHAT | Di luar scope; belum ada keputusan dan belum ada modulnya |
| Integrasi tagihan pada penutupan klinis | `IGD-DEC-021` memisahkan keduanya |
| Penulisan ke tabel Corporate/HR | IGD hanya membaca |
| Pemindahan catatan klinis IGD ke kunjungan rawat inap | `RWI-RULE-029` aturan 6 melarangnya |

---

## 5. Encounter-first — baru pada `0.4.0`, **Rencana (belum tersedia)**

Titik sentuh lintas modul yang lahir dari `IGD-DEC-139`, `142`, `144`, `145`, `146`, `151`, `153`. Semuanya
integrasi **di dalam satu proses** (panggilan service/static method dalam satu `ApplicationDbContext`) —
nol antrean pesan, nol integrasi eksternal.

**Status bagian ini: `approved`** — `IGD-DEC-157`, 22 September 2026; terkunci hash (manifest bagian 2).

### 5.1 Modul yang disentuh

| Modul | Arah | Sifat | Pemilik | Menahan rilis |
| --- | --- | --- | --- | :-: |
| Registration Management | **Dua arah** | Pintu encounter memanggil aturan episode IGD; IGD menulis kolom status akhir `RegPatientEncounter` | Belum dipetakan — disentuh di bawah `IGD-DEC-135` | Tidak — tetapi butir 10 DoD PRD tetap "belum" |
| Medical Record Management | IGD memanggil | `ClinicalDocumentIntegrityService.LockOpenDocumentsForEncounterAsync` dipakai ulang | Pemilik Medical Record | Tidak |
| Billing Management | Billing membaca | Tidak ada perubahan; membaca `EncounterStatus` seperti hari ini | Pemilik Billing | Tidak |
| Blood Bank, Medical Record (pembaca status) | Membaca | Status `NoShow` otomatis dibaca tertutup | Pemilik masing-masing | Tidak |
| Master Patient | Tidak disentuh | Rekam pengganti dibuat lewat alur pasien baru yang **sudah ada**; penggabungan rekam **tidak** dirancang IGD | Belum dipetakan (`IGD-OQ-098`) | Tidak |

### 5.2 Titik sentuh yang mengikat

| Kejadian | Siapa memanggil siapa | Kontrak | Transaksi |
| --- | --- | --- | --- |
| Encounter Emergency dibuat | `PatientEncounterController.CreateEncounterCoreAsync` (Registrasi) → aturan episode IGD (static class `EmergencyEpisodeRule`, `02-backend-architecture.md` §13.4 — **dikoreksi 22 September 2026, B1 `IGD-DEC-157`**; sebelumnya ditulis "`public static` pada `EmergencyVisitService`") | Kunci per pasien → rumus episode → tolak `409` atau tulis catatan override IGD → **baru** kunci penomoran dan sisip encounter; tanpa `TrxQueue` | Transaksi Registrasi yang sudah ada (`:559`) — satu transaksi |
| Status encounter Emergency diubah lewat jalur umum | Registrasi menolak berdasarkan `EncounterType` | Validation §10.1 aturan 8–9 | — |
| Kunjungan IGD selesai / batal | `EmergencyVisitController` (IGD) menulis kolom status akhir `RegPatientEncounter` dan memanggil penguncian catatan Medical Record | Kolom yang ditulis: API §8.3.7 | Satu `SaveChanges` milik aksi kunjungan |
| Pasien pergi sebelum ditriage | IGD menulis `EncounterStatus = NoShow`, `NoShowAt`, `NoShowByUserId`, `NoShowReason` | Validation §10.3 | Satu `SaveChanges` |
| Rekonsiliasi K1 | IGD menulis `EncounterStatus` (+ `CompletedAt` dari bukti) dan catatan run IGD | API §8.4 | Satu transaksi per run |

**Kolom `RegPatientEncounter` yang boleh ditulis IGD — daftar tertutup.** `EncounterStatus`, `CompletedAt`,
`NoShowAt`, `NoShowByUserId`, `NoShowReason`, `IsCancel`, `CancelledAt`, `CancelledByUserId`, `CancelReason`,
`IsActive`, `UpdateBy`, `UpdateDateTime`. **Tidak ada kolom baru** pada tabel ini (`IGD-DEC-145`).

### 5.3 Urutan kunci — wajib seragam

| Jalur | Kunci yang diambil, berurutan |
| --- | --- |
| `POST /patient-encounters` bertipe Emergency | (1) kunci per pasien `EMG_EPISODE_{patientId}` → (2) kunci penomoran `REG_PATIENT_ENCOUNTER_NUMBER` yang sudah ada |
| `POST /emergency-visits` (jalur lama, pasien beridentitas) | (1) kunci per pasien — jalur ini **wajib dibungkus transaksi eksplisit**, karena hari ini hanya satu `SaveChanges` tanpa transaksi dan kunci transaksi akan lepas seketika |
| `POST /start-triage` | (1) kunci per pasien |
| `POST /no-show` | (1) kunci per pasien, lalu periksa ulang "belum punya kunjungan" **di dalam** kunci — mencegah NoShow dan Mulai Triage serentak menghasilkan kunjungan pada encounter yang sudah `NoShow` |
| `PATCH /patient-encounters/{id}/cancel` bertipe Emergency | (1) kunci per pasien, lalu periksa ulang "belum punya kunjungan" di dalam kunci — alasan yang sama |

Tidak ada jalur yang mengambil kunci penomoran lebih dulu lalu kunci per pasien — urutan terbalik dapat
menimbulkan *deadlock*.

### 5.4 Perilaku saat gagal

| Kegagalan | Perilaku |
| --- | --- |
| Penguncian catatan klinis gagal saat kunjungan diselesaikan | Seluruh penyimpanan batal — kunjungan **dan** encounter tidak berubah; petugas mengulang |
| Penulisan catatan override gagal | Encounter tidak dibuat |
| Kunci per pasien menunggu lama (pendaftaran serentak) | Permintaan kedua menunggu sampai yang pertama selesai, lalu membaca hasilnya — bukan galat |
| Registrasi mengubah pintu encounter tanpa memanggil aturan IGD | Penjaga episode hilang diam-diam — karena itu acceptance `BE-IGD-053` memuat uji paralel dan uji "klien tanpa pra-cek" |

## 6. Penutupan lewat disposisi — baru pada `0.5.0`, **Rencana (belum tersedia)**

Slice `IGD-DEC-163`…`169`. Seluruhnya integrasi **di dalam satu proses** dan **di dalam satu modul**: nol modul
baru yang disentuh, nol antrean pesan, nol integrasi eksternal.

**Status bagian ini: `approved`** — `IGD-DEC-170`, Rizki Gunawan, 23 September 2026; terkunci hash (manifest bagian
0j). Amendment 30 September 2026 (`IGD-DEC-171`, `172`) **tidak** mengubah bagian ini: titik sentuh observasi §6.2
tetap sama, hanya pemetaan status di dalam aksi observasi yang berubah (state §9.5).

### 6.1 Modul yang disentuh

| Modul | Arah | Sifat | Menahan rilis |
| --- | --- | --- | :-: |
| IGD (internal) | Di dalam modul | Empat titik pemicu memanggil satu method penutupan di `EmergencyVisitService` | — |
| Registration Management | IGD menulis | Encounter ikut tertutup lewat jalur `BE-IGD-051` yang sudah ada; daftar kolom tertutup §5.2 **tidak berubah** | Tidak |
| Medical Record Management | IGD memanggil | Penguncian catatan klinis ikut jalur `BE-IGD-051`; tidak dipanggil terpisah | Tidak |
| Bank Darah, Laboratorium | **Membaca** | Keduanya membaca status encounter dan menolak transaksi baru sesudah tertutup. **Nol perubahan** pada kedua modul (`IGD-DEC-169`) | Tidak |
| Rawat Inap | Tidak disentuh | Admisi membuat encounter `Inpatient` sendiri dan nol rujukan ke entitas IGD (capability map suplemen 3.3) | Tidak |

### 6.2 Titik sentuh yang mengikat

| Kejadian | Siapa memanggil siapa | Transaksi |
| --- | --- | --- |
| Disposisi berpindah ke `Executed` | `EmergencyDispositionController` → `EmergencyVisitService.TryCloseAfterDispositionAsync` → penjaga `EmergencyDispositionService.ValidateVisitClosureAsync` | Satu `SaveChanges` milik aksi disposisi |
| Observasi ditutup | `EmergencyObservationController` → method yang sama | Satu `SaveChanges` milik aksi observasi |
| Serah terima diterima/ditolak/dibatalkan | `EmergencyDepartureController` → method yang sama | Satu `SaveChanges` milik aksi kepergian |
| Sikap pesanan ditetapkan | `EmergencyDepartureController` → method yang sama | Satu `SaveChanges` milik aksi pesanan |
| Kunjungan tertutup | `EmergencyVisitService.ApplyEncounterClosureAsync` (sudah ada) menulis kolom status akhir encounter dan mengunci catatan klinis | Ikut `SaveChanges` yang sama |

Method penutupan **tidak** membuka transaksi sendiri dan **tidak** menyimpan sendiri — persis pola
`ApplyEncounterClosureAsync`. Akibatnya bila aksi pemicunya gagal disimpan, penutupan ikut batal, dan sebaliknya.

### 6.3 Perilaku saat gagal

| Kegagalan | Perilaku |
| --- | --- |
| Penjaga penutupan menolak saat disposisi dilaksanakan | Disposisi tetap `Executed`; kunjungan menunggu penutupan; **bukan** galat (`IGD-DEC-164`) |
| Penguncian catatan klinis gagal saat kunjungan ikut tertutup | Seluruh penyimpanan batal — aksi pemicunya ikut gagal dan petugas mengulang |
| Kunjungan sudah tertutup lebih dulu oleh jalur lain | Percobaan dilewati diam-diam; nol galat, nol penulisan ulang |
| Order darah atau pemeriksaan laboratorium ditolak sesudah kunjungan tertutup | Perilaku yang diterima (`IGD-DEC-169`); petugas mendaftarkan episode baru |

---

## 7. Ruang Kerja Dokter IGD — baru pada `0.6.0`, **Rencana (belum tersedia)**

Untuk `IGD-DEC-220`…`229`. Status **`approved`** (`IGD-DEC-230`, Rizki Gunawan, 6 Oktober 2026). Semua integrasi bersifat **sinkron dalam satu
permintaan HTTP ke modul pemiliknya**; tidak ada antrean, tidak ada transaksi lintas modul, dan tidak ada salinan data
milik modul lain di IGD.

### 7.1 Modul yang disentuh

| Modul | Titik sentuh | Arah | Perubahan di modul itu | Pemilik / persetujuan |
| --- | --- | --- | --- | --- |
| ClinicalManagement | Catatan dokter, diagnosis, CPPT, kajian medis, tindakan | Layar dokter → modul | (a) timeline catatan dokter per encounter IGD; (b) kajian medis untuk kunjungan IGD; (c) endpoint tindakan keperawatan IGD | Pemilik `OPEN`; sementara Rizki (pola `IGD-DEC-174`) |
| IGD → ClinicalManagement (baca) | Diagnosis untuk penjaga konfirmasi tindak lanjut | IGD membaca | Nol — IGD membaca `TrxPatientDiagnosis` baca-saja | Sama |
| Farmasi | Resep; penyelesaian catatan dokter | Layar dokter → modul | Penyelesaian catatan dokter tidak mengirim fakta jasa konsultasi untuk encounter `Emergency` (`IGD-DEC-229`) | Pemilik Farmasi meninjau pada desain |
| Rekam Medis | Addendum, *Catatan Saya* | Layar dokter → modul | Nol | — |
| Laboratorium, Radiologi | Pesanan, instruksi, verifikasi | Layar dokter/perawat → modul | Aturan instruksi untuk encounter IGD | `IGD-OQ-117` — pemilik Laboratorium; pemilik Radiologi (Yoga Aji Pratama) |
| Bank Darah | Pesanan darah | Layar dokter → modul | Nol. Unit IGD wajib berizin memesan darah di data master | `IGD-UNK-13` — Master Data |
| Hemodialisa | Pesanan hemodialisa | Layar dokter → modul | Nol | — |
| Gizi | Pesanan konsultasi gizi | Layar dokter → modul | Nol | `IGD-OQ-119` — pemilik Gizi |
| Billing | Fakta tagih dari catatan dokter dan tindakan | Modul klinis → Billing | Nol di Billing; jasa konsultasi IGD tidak dikirim | Billing IGD dirancang pada slice tersendiri |
| Registration Management | Jenis encounter (`EncounterType.Emergency`) | Dibaca | Nol | — |

### 7.2 Perilaku saat gagal

| Kejadian | Perilaku |
| --- | --- |
| Modul tujuan menolak (`400`/`403`/`409`) | Layar menampilkan pesan modul itu **apa adanya** di tempat aksi; isian tidak dikosongkan |
| Modul tujuan tidak dapat dihubungi | Aksi gagal tanpa efek di IGD; petugas mengulang. Tidak ada pencatatan sebagian di IGD |
| Fakta tagih gagal terkirim sesudah catatan dokter selesai | Perilaku yang sudah ada: catatan tetap sah, kegagalan dicatat sebagai masalah serah-terima Billing |
| Kirim ganda tindakan keperawatan | `idempotencyKey` yang sama mengembalikan baris yang sama (validation §12 aturan 12) |

### 7.3 Fakta tagih dari layar dokter IGD

| Sumber | Dikirim untuk pasien IGD? | Dasar |
| --- | :-: | --- |
| Jasa konsultasi per catatan dokter | **Tidak** | `IGD-DEC-229` — jasa dokter IGD ditetapkan slice billing IGD (`IGD-DEC-097`, `draft`) |
| Resep yang difinalkan bersama catatan dokter | Ya | `IGD-DEC-229`; perilaku Farmasi yang sudah ada |
| Tindakan dokter dan tindakan keperawatan | Dibaca Billing dari `TrxPatientProcedure` | `IGD-DEC-225`; perilaku Billing yang sudah ada — **tidak** diaudit ulang pada pass ini (`IGD-UNK-16`) |

### 7.4 Yang sengaja tidak diintegrasikan

| Tidak diintegrasikan | Sebab |
| --- | --- |
| Verifikasi pesanan perawat untuk bank darah, hemodialisa, gizi | `IGD-OQ-118` — ketiga modul belum punya status verifikasi instruksi; perawat tidak memesan ketiganya pada slice ini |
| Jasa dokter IGD ke Billing | Slice billing IGD |
| Tabel tindakan IGD (`EmgProcedureDetail`) ke Billing | `IGD-DEC-225` — tabel itu dibekukan, tidak ditambah lagi |

## 8. Dokter jaga IGD — baru pada `0.7.0`, **Rencana (belum tersedia)**

Untuk `IGD-DEC-237`…`239`. Status **`approved`** (`IGD-DEC-240`, Rizki Gunawan, 11 Oktober 2026). IGD **membaca** data master
milik modul lain di dalam permintaan yang sama: tidak ada salinan, tidak ada sinkronisasi, tidak ada penulisan.

### 8.1 Data yang dibaca

| Data | Pemilik | Ruas yang dipakai | Untuk |
| --- | --- | --- | --- |
| `MstDoctorSchedule` | Master Data layanan kesehatan (pemilik jadwal dokter **belum dipetakan**) | `DoctorId`, `ServiceUnitId`, `ClinicId`, `ScheduleType`, `PracticeDay`, `PracticeDate`, `StartTime`, `EndTime`, `IsOvernight`, `EffectiveStartDate`, `EffectiveEndDate`, `ScheduleStatus`, `IsActive`, `SessionName`, `SortOrder` | Dokter jaga pada T (validation §13.1) |
| `MstServiceUnit` | Master Data | `ServiceUnitType` | Mengenali unit IGD — **hanya** tipe `Emergency` (`IGD-DEC-237`) |
| `MstClinic` | Master Data | Unit induk klinik | Jadwal yang hanya menyebut klinik |
| `MstDoctor` | Human Resource | `IsActive`, `SpecialistName`, `FullName`, `DoctorCode` | Dokter aktif dan kelompok di luar jadwal (validation §13.2) |

### 8.2 Perilaku saat data tidak lengkap

| Keadaan | Perilaku | Siapa membereskan |
| --- | --- | --- |
| Unit IGD tidak bertipe `Emergency`, atau tidak ada jadwal IGD | Kelompok dokter jaga kosong; setiap penetapan menjadi override beralasan. Layanan tidak terhenti | Master Data / pemilik jadwal (`IGD-UNK-18`) — syarat rilis per lingkungan |
| `SpecialistName` dokter umum diisi tidak baku (misalnya "GP") | Dokter itu tidak masuk kelompok di luar jadwal, kecuali kelompok itu kosong (validation §13 aturan 6) | Pengurus master dokter HR |
| Jadwal pengganti (`SubstituteDoctorId`) | Tidak dibaca; dokter pada baris jadwal yang dihitung (`IGD-DEC-237` butir 4) | — |
| Cuti dokter | Tidak dibaca (`IGD-DEC-237` butir 4); jembatan identitas dokter–pegawai belum terbukti | Ditinjau bila jembatan itu ada |

## 9. Form Permintaan Rawat Inap dari IGD — baru pada `0.8.0`, **Rencana (belum tersedia)**

Untuk `IGD-DEC-267`…`293`. Status **`approved`** (`IGD-DEC-294`, Rizki Gunawan, 12 Oktober 2026); titik sentuh Rawat Inap dan Billing butuh
persetujuan pemiliknya (`IGD-OQ-128`). Seluruh integrasi berada di dalam satu aplikasi dan satu basis data; tidak ada
sistem eksternal.

**Koreksi bukti usang.** Kalimat §2.2 *"Modul Inpatient Management belum ada di source"* tidak lagi benar: modul itu ada dan
dipakai (`IGD-FACT-113`). Isi kontrak §2.2 lainnya tidak diubah; janji "satu tindakan utuh" ditunda `IGD-DEC-276`.

### 9.1 Modul yang disentuh

| Modul | Arah | Yang dipakai | Sifat |
| --- | --- | --- | --- |
| Rawat Inap | IGD → Rawat Inap | `InpAdmissionReferralService` — buat dan batal permintaan bersumber IGD; baca keadaan permintaan | Tulis lewat service pemilik, dalam transaksi IGD |
| Rawat Inap | Rawat Inap → IGD | `EmergencyDispositionService.BacaRingkasanPermintaanRawatInapAsync` (static) — rincian form untuk layar admisi | Baca; static agar tidak ada ketergantungan DI melingkar |
| Clinical Management | IGD ← Clinical | Alergi aktif, diagnosis aktif encounter, konteks penjamin (`EncounterInsuranceService.GetContextAsync`) | Baca |
| Patient Management | IGD ← Pasien | Identitas pasien, teks hak kelas (`MstPatientInsurance.ClassName`) | Baca |
| Master Data / HR / Identity | IGD ← master | Unit, kelas, jadwal dokter per unit, dokter, `ApplicationUser.DoctorId` | Baca |
| Billing | Rawat Inap → Billing | `BillingInpatientEventReceiver.EnsureEncounterLinksAsync` | Perilaku berubah: sumber IGD **tidak** ditautkan (`IGD-DEC-293`) |

### 9.2 Titik sentuh yang mengikat

| Kejadian | Kontrak | Keputusan |
| --- | --- | --- |
| Form ditandatangani | Permintaan admisi `Pending` bersumber IGD lahir **dalam transaksi yang sama**. Gagal salah satu berarti tidak ada yang berubah; form tetap *Menunggu TTD* | `IGD-DEC-275` |
| Form dibatalkan sesudah terkirim | Permintaan dibatalkan dalam transaksi yang sama; ditolak bila permintaan sudah `Completed` | `IGD-DEC-278` |
| Tindak lanjut `RANAP` dibatalkan | Form aktif dan permintaan `Pending` ikut batal dalam transaksi pembatalan | `IGD-DEC-280` |
| Petugas admisi membatalkan | Ditulis Rawat Inap; IGD membacanya sebagai *Dibatalkan admisi* dan menahan *Jalankan* lagi | `IGD-DEC-279` |
| Admisi dibuka dari permintaan | Sudah ada; IGD membacanya sebagai *Selesai* | `IGD-DEC-276` |
| Admisi tanpa permintaan | Sudah ada — `409` `INP-ADM-REF-001`; teks pesan dinetralkan | `IGD-DEC-275` |
| Tagihan IGD ke invoice rawat inap | Untuk sumber IGD ditahan; kamar pulih tidak berubah | `IGD-DEC-293`, `IGD-OQ-129` |
| Data pasien dibekukan | Dibaca sebelum transaksi; gagal baca = tanda tangan ditolak `503` | `IGD-DEC-283` |

### 9.3 Perilaku saat gagal

| Kegagalan | Akibat | Pemulihan |
| --- | --- | --- |
| Pembuatan permintaan admisi gagal (misal pasien sudah punya permintaan `Pending` lain) | Tanda tangan ditolak; tidak ada yang tersimpan | Perawat melihat pesannya; permintaan lain diselesaikan dulu |
| Bacaan modul lain gagal saat menandatangani | `503`; form tetap *Menunggu TTD* | Dokter menekan *Tandatangani* lagi |
| Klik ganda / jaringan putus saat menandatangani | `Idempotency-Key` sama → hasil pertama; index unik menjaga satu permintaan per form | — |
| Dua perawat membuat form bersamaan | Satu berhasil, satu `409` (kunci per kunjungan + index unik) | Muat ulang |
| `RowVersion` basi | `409` | Muat ulang |

Tidak ada antrean kirim ulang: seluruh titik sentuh sinkron di dalam satu transaksi basis data.

### 9.4 Yang sengaja tidak diintegrasikan

| Butir | Alasan |
| --- | --- |
| Pembukaan admisi otomatis (`INP-S09`) | Ditunda `IGD-DEC-276` |
| `OriginEncounterId` | Ditunda `IGD-DEC-276`; rangkaian dibaca lewat permintaan admisi |
| Penautan invoice untuk sumber IGD | Ditahan `IGD-DEC-293` sampai owner Billing menjawab `IGD-OQ-129` |
| Notifikasi ke dokter atau petugas admisi | `IGD-DEC-288`, G-07 |
| SEP / eligibilitas penjamin rawat inap | Di luar slice — Registrasi/Asuransi |
| Penanda "tidak ada alergi" | Clinical belum menyimpannya (`IGD-FACT-117`) |

### 9.5 Amandemen `0.9.0` — Antrean Transfer IGD/Poli (`IGD-DEC-304`, `305`)

**Status: `approved`** — `IGD-DEC-306`, Rizki Gunawan, 14 Oktober 2026; titik sentuh Rawat Inap butuh konfirmasi Muhammad Hamzah.
Menggantikan baris Rawat Inap dan Billing di 9.1, seluruh 9.2 dan 9.3, serta baris "Penautan invoice" di 9.4. Persetujuan owner
Billing tidak lagi dibutuhkan untuk slice ini.

#### 9.5.1 Modul yang disentuh

| Modul | Arah | Yang dipakai | Sifat |
| --- | --- | --- | --- |
| Rawat Inap | Rawat Inap → IGD | `InpAdmissionTransferService.TransferQuery` membaca `EmgAdmissionRequest` berstatus *Signed* | Baca; satu saringan di berkas milik Rawat Inap — **konfirmasi Hamzah** |
| Registration | IGD ← Registration | `RegPatientEncounter.AdmissionTransferId` dan `AdmissionTransferSourceType = "Emergency"`, diisi *admit* Transfer yang sudah ada | Baca — penentu "sudah diproses" |
| Rawat Inap | IGD → Rawat Inap | **Tidak ada tulis.** IGD tidak lagi memanggil `InpAdmissionReferralService` | — |
| Billing | — | **Tidak disentuh** | `IGD-DEC-293` terjaga |
| Clinical, Patient, Master Data, HR, Identity | IGD ← modul itu | Sama seperti 9.1 | Baca |

`EmergencyDispositionService.BacaRingkasanPermintaanRawatInapAsync` (static, sudah ada sejak `BE-IGD-072`) tetap tersedia tanpa
pemakai — calon pembaca isi form untuk layar antrean bila `IGD-OQ-131` diputuskan.

#### 9.5.2 Titik sentuh yang mengikat

| Kejadian | Kontrak | Keputusan |
| --- | --- | --- |
| Form ditandatangani | Satu transaksi IGD: form *Signed* + salinan beku. Antrean membaca form itu; tidak ada data yang disalin ke tabel Rawat Inap | `IGD-DEC-304` |
| Petugas admisi memproses (*admit* Transfer, sudah ada) | Kunjungan rawat inap lahir dengan `AdmissionTransferId` = tindak lanjut; tindak lanjut IGD menjadi *Executed*; IGD membaca form sebagai *Selesai* | `IGD-DEC-304`; `EPS-006` |
| Form dibatalkan sesudah terkirim | Form *Cancelled*; pasien hilang dari antrean; ditolak bila sudah diproses | `IGD-DEC-278`, `304` |
| Tindak lanjut dibatalkan | Semua form yang belum batal *Cancelled*; ditolak bila sudah diproses | `IGD-DEC-280`, `304` |
| Serah terima saat mengantar | Kepergian IGD (`EmgDeparture`, `MVP-8`): SBAR, perawat pengirim, perawat penerima; kunjungan IGD tertutup saat tiba bila tindak lanjut sudah *Executed* | `IGD-FACT-120`, `121` |
| Tagihan IGD ke invoice rawat inap | Tidak terjadi — penautan hanya dipicu `InpAdmissionReferral` | `IGD-DEC-293` |

#### 9.5.3 Perilaku saat gagal

| Kegagalan | Akibat | Pemulihan |
| --- | --- | --- |
| Pembatalan dari IGD dan *admit* terjadi bersamaan | Keduanya mengunci baris `EmgDisposition` (`FOR UPDATE`). Yang kedua membaca hasil yang pertama: *admit* kalah → `404`; pembatalan kalah → `409` aturan 19 atau 24 | Muat ulang layar |
| Unit tujuan tindak lanjut bukan unit rawat inap | Form tidak dapat dibuat atau ditandatangani (aturan 27), sehingga tidak ada form bertanda tangan yang tidak pernah tampil di antrean | Dokter memperbaiki tindak lanjut |
| Pasien dicatat tiba sebelum admisi diproses | Kunjungan IGD menunggu penutupan | `IGD-OQ-130` |

#### 9.5.4 Yang sengaja tidak diintegrasikan

| Butir | Alasan |
| --- | --- |
| Isi form di layar antrean (unit, kelas, DPJP usulan, CITO, kebutuhan khusus) | `IGD-OQ-131` — layar milik Rawat Inap |
| Penolakan beralasan oleh petugas admisi | `IGD-OQ-131` |
| Penutupan kunjungan IGD saat *admit* | `IGD-OQ-130` |
| Perluasan `InpAdmissionReferral` dan mode *Admisi dari Kamar Pulih* untuk pasien IGD | Dicabut `IGD-DEC-304` |

### 9.6 Amandemen `0.10.0` — Integrasi Pembatalan oleh Admisi Rawat Inap & Penutupan Susulan Dua Arah (`IGD-DEC-308`, `309`)

**Status: `approved`** — Rizki Gunawan & Muhammad Hamzah, 15 Oktober 2026 (`IGD-DEC-308`, `IGD-DEC-309`).

#### 9.6.1 Integrasi Antar-Modul (Rawat Inap ↔ IGD)

| Modul Pemanggil | Modul Target | Jalur / Method | Deskripsi & Kontrak Transaksi |
| --- | --- | --- | --- |
| Rawat Inap (`InpatientAdmissionTransferController` / Service) | IGD (`EmergencyDispositionService.AdmissionRequest.cs`) | `BatalkanFormPermintaanOlehPetugasAdmisiAsync(dispositionId, alasan, actor, ct)` | Dipanggil saat petugas admisi menekan *Tolak* di antrean transfer. IGD mengunci baris disposisi `FOR UPDATE`, membatalkan form aktif (`CancelledByAdmission`), mencatat event `AdmissionRejected`, tanpa menyentuh disposisi rawat inap IGD |
| Rawat Inap (`InpAdmissionTransferService`) | IGD (`EmergencyDispositionService.AdmissionRequest.cs`) | `EvaluasiPenutupanKunjunganSusulanAsync(emergencyVisitId, ct)` | Dipanggil di akhir transaksi *admit* transfer Rawat Inap tepat setelah status tindak lanjut menjadi `Executed`. IGD mengevaluasi apakah dokumen kepergian SBAR sudah diterima dan fisik tiba; jika ya, kunjungan IGD ditutup otomatis `Completed` |
| Rawat Inap (`InpAdmissionTransferService.Queries.cs`) | IGD | `EmergencyDispositionService.BacaRingkasanPermintaanRawatInapAsync(db, requestId, ct)` | Kueri baca-saja untuk mengisi DTO antrean transfer: lencana CITO, penanda ICU/isolasi, dan isian awal unit/kelas/DPJP usulan |

#### 9.6.2 Kegagalan & Penanganan Error

| Kegagalan | Dampak | Penanganan |
| --- | --- | --- |
| Petugas admisi menolak saat pasien sudah di-admit oleh petugas lain | Transaksi ditolak `409 Conflict` | Layar admisi menampilkan pesan galat konkurensi; baris antrean di-refresh |
| Evaluasi penutupan susulan gagal / error | Transaksi *admit* rawat inap tidak boleh gagal total bila penutupan IGD sekadar tertahan blocker klinis lain | Penjaga penutupan IGD mengembalikan evaluasi tanpa melempar exception fatal ke transaksi utama *admit* |

