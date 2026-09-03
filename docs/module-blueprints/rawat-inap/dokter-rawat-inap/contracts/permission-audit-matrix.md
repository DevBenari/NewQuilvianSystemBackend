# Permission dan Audit Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.1.0` |
| `last_changed_in` | `0.1.0` |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`) |
| `approved_by` / `approved_at` | — belum |
| Tanggal | 2 September 2026 |

---

## 0. Yang **tidak** ada di dokumen ini

Dokumen ini **MUST NOT** memuat tabel seluruh endpoint. Pemetaan endpoint ke hak akses dipegang
kolom `Hak akses` pada [`api-contract.md`](./api-contract.md). Dua turunan berikut **dihitung**:

| Yang tidak ditulis ulang | Cara menurunkannya |
| --- | --- |
| String atribut | `[AccessPermission("<Resource>", "<Action>")]` disalin dari kolom `Hak akses` |
| Status pencatatan logger | `GET` tidak dicatat, selain `GET` dicatat |

**Pengecualian bernama:** tidak ada.

---

## 1. Cara kerja hak akses di repository ini

| Hal | Isinya |
| --- | --- |
| Atribut | `[AccessPermission("Resource", "Action")]` beserta `[AccessAction(...)]` |
| Filter | `AccessPermissionFilter` mencocokkan pasangan Resource–Action terhadap baris hak akses peran |
| Jebakan yang sudah terjadi | `BE-RWI-034`: `[AccessAction]` dan `[AccessPermission]` menyebut nama berbeda, sehingga sembilan endpoint menjawab `403` bagi siapa pun kecuali SuperAdmin, dan menahan tujuh task frontend |

> **Sub-modul ini menambah Action baru pada Resource yang sudah ada** — `Amend` dan `Verify` —
> ditambah satu Resource baru `PhysicianVisit`. Ketiganya wajib memakai nama yang sama persis pada
> kedua atribut, dan wajib diuji dengan peran non-SuperAdmin.

---

## 2. Peta peran ke butir hak akses

| Peran rumah sakit | Resource | Action |
| --- | --- | --- |
| DPJP | `DoctorConsultation` | `Read`, `Create`, `Update`, `Amend` |
| DPJP | `PatientAssessment` | `Read`, `Create`, `Update`, `Amend` — **terbatas jenis medis**, lihat 3 |
| DPJP | `PatientIntegratedProgressNote` | `Read`, `Create`, `Amend`, **`Verify`** |
| DPJP | `PatientProcedure` | `Read`, `Create`, `Update`, `Amend` |
| DPJP | `Prescription` | `Read`, `Create` |
| DPJP | `LabOrder` | `Read`, `Create` |
| DPJP | `PhysicianVisit` | `Read`, `Create`, `Update` |
| Dokter jaga ruangan | Sama dengan DPJP **kecuali** `Verify` | — |
| Dokter konsulen | `Read` pada seluruhnya; `Create` pada CPPT dan `PhysicianVisit` | — |
| Perawat | `Read` pada SOAP, kajian medis, tindakan, resep, lab; `Create` pada CPPT | Tidak ada `Amend`, tidak ada `Verify` |
| Ahli gizi | `Read` pada kajian medis dan CPPT | — |
| Petugas Farmasi | Milik modulnya sendiri | Sub-modul ini tidak mengaturnya |
| Petugas admisi, kasir | — | **Tidak ada** |

> **`Verify` hanya milik DPJP, dan itu inti `CAP-021` aturan 5.** Memberikannya kepada dokter jaga
> membuat verifikasi kehilangan artinya: yang diverifikasi adalah catatan yang menjadi tanggung
> jawab DPJP.
>
> **Perawat tidak punya `Amend` pada catatan dokter**, dan dokter tidak punya `Amend` pada catatan
> keperawatan — `keperawatan/contracts/permission-audit-matrix.md` bagian 2.

---

## 3. Kewenangan yang **tidak dapat** dijaga mesin hak akses

Mesin hak akses tahu peran, tidak tahu pasien maupun jenis dokumen. Empat hal berikut dijaga di
tingkat aturan bisnis.

| Yang dijaga | Penjaganya | Yang **tidak** dijaganya | Risikonya |
| --- | --- | --- | --- |
| Dokter hanya menulis untuk pasien yang menjadi tanggung jawabnya | `VAL-DOK-06`, membaca `InpDoctorAssignment` **berperiode** | Dokter yang memang DPJP tetap dapat menulis apa saja | Kesalahan isi, bukan kewenangan. Dijaga jejak audit |
| **Dokter menulis kajian medis, perawat menulis pengkajian keperawatan** | `VAL-DOK-05`, bercabang menurut `AssessmentType` | Mesin hak akses melihat satu Resource `PatientAssessment` untuk keduanya | **Ini akibat langsung berbagi satu tabel** — `02-backend-architecture.md` bagian 4.2. Bila pemilik memilih tabel terpisah, penjagaan ini naik ke mesin hak akses |
| Verifikator bukan penulis asli | `VAL-DOK-07` beserta kolom terpisah | — | — |
| Visite hanya dicatat dokter | `VAL-DOK-08` | Kebijakan *administrative attestation* belum ada | Bawaan dipilih yang aman: hanya dokter |

Baris kedua adalah harga yang dibayar jalan A pada bagian 4.2 arsitektur. Ia **ditulis di sini**
supaya pemilik melihatnya sebelum menyetujui, bukan menemukannya saat implementasi.

---

## 4. Audit

| Lapisan | Yang dicatat |
| --- | --- |
| Kolom warisan `IdentityModel` | `CreateBy`, `CreateDate`, `UpdateBy`, `UpdateDate` pada setiap baris |
| Custom logger | Seluruh permintaan selain `GET` |
| Jejak tahan lama yang **wajib** | Penyelesaian dan amandemen kajian medis serta SOAP; **setiap verifikasi CPPT** beserta verifikatornya; setiap visite beserta pencatatnya; amandemen tindakan |

| Kejadian | Kenapa wajib berjejak |
| --- | --- |
| Verifikasi CPPT | `AC-CAP021-03`: verifikator dapat diaudit dan **bukan** penulis asli |
| Visite | `AC-CAP025-03`: penulis, waktu, dan peran dapat diaudit |
| Amandemen catatan final | PRD `CAP-020` aturan 5, `CAP-022` aturan 4 |

---

## 5. Kolom sensitif dan masa simpan

Kolom bertanda **Sensitif** pada [`../data/data-dictionary.md`](../data/data-dictionary.md)
**MUST NOT** masuk payload custom logger.

| Kolom | Tabel | Kenapa |
| --- | --- | --- |
| `Subjective`, `Objective`, `Assessment`, `Plan` | `TrxDoctorConsultation` | Isi klinis lengkap |
| Seluruh kolom rencana — tindakan, resep, penunjang, rujukan, edukasi | `TrxDoctorConsultation` | Sama |
| `Content` catatan | `TrxPatientIntegratedProgressNote` | Isi klinis |
| `AmendReason` | Seluruh tabel yang punya | Sering memuat alasan klinis atau nama pihak ketiga |
| `Note` | `TrxPhysicianVisit` | Catatan bebas |
| `ResultNote` | `TrxPatientProcedure` | Hasil tindakan |

**Masa simpan belum ditetapkan** — `RWI-OQ-035` menunggu pemilik hukum. Tidak ada penghapusan
otomatis yang dirancang, dan itu lebih aman daripada menebak masa simpan rekam medis.
