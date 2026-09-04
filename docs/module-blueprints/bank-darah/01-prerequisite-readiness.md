# Bank Darah — Prerequisite Readiness

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Revision | `4` |
| Status | `CURRENT` — register dependency yang sedang berlaku dan dikutip roadmap revisi 2. Seluruh bukti terverifikasi lewat impact scan 4 September 2026 |
| Diperbarui | `2026-09-04` — `BD-DEP-004` dan `BD-DEP-005` ditutup implementasi `MVP-0` |
| Sumber bukti | `02-existing-capability-map.md` revisi 2 dan `00-interview-decisions.md` revisi 2 |
| Backend SHA | bukti dikumpulkan pada `9522caacf29371b1fddd1584e9a71ad94fe48d19`; **source terkini `5f7acaf`** cabang `sukmagp` |
| Frontend SHA | bukti dikumpulkan pada `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254`; **source terkini `101ec5d3a560bd6e54d4665ae53d425f255c609f`** cabang `sukmagpV2` |

Dokumen ini mencatat apa saja yang harus sudah tersedia sebelum Bank Darah bisa dikerjakan, dan
sejauh mana masing-masing sudah siap.

Nilai `capability_status` hanya boleh memakai salah satu dari taksonomi baku berikut:
`READY TO REUSE`, `REUSE WITH ADAPTER`, `EXTEND`, `REPAIR`, `MISSING`, `CONFLICT`, atau `UNKNOWN`.

Nilai `dependency_type` hanya boleh memakai `MODULE_FOUNDATION`, `PHASE`, `INTEGRATION`, atau
`EXTERNAL`.

> **Perubahan pada revisi 4 — 4 September 2026.** Dua dependency ditutup oleh implementasi gelombang
> `MVP-0`, bukan oleh keputusan baru. `BD-DEP-005` katalog komponen darah naik dari `MISSING` menjadi
> **`RESOLVED`** lewat `BE-BD-001`, dan `BD-DEP-004` kewenangan unit memesan darah naik dari `EXTEND`
> menjadi **`RESOLVED`** lewat `BE-BD-002`. Keduanya terbukti: build hijau dan 101 pengujian Bank Darah
> lulus pada `5f7acaf`.
>
> ✅ **Penanda `STALE` sudah dicabut.** Impact scan terbatas dijalankan `trace-existing-capabilities`
> pada 4 September 2026 atas rentang `4205d18..5f7acaf` (backend) dan `afbb8ab..101ec5d3` (frontend).
> Bukti `BD-DEP-014` (komponen dasar tampilan frontend) **terverifikasi tetap sahih**: kesepuluh
> komponen yang dikutip tidak berubah. Seluruh dependency lain juga diperiksa per berkas dan tidak
> ada yang buktinya bergeser.

> **Perubahan pada revisi 3.** Dua `CONFLICT` yang muncul pada revisi 2 sudah ditutup keputusan
> pemilik: `BD-DEP-002` oleh `DEC-BD-014`, dan `BD-DEP-007` oleh `DEC-BD-015`. Tidak ada lagi
> dependency berstatus `CONFLICT`.
>
> **Catatan sinkronisasi 2 September 2026.** Keputusan sampai `DEC-BD-034` dan
> `03-domain-architecture.md` revisi 3 (`DOMAIN_ARCHITECTURE_READY`) **tidak** menambah, menghapus,
> atau mengubah status satu pun dependency di bawah. `BD-DEP-005` katalog komponen darah kini juga
> memikul atribut masa berlaku bukti kecocokan per komponen (`DEC-BD-032`), tetapi tetap `MISSING` dan
> tetap dibangun modul ini sendiri. Peta dependency terikat pada backend `9522caa`; backend kini
> `db08c14` dengan perbedaan hanya dokumen blueprint, sehingga dokumen ini **tidak** basi. Revisi tidak
> dinaikkan karena tidak ada dependency yang berubah secara material.

## Catatan dependency

| dependency_id | capability_or_module | dependency_type | owner | evidence | capability_status | required_by | blocking_impact | independent_continuation | source_sha | next_owner_or_action |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BD-DEP-001` | Data pasien | `MODULE_FOUNDATION` | PatientManagement | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs#MstPatient` — lihat `BD-CAP-001` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir | — | `9522caa` | Simpan `PatientId` sebagai rujukan |
| `BD-DEP-002` | Kunjungan pasien beserta sinyal penutupannya | `MODULE_FOUNDATION` | RegistrationManagement dan InPatientManagement | `TrxPatientEncounter.cs`, `EncounterStatus.cs`, `InpEpisode.cs` — lihat `BD-CAP-002` dan `BD-CAP-003` | `REUSE WITH ADAPTER` | `BD-PH-005` | Tidak memblokir. `DEC-BD-014` menetapkan dua penyesuai: status akhir kunjungan untuk rawat jalan dan IGD, waktu pasien meninggalkan rumah sakit untuk rawat inap | — | `9522caa` | Bank Darah hanya membaca kedua sumber; dilarang mengubah status kunjungan atau episode |
| `BD-DEP-003` | Data dokter | `MODULE_FOUNDATION` | HR — Master Data Workforce | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs#MstDoctor` — lihat `BD-CAP-004` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir | — | `9522caa` | Simpan `DoctorId` sebagai rujukan |
| `BD-DEP-004` | Kewenangan unit pelayanan memesan darah | `MODULE_FOUNDATION` | HealthServices — Master Data | `MstServiceUnit.cs#MstServiceUnit` kini **sudah memuat** `IsAvailableForBloodOrder` — lihat `BD-CAP-005` | **`RESOLVED`** — semula `EXTEND`, ditutup 4 September 2026 | — | **Tidak lagi memblokir.** `DEC-BD-012` terpenuhi: kewenangan memesan darah berasal dari kolom konfigurasi, nol daftar unit ditanam di kode | — | `5f7acaf` | **Selesai** lewat `BE-BD-002`. Kolom dititipkan pada tabel milik Master Data, nol butir hak akses baru, 8 pengujian lulus |
| `BD-DEP-005` | Katalog komponen darah — PRC, TC, FFP | `MODULE_FOUNDATION` | belum ada pemilik | `MstBloodComponent.cs` kini ada pada `Areas/HealthServices/MasterData/Models/`, beserta `MstBloodBankReason.cs` — lihat `BD-CAP-018` | **`RESOLVED`** — semula `MISSING`, ditutup 4 September 2026 | — | **Tidak lagi memblokir.** `DEC-BD-005` terpenuhi: komponen darah berasal dari katalog terkendali, bukan ketikan bebas, sehingga deteksi order ganda punya penanda yang sah | — | `5f7acaf` | **Selesai** lewat `BE-BD-001`. Dua master, 18 endpoint, dua migration, seeder PRC/TC/FFP dan sepuluh kategori alasan, 56 pengujian lulus |
| `BD-DEP-006` | Penyerahan fakta biaya ke Billing | `INTEGRATION` | BillingManagement | `BillingSourceContract.cs` memuat daftar sumber tertutup tanpa Bank Darah; `ClinicalMilestoneFactProducer.cs#EmitChargeEligibilityAsync` — lihat `BD-CAP-015` | `EXTEND` | `BD-PH-005` | BR-BD-004 tidak dapat mengirim biaya ke Billing sebelum konteks sumber Bank Darah ditambahkan | Seluruh alur order dan kantong tetap dapat dirancang | `9522caa` | Minta persetujuan pemilik Billing untuk menambah konteks sumber dan jenis efek biaya Bank Darah |
| `BD-DEP-007` | Sumber sah golongan darah dan Rhesus | `MODULE_FOUNDATION` | Bank Darah sendiri, sesuai `DEC-BD-015` | `MstPatient.BloodType` adalah data induk administratif, bukan hasil pemeriksaan tervalidasi. Tidak ditemukan entity hasil pemeriksaan golongan darah di `LaboratoryManagement` — lihat `BD-CAP-017` | `MISSING` | `BD-PH-005` | Tidak memblokir rancangan. `DEC-BD-015` menetapkan sumber sah berupa hasil pemeriksaan tersendiri milik Bank Darah, dan `DEC-BD-018` menetapkan sampelnya juga milik Bank Darah. Kemampuannya dibangun baru | Seluruh alur inti tetap dapat dirancang | `9522caa` | Siapa yang berhak memvalidasi hasil masih `DEF-BD-004`; mekanik label masih `OQ-BD-011` |
| `BD-DEP-008` | Entri registry kepemilikan modul dan prefix untuk Bank Darah | `EXTERNAL` | Pemilik registry engineering | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` kini memuat baris Bank Darah: Area `HealthServices`, Module `BloodBankManagement / Blood Bank`, Category `BUSINESS DOMAIN / MODULE`, Prefix `Bbk`, Lifecycle **`ACTIVE`** sejak commit `8075784` | **`READY TO REUSE`** — semula `MISSING`, ditutup 3 September 2026 | — | **Tidak lagi memblokir penamaan.** `QBE-NAM-004` terpenuhi: prefix `Bbk` berasal dari registry, bukan disimpulkan dari nama folder. Prefix yang disahkan **persis** seperti yang diajukan blueprint sejak `v1` | — | `ed7fba8` | **Selesai.** Wewenang implementasi menyusul lewat `BD-DEP-016`, bukan dari entri ini |
| `BD-DEP-016` | Keputusan aktivasi modul Bank Darah pada registry | `EXTERNAL` | Pemilik registry engineering | Baris registry Bank Darah berstatus **`ACTIVE`** sejak 3 September 2026, commit `8075784`. Changelog registry: "Membuka wewenang implementasi entity operasional `Bbk*` sesuai `QBE-MOD-002`" | **`READY TO REUSE`** — semula `MISSING`, ditutup 3 September 2026 | — | **Tidak lagi memblokir.** Wewenang implementasi entity operasional `Bbk*` dan migration modul sudah terbuka | — | `8075784` | **Selesai.** Eksekusi database di luar dev pemilik dan deployment tetap wewenang terpisah |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang dirujuk BRD | `EXTERNAL` | Pemilik kebutuhan | `Bank Darah(1).md`, `Artifact_Bank_Darah_Bagian_Kedua(1).md`, `Bank_Darah_Bagian_Ketiga(1).md` tidak ditemukan di kedua repository | `MISSING` | `BD-PH-003` | Penelusuran dari bukti ke kebutuhan tidak dapat diverifikasi, sehingga gerbang kelengkapan requirement tidak dapat lulus penuh | Perancangan tetap berjalan | `9522caa` | Sediakan berkas bukti atau tunjukkan lokasinya |
| `BD-DEP-010` | Pola otorisasi tingkat controller dan tindakan | `MODULE_FOUNDATION` | Platform backend | `Attributes/AccessControllerAttribute.cs`, `AccessActionAttribute.cs`, `AccessPermissionAttribute.cs`; contoh pemakaian `LabOrderController.cs` dengan `[AccessPermission("LabOrder", "Read")]` — lihat `BD-CAP-013` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir | — | `9522caa` | Petakan kelompok kewenangan BRD §14 ke pola ini |
| `BD-DEP-011` | Kontrak response dan pagination bersama | `MODULE_FOUNDATION` | Platform backend | `Responses/ApiResponse.cs`, `Responses/PagedResult.cs` — lihat `BD-CAP-012` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir | — | `9522caa` | Pakai apa adanya saat kontrak API disusun |
| `BD-DEP-012` | Pemisahan data per fasilitas kesehatan | `MODULE_FOUNDATION` | Platform backend | `MstHospitalSite.cs` hanya dipakai Corporate; tidak ada rujukan `HospitalSite` di seluruh `Areas/HealthServices/`; `TrxPatientEncounter` tidak punya kolom fasilitas — lihat `BD-CAP-022` | `READY TO REUSE` | `BD-PH-005` | Tidak memblokir. Layanan kesehatan Quilvian saat ini tidak memisahkan data per fasilitas | — | `9522caa` | Bank Darah mengikuti pola yang sama; jangan menambah pemisahan baru |
| `BD-DEP-013` | Pola pesanan klinis, riwayat perpindahan status, dan kunci konkurensi | `MODULE_FOUNDATION` | LaboratoryManagement | `LabOrder.cs`, `TrxLabSpecimen.cs`, `TrxLabTransitionHistory.cs` — lihat `BD-CAP-007`, `BD-CAP-008`, `BD-CAP-009`, `BD-CAP-010` | `REUSE WITH ADAPTER` | `BD-PH-005` | Tidak memblokir. Dipakai sebagai pola, bukan sebagai entity bersama | — | `9522caa` | Ikuti bentuknya saat merancang entity Bank Darah |
| `BD-DEP-014` | Komponen dasar tampilan frontend | `MODULE_FOUNDATION` | Frontend V2 | `src/components/features/base-features/` memuat `hero.jsx`, `data-table.jsx`, `data-filter.jsx`, `filter-select.jsx`, `filter-date-picker.jsx`, `base-button.jsx`, `confirm-modal.jsx`, `access-denied-gate.jsx` — lihat `BD-CAP-021` | `READY TO REUSE` | `BD-PH-008` | Tidak memblokir | — | `afbb8ab` | Dilarang membuat komponen dasar tandingan |
| `BD-DEP-015` | Integrasi HCLAB — workstation `BANK DARAH`, kode `BBW`, Lab Sec `GL` | `EXTERNAL` | belum diketahui | Tidak ditemukan rujukan HCLAB, `BBW`, maupun `GL` pada `Areas/HealthServices/LaboratoryManagement/` — lihat `BD-CAP-024` | `UNKNOWN` | `BD-PH-005` | Hanya memblokir BR-BD-014, yang memang baru menuntut dokumen penelusuran dan bukan implementasi | Seluruh scope lain berjalan | `9522caa` | Sediakan bukti integrasi dari luar repository |

## Ringkasan kesiapan

| Status | Jumlah | Dependency |
| --- | --- | --- |
| `READY TO REUSE` | 6 | `BD-DEP-001`, `003`, `010`, `011`, `012`, `014` |
| **`RESOLVED`** | **3** | **`BD-DEP-004`, `005`** ditutup implementasi `MVP-0` 4 September 2026; **`BD-DEP-008`** ditutup pendaftaran prefix 3 September 2026 |
| `REUSE WITH ADAPTER` | 2 | `BD-DEP-002`, `013` |
| `EXTEND` | 1 | `BD-DEP-006` |
| `CONFLICT` | 0 | keduanya ditutup pada closure pass 2026-09-02 |
| `MISSING` | 2 | `BD-DEP-007`, `009` |
| `UNKNOWN` | 1 | `BD-DEP-015` |
| `REPAIR` | 0 | — |

## Apa artinya bagi fase

| Fase | Terpengaruh? | Alasan |
| --- | --- | --- |
| `BD-PH-002` audit kemampuan | Selesai | Delapan `UNKNOWN` sudah berubah menjadi status berbukti |
| `BD-PH-003` gerbang kelengkapan | Sebagian | `BD-DEP-009` menghalangi kelulusan penuh, tetapi penilaian tetap boleh dijalankan dan mencatat kekurangannya |
| `BD-PH-005` perancangan blueprint | Hampir penuh | Tidak ada lagi `CONFLICT`. Yang tersisa hanya `BD-DEP-006` yang menunggu persetujuan pemilik Billing (`DEC-BD-016`), sehingga penyerahan biaya belum dapat dikontrakkan. Seluruh alur order, permintaan PMI, penerimaan, alokasi, pemberian, kedaluwarsa, dan penyelesaian kantong dapat dirancang penuh |
| `BD-PH-007` implementasi backend | **`READY`** | `BD-DEP-008` penamaan dan `BD-DEP-016` aktivasi modul **keduanya tertutup**, dan `G1` approval desain turun 3 September 2026 (`Sukmagp`). Tidak ada dependency maupun gerbang yang menahan |

Sesuai aturan gerbang dependency, `MISSING` dan `UNKNOWN` hanya memblokir fase yang membutuhkannya.
Tiga `MISSING` yang tersisa — katalog komponen darah, hasil golongan darah Bank Darah, dan seluruh
kapabilitas Bank Darah — memang kemampuan yang akan dibangun modul ini sendiri, jadi keberadaannya
wajar dan bukan penghalang. `BD-DEP-008` penamaan dan `BD-DEP-016` aktivasi modul **keduanya sudah
tertutup** pada 3 September 2026. Satu-satunya `MISSING` yang masih menahan adalah `BD-DEP-009` berkas
bukti, dan itu menahan penelusuran bukti, bukan perancangan maupun implementasi.

Status modul karena itu naik dari `PARTIAL` ke **`READY`** pada 3 September 2026, setelah approval desain
(`G1`) tercatat atas nama `Sukmagp`. Tidak ada satu pun fase yang `BLOCKED`.
