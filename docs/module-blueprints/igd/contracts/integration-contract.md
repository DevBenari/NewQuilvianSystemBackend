# Kontrak Integrasi — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.4.0` — encounter-first, 22 September 2026, **Rencana (belum tersedia)**. **Aditif pada dokumen**: bagian 5 baru; bagian 1–4 tidak diubah. Secara perilaku **memutus** untuk Registrasi (penolakan Emergency pada jalur umum, pintu encounter memanggil aturan IGD). Sebelumnya `0.3.0` |
| Status | `draft` |
| Owner | Product/Domain Owner IGD: **Rizki Gunawan** (`IGD-DEC-089`) |
| `approved_by` / `approved_at` | — / — |
| Versi sebelumnya | `0.2.0` |

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
| Simpul organisasi unit belum dipetakan | **Belum diputuskan** — `IGD-OQ-071` | — |
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
| Encounter Emergency dibuat | `PatientEncounterController.CreateEncounterCoreAsync` (Registrasi) → aturan episode IGD (`public static` pada `EmergencyVisitService`) | Kunci per pasien → rumus episode → tolak `409` atau tulis catatan override IGD → **baru** kunci penomoran dan sisip encounter; tanpa `TrxQueue` | Transaksi Registrasi yang sudah ada (`:559`) — satu transaksi |
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
