# Permission dan Audit Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.3.0` |
| `last_changed_in` | `0.3.0` |
| Status | `approved` — disetujui Muhammad Hamzah, 2026-09-03 |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`) |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-03** |
| `input_revision` | `api-contract.md` `0.2.0`; arsitektur domain `0.2` bagian V dan W |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Compatibility impact | `0.3.0`: Resource `ClinicalNoteAuthorDelegation` dan Action `CreateAsSubstitute` masuk peta peran; satu batas kewenangan per pasien yang baru ditambahkan pada bagian 3. Sebelumnya `0.2.0` mencabut Action `Amend` dan menambah `Cancel` pada `PhysicianVisit` |
| Tanggal | 2 September 2026 |

---

## 0. Yang tidak ada di dokumen ini

Dokumen ini **MUST NOT** memuat tabel seluruh endpoint. Pemetaan endpoint ke hak akses dipegang
kolom `Hak akses` pada [`api-contract.md`](./api-contract.md). Dua turunan berikut **dihitung**:

| Yang tidak ditulis ulang | Cara menurunkannya |
| --- | --- |
| String atribut | `[AccessPermission("<Resource>", "<Action>")]` disalin dari kolom `Hak akses` |
| Status pencatatan logger | `GET` tidak dicatat; selain `GET` dicatat |

**Pengecualian bernama:** tidak ada.

---

## 1. Cara kerja hak akses di repository ini

| Hal | Isinya |
| --- | --- |
| Atribut | `[AccessPermission("Resource", "Action")]` beserta `[AccessAction(...)]` pada setiap endpoint, dan `[AccessController]` pada kelasnya |
| Filter | `AccessPermissionFilter` mencocokkan pasangan Resource–Action terhadap baris hak akses peran |
| Jebakan yang sudah terjadi | `BE-RWI-034`: `[AccessAction]` dan `[AccessPermission]` menyebut nama berbeda, sehingga sembilan endpoint menjawab `403` bagi siapa pun kecuali SuperAdmin, dan menahan tujuh task frontend |

### 1.1 Yang ditambahkan sub-modul ini

| Tambahan | Bentuknya | Catatan |
| --- | --- | --- |
| Resource baru | `PhysicianVisit` | Dengan Action `Read`, `Create`, `Update`, dan **`Cancel`** |
| Action baru pada Resource yang sudah ada | `Verify` pada `PatientIntegratedProgressNote` | Hanya untuk DPJP |
| **Tidak ditambahkan** | Action `Amend` | Koreksi memakai `ClinicalNoteAddendum : Create` yang **sudah ada** |
| Butir yang dipakai dari modul lain | `ClinicalNoteAddendum : CreateAsSubstitute` | Koreksi atas nama dokter yang berhalangan. **Sudah ada** di source |
| Butir yang dipakai dari modul lain | `ClinicalNoteAuthorDelegation : Create`, `Read`, `Update` | Menerbitkan, membaca, dan mencabut penetapan berhalangan. **Sudah ada** di source |

> Ketiganya wajib memakai nama yang sama persis pada `[AccessAction]` dan `[AccessPermission]`, dan
> wajib diuji dengan peran non-SuperAdmin. Ini pelajaran langsung dari `BE-RWI-034`.

---

## 2. Peta peran ke butir hak akses

| Peran rumah sakit | Resource | Action |
| --- | --- | --- |
| DPJP | `DoctorConsultation` | `Read`, `Create`, `Update` |
| DPJP | `PatientAssessment` | `Read`, `Create`, `Update` — **terbatas jenis medis**, lihat bagian 3 |
| DPJP | `PatientIntegratedProgressNote` | `Read`, `Create`, `Update`, **`Verify`** |
| DPJP | `PatientProcedure` | `Read`, `Create`, `Update` |
| DPJP | `Prescription` | `Read`, `Create` |
| DPJP | `LabOrder` | `Read`, `Create` |
| DPJP | `RadOrder` | `Read`, `Create` |
| DPJP | `PhysicianVisit` | `Read`, `Create`, `Update`, `Cancel` |
| DPJP | `ClinicalNoteAddendum` | `Read`, `Create`, **`CreateAsSubstitute`** |
| **Kepala unit rawat inap** | `ClinicalNoteAuthorDelegation` | `Create`, `Read`, `Update` — menerbitkan dan mencabut penetapan berhalangan |
| Dokter jaga ruangan | Sama dengan DPJP **kecuali** `Verify` | — |
| Dokter konsulen | `Read` pada seluruhnya; `Create` pada CPPT dan `PhysicianVisit` | — |
| Perawat | `Read` pada catatan dokter, kajian medis, tindakan, resep, lab, dan radiologi; `Create` pada CPPT | Tidak ada `Verify`; addendum hanya pada catatannya sendiri |
| Ahli gizi | `Read` pada kajian medis dan CPPT | — |
| Supervisor klinis | `Read` pada seluruhnya; `Cancel` pada `PhysicianVisit` | Untuk membatalkan event salah catat bila dokternya berhalangan |
| Dokter jaga dan konsulen | `ClinicalNoteAddendum : Create` pada catatannya sendiri | **Tanpa** `CreateAsSubstitute`; koreksi atas nama dokter lain hanya milik DPJP aktif |
| Petugas Farmasi, Laboratorium, Radiologi | Milik modulnya sendiri | Sub-modul ini tidak mengaturnya |
| Petugas admisi, kasir | — | **Tidak ada** |

> **`Verify` hanya milik DPJP, dan itu inti `CAP-021` aturan 5.** Memberikannya kepada dokter jaga
> membuat verifikasi kehilangan artinya: yang diverifikasi adalah catatan yang menjadi tanggung
> jawab DPJP.
>
> **`Cancel` pada visite diberikan juga kepada supervisor klinis**, karena event salah catat bisa
> ditemukan setelah dokternya pulang. Yang tidak diberikan kepada siapa pun adalah penyuntingan
> waktu visite — jalur itu memang tidak ada.

---

## 3. Kewenangan yang tidak dapat dijaga mesin hak akses

Mesin hak akses tahu peran, tidak tahu pasien maupun jenis dokumen. Lima hal berikut dijaga di
tingkat aturan bisnis, sesuai arsitektur domain bagian V.2.

| Yang dijaga | Penjaganya | Yang **tidak** dijaganya | Risikonya |
| --- | --- | --- | --- |
| Dokter hanya menulis untuk pasien yang menjadi tanggung jawabnya — `INV-DOK-13` | `VAL-DOK-06`, memakai pemeriksaan dokter aktif per episode yang **sudah ada** di layanan episode | Dokter yang memang berwenang tetap dapat menulis apa saja | Kesalahan isi, bukan kewenangan. Dijaga jejak audit |
| **Dokter menulis kajian medis, perawat menulis pengkajian keperawatan** | `VAL-DOK-05`, bercabang menurut jenis kajian | Mesin hak akses melihat **satu** Resource untuk keduanya | **Ini akibat langsung berbagi satu tabel.** Bila pemilik memilih bentuk penyimpanan terpisah, penjagaan ini naik ke mesin hak akses |
| Verifikator adalah DPJP yang aktif saat verifikasi, dan bukan penulis asli — `INV-DOK-11` | `VAL-DOK-07` beserta kolom verifikator yang terpisah | Mesin hak akses hanya tahu peran DPJP secara umum | Verifikasi oleh DPJP episode lain |
| Visite hanya dicatat dokter | `VAL-DOK-08` | Kebijakan pencatatan atas nama dokter belum ada | Bawaan dipilih yang aman: hanya dokter |
| Dokumen dan hasil yang dibaca milik episode yang sedang dibuka — `INV-DOK-12` | `VAL-DOK-26`, `VAL-DOK-31` | Mesin hak akses tidak mengenal episode | **Risiko tertinggi pada scope ini**: membaca hasil pasien lain |
| **Koreksi atas nama dokter lain hanya oleh DPJP aktif episode itu** — `RWI-DEC-088` | `VAL-DOK-35`, memakai pemeriksaan dokter aktif per episode | **Penetapan berhalangan bersifat milik penulis**, bukan milik penggantinya — ia menyatakan "dokter ini berhalangan" tanpa menyebut siapa yang boleh menggantikan. Begitu penetapan berlaku, siapa pun pemegang butir pengganti dapat mengoreksi | **Blast radius lebih lebar dari yang diputuskan.** Tanpa penjaga tambahan, dokter mana pun yang memegang butir pengganti dapat mengoreksi catatan dokter yang berhalangan, termasuk pasien yang bukan tanggung jawabnya |

Baris kedua adalah harga yang dibayar jalan berbagi tabel pada `02-backend-architecture.md` bagian
4.2. Ia **ditulis di sini** supaya pemilik melihatnya sebelum menyetujui, bukan menemukannya saat
implementasi.

---

## 4. Audit

| Lapisan | Yang dicatat |
| --- | --- |
| Kolom warisan `IdentityModel` | Pembuat, waktu buat, pengubah, dan waktu ubah pada setiap baris |
| Custom logger | Seluruh permintaan selain `GET`, berisi id entity, controller, action, dan status |
| Mesin integritas dokumen | Penandatanganan, penguncian, pembatalan, dan setiap addendum beserta alasannya |
| Jejak tahan lama yang **wajib** | Penyelesaian kajian medis dan catatan dokter; **setiap verifikasi CPPT** beserta verifikatornya; **setiap event visite** beserta pencatatnya; **setiap pembatalan event visite** beserta alasannya; pelaksanaan tindakan beserta penerbitan faktanya |

| Kejadian | Kenapa wajib berjejak | Acceptance |
| --- | --- | --- |
| Verifikasi CPPT | Verifikator dapat diaudit dan **bukan** penulis asli | `AC-CAP021-03` |
| Pencatatan visite | Episode, dokter, peran, waktu, dan pencatat dapat diaudit | `RWI-AC-153` |
| Pembatalan visite | Riwayat tetap menampilkan event batal beserta alasannya; hitungan tidak menghitungnya | `INV-DOK-08` |
| Agregasi tagihan oleh Billing | Riwayat klinis **tidak berubah sedikit pun** setelah agregasi | `RWI-AC-156` |
| Koreksi dokumen final | Alasan, penulis, penulis pengganti, dan nomor urut tersimpan | `INV-DOK-10` |
| Pendaftaran dokumen ke mesin keutuhan saat finalisasi | Penanda tangan, waktu, dan jenis dokumen | `RWI-AC-157` |
| Koreksi atas nama dokter lain | Penulis asli **tidak berubah**; dokter pengganti dan penetapan yang mendasarinya tersimpan | `RWI-AC-163`, `RWI-AC-164` |
| Penerbitan dan pencabutan penetapan berhalangan | Penerbit, dokter yang berhalangan, alasan, masa berlaku, dan pencabutnya | `RWI-AC-165` |

> **Jumlah visite tidak boleh dihitung dari catatan aktivitas teknis**, melainkan hanya dari event
> berstatus `Recorded`. Menghitung dari sumber lain melahirkan angka kedua yang berpotensi
> berselisih — persis yang dilarang `RWI-DEC-085`.

---

## 5. Kolom sensitif dan masa simpan

Kolom bertanda **Sensitif** pada [`../data/data-dictionary.md`](../data/data-dictionary.md)
**MUST NOT** masuk payload custom logger.

| Kolom | Tabel | Kenapa |
| --- | --- | --- |
| Isi S/O/A/P beserta seluruh kolom rencana | `TrxDoctorConsultation` | Isi klinis lengkap |
| Isi catatan | `TrxPatientIntegratedProgressNote` | Isi klinis |
| Isian kajian | `TrxPatientAssessment` | Isi klinis |
| `Note` | `CliPhysicianVisit` | Catatan bebas dokter |
| `CancelReason` | `CliPhysicianVisit` | Sering memuat alasan klinis |
| Alasan koreksi dan teks addendum | `MrcClinicalNoteAddendum` | Sering memuat alasan klinis atau nama pihak ketiga |
| Hasil tindakan | `TrxPatientProcedure` | Isi klinis |

**Masa simpan belum ditetapkan** — `RWI-OQ-035` menunggu pemilik hukum. Tidak ada penghapusan
otomatis yang dirancang, dan itu lebih aman daripada menebak masa simpan rekam medis.
