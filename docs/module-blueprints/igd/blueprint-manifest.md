# IGD Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `IGD-BP-001` |
| `revision` | `6` |
| `status` | `draft` — **tidak disetujui siapa pun**. Revision `4` yang berstatus `approved sebagian` tetap berlaku sebagai baseline sampai revisi ini disetujui |
| `module` | `igd` / `EmergencyInstallationManagement`, prefix entity `TrxEmergency`/`MstEmergency` |
| `registry_lifecycle` | `ACTIVE` |
| `design_snapshot_at` | `2026-08-26` (revisi 6); revisi 5 pada `2026-08-24` |
| `backend_commit_sha` | `300922c` (branch `rizkiG`) — merge Hamzah/Ikbal/Yasmina. Revisi 5 disusun pada `f69e9e483052845d11c91d8b7bbdce33c4acc8d8` |
| `frontend_commit_sha` | `96a9120111f6acc6b7c0f37973ea0c717ba41f17` (branch `RizkiV2`) |
| `owners` | Product/Domain: **Rizki Gunawan**, ditetapkan `IGD-DEC-089` 2026-08-24. Clinical Governance, Nursing authority, Security/Privacy: `OPEN`. Pemilik `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`: **belum ditunjuk** |
| `approved_by` | Sebagian: **Rizki Gunawan** menyetujui `IGD-DEC-067`, `IGD-DEC-088`, `IGD-DEC-089`, dan `IGD-DEC-093` pada 24 Agustus 2026. Blueprint secara keseluruhan **belum** disetujui |
| `approved_at` | Sebagian: `2026-08-24` |
| `requirement_readiness` | **`UNCLASSIFIED`** — lihat bagian 0 |
| `domain_architecture_revision` | **Tidak ada** — lihat bagian 0 |
| `domain_architecture_readiness` | **`NOT_ASSESSED`** |
| `input_revisions` | `00-interview-decisions.md` **105 keputusan**, terakhir `IGD-DEC-105`; `01-existing-capability-map.md` revision `3` + **suplemen `3.1`** (audit terarah `EmergencyTransfer` pada `300922c`) |
| `delivery_state` | **`MVP-0` berjalan.** `BE-IGD-017`…`020` selesai 2026-08-26, **belum di-commit**. Suite `518 → 720`. Berikutnya `BE-IGD-021` (lima titik tulis observasi/resusitasi/disposisi) |
| `amendment` | **2026-08-24 (kedua)** — `IGD-OQ-068`/`070`/`071` ditutup. **2026-08-26 (correction pass revisi 6)** — enam butir, lihat 0a.2 sampai 0a.4 |
| `contract_versions` | API `0.4.0` **bukan aditif**; validation `0.4.0` **bukan aditif**; state `0.4.0` aditif; permission/audit `0.4.0` aditif; integration `0.3.0` tidak berubah. Rinciannya di 0a.2. Seluruhnya `draft`. `IGD-DEC-093` **tidak diperluas**: yang `approved` tetap hanya state §1/§1.1/§1.2 dan validation §2 aturan 4–5 |
| `roadmap_revision` | `3` — 2026-08-26, diperluas ke perjalanan pasien penuh: pendaftaran & triase, pengkajian, kepergian. Revision `2` (`MVP-0`) tetap di berkas yang sama; revision `1` diarsipkan ke `roadmap/archive/revision-1/`. **Penomoran gelombang bergeser**: pengkajian masuk `MVP-3`, kepergian ke `MVP-4`, serah terima `MVP-5`, kewenangan unit `MVP-6` |
| `belum_direncanakan` | **Penunjang medis, pemakaian alat, billing IGD.** Batas lingkup ditutup `IGD-DEC-095`…`105`; masih nol epic, nol FR, nol kontrak. **Ditahan atas instruksi Product/Domain Owner** sampai correction pass revisi 6 tuntas dan `MVP-0` selesai |
| `koreksi_desain_tertunda` | **Nihil.** Empat koreksi selesai pada revisi 6; enam butir correction pass 26 Agustus selesai — audit `EmergencyTransfer`, koreksi klaim aditif, penyelarasan metadata, pembentukan pesanan internal, unique constraint, kewenangan pesanan |
| `compatibility_impact` | **Empat perubahan memutus.** Lihat bagian 3 |

---

## 0. Gerbang kemampuan rumah sakit — BELUM TERPENUHI

Modul IGD **tidak memiliki** dua artefak hulu yang diwajibkan untuk kemampuan bisnis rumah
sakit:

| Artefak | IGD | Rawat Inap sebagai pembanding |
|---|---|---|
| `evidence/02-requirement-completeness-gate.md` | **Tidak ada** | Ada |
| `evidence/03-hospital-domain-architecture.md` | **Tidak ada** | Ada |

Akibatnya modul IGD tidak punya klasifikasi kesiapan requirement per slice. Substansinya
tersebar pada 88 keputusan, bukan pada berkas hulu tersendiri.

Gerbang ini **tidak** ditandai terpenuhi. Bila Product/Domain Owner menghendaki kesetaraan
dengan Rawat Inap, `/requirement-completeness-gate` dan `hospital-domain-architect` perlu
dijalankan lebih dulu dan revisi ini ditinjau ulang terhadap hasilnya.

---

## 0a. Yang berubah pada revision 6 — 26 Agustus 2026

Revisi sempit. **Empat koreksi** yang diminta Product/Domain Owner setelah Scope Pass ketiga,
ditambah satu penegasan arti. Tidak ada tabel baru, tidak ada endpoint yang dibuang.

| # | Koreksi | Keputusan | Berkas |
| ---: | --- | --- | --- |
| 1 | `EmergencyOrderAction` menjadi **`Continue`=1, `Handover`=2, `Cancel`=9** | `IGD-DEC-100` | `02-…` §3.4, `erd/emergency-departure.md` |
| 2 | `EmergencyOrderKind` menambah **`RadiologyOrder`=4** | `IGD-DEC-099` | Sama |
| 3 | **`EmergencyOrderSource`** memisahkan pesanan internal dari luar sistem; `OrderReferenceId` menjadi nullable, `ExternalReference` dan `OrderDescription` ditambahkan | `IGD-DEC-103` | Sama, + `contracts/api-contract.md` §2.3 |
| 4 | **`EmergencyOrderAcceptanceStatus`** — lifecycle penerimaan **per pesanan**, terpisah dari `EmergencyHandoverStatus` | `IGD-DEC-102` | Sama, + `contracts/state-transition-matrix.md` §6a |
| 5 | Arti validation §2 aturan 5 dipertegas per status kunjungan | `IGD-DEC-104` | `contracts/validation-matrix.md` §2.1 |

### 0a.1 Mengapa `Completed` bukan sekadar salah nama

Daftar sikap **hanya memuat pesanan yang belum selesai**, sehingga `Completed` adalah nilai
yang tidak pernah terpakai. Yang justru tidak punya nilai adalah keadaan sebenarnya: pesanan
yang **masih berjalan** dan akan diproses sampai hasil final meski pasien sudah pergi.

### 0a.2 Koreksi — revisi 6 **bukan** murni aditif

> **Klaim yang diperbaiki.** Terbitan pertama revisi 6 menyatakan kenaikan `0.3.0 → 0.4.0`
> bersifat *"aditif — nol bagian lama diubah"*. **Klaim itu salah.** Endpoint lama diganti, dan
> beberapa bagian validation berubah teksnya. Diperbaiki pada correction pass 26 Agustus 2026.

#### Yang benar-benar berubah, bukan sekadar bertambah

| # | Perubahan | Sifat |
| ---: | --- | --- |
| 1 | `GET /{id}/pending-orders` dan `POST /{id}/order-actions` **dihapus** dari tabel endpoint, diganti lima route keluarga `order-items` | **Memutus** secara kontrak — meski nol pemakai nyata, karena keduanya belum pernah diimplementasikan |
| 2 | Validation §5 aturan 2: *"Sikap `Cancelled` wajib beralasan"* → *"Sikap **`Cancel`** wajib beralasan"* | Teks berubah mengikuti `IGD-DEC-100` |
| 3 | Validation §5 aturan 4: *"Pemeriksaan penunjang tidak ikut dihitung"* → *"…tidak ikut dihitung **otomatis**"* | Teks berubah — artinya menyempit, bukan sekadar bertambah |
| 4 | `EmergencyOrderAction` dan `EmergencyOrderKind` **diganti nilainya**, bukan ditambah | Entitas `New` yang belum diimplementasikan; nol data terdampak |
| 5 | `OrderReferenceId` berubah dari wajib menjadi **nullable** | Sama seperti nomor 4 |
| 6 | Rumusan unique constraint diganti seluruhnya | Correction pass; rumusan lama **tidak dapat ditegakkan** — lihat `02-backend-architecture.md` §11.2 |

#### Satu baris `approved` yang ikut tersentuh

| Bagian | Yang berubah | Yang **tidak** berubah |
| --- | --- | --- |
| Validation §2 **aturan 5** | Kolom *Keputusan* bertambah *"; artinya dipertegas `IGD-DEC-104`"* | Kolom **Aturan, Kode, dan Pesan identik** — isi normatifnya utuh |
| Validation §2 aturan 4 | — | **Seluruhnya identik** |
| State §1, §1.1, §1.2 | — | **Seluruhnya identik.** `§6a` adalah bagian baru yang berdiri sendiri |

Perubahan pada aturan 5 hanyalah rujukan ke keputusan yang menafsirkannya, ditetapkan
**approver yang sama** lewat `IGD-DEC-104`. Aturan yang ditegakkan kode **tidak berubah**, dan
`BE-IGD-019` yang berjalan di atasnya tetap sahih.

Meski begitu, menyebutnya "teks identik" **tidak akurat** dan sudah diperbaiki di sini.

#### Akibat pada penomoran versi

Karena bukan murni aditif, kenaikan `0.3.0 → 0.4.0` pada API dan validation adalah
**perubahan yang memutus pada tingkat kontrak**, bukan penambahan. State `0.4.0` tetap aditif —
`§6a` murni bagian baru.

### 0a.3 Gerbang yang dilanggar sebagian, dan alasannya dicatat

| Gerbang `/qv-design` | Keadaan |
| --- | --- |
| Decision log `approved` | **Tidak terpenuhi.** `IGD-DEC-099`…`104` seluruhnya `draft`. Sama seperti revisi 5, yang juga disusun di atas keputusan `draft`; keluaran desain pun `draft` |
| Capability map terbaru | **Stale** — revision `3` dihitung pada `f69e9e48`, `HEAD` kini `300922c`. **Tidak berdampak pada revisi ini**: kelima entitas yang dikoreksi — `EmergencyOrderAction`, `EmergencyOrderKind`, `TrxEmergencyHandoverOrderItem`, `TrxEmergencyDeparture`, `EmergencyPhysicalStatus` — **nol berkas di source**. Tidak ada perilaku existing yang dapat salah dibaca |

Capability map tetap **perlu** diperbarui sebelum gelombang yang menyentuh kode yang sudah ada.

---

### 0a.4 Revisi 6 tetap `draft` — approval yang diajukan

**Tidak satu pun bagian revisi 6 ditandai `approved`.** Correction pass 26 Agustus 2026
menegaskannya kembali atas permintaan Product/Domain Owner.

`IGD-DEC-093` yang lama **tetap berlaku apa adanya** dan **tidak diperluas** oleh revisi ini:
ia menyetujui state §1/§1.1/§1.2 dan validation §2 aturan 4–5, dan hanya itu. `BE-IGD-018`,
`019`, dan `020` berjalan di atas irisan itu — sah, dan tidak terpengaruh revisi 6.

#### Yang membutuhkan approval, dan dari siapa

| Bagian | Approver | Kenapa mereka |
| --- | --- | --- |
| `IGD-DEC-100` — tiga sikap pesanan, larangan pembatalan otomatis | **Clinical Governance** | Menentukan pesanan klinis mana yang boleh dihentikan saat pasien pergi |
| `IGD-DEC-101` — sikap pesanan lab ditetapkan manual klinisi | **Clinical Governance** + **pemilik `LaboratoryManagement`** | Menetapkan sikap tanpa data status adalah penilaian klinis; dan lab yang menanggung akibatnya |
| `IGD-DEC-102` — penerimaan per pesanan, penolakan tidak membatalkan penerimaan pasien | **Nursing authority** | Serah terima antar-unit adalah pekerjaan keperawatan |
| Permission §3.1 — kewenangan `accept`/`reject` atas unit tujuan | **Nursing authority** + Security/Privacy owner | Menentukan siapa berhak menyatakan penerimaan |
| Validation §5 aturan 5 — larangan menampilkan sikap lab seolah dari `LabOrder` | **Pemilik `LaboratoryManagement`** | Melindungi lab dari klaim yang tidak mereka buat |
| `02-backend-architecture.md` §11.1 — pembentukan baris `Medication` dan `Procedure` | Pemilik `PharmacyManagement` dan `ClinicalManagement` | Membaca tabel milik mereka |

#### Tiga peran approver itu **belum ditunjuk**

Clinical Governance, Nursing authority, dan pemilik `LaboratoryManagement` seluruhnya masih
kosong. Permintaan penunjukannya sudah disiapkan di
`approval-requests/2026-08-24-permintaan-penunjukan-pemilik-modul.md`, dan `IGD-OQ-081`
mencatat langkah termurahnya: menanyakan `andryzainhome` yang membuat fondasi
`LaboratoryManagement` lewat commit `1a8a9ce`.

**Akibatnya butir 10 Definition of Done tidak dapat dijawab "ya"** untuk `EPIC IGD-07` maupun
gelombang mana pun yang memakai bagian di atas. Ini dicatat terbuka, bukan dilewati.

---

## 1. Yang berubah pada revision 5

| Area | Perubahan |
|---|---|
| Capability map | Revision `2` **dibuang seluruhnya**, diganti revision `3`. Bukti lama menunjuk nama repository dan path yang tidak ada lagi |
| Keputusan | Dua puluh dua keputusan baru `IGD-DEC-067` sampai `IGD-DEC-088`; `IGD-DEC-081` `superseded` sebagian |
| Jenis kunjungan | Kunjungan IGD menjadi `EncounterType.Emergency` |
| Pencatatan klinis | Pembatas antrean dan konsultasi dilonggarkan untuk kunjungan `Emergency` |
| Kepergian pasien | `TrxEmergencyTransfer` menjadi `TrxEmergencyDeparture` dengan dua rangkaian status |
| Tempat tidur | Seluruh urusan tempat tidur pindah ke Rawat Inap |
| Kepemilikan pasien | Matriks lengkap tanpa satu pun keadaan tanpa pemilik |
| Audit klinis | Koreksi bersifat tambah-saja |
| Kewenangan unit | Jembatan `MstServiceUnit` ke simpul organisasi, bukan tabel penugasan baru |
| Dokumen baru | `04-prd-to-mvp.md`, `erd/emergency-departure.md` |
| Arsip | Revision `4` disimpan di `archive/`, tidak dihapus |

---

## 2. Artifact hashes

Dihitung 24 Agustus 2026 pada keadaan akhir seluruh berkas.

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | `a48c1e9105e8caa595daade7cd6b7903993d1ce275e104dd0bc811761fb10dea` |
| `01-existing-capability-map.md` | `75da54a6f529b76bc42e996c924f4004723046d84355694b1e11474036244bb5` |
| `02-backend-architecture.md` | `b7c93dda4ffeb66b8f9541f136eb09aa89a43fb305b7a6dd8856c2b92ec23992` |
| `03-frontend-architecture.md` | `2b4339f9587ed1daff8444ccb68cb5415df578d76a2157dd3ec168f9a2a1fd95` |
| `04-prd-to-mvp.md` | `7061525001d9a7e6b311424b8e3a8d85de13e35f59e545a78dcefedd600b79db` |
| `erd/00-context-erd.md` | `60c862c6516e6bc641c3fea61725d58fa805a2acfdab5315c567f250a3403c63` |
| `erd/emergency-episode.md` | `43f2403785d3b0cd5ba6bdd59854b8f24eb0904e03d8f8d746a0772a0a727822` |
| `erd/emergency-departure.md` | `0f5445e06cc9bbd60ed15942cb9aeae34d109baee142af76995f0664909a8097` |
| `erd/data-dictionary.md` | `fb6586a130ba527c3d27ec979c66f5a3e3f8a212270f928a16440170f28b0636` |
| `contracts/api-contract.md` | `a1dd52d376e7e434554a26379ff3bb0258f23816934f84251c95c9bd9b70d9ef` |
| `contracts/state-transition-matrix.md` | `ca6f3e8b8cfee84cdcf94a56ce306bea84df0db5372ce638059650a7859ccea8` |
| `contracts/validation-matrix.md` | `f0831b4d89f13b152cc587a3951735875fc5486d5b17990947b3c56b157095b8` |
| `contracts/integration-contract.md` | `98590b1b5e587fe10f0338f4f7da3aba3a0a448231ffef02b06592f2f7633bdb` |
| `contracts/permission-audit-matrix.md` | `cb2cc6a102726f523098a56317b92a1dd8f4b9d5b88a243b4e3ac42e9cc6b993` |
| `testing/acceptance-test-matrix.md` | `c3c37e0b38f9fcaf8b6cfc5ebdd5809cddfaff7992554bed47e807f1bc701de2` |

Manifest tidak menghitung hash dirinya sendiri.

`roadmap/` **tidak** diperbarui pada revisi ini. Isinya masih roadmap revision `1` yang
seluruh task-nya sudah dikerjakan. Roadmap baru adalah keluaran `/qv-plan`, bukan `/qv-design`.

---

## 3. Dampak kompatibilitas

### 3.1 Empat perubahan yang memutus

| Perubahan | Siapa yang terdampak |
|---|---|
| Kunjungan IGD wajib `EncounterType.Emergency` | Setiap pemanggil yang mengirim `Outpatient`, termasuk test `FE-IGD-001 K1` yang **akan gagal** |
| Grup `emergency-transfers` menjadi `emergency-departures` | Seluruh pemanggil route lama |
| `TransferStatus` dipecah menjadi dua kolom | Pembaca satu kolom status |
| Empat field tempat tidur dan ruangan dihapus | Pengirim field tersebut |

### 3.2 Perubahan pada tabel milik modul lain

**Sembilan tabel** yang bukan milik IGD terdampak: `TrxPatientEncounter`, `MstServiceUnit`,
`TrxPatientAssessment`, `TrxDoctorConsultation`, `TrxPatientDiagnosis`, `TrxPatientProcedure`,
`TrxPatientVitalSign`, `TrxPatientIntegratedProgressNote`, `TrxPrescription`.

Janji "nol perubahan kolom pada tabel modul lain" yang dipegang blueprint Rawat Inap **tidak
dapat dipertahankan** untuk IGD, dan `IGD-DEC-075` juga membatalkannya bagi Rawat Inap.

### 3.3 Yang bersifat aditif

Empat tabel baru, sebelas endpoint baru, lima enum baru, tiga service baru, dua controller
baru. Tidak satu pun memutus pemakai lama.

---

## 4. Gerbang sebelum produksi

| Gerbang | Menunggu | Memblokir |
|---|---|---|
| Penunjukan pemilik `ClinicalManagement` dan `PharmacyManagement` | Organisasi | **`EPIC IGD-09`** — pengkajian, diagnosis, tindakan, resep IGD |
| Persetujuan Muhammad Hamzah atas revisi `RWI-RULE-026` dan `compatibility_impact` | Product/Domain Owner Rawat Inap | `EPIC IGD-09` |
| Persetujuan pemilik IGD atas `RWI-OQ-034` / `DEC-INP-002` | **Pemilik IGD, nama belum tercatat** | Slice `INP-S09` milik Rawat Inap |
| ~~`IGD-OQ-068` penafsiran kolom status dan tabel kejadian~~ | **Terjawab** `IGD-DEC-090` 2026-08-24 | — |
| ~~`IGD-OQ-070` penggantian nama tabel dan route~~ | **Terjawab** `IGD-DEC-091` 2026-08-24 | — |
| ~~`IGD-OQ-071` perilaku unit tanpa pemetaan organisasi~~ | **Terjawab sementara** `IGD-DEC-092` 2026-08-24. Pengesahan Security/Privacy owner masih ditunggu | Penyalaan penjagaan di produksi, **bukan** implementasi |
| Pengisian pemetaan unit selesai **sebelum** penjagaan dinyalakan | Pemilik Master Data | Syarat melekat `IGD-DEC-092`; `MVP-5` |
| `IGD-UNK-01` … `IGD-UNK-07` | Kueri ke basis data bersama | `MVP-1`, `MVP-3`, `MVP-5` |
| Otorisasi menjalankan migration | Pemilik basis data pengembangan | Seluruh gelombang |
| Data master kelas pasien IGD | Penanggung jawab data master | `MVP-1` |
| Pemetaan unit ke simpul organisasi | Master Data + Corporate/HR | `MVP-5` |
| SOP triase dan SOP pengkajian ulang | Clinical governance MMC | Nilai batas waktu; **tidak** memblokir kode |
| Break-glass dan pemisahan SuperAdmin | Security/Privacy owner | Produksi |

Gerbang yang belum terpenuhi berarti menolak tindakan privileged, integrasi, atau finansial
yang terdampak. Gerbang **tidak pernah** memblokir pelayanan klinis darurat.

### 4.0 Amendment 2026-08-24 (kedua) — larangan `/qv-plan` dicabut

`04-prd-to-mvp.md` bagian 7 berbunyi: dokumen ini *"**tidak boleh** diteruskan ke `/qv-plan`
sebelum `IGD-OQ-068`, `IGD-OQ-070`, dan `IGD-OQ-071` dijawab"*. **Ketiganya sudah dijawab**
pada 24 Agustus 2026 lewat `IGD-DEC-090`, `IGD-DEC-091`, dan `IGD-DEC-092`. Larangan itu
**tidak lagi berlaku**, dan `/qv-plan` boleh berjalan untuk seluruh gelombang.

`04-prd-to-mvp.md` **sengaja tidak disunting** oleh Amendment Pass: menyunting keluaran
`/qv-design` dari dalam pass wawancara melanggar batas peran, dan hash-nya akan melenceng
tanpa perhitungan ulang seluruh artefak. Manifest inilah yang berwenang atas keadaan gerbang.
Bagian 7 dokumen tersebut akan diselaraskan pada pass `/qv-design` berikutnya.

`IGD-DEC-092` bersifat **sementara** — ia membuka implementasi `EPIC IGD-08`, tetapi
**tidak** membuka penyalaan penjagaan di produksi. Dua hal itu berbeda.

### 4.1 Satu gelombang yang tidak terhalang apa pun

**`MVP-0` tidak bergantung pada satu pun gerbang di atas.** Isinya: pengisian master kelas
pasien IGD, pemetaan unit ke simpul organisasi, dan `EPIC IGD-03` — perbaikan status kunjungan
yang dapat mundur. `EPIC IGD-03` adalah perbaikan cacat murni yang tidak memerlukan keputusan
siapa pun.

---

## 5. Yang tidak dikerjakan revisi ini

| Yang tidak dikerjakan | Alasan |
|---|---|
| Menandai desain `approved` | Approval adalah tindakan manusia |
| Source code, migration, endpoint, atau UI | Di luar wewenang tahap desain |
| Roadmap dan task | Keluaran `/qv-plan` |
| Pemetaan proses ke ClickUp | Tidak diminta |
| Menjalankan kueri ke basis data | Basis data dipakai bersama satu tim |
| Memperbarui `blueprint-manifest.md` milik Rawat Inap | Bukan wewenang IGD; diusulkan, bukan diberlakukan |

---

## 6. Impact scan 2026-08-24

| Repository | SHA capability map revision 3 | SHA saat desain disusun | Hasil |
|---|---|---|---|
| Backend | `f69e9e48` | `f69e9e48` | **Sama.** Nol berkas `.cs` berubah |
| Frontend | `96a91201` | `96a91201` | **Sama.** Working tree bersih |

Bukti source pada capability map revision `3` karena itu **sahih** dan tidak perlu diaudit
ulang sebelum implementasi dimulai.
