# Laporan Perubahan Backend — `BE-BKC-045`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-045` |
| **Judul** | Serah terima kontrak ubah sumber pembayaran ke `RegistrationManagement` (*Handover Contract of Encounter Payment Source Modification*) |
| **Slice / Milestone** | `MVP-17` (Penggantian penanggung kunjungan sebelum pembayaran / Ganti Payer) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1762 |
| **Trace** | `FR-BKC-068`, `FR-BKC-069`, `FR-BKC-071`; `MPY-DEC-007`, `MPY-DEC-010`, `MPY-DEC-011`, `MPY-DES-001`, `MPY-DES-002`, `MPY-DES-004`; `CAP-33` (Missing) |
| **Contract Version** | `MPY-ENC-PAYER-001` versi `1.0.0` (terhubung ke `BIL-INTEGRATION-0.8`) |
| **Dependency** | Tidak ada. Wewenang lintas modul telah diberikan via `MPY-DEC-011` (Muhammad Hamzah — Product/Domain Owner `RegistrationManagement`) |
| **Klasifikasi** | `GOVERNANCE / CONTRACT HANDOVER` (Dokumen kontrak serah terima spesifikasi teknis lintas bounded-context; 0 baris source di `billing-kasir`, 0 perubahan skema/tabel/migration) |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 11 September 2026 |
| **Status** | ✅ `Selesai` (Dokumen kontrak serah terima `MPY-ENC-PAYER-001` lengkap dengan 10 bagian kanonikal, batasan wewenang, pembagian tanggung jawab, urutan transaksi, dan kewajiban pengujian telah disahkan dan siap dieksekusi di modul `RegistrationManagement`) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | Peminta: `BillingManagement / Billing`; Pemilik Target Source: `RegistrationManagement` |
| **Owner / Prefix Registry** | Prefix `BIL-` (`HealthServices / BillingManagement`) & `REG-` (`HealthServices / RegistrationManagement`) |
| **Keberlakuan** | `CONTRACT HANDOVER` — Spesifikasi kontrak integrasi to-be lintas modul |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-CON-001`, `QBE-INT-001` |
| **Pengecualian / Catatan** | Task ini **tidak menulis kode aplikasi** di modul `billing-kasir`, karena wewenang mutasi tabel `RegPatientEncounterGuarantor` mutlak milik modul `RegistrationManagement`. Task ini menyerahkan kontrak tertulis definitif (`MPY-ENC-PAYER-001`) agar pemilik modul Registrasi dapat membangun `EncounterPaymentSourceService`. |

---

## 1. Masalah & Latar Belakang Bisnis

### 1.1 Kebutuhan Operasional Kasir Rumah Sakit
Dalam alur pelayanan rumah sakit, sering terjadi situasi di mana penanggung kunjungan pasien perlu disesuaikan sebelum transaksi pembayaran diselesaikan di kasir:
- **Contoh Kasus Nyata (Data Samaran):**
  Seorang pasien, **Ny. S**, datang berobat ke Poli Penyakit Dalam dan didaftarkan sebagai pasien Umum (Tunai) oleh petugas registrasi karena pasien lupa membawa kartu asuransi kesehatannya.
  Setelah selesai berkonsultasi dengan dokter spesialis dan menerima resep obat di farmasi, pasien mendatangi kasir rawat jalan untuk menyelesaikan administrasi. Di depan kasir, pasien menunjukkan kartu asuransi **Prudential** aktif miliknya yang baru saja dibawakan oleh keluarganya.
  Pasien memohon agar penjamin kunjungannya diubah menjadi asuransi Prudential tersebut agar biayanya dapat di-cover sesuai hak polisnya.

### 1.2 Keterbatasan Sistem Saat Ini (`CAP-33` — Missing)
- Pada implementasi sistem saat ini, sumber pembayaran kunjungan (`RegPatientEncounterGuarantor`) **hanya dapat diisi satu kali pada saat pendaftaran awal kunjungan**.
- Tidak tersedia metode, endpoint, maupun service untuk mengubah data penjamin kunjungan setelah registrasi terbentuk.
- Petugas terpaksa membatalkan pendaftaran kunjungan secara keseluruhan, mendaftar ulang pasien dari awal, dan menginput ulang seluruh tindakan serta resep obat yang sudah berjalan. Ini memicu risiko ketidaksinkronan data medis dan komplain pasien karena proses administrasi berulang yang panjang.

### 1.3 Batasan Arsitektur & Bounded Context (`MPY-DEC-007`, `MPY-DEC-011`)
- Tabel `RegPatientEncounterGuarantor` berada di bawah kepemilikan bounded context **`RegistrationManagement`**.
- Terdapat invariant ketat pada basis data: **satu kunjungan wajib memiliki tepat satu sumber pembayaran aktif** (ditegakkan melalui *unique index* pada kolom `PatientEncounterId`).
- Tim `billing-kasir` **dilarang keras** melakukan mutasi langsung (INSERT/UPDATE/DELETE) pada tabel `RegPatientEncounterGuarantor`.
- Berdasarkan keputusan `MPY-DEC-007`:
  > **`billing-kasir` mengorkestrasi alur tagihan, sedangkan `RegistrationManagement` yang berwenang menulis pembaruan data sumber pembayaran kunjungan.**

Oleh karena itu, `BE-BKC-045` bertugas menyusun dan menyerahkan dokumen spesifikasi kontrak serah terima tertulis (`MPY-ENC-PAYER-001`) kepada pemilik modul `RegistrationManagement` (Muhammad Hamzah).

---

## 2. Rangkuman Kontrak Serah Terima `MPY-ENC-PAYER-001`

Dokumen kontrak kanonikal disimpan di:
[`docs/module-blueprints/rawat-inap/episode-rawat-inap/contracts/encounter-payment-source-change-contract.md`](../../../rawat-inap/episode-rawat-inap/contracts/encounter-payment-source-change-contract.md)
*(Disimpan berdampingan dengan kontrak addendum `RWI-ENC-PAYER-001` milik `RegistrationManagement` sesuai preseden pengelolaan addendum lintas modul)*.

### 2.1 Yang Diminta Dibangun di `RegistrationManagement`
1. **Satu Berkas Layanan Baru:**
   - Nama: `EncounterPaymentSourceService`
   - Lokasi: `Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs`
   - Registrasi DI: `builder.Services.AddScoped<EncounterPaymentSourceService>();`
   - Endpoint Publik: **Nol / Tidak ada** (layanan ini merupakan internal domain service yang diorkestrasi oleh `BillingPayerEditService` milik kasir).
2. **Spesifikasi Metode Pemanggilan:**
   - Menerima parameter:
     - `encounterId` (`Guid`, wajib)
     - `paymentType` (`EncounterPaymentType`: Tunai, Asuransi, atau Penjamin Perusahaan)
     - `paymentMethodId` (`Guid?`, wajib jika Tunai)
     - `patientInsuranceCardId` (`Guid?`, wajib jika Asuransi)
     - `patientCompanyGuarantorCardId` (`Guid?`, wajib jika Penjamin Perusahaan)
     - `serviceDate` (`DateTime`, wajib untuk validasi masa berlaku kartu)
     - `reason` (`string`, wajib)
   - Mengembalikan data:
     - Sumber pembayaran sebelum perubahan (`OldPaymentType`, `OldPayerName`, `OldCardNumber`)
     - Sumber pembayaran sesudah perubahan (`NewPaymentType`, `NewPayerName`, `NewCardNumber`)
3. **In-Place Update (Tanpa Insert Baris Baru):**
   - Baris pada `RegPatientEncounterGuarantor` yang sudah ada **wajib diperbarui di tempat (*in-place update*)**, bukan dihapus/dinonaktifkan lalu disisipkan baris baru. Hal ini karena *unique index* pada basis data tidak difilter, sehingga penyisipan baris baru akan selalu menabrak *unique constraint violation*.
4. **Pembersihan Kolom yang Tidak Relevan:**
   - Kolom yang tidak relevan dengan jenis pembayaran baru wajib diatur menjadi `null` (misal jika berganti dari Asuransi ke Perusahaan, kolom asuransi dikosongkan).

### 2.2 Batasan Wewenang (Yang MUST NOT Dilakukan Layanan)
| Larangan | Alasan Teknis & Bisnis |
| :--- | :--- |
| **MUST NOT membuka transaksi database mandiri** | Perubahan penjamin kunjungan dan kalkulasi ulang tagihan wajib berada dalam **satu kesatuan transaksi atomik (`IDbContextTransaction`)** milik pemanggil (`billing-kasir`). Jika kalkulasi gagal, perubahan penjamin wajib ikut rollback. |
| **MUST NOT menyisipkan baris baru** | Mencegah pelanggaran *unique index constraint* pada `RegPatientEncounterGuarantor`. |
| **MUST NOT mengubah skema tabel** | Perubahan bersifat murni aditif kode logic; **nol migration database** di sisi Registrasi. |
| **MUST NOT membuat/mengubah kartu pasien** | Master kartu dimiliki `PatientManagement`. Registrasi hanya memvalidasi dan mengikat kartu yang sudah ada. |
| **MUST NOT menyentuh tabel invoice / tagihan** | Seluruh entitas tagihan, pembayaran kasir, dan penomoran kwitansi adalah tanggung jawab eksklusif `billing-kasir`. |

### 2.3 Matriks Pembagian Tanggung Jawab Pemeriksaan
| Objek Pemeriksaan | Tanggung Jawab Modul |
| :--- | :--- |
| Status tagihan masih `DRAFT` / `OPEN` (belum lunas) | `billing-kasir` |
| Belum ada pembayaran parsial/lunas yang tersimpan | `billing-kasir` |
| Idempotency key & row version check | `billing-kasir` |
| Alasan perubahan terisi lengkap | `billing-kasir` |
| **Kartu terdaftar atas nama pasien yang sama** | **`RegistrationManagement`** |
| **Kartu dalam status aktif dan tidak terhapus** | **`RegistrationManagement`** |
| **Kartu berlaku pada tanggal pelayanan kunjungan** | **`RegistrationManagement`** |
| **Polis kartu berstatus layak (*eligible*)** | **`RegistrationManagement`** |
| **Validasi pasangan jenis pembayaran & ID rujukan** | **`RegistrationManagement`** |
| **Penjaminan tepat 1 baris per kunjungan terjaga** | **`RegistrationManagement`** |
| Perhitungan ulang invoice & pembuatan versi kalkulasi baru | `billing-kasir` |
| Pencatatan log jejak audit perubahan penjamin | `billing-kasir` |

---

## 3. Urutan Transaksi Atomik (Single Unit of Work)

Diagram urutan berikut menjelaskan orkestrasi transaksi pada saat kasir menjalankan aksi ubah penjamin di `BE-BKC-047`:

```text
[Kasir / UI]
     │
     │ 1. PUT /api/v1/billing-invoices/{id}/payment-source
     ▼
[BillingInvoicesController]
     │
     │ 2. Panggil BillingPayerEditService.ChangePaymentSourceAsync(...)
     ▼
[BillingPayerEditService (billing-kasir)]
     │
     ├──► 3. Mulai Database Transaction (_dbContext.Database.BeginTransactionAsync)
     ├──► 4. Validasi prasyarat tagihan: belum lunas, versi baris sesuai, alasan terisi
     │
     ├──► 5. Panggil EncounterPaymentSourceService.UpdatePaymentSourceAsync(...)
     │         │
     │         ▼
     │   [EncounterPaymentSourceService (RegistrationManagement)]
     │         ├──► 6. Verifikasi keabsahan kartu (milik pasien, aktif, masa berlaku)
     │         ├──► 7. Mutasi in-place pada RegPatientEncounterGuarantor
     │         ├──► 8. Selaraskan ringkasan PaymentType pada TrxPatientEncounter
     │         └─► Kembalikan nilai (OldPayer, NewPayer) ke pemanggil
     │
     ├──► 9. Kembalikan penanggung baris biaya yang tidak lagi sah ke penanggung utama
     ├──► 10. Kalkulasi ulang seluruh komponen biaya tagihan
     ├──► 11. Simpan audit log perintah di BilInvoicePayerChangeCommand
     ├──► 12. Commit Transaction (seluruh perubahan tersimpan atomik)
     │
     └─► 13. Kembalikan respons sukses 200 OK dengan rincian tagihan terbarukan
```

---

## 4. Status Serah Terima & Dampak Terhadap Roadmap

1. **Kelengkapan Dokumen Kontrak:**
   - Kontrak `MPY-ENC-PAYER-001` telah memenuhi 10 bagian wajib sesuai definisi selesai (*Definition of Done*).
   - Seluruh poin keputusan (jenis layanan generik, penamaan service, dan pencatatan jejak audit di sisi billing) telah dirumuskan dengan jelas.
2. **Keterkaitan dengan Task Berikutnya pada Roadmap:**
   - `BE-BKC-045`: **✅ Selesai** (kewajiban serah terima kontrak dari `billing-kasir` telah tuntas).
   - `BE-BKC-046`: Dapat dikerjakan paralel / langsung (penambahan konteks payer eksplisit pada mesin kalkulasi tanggungan untuk keperluan pratinjau perbandingan).
   - `BE-BKC-047`: Menunggu implementasi `EncounterPaymentSourceService` selesai di sisi `RegistrationManagement` untuk endpoint eksekusi ganti payer, namun endpoint `edit-context` dan `payer-comparison-preview` dapat disiapkan terlebih dahulu.

---

## 5. Ringkasan Verifikasi

| Komponen | Status | Keterangan |
| :--- | :---: | :--- |
| Keberadaan Kontrak `MPY-ENC-PAYER-001` | ✅ Terpenuhi | Tersedia di `docs/module-blueprints/rawat-inap/episode-rawat-inap/contracts/encounter-payment-source-change-contract.md` |
| 10 Bagian Kanonikal Kontrak | ✅ Terpenuhi | Latar belakang, hasil bisnis, spesifikasi service, invariant, tanggung jawab validasi, transaksi, tes, keputusan, dampak, daftar file |
| Kebijakan Nol Modifikasi Source Ilegal | ✅ Terpenuhi | Tidak ada penulisan paksa source code pada modul `RegistrationManagement` tanpa mandat langsung dari roadmap Registrasi |
| Ketertelusuran (*Traceability*) | ✅ Terpenuhi | Terhubung langsung dengan `FR-BKC-068`, `FR-BKC-069`, `FR-BKC-071`, `CAP-33`, dan `MVP-17` |
